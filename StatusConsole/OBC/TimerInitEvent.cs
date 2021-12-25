using System;

namespace StatusConsole.OBC {
    public class TimerInitEvent : ObcEvent {
        public TimerInitEvent(byte[] data, int len) : base(data, len) {
            if (len == 18) {
                bool crcError = (data[14] != 0);
                UInt32 resetCount = BitConverter.ToUInt32(data.AsSpan(2, 4));
                UInt32 date = BitConverter.ToUInt32(data.AsSpan(6, 4));
                UInt32 time = BitConverter.ToUInt32(data.AsSpan(10, 4));
                String statusOld = BitConverter.ToString(data, 15, 1);
                String statusNew = BitConverter.ToString(data, 16, 1);
                int oscilatorDelay = (int)((byte)data[17]) * 2;

                if (crcError) {
                    MyText = $"Timer Init({oscilatorDelay} ms): CRC Error Stat -> {statusNew} Date: {date} Time: {time} reset#: {resetCount} ";
                } else {
                    MyText = $"Timer Init({oscilatorDelay} ms): Status x{statusOld}->x{statusNew} Date:{date} Time:{time} reset#:{resetCount} ";
                }
            } else {
                MyText = "Data Error - wrong len";
            }
        }
    }
}