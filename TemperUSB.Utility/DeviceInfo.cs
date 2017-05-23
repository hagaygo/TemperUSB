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

        byte[] _tempData = { 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 };

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
                outData.Data = _tempData;
                Bulk.WriteReport(outData);
                System.Threading.Thread.Sleep(250);
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

            ClearData();

            InitStep1();
            InitStep2();
            InitStep3();

            ClearData();

            IsInitialised = true;
        }

        private void InitStep3()
        {
            byte[] data = { 0x01, 0x86, 0xff, 0x01, 0x00, 0x00, 0x00, 0x00 };
            var outData4 = Bulk.CreateReport();
            outData4.ReportId = 0x00;
            outData4.Data = data;
            Bulk.WriteReport(outData4);            
            while (outData4.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);
        }

        private void InitStep2()
        {
            byte[] data = { 0x01, 0x82, 0x77, 0x01, 0x00, 0x00, 0x00, 0x00 };

            var outData3 = Bulk.CreateReport();
            outData3.ReportId = 0x00;
            outData3.Data = data;
            Bulk.WriteReport(outData3);            
            while (outData3.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);
        }

        private void InitStep1()
        {
            byte[] data = { 0x01, 0x01 };

            var outData1 = Control.CreateReport();
            outData1.ReportId = 0x01;
            outData1.Data = data;
            Control.WriteReport(outData1);            
            while (outData1.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Control.ReadReport(ReadBogus);
        }

        private void ClearData()
        {
            var tmp = Bulk.CreateReport();
            tmp.ReportId = 0x00;
            tmp.Data = _tempData;
            Bulk.WriteReport(tmp);            
            while (tmp.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);
            Bulk.WriteReport(tmp);            
            while (tmp.ReadStatus != HidDeviceData.ReadStatus.Success) ;
            Bulk.ReadReport(ReadBogus);
        }

        public void Close()
        {
            Control.CloseDevice();
            Bulk.CloseDevice();
            IsInitialised = false;
        }
    }
}
