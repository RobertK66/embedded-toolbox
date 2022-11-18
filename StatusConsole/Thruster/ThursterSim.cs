using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatusConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Thruster {

    public class ThrusterSim :ISerialProtocol {

        enum Msgtype {
            OK = 1, 
            ERROR,
            READ,
            WRITE,
            DATA,
            RESET,
            UPDATE,
            CONFIG
        }

        private IConfigurationSection thrConfig;
        private IOutputWrapper screen;
        private ILogger Log;
        private ITtyService tty;
        private CRC8Calc crcCalculator;

        private byte thrusterAdress = 0x01;             // TODO: config....
        private int rxIdx  = 0;
        private bool ignoreMessage = false;
        private Msgtype currentMsg;
        private byte currentCRC;
        private UInt16 currentPayloadLen;
        private byte[] currentPayload = new byte[1000];

        private byte[] currentRegisterValues = new byte[0xFF];


        public ThrusterSim(IConfigurationSection thrConfig, IOutputWrapper screen, ILogger log, ITtyService tty) {
            this.thrConfig = thrConfig;
            this.screen = screen;
            this.Log = log;
            this.tty = tty;
            this.crcCalculator = new CRC8Calc(CRC8_POLY.CRC8_CCITT);
            rxIdx = 0;
            for (int i = 0; i < 0xFF; i++) {
                currentRegisterValues[i] = (byte)i;
            }
        }

        public ThrusterSim() {
            this.crcCalculator = new CRC8Calc(CRC8_POLY.CRC8_CCITT);
            rxIdx = 0;
            for (int i = 0; i < 0xFF; i++) {
                currentRegisterValues[i] = (byte)i;
            }
        }


        public void ProcessByte(byte b) {
            switch (rxIdx) {
                case 0:   // sender adress
                    if (b == 0) {
                        // Sender always has to be 0
                        rxIdx++;
                    } else {
                        Log.LogError("received msg does not start with Adr 0x00!");
                        rxIdx = 0; //-1; // Error !?ComState.Error;
                    }
                    break;
                case 1:  // receiver adress 
                    rxIdx++;
                    if ((b == 0xff) || (b == thrusterAdress)) {
                        ignoreMessage = false;
                    } else {
                        // The message is adressed to another slave.
                        ignoreMessage = true;
                    }
                    break;
                case 2:  // messageType
                    currentMsg = (Msgtype)b;
                    rxIdx++;
                    break;
                case 3:  // CRC8 
                    currentCRC = (byte)b;
                    rxIdx++;
                    break;
                case 4: // Payloadlen LSB
                    currentPayloadLen = (ushort)b;
                    rxIdx++;
                    break;
                case 5: // Payloadlen MSB
                    currentPayloadLen |= (ushort)(b << 8);
                    if (currentPayloadLen == 0) {
                        // message has no payload -> reception finished. wait for next one.
                        processMessage(currentMsg, 0);     // RESET/OK or ERROR only
                        rxIdx = 0;
                    }
                    rxIdx++;
                    break;
                default:
                    currentPayload[(rxIdx - 6)] = (byte)b;
                    rxIdx++;
                    if (rxIdx >= currentPayloadLen + 6) {
                        // Payload finished. wait for next message
                        if (!ignoreMessage) {
                            processMessage(currentMsg, currentPayloadLen, currentPayload);
                        }
                        rxIdx = 0;
                    }
                    break;
            }
        }

        private void processMessage(Msgtype currentMsg, ushort currentPayloadLen, byte[] currentPayload = null) {
            // TODO CRC Check
            screen.Write("Msg: " + currentMsg.ToString());
            switch (currentMsg) {
                case Msgtype.READ:
                    int offset = currentPayload[0];
                    int lenToread = currentPayload[1];
                    screen.WriteLine(String.Format(" - baseAdr: {0}, len: {1}", offset, lenToread));
                    SendReadAnswer(offset, lenToread);
                    break;
                case Msgtype.WRITE:
                    int offset2 = currentPayload[0];
                    int data0 = currentPayload[1];
                    screen.WriteLine(String.Format(" - baseAdr: {0}, len: {1}, data[0]: {2}", offset2, currentPayloadLen-1, data0));
                    WriteDataToRegisters(currentPayload, currentPayloadLen);
                    SendOkAnswer();
                    break;
                default:
                    screen.WriteLine(String.Format("Not implemented yet"));
                    break;

            }
                

        }

        private void WriteDataToRegisters(byte[] currentPayload, ushort len) {
            int offset = currentPayload[0];
                //for (int idx = 1; idx < len; idx++) {
                //    currentRegisterValues[offset + idx - 1] = currentPayload[idx];
                //}
        }

        private void SendOkAnswer() {
            byte[] response = new byte[6];
            response[0] = thrusterAdress;
            response[1] = 0x00;     // Master (OBC) address 
            response[2] = (byte)Msgtype.OK;
            response[3] = 0x00;     //CRC8
            response[4] = 0x00;     
            response[5] = 0x00;
            response[3] = crcCalculator.Checksum(response.ToArray());
        }

        private void SendReadAnswer(int offset, int lenToRead) {
            byte[] response = new byte[6+lenToRead];
            response[0] = thrusterAdress;
            response[1] = 0x00;     // Master (OBC) address 
            response[2] = (byte)Msgtype.DATA;
            response[3] = 0x00;     //CRC8
            response[4] = (byte)(lenToRead & 0x00FF);       //LSB
            response[5] = (byte)((lenToRead>>8) & 0x00FF);  //MSB
            for (int i = 0; i < lenToRead; i++) {
                response[6 + i] = currentRegisterValues[offset + i];
            }
            // TODO: calculate CRC
            byte crc = crcCalculator.Checksum(response.Take(lenToRead+6).ToArray());
            response[3] = crc;
            screen.WriteLine("Send " + lenToRead + " bytes as Register Values.");
            for (int i = 0; i < lenToRead+6; i++) {
                screen.Write(" " + response[i].ToString("x2"));
            }
            screen.WriteLine("");
            tty.SendUart(response, 6 + lenToRead);



        }

        public void SetScreen(IConfigurationSection debugConfig, IOutputWrapper screen, ILogger log, ITtyService tts) {
            thrConfig = debugConfig;
            this.screen = screen;
            Log = log;
            tty = tts; 
        }

        public void ProcessCommand(string cmd) {
            //throw new NotImplementedException();
        }
    }       
}
