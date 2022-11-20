namespace ClimbPlugins.OBC {
    internal class MramJobErrorEvent : ObcEvent {
        public MramJobErrorEvent(byte[] data, int len) : base(data, len) {
        }
    }
}