Connect-ServiceFabricCluster "localhost:19000"

Remove-ServiceFabricService -ServiceName fabric:/NumberCounterApp/NumberCountingService -Force

Remove-ServiceFabricApplication -ApplicationName fabric:/NumberCounterApp -Force

Unregister-ServiceFabricApplicationType -ApplicationTypeName NumberCounterAppType -ApplicationTypeVersion 1.0.0.0 -Force

Remove-ServiceFabricApplicationPackage -ApplicationPackagePathInImageStore NumberCounterAppType -ImageStoreConnectionString  fabric:ImageStore