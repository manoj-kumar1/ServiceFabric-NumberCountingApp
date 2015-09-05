#Remove app and service
$ScriptPath = Split-Path $MyInvocation.InvocationName
& "$ScriptPath\RemoveCloudFrontEnd.ps1"

$dp0 = Split-Path -parent $PSCommandPath
$SolutionDir = (Get-Item "$dp0\..\").FullName
$applicationPath = "$SolutionDir"+"bin\Debug\CloudFrontEndApp"

#Write-Output $applicationPath
Connect-ServiceFabricCluster "hk-fab-simc.westus.cloudapp.azure.com:19000"

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $applicationPath -ImageStoreConnectionString fabric:ImageStore

Register-ServiceFabricApplicationType -ApplicationPathInImageStore CloudFrontEndApp

New-ServiceFabricApplication -ApplicationName fabric:/CloudFrontEndApp -ApplicationTypeName CloudFrontEndApp -ApplicationTypeVersion 1.0

New-ServiceFabricService -ApplicationName fabric:/CloudFrontEndApp -ServiceName fabric:/CloudFrontEndApp/CloudFrontEndService -ServiceTypeName CloudFrontEndService -Stateful -HasPersistedState -PartitionSchemeSingleton -TargetReplicaSetSize 2 -MinReplicaSetSize 2

