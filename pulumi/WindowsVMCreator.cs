using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.Network.Inputs;

namespace k3s_azure;
using Pulumi;
using AzureNative = Pulumi.AzureNative;
public static class WindowsVMCreator
{
    public static void Create(string location, string resourceGroupName, string virtualNetworkName, string subnetName, string networkInterfaceName)
    {
        var resourceGroup = new AzureNative.Resources.ResourceGroup(resourceGroupName, new AzureNative.Resources.ResourceGroupArgs
        {
            Location = location,
        });
        
        var publicIp = new AzureNative.Network.PublicIPAddress("public-ip", new AzureNative.Network.PublicIPAddressArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = location,
            PublicIPAllocationMethod = "Static",
            Sku = new PublicIPAddressSkuArgs
            {
                Name = "Standard"
            }
        });
    
        var virtualNetwork = new AzureNative.Network.VirtualNetwork(virtualNetworkName, new()
        {
            AddressSpace = new AzureNative.Network.Inputs.AddressSpaceArgs
            {
                AddressPrefixes = new[]
                {
                    "10.0.0.0/16",
                },
            },
            Location = location,
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = virtualNetworkName,
        });
    
        var subnet = new AzureNative.Network.Subnet(subnetName, new()
        {
            AddressPrefix = "10.0.0.0/16",
            ResourceGroupName = resourceGroup.Name,
            SubnetName = subnetName,
            VirtualNetworkName = virtualNetwork.Name,
        });

        var nsg = new AzureNative.Network.NetworkSecurityGroup("nsg", new AzureNative.Network.NetworkSecurityGroupArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = location,
            SecurityRules = new SecurityRuleArgs[]
            {
                new SecurityRuleArgs
                {
                    Name = "RDP",
                    Priority = 100,
                    Direction = "Inbound",
                    Access = "Allow",
                    Protocol = "Tcp",
                    SourceAddressPrefix = "*",
                    SourcePortRange = "*",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "3389"
                }
            }
        });

        var networkInterface = new AzureNative.Network.NetworkInterface(networkInterfaceName, new()
        {
            IpConfigurations = new InputList<NetworkInterfaceIPConfigurationArgs>()
            {
                new []{ new NetworkInterfaceIPConfigurationArgs()
                {
                    Name = "internal",
                    Subnet = new SubnetArgs()
                    {
                        Id = subnet.Id
                    },
                    PrivateIPAllocationMethod = 
                        "Dynamic",
                    PublicIPAddress = new PublicIPAddressArgs
                    {
                        Id = publicIp.Id
                    }
                } }
            },
            NetworkSecurityGroup = new NetworkSecurityGroupArgs
            {
                Id = nsg.Id
            },
            ResourceGroupName = resourceGroup.Name,
            NetworkInterfaceName = networkInterfaceName,
            Location = location,
        });        
      
         var virtualMachine = new AzureNative.Compute.VirtualMachine("windows2019", new()
        {
            HardwareProfile = new AzureNative.Compute.Inputs.HardwareProfileArgs
            {
                VmSize = VirtualMachineSizeTypes.Standard_D2,
            },
            Location = resourceGroup.Location,
            NetworkProfile = new AzureNative.Compute.Inputs.NetworkProfileArgs
            {
                NetworkInterfaces = new[]
                {
                    new AzureNative.Compute.Inputs.NetworkInterfaceReferenceArgs
                    {
                        Id = networkInterface.Id,
                        Primary = true,
                    },
                },
            },
            OsProfile = new AzureNative.Compute.Inputs.OSProfileArgs
            {
                AdminUsername = "drakon660",
                AdminPassword = "WYUP6x4g9v!d", 
                ComputerName = "windows2019",
                WindowsConfiguration = new AzureNative.Compute.Inputs.WindowsConfigurationArgs()
                {
                    PatchSettings = new AzureNative.Compute.Inputs.PatchSettingsArgs()
                    {
                        AssessmentMode = "ImageDefault",
                    },
                    ProvisionVMAgent = true,
                },
            },
            ResourceGroupName = resourceGroup.Name,
            StorageProfile = new AzureNative.Compute.Inputs.StorageProfileArgs
            {
                ImageReference = new AzureNative.Compute.Inputs.ImageReferenceArgs
                {
                    Offer = "WindowsServer",
                    Publisher = "MicrosoftWindowsServer",
                    Sku = "2019-Datacenter",
                    Version = "latest",
                },
                OsDisk = new AzureNative.Compute.Inputs.OSDiskArgs
                {
                    Caching = AzureNative.Compute.CachingTypes.ReadWrite,
                    CreateOption = "FromImage",
                    ManagedDisk = new AzureNative.Compute.Inputs.ManagedDiskParametersArgs
                    {
                        StorageAccountType = StorageAccountTypes.StandardSSD_LRS,
                    },
                    Name = "windows2019Disk",
                    DeleteOption = DiskDeleteOptionTypes.Delete,
                },
                
            },
            VmName = "windows2019",
        });
    }
}