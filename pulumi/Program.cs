using k3s_azure;
using AzureNative = Pulumi.AzureNative;

return await Pulumi.Deployment.RunAsync(() =>
{
    WindowsVMCreator.Create("westus","westus-1", "vm-windows-vn","vm-windows-subnet","vm-windows-vi");

    //VmCreator.CreateLinuxVm("northcentralus","northcentralus-1","vm-linux-vi");

    // Create an Azure Resource Group  
});