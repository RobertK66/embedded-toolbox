using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimbPlugins.OBC {
    public enum EventSeverity {
        INFO = 0,
        WARNING = 1,
        ERROR = 2,
        FATAL = 3
    }

    public class ObcEvent {
        protected int  moduleNr;
        public EventSeverity severity;
        protected int  eventNr;
        protected int dataLen;

        protected string ModuleName = String.Empty;
        protected string EventName = String.Empty;
        protected string MyText = String.Empty;

        public ObcEvent(Byte[] data, int len) {
            this.moduleNr = data[0];
            this.severity = (EventSeverity)((data[1] & 0xC0) >> 6);
            this.eventNr = data[1] & 0x3F;
            this.dataLen = len;
            this.MyText = BitConverter.ToString(data, 2, len - 2);
        }

        public ObcEvent(Byte[] data, int len, String ModuleName, String EventName) : this(data, len) { 
            this.ModuleName = ModuleName;
            this.EventName = EventName;
        }

        //public static ObcEvent createOBCEvent(Byte[] data, int len) {
        //    if (len >= 2) {
        //        var moduleNr = data[0];
        //        switch (moduleNr) {
        //            case 0:
        //                return createL7AppEvents(data, len);
        //            case 1:
        //                return createTimerEvents(data, len);
        //            case 0x80:
        //                return createMramEvents(data, len);
        //            case 0x81:
        //                return createSdcardEvents(data, len);
        //            default:
        //                return new ObcEvent(data, len);
        //        }
        //    } else {
        //        return null;
        //    }
        //}

        //private static ObcEvent createL7AppEvents(byte[] data, int len) {
        //    var eventNr = data[1] & 0x3F;
        //    switch (eventNr) {
        //        case 1:
        //            return new SensorValuesEvent(data, len);
        //        case 2:
        //            return new RawDataEvent(data, len);
        //        case 3:
        //            return new StringEvent(data, len);

        //        default:
        //            return new ObcEvent(data, len);
        //    }
        //}


        //private static ObcEvent createTimerEvents(byte[] data, int len) {
        //    var eventNr = data[1] & 0x3F;
        //    switch (eventNr) {
        //        case 1:
        //            return new TimerInitEvent(data, len);
        //        default:
        //            return new ObcEvent(data, len);
        //    }
        //}

        //private static ObcEvent createSdcardEvents(byte[] data, int len) {
        //    var eventNr = data[1] & 0x3F;
        //    switch (eventNr) {
        //        case 1:
        //            return new SdcStatusEvent(data, len);
        //        case 2:
        //            return new SdcUnimplementedEvent(data, len);
        //        default:
        //            return new ObcEvent(data, len);
        //    }
        //}

        //private static ObcEvent createMramEvents(byte[] data, int len) {
        //    var eventNr = data[1] & 0x3F;
        //    switch (eventNr) {
        //        case 1:
        //            return new MramJobErrorEvent(data, len);
        //        default:
        //            return new ObcEvent(data, len);
        //    }
        //}

        public override string ToString() {
            if (String.IsNullOrEmpty(MyText)) {
                return $"{severity}: M-{moduleNr} E-{eventNr} len:{dataLen} {MyText}";
            } else {
                return MyText;
            }
        }
    }

   
}
