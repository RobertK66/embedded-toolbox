using System;

namespace StatusConsole.OBC {
    internal class RawDataEvent : ObcEvent {
        public RawDataEvent(byte[] data, int len) : base(data, len) {
            if (len > 2) {
                MyText = $"{severity}: ";
                MyText += BitConverter.ToString(data, 2, len-2);
            } else {
                MyText = $"{severity}: Data Error - Empty";
            }
        }
    }
}