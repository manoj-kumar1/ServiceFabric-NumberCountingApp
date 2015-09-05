using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services;
using Microsoft.ServiceFabric.Services.Wcf;
using System.ServiceModel;

namespace NumberCountingService
{
    public class NumberCountingService : StatefulService, INumberCounter
    {
        protected override ICommunicationListener CreateCommunicationListener()
        {
            try
            {
                return new WcfCommunicationListener(typeof(INumberCounter), this)
                {
                    //
                    // The name of the endpoint configured in the ServiceManifest under the Endpoints section
                    // which identifies the endpoint that the wcf servicehost should listen on.
                    //
                    EndpointResourceName = "ServiceEndpoint",

                    // 
                    // Populate the binding information that you want the service to use.
                    //
                    Binding = CreateListenBinding()
                };
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.LogServiceError(ex);
                throw;
            }
            
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var numbersDic = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("numbersDic");

                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        var result = await numbersDic.TryGetValueAsync(tx, "Counter-1");
                        ServiceEventSource.Current.ServiceMessage(
                            this,
                            "Current Counter Value: {0}",
                            result.HasValue ? result.Value.ToString() : "Value does not exist.");

                        await numbersDic.AddOrUpdateAsync(tx, "Counter-1", 0, (k, v) => ++v);

                        await tx.CommitAsync();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(0.5), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.LogServiceError(ex);
                throw;
            }
        }

        public async Task<long> GetCurrentNumber()
        {
            try
            {
                using (var tx = this.StateManager.CreateTransaction())
                {
                    var numbersDic = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("numbersDic");
                    var result = await numbersDic.TryGetValueAsync(tx, "Counter-1");

                    return result.Value;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.LogServiceError(ex);
                throw;
            }
        }

        private static NetTcpBinding CreateListenBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                //
                // Pick these values from service config
                //
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxConnections = int.MaxValue,
                MaxReceivedMessageSize = 1024 * 1024
            };

            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            return binding;
        }
    }
}
