using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperUSB.Utility
{
    public static class DeviceFinder
    {
        public const string ProductId = "0x7401";
        public const string VendorId = "0x0C45";
        public const string DevicePathControl = "mi_00";
        public const string DevicePathBulk = "mi_01";

        public static List<DeviceInfo> GetDevices()
        {
            var lst = new List<DeviceInfo>();

            var temperItems = HidDevices.Enumerate().Where(x => x.Attributes.ProductHexId == ProductId & x.Attributes.VendorHexId == VendorId).ToList();            

            var controls = temperItems.Where(x => x.DevicePath.Contains(DevicePathControl)).ToList();
            var bulks = temperItems.Where(x => x.DevicePath.Contains(DevicePathBulk)).ToList();

            var counter = 0;

            while (controls.Count > counter && bulks.Count > counter)
            {
                var d = new DeviceInfo { Control = controls[counter], Bulk = bulks[counter] };
                lst.Add(d);
                counter++;
            }            

            return lst;
        }
    }
}
