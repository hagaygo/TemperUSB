using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperUSB.Utility
{
    public class DeviceInfo
    {
        public HidDevice Control { get; set; }
        public HidDevice Bulk { get; set; }

        public double Calibration_Offset = 0;
        public double Calibration_Scale = 1;

        byte[] ini = { 0x01, 0x01 };
        byte[] temp = { 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 };
        byte[] ini1 = { 0x01, 0x82, 0x77, 0x01, 0x00, 0x00, 0x00, 0x00 };
        byte[] ini2 = { 0x01, 0x86, 0xff, 0x01, 0x00, 0x00, 0x00, 0x00 };

        private void ReadBogus(HidReport report)
        {
        }

        public bool IsInitialised { get; set; }

        public double? GetTemperature()
        {
            if (Bulk.IsOpen && IsInitialised)
            {
                var outData = Bulk.CreateReport();
                outData.ReportId = 0x00;
                outData.Data = temp;
                Bulk.WriteReport(outData);
                while (outData.ReadStatus == HidDeviceData.ReadStatus.NoDataRead) ;
                
                var report = Bulk.ReadReport();
                int RawReading = (short)(report.Data[3] | ((int)report.Data[2] << 8));//(report.Data[3] & 0xFF) + (report.Data[2] << 8);
                return (Calibration_Scale * (RawReading * (125.0 / 32000.0))) + Calibration_Offset;                
            }
            return null;
        }

        const int Timeout = 100;

        public void Open()
        {
            Control.OpenDevice();
            Bulk.OpenDevice();
            IsInitialised = false;

            var outData1 = Control.CreateReport();
            outData1.ReportId = 0x01;
            outData1.Data = ini;
            Control.WriteReport(outData1);
            while (outData1.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Control.ReadReport(ReadBogus);

            var outData3 = Bulk.CreateReport();
            outData3.ReportId = 0x00;
            outData3.Data = ini1;
            Bulk.WriteReport(outData3);
            while (outData3.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);

            var outData4 = Bulk.CreateReport();
            outData4.ReportId = 0x00;
            outData4.Data = ini2;
            Bulk.WriteReport(outData4);
            while (outData4.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);
            
            var outData2 = Bulk.CreateReport();
            outData2.ReportId = 0x00;
            outData2.Data = temp;
            Bulk.WriteReport(outData2);
            while (outData2.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);
            Bulk.WriteReport(outData2);
            while (outData2.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);

            IsInitialised = true;
        }

        public void Close()
        {
            Control.CloseDevice();
            Bulk.CloseDevice();
            IsInitialised = false;
        }
    }
}
