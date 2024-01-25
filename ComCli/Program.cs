using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatusConsoleApi;
using System.IO.Ports;
using System.Management;
using System.Text;

namespace ComCli {
    internal class Program : IOutputWrapper {
        private string port;
        private int baud = 9600;
        private string newLine = "\n";
        private ISerialProtocol? protocol;

        private SerialPort Port;

        public Program(string port) {
            this.port = port;
        }

        static void Main(string[] args) {
            String port = "";
            String prot = "";
            String typeName = "";
            if (args.Length > 0 ) {
                port = args[0];
            }
            if ( args.Length > 1 ) {
                prot = args[1];
                if (prot=="OBCD") {
                    typeName = "ClimbPlugins.OBC.ObcDebug";
                }
            }

            Console.WriteLine($"Hello {prot} on {port}!");
            //var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            //IConfigurationSection pluginConfig = config.GetSection(prot);
            //var protocol = PluginSystem.LoadPlugin<ISerialProtocol>(typeName, pluginConfig);
            //Console.WriteLine($"Loaded: {protocol?.GetType().FullName ?? "<null>"}");


            var prog = new Program(port);
            prog.RunIt();
 
        }

        public void StartListening() {

            try {
                Port = new SerialPort();
                Port.PortName = port;
                Port.BaudRate = baud; // Config?.GetValue<int?>("Baud") ?? 9600;
                Port.Parity = Parity.None;
                Port.DataBits = 8;
                Port.StopBits = StopBits.One;
                Port.Handshake = Handshake.None;
                Port.Encoding = Encoding.ASCII;
                Port.DtrEnable = true;
                Port.NewLine = newLine; //Config?.GetValue<String>("NewLine") ?? "\r";

                //Log?.LogInformation(new EventId(0), "Uart Uart {@portname} connected. Using {@newline} as newline char.", Port.PortName, "0x" + Convert.ToByte(Port.NewLine[0]).ToString("X2"));

                //Port.DataReceived += Port_DataReceived;
                Port.ErrorReceived += Port_ErrorReceived;
                Port.PinChanged += Port_PinChanged;
                Port.Disposed += Port_Disposed;
                Port.Open();

                byte[] buffer = new byte[1000];
                string cmd = "";

                var readerResult = Port.BaseStream.ReadAsync(buffer);

                while (true) {
                    if (readerResult.IsCompleted) {
                        byte[] received = new byte[readerResult.Result];
                        //Console.Write(" " + readerResult.Result);
                        Buffer.BlockCopy(buffer, 0, received, 0, readerResult.Result);
                        raiseAppSerialDataEvent(received);
                        // Restart reading.
                        readerResult = Port.BaseStream.ReadAsync(buffer);
                    }

                    if (Console.KeyAvailable) {
                        var k = Console.ReadKey();
                        if (k.Key == ConsoleKey.Enter) {
                            Port.Write(Encoding.ASCII.GetBytes(cmd + newLine), 0, cmd.Length + newLine.Length);
                            cmd = "";
                            Console.WriteLine();
                        } else {
                            cmd += k.KeyChar;
                        }
                    }
                }

               

                //Action? kickoffRead = null;
                //kickoffRead = delegate {

                //    //Port.BaseStream.ReadAsync(buffer);

                //    Port.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar) {
                //        try {
                //            int actualLength = Port.BaseStream.EndRead(ar);
                //            byte[] received = new byte[actualLength];
                //            Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                //            raiseAppSerialDataEvent(received);
                //        } catch (IOException exc) {
                //            handleAppSerialError(exc);
                //        }
                //        if (kickoffRead != null) {
                //            kickoffRead();
                //        }
                //    }, null);
                //};
                //kickoffRead();

          


                //Log?.LogInformation(new EventId(0), "Sending OnConnect command {@cmd}", OnConnect);
                // SendUart(Encoding.ASCII.GetBytes((OnConnect ?? "") + Port.NewLine), Encoding.ASCII.GetBytes(OnConnect ?? "").Length + 1);
            } catch (Exception ex) {
                //Continue = false;
                // Log?.LogError(new EventId(0), ex, "Error starting '" + Config?.GetValue<String>("ComName") ?? "<null>->COM1" + "' !");
            }
            //return Task.CompletedTask;
        }


        private void Port_Disposed(object? sender, EventArgs e) {
            //Log?.LogInformation(new EventId(4, "Rx"), "Serial Port disoposed ");
        }

        private void Port_PinChanged(object sender, SerialPinChangedEventArgs e) {
            //Log?.LogInformation(new EventId(3, "Rx"), "Pin Changed: " + e.EventType);
        }

        private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
            //Log?.LogError(new EventId(2, "Rx"), "Serial Error: " + e.EventType);
        }


