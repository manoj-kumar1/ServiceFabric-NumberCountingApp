using Microsoft.ServiceFabric.Services;
using Microsoft.ServiceFabric.Services.Wcf;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NumberCountingService.Client
{
    class Program
    {
        private static readonly Uri ServiceName = new Uri("fabric:/NumberCounterApp/NumberCountingService");

        static void Main(string[] args)
        {
            try
            {

                var client = GetNumCountingSvcClient();

                Timer timer = new Timer()
                {
                    Enabled = true,
                    Interval = 2000
                };
                var tickCounter = 1;
                Console.WriteLine("Observing counter:");
                Task.Run(() => PrintCurrentNumber(client, tickCounter));
                timer.Elapsed += (object sender, ElapsedEventArgs e) =>
                {
                    tickCounter++;
                    Task.Run(() => PrintCurrentNumber(client, tickCounter));
                };
                timer.Start();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task PrintCurrentNumber(NumCountingSvcClient client, int tickCounter)
        {
            var num = await client.GetCurrentNumber();

            Console.WriteLine(string.Format("Tick {0} : {1}", tickCounter, num));
        }

        private static NumCountingSvcClient GetNumCountingSvcClient()
        {
            ServicePartitionResolver serviceResolver = new ServicePartitionResolver(() => new FabricClient() { });

            // Binding for WCF
            NetTcpBinding binding = CreateClientConnectionBinding();

            return new NumCountingSvcClient(
                new WcfCommunicationClientFactory<INumberCounter>(serviceResolver, binding, null, null),
                ServiceName);
        }

        private static NetTcpBinding CreateClientConnectionBinding()
        {
            //
            // Pick these values from client's config.
            //
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxReceivedMessageSize = 1024 * 1024
            };
            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            return binding;
        }
    }
}
