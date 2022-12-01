using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StatusConsoleApi;

namespace StatusConsole {
    public class SerialPortConnector : ITtyService {

        private ILogger? Log;
        private IConfigurationSection Config;
        private ISerialProtocol protocol;
        private IOutputWrapper Screen;

        private SerialPort Port;
        private String? OnConnect;
        private bool Continue;

        public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger) {
            Config = cs;
            OnConnect = cs.GetValue<String>("OnConnect", "");
            Log = logger;

            // Here we Instanciate the configured protocol for this Serial Connector. If there is an protocol config section declared we pass it on to the 
            // ISerialProtocol constructor.
            string typeName = Config?.GetValue<String>("ProtClass") ?? "StatusConsole.DefaultCli";

            try {
                String configName = Config?.GetValue<String>("ProtConfig") ?? "dummyCfg";
                IConfigurationSection pluginConfig = rootConfig.GetSection(configName);
                protocol = PluginSystem.LoadPlugin<ISerialProtocol>(typeName, pluginConfig);
            } catch (Exception ex) {
                throw new ApplicationException("Protocol Class (" + typeName + ") not found for '" + cs.Path + "' ");
            }
        }

        //public void Read(SerialPort port) {
        //    // Avoid blocking the thread;
        //    // If nothing gets received, we sometimes have to check for the Continuation flag here.
        //    //if (port.ReadTimeout == -1) {
        //    //    port.ReadTimeout = 50000;
        //    //}

        //    // Set the event Handler to receive the bytes



        //    //port.DataReceived += (s, e) => {
        //    //    if (e.EventType == SerialData.Chars) {
        //    //        try {
        //    //            SerialPort sp = (SerialPort)s;
        //    //            while (sp.BytesToRead > 0) {
        //    //                var b = sp.ReadByte();
        //    //                if (b >= 0) {
        //    //                    Log?.LogTrace("Rx: {@mycharHex} '{@mychar}'", "0x" + b.ToString("X2"), (b == '\n') ? ' ' : b);
        //    //                    protocol.ProcessByte((byte)b);
        //    //                }
        //    //            };
        //    //        } catch(Exception ex) {
        //    //            Log?.LogError("Rx error using SerialPort in event handler. " + this.Config.Key, ex);
        //    //        }
        //    //    }
        //    //};

        //    port.ErrorReceived += (s, e) => {
        //        Log?.LogError(new EventId(2, "Rx"), "Serial Error: " + e.EventType);
        //        Continue = false;
        //    };

        //    port.PinChanged += (s, e) => {
        //        Log?.LogInformation(new EventId(3, "Rx"), "Pin Changed: " + e.EventType);
        //    };

        //    port.Disposed += (s, e) => {
        //        Log?.LogInformation(new EventId(4, "Rx"), "Serial Port disoposed ");
        //        Continue = false;
        //    };

        //    // and wait in empty loop until shutdown.
        //    while (Continue) {
        //        //Thread.Sleep(100);
        //        while (port.BytesToRead > 0) {
        //            var b = port.ReadByte();
        //            if (b >= 0) {
        //                Log?.LogTrace("Rx: {@mycharHex} '{@mychar}'", "0x" + b.ToString("X2"), (b == '\n') ? ' ' : b);
        //                protocol.ProcessByte((byte)b);
        //            }
        //        };
        //        if (!port.IsOpen) {
        //            Log?.LogInformation(new EventId(4, "Rx"), "Serial Port was closed.");
        //            Continue = false;
        //        }
        //    }
        //    Log?.LogInformation(new EventId(4, "Rx"), "Terminating reader thread. Try to reconnect with <ESC>.");
        //}


        public void SetScreen(IOutputWrapper scr) {
            Screen = scr;
            protocol.SetScreen(scr, Log, this);
        }

        public void ProcessCommand(String s) {
            protocol.ProcessUserInput(s);
            //Port.Write(Encoding.ASCII.GetBytes(s + Port.NewLine), 0, (s + Port.NewLine).Length);
        }

        public void SendUart(byte[] bytes, int len) {
            if (Continue) {
                try {
                    Port?.Write(bytes, 0, len);
                    Log?.LogDebug(new EventId(1, "Tx"), "{@line}", bytes);
                } catch (Exception ex) {
                    Log?.LogError(new EventId(1, "Tx"), "Send Error: " + ex.Message);
                }
            }
        }

        public string GetViewName() {
            return Config.GetValue<String>("Screen", "");
        }

        public string GetInterfaceName() {
            return Config.Key;
        }

        public bool IsConnected() {
            return Continue;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            try {
                Port = new SerialPort();
                Port.PortName = Config?.GetValue<String>("ComName") ?? "COM1";
                Port.BaudRate = Config?.GetValue<int?>("Baud") ?? 9600;
                Port.Parity = Parity.None;
                Port.DataBits = 8;
                Port.StopBits = StopBits.One;
                Port.Handshake = Handshake.None;
                Port.Encoding = Encoding.ASCII;
                Port.Open();
                Port.DtrEnable = true;
                Port.NewLine = Config?.GetValue<String>("NewLine") ?? "\r";
                //_serialPort.ReadTimeout = 10;
                Log?.LogInformation(new EventId(0), "Uart Uart {@portname} connected. Using {@newline} as newline char.", Port.PortName, "0x" + Convert.ToByte(Port.NewLine[0]).ToString("X2"));


                if (Port.ReadTimeout == -1) {
                    Port.ReadTimeout = 50000;
                }

                Port.DataReceived += Port_DataReceived;
                Continue = true;

                Port.ErrorReceived += (s, e) => {
                    Log?.LogError(new EventId(2, "Rx"), "Serial Error: " + e.EventType);
                    Continue = false;
                };

                Port.PinChanged += (s, e) => {
                    Log?.LogInformation(new EventId(3, "Rx"), "Pin Changed: " + e.EventType);
                };

                Port.Disposed += (s, e) => {
                    Log?.LogInformation(new EventId(4, "Rx"), "Serial Port disoposed ");
                    Continue = false;
                };

                //Log?.LogInformation(new EventId(0), "Sending OnConnect command {@cmd}", OnConnect);
                SendUart(Encoding.ASCII.GetBytes((OnConnect ?? "") + Port.NewLine), Encoding.ASCII.GetBytes(OnConnect ?? "").Length + 1);
            } catch (Exception ex) {
                Continue = false;
                Log?.LogError(new EventId(0), ex, "Error starting '" + Config?.GetValue<String>("ComName") ?? "<null>->COM1" + "' !");
            }
            return Task.CompletedTask;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            if (e.EventType == SerialData.Chars) {
                try {
                    SerialPort sp = (SerialPort)sender;
                    while (sp.BytesToRead > 0) {
                        var b = sp.ReadByte();
                        if (b >= 0) {
                            Log?.LogTrace("Rx: {@mycharHex} '{@mychar}'", "0x" + b.ToString("X2"), (b == '\n') ? ' ' : b);
                            protocol.ProcessByte((byte)b);
                        }
                    };
                } catch (Exception ex) {
                    Log?.LogError("Rx error using SerialPort in event handler. " + this.Config.Key, ex);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Continue = false;
            //if (Receiver != null) {
            //    Receiver.Wait();       // reader Task will be finished and execution "awaits it" and continues afterwards. (Without blocking any thread here)
            Port.Close();
            Log?.LogInformation(new EventId(0), "Uart {@portname} closed.", Port.PortName);
            return Task.CompletedTask;
        }
    }
}