        private void raiseAppSerialDataEvent(byte[] data) {
            for (int i=0; i<data.Length;i++) {
                protocol?.ProcessByte(data[i]);
            }
        }

        void handleAppSerialError(Exception exc) {
            Console.WriteLine(exc.ToString());
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            if (e.EventType == SerialData.Chars) {
                try {
                    
                    SerialPort sp = (SerialPort)sender;
                    int b;
                    while (sp.BytesToRead > 0) {
                    //do { 
                        b = sp.BaseStream.ReadByte();
                        if (b >= 0) {
                            //Log?.LogTrace("Rx: {@mycharHex} '{@mychar}'", "0x" + b.ToString("X2"), (b == '\n') ? ' ' : b);
                            protocol?.ProcessByte((byte)b);
                        }
                    } //while (b>=0);
                } catch (Exception ex) {
                    //Log?.LogError("Rx error using SerialPort in event handler. " + this.Config.Key, ex);
                }
                Console.Write(".");
            } else if (e.EventType == SerialData.Eof) {
                //Log?.LogInformation("Port received EOF." + this.Config.Key);
                Console.WriteLine("-eof-");
            } else {
                //Log?.LogInformation("Port received ???." + this.Config.Key);
                Console.WriteLine("-??-");
            }
            
        }

        public void StopListening() {
            if (Port != null) {
                Port.DtrEnable = false;
                Port.DataReceived -= Port_DataReceived;
                Port.ErrorReceived -= Port_ErrorReceived;
                Port.PinChanged -= Port_PinChanged;
                Port.Disposed -= Port_Disposed;
                Port.Close();
            }
            //Log?.LogInformation(new EventId(0), "Uart {@portname} closed.", Port?.PortName ?? "<null>");
            //return Task.CompletedTask;
        }

        public void RunIt() {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            IConfigurationSection pluginConfig = config.GetSection("OBCD");
            protocol = PluginSystem.LoadPlugin<ISerialProtocol>("ClimbPlugins.OBC.ObcDebug", pluginConfig, this);
            Console.WriteLine($"Loaded: {protocol?.GetType().FullName ?? "<null>"}");

            
           
            

#pragma warning disable CA1416 // Validate platform compatibility
            var query = new WqlEventQuery() {
                EventClassName = "__InstanceOperationEvent",
                WithinInterval = new TimeSpan(0, 0, 3),
                //Condition = @"TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.ClassGuid = '{4d36e978-e325-11ce-bfc1-08002be10318}'"  
                Condition = @"TargetInstance ISA 'Win32_SerialPort'" // or TargetInstance ISA 'Win32_USBHub'"
            };

            var scope = new ManagementScope("root\\CIMV2");
            using (var moWatcher = new ManagementEventWatcher(scope, query)) {
                moWatcher.Options.Timeout = ManagementOptions.InfiniteTimeout;
                moWatcher.EventArrived += new EventArrivedEventHandler(DeviceChangedEvent);
                moWatcher.Start();
            }

            if (SerialPort.GetPortNames().Contains(port)) {
                // Lets start the com 
                StartListening();
            }

            var txt = Console.ReadLine();

        }


        private void DeviceChangedEvent(object sender, EventArrivedEventArgs e) {
            using (var moBase = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value) {
                //foreach (var m in moBase?.Properties) {
                //    Console.WriteLine(m?.Name + " = " + m?.Value);
                //}
                string devicePNPId = moBase?.Properties["PNPDeviceID"]?.Value.ToString();
                string deviceDescription = moBase?.Properties["Caption"]?.Value.ToString();
                string eventMessage = $"{devicePNPId}: {deviceDescription}  ";

                switch (e.NewEvent.ClassPath.ClassName) {
                    case "__InstanceDeletionEvent":
                        eventMessage += " removed";
                        break;
                    case "__InstanceCreationEvent":
                        eventMessage += "inserted";
                        break;
                    case "__InstanceModificationEvent":
                    default:
                        foreach (var m in moBase?.Properties) {
                            Console.WriteLine(m?.Name + " = " + m?.Value);
                            
                        }
                        eventMessage += $" {e.NewEvent.ClassPath.ClassName} " + e.Context.ToString();
                        break;
                }
                //BeginInvoke(new Action(() => UpdateUI(eventMessage)));
                Console.WriteLine(eventMessage);
            }
        }

        public void WriteLine(string v) {
            Console.WriteLine(v);
        }

        public void WriteLine(string v, ConsoleColor col) {
            Console.WriteLine(col.ToString() + v); 
        }

        public void WriteData(byte[] buffer, int bytesRead) {
            Console.Write(Encoding.ASCII.GetString(buffer, 0, bytesRead));
        }

        public void Write(string v) {
            Console.Write(v);
        }

#pragma warning restore CA1416 // Validate platform compatibility

    }
}