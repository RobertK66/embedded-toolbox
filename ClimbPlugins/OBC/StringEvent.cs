using System.Text;

namespace ClimbPlugins.OBC {
    internal class StringEvent : ObcEvent {
        public StringEvent(byte[] data, int len) : base(data, len) {
            if (len > 2) {
                MyText = $"{severity}: '";
                MyText += Encoding.ASCII.GetString(data, 2, len-2) + "'";
            } else {
                MyText = $"{severity}: Data Error - Empty";
            }
        }
    }
}