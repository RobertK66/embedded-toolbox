namespace StatusConsole.OBC {
    internal class SdcStatusEvent : ObcEvent {
        public SdcStatusEvent(byte[] data, int len) : base(data, len) {
            if (len == 4) {
                MyText = $"{severity}: ";
                MyText += System.BitConverter.ToString(data, 2, len - 2);
                //byte statOld = data[3];
                //byte statNew = data[4];
                //MyText = $"{severity}: SDC Status:{statOld} Stat:{statNew}";
            } else {
                MyText = $"{severity}: SdcStatusEvent - Data Error - wrong len";
            }
        }
    }
}