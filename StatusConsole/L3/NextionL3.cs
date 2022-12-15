using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.L3 {

    public enum NextionEventId {
        Ok = 0x01,
        InvalidInstruction = 0x00,
        InvalisComponentId = 0x02,
        InvalidPageId,
        InvalidPictureId,
        InvalidFontId,
        InvalidFileOperation,
        InvalidCrc = 0x09,
        InvalidBaudRate = 0x11,
        InvalidWaveformId,
        InvalidVariable = 0x1A,
        InvalidVarOperation,
        FailedAssignment,
        FailedEEPROMOp,
        InvalidParamCount,
        FailedIoOp,
        InvalidEscapeChar,
        VariableNameTooLong = 0x23,
        BufferOverflow,

        TouchEvent = 0x65,
        CurrentPage,
        TouchCoordinateAwake,
        TouchCoordinateSleep,
        StringData = 0x70,
        NumericData, 

        SleepEntered = 0x86,
        WakeUp,
        PowerUpReady,
        MicroSdUpgradeStart,

        TransDataFinished = 0xFD,
        TransDataReady
    }

    public enum NextionRxEvent {
        Ok,
        Error,
        Startup,
        TouchPressed,
        TouchReleased,
        PageSwitched
    }

    public class NextionEventArgs {
        public NextionEventArgs() {
        }
        
        public NextionRxEvent RxEvent { get; set; }
        public String Message { get; set; }

        public int? PageId { get; set; }
        public int? ComponentId { get; set; }

    }


    public class NextionL3 : IL3Protocol {

        private enum RxStat { rx_idle, rx_f1, rx_f2 };

        private RxStat Status = RxStat.rx_idle;

        private readonly byte[] Buffer = new byte[500];
        private int bytesInBuffer = 0;


        public event Action<object, L3PackageRxEventArgs> L3PackageReceived;


        public event Action<object, NextionEventArgs> NextionEventReceived;


        public void ProcessReceivedData(byte[] bytes, int bytesReceivedL2) {
            int idx = 0;
            while (bytesReceivedL2-- > 0) {
                byte b = bytes[idx++];
                Buffer[bytesInBuffer++] = b;

                if (b == 0xff) {
                    if (Status == RxStat.rx_idle) {
                        Status = RxStat.rx_f1;
                    } else if (Status == RxStat.rx_f1) {
                        Status = RxStat.rx_f2;
                    } else if (Status == RxStat.rx_f2) {
                        Nextion_L3PackageReceived(Buffer, bytesInBuffer - 3);
                        //L3PackageReceived?.Invoke(this, new L3PackageRxEventArgs() { l3bytes = bytesInBuffer - 3, l3Data = Buffer });
                        Status = RxStat.rx_idle;
                        bytesInBuffer = 0;
                    }
                } else {
                    Status = RxStat.rx_idle;
                }

            }
        }


        private void Nextion_L3PackageReceived(byte[] buffer, int byteCnt) {
            var evArgs = new NextionEventArgs();

            if (byteCnt > 0) {  
                NextionEventId id = (NextionEventId)buffer[0];

                switch (id) {
                    case NextionEventId.Ok:
                        evArgs.RxEvent = NextionRxEvent.Ok;
                        evArgs.Message = "OK!";
                        break;

                    case NextionEventId.InvalidInstruction: 
                        // Special case Startup (0x00 0x00 0x00 0xFF 0xFF 0xFF)
                        if (byteCnt == 3) {
                            evArgs.RxEvent = NextionRxEvent.Startup;
                            evArgs.Message = "Startup!";
                            break;
                        } 
                        goto case default;

                    case NextionEventId.TouchEvent:
                        if (byteCnt == 4) {
                            evArgs.RxEvent = (buffer[3] == 0x01)?NextionRxEvent.TouchPressed: NextionRxEvent.TouchReleased;
                            evArgs.PageId = buffer[1];
                            evArgs.ComponentId = buffer[2];
                            evArgs.Message = $"Page {evArgs.PageId}, Comp {evArgs.ComponentId}: " + evArgs.RxEvent.ToString();
                            break;
                        }
                        goto case default;


                    case NextionEventId.CurrentPage:
                        if (byteCnt == 2) {
                            evArgs.RxEvent = NextionRxEvent.PageSwitched;
                            evArgs.PageId = buffer[1];
                            evArgs.Message = $"Page {evArgs.PageId}";
                            break;
                        }
                        goto case default;


                    default:  
                        evArgs.RxEvent = NextionRxEvent.Error;
                        evArgs.Message = id.ToString();
                        break;
                }

                this.NextionEventReceived?.Invoke(this, evArgs);    
            } 
            
        }



        public void ConvertAndSendL3CommandLine(String commandLine, Action<byte[], int> SendL2Data) {
            var data = Encoding.ASCII.GetBytes(commandLine);
            data = data.Concat(new byte[] { 0xFF, 0xFF, 0xFF}).ToArray();
            SendL2Data(data, data.Length);
        }

    }
}
