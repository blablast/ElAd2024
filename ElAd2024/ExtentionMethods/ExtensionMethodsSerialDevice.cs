using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ElAd2024.Models;
using Windows.Devices.SerialCommunication;

namespace ElAd2024.ExtentionMethods;

public static class ExtensionMethodsSerialDevice
{
    public static void CopyTo(this SerialDevice serialDevice, SerialPortInfo serialPortInfo )
    {
        var serialPortInfoProperties = serialPortInfo.GetType().GetProperties();
        var serialDeviceProperties = serialDevice.GetType().GetProperties();

        foreach (var spDeviceProp in serialDeviceProperties)
        {
            // Check if the corresponding property in SerialPortInfo is marked with the OmitProperty attribute
            var spInfoProp = serialPortInfoProperties.FirstOrDefault(p => p.Name == spDeviceProp.Name && p.PropertyType == spDeviceProp.PropertyType && p.CanWrite && p.GetCustomAttribute<OmitPropertyAttribute>() is null);

            if (spInfoProp is not null)
            {
                try
                {
                    spInfoProp.SetValue(serialPortInfo, spDeviceProp.GetValue(serialDevice));
                }
                catch
                {
                    Debug.WriteLine($"Failed to set {spInfoProp.Name} from {spDeviceProp.Name}");
                }
            }
        }
    }




}
