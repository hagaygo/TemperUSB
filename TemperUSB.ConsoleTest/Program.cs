using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemperUSB.Utility;

namespace TemperUSB.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var lst = DeviceFinder.GetDevices();
            if (lst.Count > 0)
            {                
                for (int i = 0; i < 1000; i++)
                {
                    int deviceCounter = 0;
                    foreach (var d in lst)
                    {
                        d.Open();
                        deviceCounter++;
                        Console.WriteLine("Device {0} : {1}", deviceCounter, d.GetTemperature());
                        d.Close();
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
                
        }
    }
}
