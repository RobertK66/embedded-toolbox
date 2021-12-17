using ScreenLib;
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
        private IConOutput screen;
        private IConfigurationSection config;


        public ObcDebug(IConfigurationSection debugConfig, IConOutput screen) {
            this.screen = screen;
            config = debugConfig;
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
                        ProcessData(rxData, rxIdx);
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

        private void ProcessData(byte[] rxData, int len) {
            //throw new NotImplementedException();
            var package = createOBCEvent(rxData, len);
            screen.WriteLine( package.ToString() );
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


        private string translateModuleToName(byte moduleNr) {
            //config.GetSection("MODULES").
            return moduleNr.ToString();
        }



    }
}
