using System;

namespace StatusConsole.OBC {
    public class SensorValuesEvent : ObcEvent {
        public SensorValuesEvent(byte[] data, int len) : base(data, len) {
            if (len == 18) {
                float VPP_Volt = BitConverter.ToSingle(data.AsSpan(2, 4));
                float Cur_mA = BitConverter.ToSingle(data.AsSpan(6, 4)) * 1000;
                float CurSp_mA = BitConverter.ToSingle(data.AsSpan(10, 4)) * 1000;
                float Temp = BitConverter.ToSingle(data.AsSpan(14, 4));
                MyText = $"Vpp:{VPP_Volt.ToString("0.00")}V/{Cur_mA.ToString("0")}mA SidePanels:{CurSp_mA.ToString("0")}mA Temp:{Temp.ToString("0.0")}°C";
            } else {
                MyText = "Data Error - wrong len";
            }
        }
    }
}