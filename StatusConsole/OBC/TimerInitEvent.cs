using System;

namespace StatusConsole.OBC {
    public class TimerInitEvent : ObcEvent {
        public TimerInitEvent(byte[] data, int len) : base(data, len) {
            if (len == 18) {
                bool crcError = (data[2] == 0);
                byte statOld = data[3];
                byte statNew = data[4];
                ulong time = BitConverter.ToUInt64(data.AsSpan(5,8));
                MyText = $"Timer Init CRC:{crcError} Stat:{statOld} {statNew} Tim:{time}";
            } else {
                MyText = "Data Error - wrong len";
            }
        }
    }
}