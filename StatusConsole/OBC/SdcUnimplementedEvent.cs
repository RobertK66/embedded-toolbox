namespace StatusConsole.OBC {
    internal class SdcUnimplementedEvent : ObcEvent {
        public SdcUnimplementedEvent(byte[] data, int len) : base(data, len) {
            if (len == 6) {
                byte statOld = data[3];
                byte statNew = data[4];
                MyText = $"{severity}: SDC Unimplemented type! {statOld} Stat:{statNew}";
            } else {
                MyText = $"{severity}: Data Error - wrong len";
            }
        }
    }
}