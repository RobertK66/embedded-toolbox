using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatusConsoleApi;

namespace ClimbPlugins.OBC
{

    enum L2Status {
        IDLE,
        ESCAPE,
        DATA
    }

    public class ObcDebug :ISerialProtocol{
        L2Status Status = L2Status.IDLE;
        Byte[] rxData = new Byte[1000];
        int rxIdx = 0;
        private string cmdTerminator = "\n";
        private EventFactory eventFactory;
        // Gets initialized by factory after constructor:
        private ITtyService? tty;
        private IOutputWrapper? screen;
        private ILogger? Log;

        public ObcDebug(IConfigurationSection debugConfig, IOutputWrapper screen, ILogger log) :this(debugConfig) {
            this.screen = screen;
            Log = log;
        }

        public ObcDebug(IConfigurationSection debugConfig) {
            eventFactory = new EventFactory(debugConfig);
        }

        public void ProcessByte(byte dataByte) {
            switch (Status) {
                case L2Status.IDLE:
                    if (dataByte == 0x7E) {
                        Status = L2Status.DATA;
                        rxIdx = 0;
                        Log?.LogTrace("Rx: Frame Start");
                    }
                    break;
                case L2Status.ESCAPE:
                    rxData[rxIdx++] = (Byte)dataByte;
                    Log?.LogTrace("Rx(e): {@mycharHex}", "0x" + Convert.ToByte(dataByte).ToString("X2"));
                    Status = L2Status.DATA;
                    break;
                case L2Status.DATA:
                    if (dataByte == 0x7E) {
                        if (rxIdx == 0) {
                            // 2 Frame marker without data in between -> This is very likely to be a frame start now!
                            Status = L2Status.DATA;
                            rxIdx = 0;
                            Log?.LogTrace("Rx: Dual 0x7E -> Asume new Frame Start");
                        } else {
                            Log?.LogTrace("Rx: Frame End");
                            ProcessFrame(rxData, rxIdx);
                            Status = L2Status.IDLE;
                            rxIdx = 0;
                        }
                    } else if (dataByte == 0x7D) {
                        Log?.LogTrace("Rx: Esc");
                        Status = L2Status.ESCAPE;
                    } else {
                        rxData[rxIdx++] = (Byte)dataByte;
                        Log?.LogTrace("Rx(e): {@mycharHex}", "0x" + Convert.ToByte(dataByte).ToString("X2"));
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

                Log?.LogDebug("Rx: Frame mod:{@moduleId}, ev: {@eventId}, [{@rxData}],len: {@len}", moduleId, eventId, (rxData.AsSpan(0, len).ToArray()), len);
                string eventString = eventFactory.GetAsString(moduleId, eventId, rxData, len);
                if (severity == EventSeverity.INFO) {
                    screen?.WriteLine(eventString);
                } else {
                    ConsoleColor col = ConsoleColor.Blue;
                    if (severity == EventSeverity.WARNING) {
                        col = ConsoleColor.Green;
                    } else if (severity == EventSeverity.ERROR) {
                        col = ConsoleColor.Red;
                    }
                    screen?.WriteLine(eventString, col);
                }
            } else {
                Log?.LogError("Fragmented Frame with {@len}.", len);
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


        public ObcEvent? createOBCEvent(Byte[] data, int len) {
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

        public void SetScreen(IOutputWrapper scr, ILogger log, ITtyService tty, string cmdTerminate) {
            this.screen = scr;
            this.Log = log;
            this.tty = tty;
            this.cmdTerminator = cmdTerminate;
        }

        public void ProcessUserInput(string cmd) {
            tty?.SendUart(Encoding.ASCII.GetBytes(cmd + cmdTerminator), cmd.Length + cmdTerminator.Length);
            //Port.Write(Encoding.ASCII.GetBytes(cmd + Port.NewLine), 0, (s + Port.NewLine).Length);
        }
    }
}
