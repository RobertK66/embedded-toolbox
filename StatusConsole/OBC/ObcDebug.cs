using System;
using StatusConsole.OBC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StatusConsole.OBC {

    enum L2Status {
        IDLE,
        ESCAPE,
        DATA
    }

    public class ObcDebug {
        L2Status Status = L2Status.IDLE;
        Byte[] rxData = new Byte[1000];
        int rxIdx = 0;
        private IOutputWrapper screen;
        private EventFactory eventFactory;


        public ObcDebug(IConfigurationSection debugConfig, IOutputWrapper screen) {
            this.screen = screen;
            eventFactory = new EventFactory(debugConfig);
        }

        internal void ProcessByte(int dataByte) {
            switch (Status) {
                case L2Status.IDLE:
                    if (dataByte == 0x7E) {
                        Status = L2Status.DATA;
                    }
                    break;
                case L2Status.ESCAPE:
                    rxData[rxIdx++] = (Byte)dataByte;
                    Status = L2Status.DATA;
                    break;
                case L2Status.DATA:
                    if (dataByte == 0x7E) {
                        ProcessFrame(rxData, rxIdx);
                        Status = L2Status.IDLE;
                        rxIdx = 0;
                    } else if (dataByte == 0x7D) {
                        Status = L2Status.ESCAPE;
                    } else {
                        rxData[rxIdx++] = (Byte)dataByte;   
                    }
                    break;
                default:
                    break;
            }
        }

        private void ProcessFrame(byte[] rxData, int len) {
            if (len > 2) {
                var moduleId = rxData[0];
                var eventId = (byte)(rxData[1] & 0x3F);
                var severity = (EventSeverity)((rxData[1] & 0xC0) >> 6);

                string eventString = eventFactory.GetAsString(moduleId, eventId, rxData, len);
                if (severity == EventSeverity.INFO) {
                    screen.WriteLine(eventString);
                } else {
                    ConsoleColor col = ConsoleColor.Blue;
                    if (severity == EventSeverity.WARNING) {
                        col = ConsoleColor.Green;
                    } else if (severity == EventSeverity.ERROR) {
                        col = ConsoleColor.Red;
                    }
                    screen.WriteLine(eventString, col);
                }


            } else {
                // No usefull data in this 'frame'.
            }


            //ObcEvent package = createOBCEvent(rxData, len);
            //if (package?.severity == EventSeverity.INFO) {
            //    screen.WriteLine(package.ToString());
            //} else {
            //    ConsoleColor col = ConsoleColor.Blue;
            //    if (package?.severity == EventSeverity.WARNING) {
            //        col = ConsoleColor.Green;
            //    } else if (package?.severity == EventSeverity.ERROR) {
            //        col = ConsoleColor.Red;
            //    }
            //    screen.WriteLine(package?.ToString(), col);
            //}
        }


        public ObcEvent createOBCEvent(Byte[] data, int len) {
            if (len >= 2) {
                var moduleNr = data[0];
                string moduleName = translateModuleToName(moduleNr);
                switch (moduleNr) {
                    case 0:
                        return createL7AppEvents(data, len);
                    case 1:
                        return createTimerEvents(data, len);
                    case 2:
                        return createSensorEvents(data, len);
                    case 3:
                        return createMemoryEvents(data, len);

                    case 0x80:
                        return createMramEvents(data, len);
                    case 0x81:
                        return createSdcardEvents(data, len);
                    default:
                        return new ObcEvent(data, len);
                }
            } else {
                return null;
            }
        }

        private ObcEvent createL7AppEvents(byte[] data, int len) {
            var eventNr = data[1] & 0x3F;
            switch (eventNr) {
                case 1:
                    return new SensorValuesEvent(data, len);
                case 2:
                    return new RawDataEvent(data, len);
                case 3:
                    return new StringEvent(data, len);

                default:
                    return new ObcEvent(data, len);
            }
        }


        private ObcEvent createTimerEvents(byte[] data, int len) {
            var eventNr = data[1] & 0x3F;
            switch (eventNr) {
                case 1:
                    return new TimerInitEvent(data, len);
                default:
                    return new ObcEvent(data, len);
            }
        }

        private ObcEvent createSensorEvents(byte[] data, int len) {
            var eventNr = data[1] & 0x3F;
            switch (eventNr) {
                case 1:
                    return new SensorValuesEvent(data, len);
                default:
                    return new ObcEvent(data, len);
            }
        }


        private ObcEvent createSdcardEvents(byte[] data, int len) {
            var eventNr = data[1] & 0x3F;
            switch (eventNr) {
                case 1:
                    return new SdcStatusEvent(data, len);
                case 2:
                    return new SdcUnimplementedEvent(data, len);
                default:
                    return new ObcEvent(data, len);
            }
        }

        private static ObcEvent createMramEvents(byte[] data, int len) {
            var eventNr = data[1] & 0x3F;
            switch (eventNr) {
                case 1:
                    return new MramJobErrorEvent(data, len);
                default:
                    return new ObcEvent(data, len);
            }
        }

        private static ObcEvent createMemoryEvents(byte[] data, int len) {
            var eventNr = data[1] & 0x3F;
            switch (eventNr) {
                case 1:
                    return new MemoryOperationalEvent(data, len);
                case 2:
                    return new ObcBlock0UpdatedEvent(data, len);

                default:
                    return new ObcEvent(data, len);
            }
        }


        private string translateModuleToName(byte moduleNr) {
            //config.GetSection("MODULES").
            return moduleNr.ToString();
        }



    }
}
