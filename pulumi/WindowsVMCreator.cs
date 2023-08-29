using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.Network.Inputs;

namespace k3s_azure;
using System.Collections.Generic;
using Pulumi;
using AzureNative = Pulumi.AzureNative;
public static class UbuntuVMCreator
{
    public static void Create(string location, string resourceGroupName, string virtualNetworkName, string subnetName, string networkInterfaceName)
    {
        var publicIp = new AzureNative.Network.PublicIPAddress("public-ip", new AzureNative.Network.PublicIPAddressArgs
        {
            ResourceGroupName = resourceGroupName,
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
            ResourceGroupName = resourceGroupName,
            VirtualNetworkName = virtualNetworkName,
        });
    
        var subnet = new AzureNative.Network.Subnet(subnetName, new()
        {
            AddressPrefix = "10.0.0.0/16",
            ResourceGroupName = resourceGroupName,
            SubnetName = subnetName,
            VirtualNetworkName = virtualNetwork.Name,
        });

        var nsg = new AzureNative.Network.NetworkSecurityGroup("nsg", new AzureNative.Network.NetworkSecurityGroupArgs
        {
            ResourceGroupName = resourceGroupName,
            Location = location,
            SecurityRules = new SecurityRuleArgs[]
            {
                new SecurityRuleArgs
                {
                    Name = "SSH",
                    Priority = 100,
                    Direction = "Inbound",
                    Access = "Allow",
                    Protocol = "Tcp",
                    SourceAddressPrefix = "*",
                    SourcePortRange = "*",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "22"
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
            ResourceGroupName = resourceGroupName,
            NetworkInterfaceName = networkInterfaceName,
            Location = location,
        });        
      
         var virtualMachine = new AzureNative.Compute.VirtualMachine("ubuntu1804", new()
        {
            HardwareProfile = new AzureNative.Compute.Inputs.HardwareProfileArgs
            {
                VmSize = VirtualMachineSizeTypes.Standard_D2,
            },
            Location = location,
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
                AdminPassword = "fruy4brEC$", //WYUP6x4g9v!d
                ComputerName = "myVM",
                LinuxConfiguration = new AzureNative.Compute.Inputs.LinuxConfigurationArgs
                {
                    PatchSettings = new AzureNative.Compute.Inputs.LinuxPatchSettingsArgs
                    {
                        AssessmentMode = "ImageDefault",
                    },
                    ProvisionVMAgent = true,
                },
            },
            ResourceGroupName = resourceGroupName,
            StorageProfile = new AzureNative.Compute.Inputs.StorageProfileArgs
            {
                ImageReference = new AzureNative.Compute.Inputs.ImageReferenceArgs
                {
                    Offer = "UbuntuServer",
                    Publisher = "Canonical",
                    Sku = "22_04-lts",
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
                    Name = "myubuntu1804Disk",
                    DeleteOption = DiskDeleteOptionTypes.Delete,
                },
                
            },
            VmName = "drakonUbuntuAzure",
        });
    }
}