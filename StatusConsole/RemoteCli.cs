using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ScreenLib;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {

    // This allows to access a remote socket on hostname:PORTNR 
    // If the remote machine is running linux there we could connect up any available tty device by running
    //    nc -l 9000 > /dev/ttyUSB0 < /dev/ttyUSB0
    // where 9000 is the port number and ttyUSB0 an example device.

    public class RemoteCli : ITtyService {
        private IConfigurationSection Config;
        Socket sock = null;
        private bool Continue;
        IConOutput Screen;
        Task Receiver;
        char NewLine = '\n';

        public void Initialize(IConfigurationSection cs) {
            Config = cs;
        }

        string ITtyService.GetInterfaceName() {
            return Config.Key;
        }

        IConfigurationSection ITtyService.GetScreenConfig() {
            return Config.GetSection("Screen");
        }
        
        Task IHostedService.StartAsync(CancellationToken cancellationToken) {
            try {
                String server = Config?.GetValue<String>("RemoteHost") ?? "localhost";
                int port = Config?.GetValue<int?>("RemotePort") ?? 9000;

                // Instanziere ein gültiges Socket Objekt mit den übergebenen Argumenten
                sock = ConnectSocket(server, port);
              
                //_serialPort.ReadTimeout = 10;
                Screen.WriteLine("Remote port " + server + ":" + port + " connected");
                Continue = true;


                // Receiver = Task.Run(() => Read());
                sock.BeginReceive(buffer, 0, 5, 0, new AsyncCallback(ReceiveCallback), sock);

            } catch (Exception ex) {
                Continue = false;
                //ConsoleColor csave = Screen.TextColor;
                //Screen.TextColor = ConsoleColor.Red;
                Screen.WriteLine("Error starting '" + Config?.GetValue<String>("ComName")??"<null>->COM1" + "' !", ConsoleColor.Red);
                Screen.WriteLine(ex.Message, ConsoleColor.Red);
                //Screen.TextColor = csave;
            }
            return Task.CompletedTask;
        }

        byte[] buffer = new byte[256];

        // Initialisiert die Socketverbindung und gibt diese zurück
        private static Socket ConnectSocket(string server, int port) {
            Socket sock = null;
            IPHostEntry hostEntry = null;

            hostEntry = Dns.GetHostEntry(server);

            // Nutze die Eigenschaft AddressFamily von IPEndPoint um Konflikte zwischen
            // IPv4 und IPv6zu vermeiden. Gehe dazu die Adressliste mit einer Schleife durch.
            foreach (IPAddress address in hostEntry.AddressList) {
                IPEndPoint ipo = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipo.AddressFamily,
                                               SocketType.Stream,
                                               ProtocolType.Tcp);

                tempSocket.Connect(ipo);

                if (tempSocket.Connected) {
                    sock = tempSocket;
                    break;
                } else {
                    continue;
                }
            }
            return sock;
        }


        private void ReceiveCallback(IAsyncResult ar) {
            try {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                Socket client = (Socket)(ar.AsyncState);

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0) {
                    // Encoding is ISO Layer 6 !!!??? 
                    // How to solve this here for chunked Buffer reads !?
                    //String received = Encoding.UTF8.GetString(buffer, 0, bytesRead);    

                    //for (int i=0; i<received.Length; i++) {
                    //    String singleChar = received.Substring(i, 1);
                    //    if (singleChar.Equals(NewLine)) {
                    //        Screen.WriteLine("");
                    //    } else {
                    //        Screen.Write(singleChar);
                    //    }
                    //}
                    Screen.WriteData(buffer, bytesRead);
                    // Get the rest of the data.  
                    client.BeginReceive(buffer, 0, 5, 0,
                        new AsyncCallback(ReceiveCallback), client);
                } else {
                    // Nothing left -> rrestart receive again....
                    // 
                    Screen.Write(".");
                    client.BeginReceive(buffer, 0, 5, 0,
                        new AsyncCallback(ReceiveCallback), client);
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        //public void Read() {
        //    int idx = 0;
        //    byte[] buffer = new byte[256];

        //    while (Continue) {
        //        int rec = sock.Receive(buffer, 1, SocketFlags.None);
        //        if (rec == 1) {
        //            char ch = (char)buffer[0];
        //            if (ch.ToString().Equals(NewLine)) {
        //                Screen.WriteLine("");
        //            } else {
        //                Screen.Write(ch.ToString());
        //            }
        //        }
        //    }

        //}




        async Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            // terminate the reader Task.
            Continue = false;
            if(Receiver != null) {
                //await Receiver;       // reader Task will be finished and execution "awaits it" and continues afterwards. (Without blocking any thread here)
                sock.Close();
                Screen.WriteLine("Socket closed.");
            }
        }

        void ITtyService.SendUart(string line) {
            if(Continue) {
                line += '\n';
                sock.Send(Encoding.UTF8.GetBytes(line));
            }
        }

        public bool IsConnected() {
            return Continue;
        }

        public void SetScreen(IConOutput scr) {
            Screen = scr;
        }
      
    }
}
