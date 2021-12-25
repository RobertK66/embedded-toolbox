using System;

namespace StatusConsole.OBC {
    public class ObcBlock0UpdatedEvent : ObcEvent {
        public ObcBlock0UpdatedEvent(byte[] data, int len) : base(data, len) {
            MyText = $"{severity}: M-{moduleNr} E-{eventNr} ";
            MyText += BitConverter.ToString(data, 2, len - 2);
        }
    }
}