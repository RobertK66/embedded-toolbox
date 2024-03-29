﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StatusConsole.L3;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using StatusConsoleApi;

namespace StatusConsole {
    //
    // This allows to access a remote socket on hostname:PORTNR 
    // If the remote machine is running linux there we could connect up any available tty device by running
    //    nc -l 9000 > /dev/ttyUSB0 < /dev/ttyUSB0
    // where 9000 is the port number and ttyUSB0 an example device.
    // The version
    //    while :; do nc -l 9801 > /dev/ttyUSB1 < /dev/ttyUSB1 ; done
    // gives repeated call to the nc commans when connection was closed somehow.
    //
    public class RemoteNextionDisplay : ITtyService {
        protected String IfName;

        private String HostName;
        private int Port;

        private const int BufferSize = 255;
        private byte[] Buffer = new byte[BufferSize + 5];
        private Socket socket = null;
        private bool Continue;
        private String NewLine = String.Empty;

        // TODO: refactor to seperate UI Config ...???...
        IOutputWrapper Screen;
        private IConfigurationSection screenConfig;

        private IL3Protocol Nextion = new NextionL3();

        public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger)  {
            IfName = cs.Key;
            HostName = cs.GetValue<String>("RemoteHost") ?? "localhost";
            Port = cs.GetValue<int?>("RemotePort") ?? 9000;
            NewLine = cs.GetValue<String>("NewLine");

            screenConfig = cs.GetSection("Screen");
        }

        string ITtyService.GetInterfaceName() {
            return IfName;
        }

        //IConfigurationSection ITtyService.GetScreenConfig() {
        //    return screenConfig;
        //}

        // TODO: refactor ....
        public void SetScreen(IOutputWrapper scr) {
            Screen = scr;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            try {
                BeginnConnect(HostName, Port);
                Nextion.L3PackageReceived += Nextion_L3PackageReceived;
            } catch (Exception ex) {
                Continue = false;
                Screen.WriteLine("Error starting '" + IfName + "' !", ConsoleColor.Red);
                Screen.WriteLine(ex.Message, ConsoleColor.Red);
            }
            return Task.CompletedTask;
        }

        private void Nextion_L3PackageReceived(object arg1, L3PackageRxEventArgs arg2) {
            String msg = "unknown data";
            int len = arg2.l3bytes;
            if (len == 1) {
                switch (arg2.l3Data[0]) {
                    case 0x00:
                        msg = "invalid commamnd";
                        break;

                    case 0x01:
                        msg = "ok";
                        break;

                    case 0x02:
                        msg = "invalid id";
                        break;

                    case 0x03:
                        msg = "invalid page";
                        break;

                    case 0x20:
                        msg = "unsupported escape char";
                        break;

                    case 0x24:
                        msg = "buffer overflow";
                        break;

                    default:
                        msg = "Error " + BitConverter.ToString(arg2.l3Data, 0, 1); ;
                        break;
                }
            } else {

            }
            Screen.WriteLine(msg);
            //Screen.WriteData(arg2.l3Data, arg2.l3bytes);
        }

        // Searches for all Host Endpoints and initates Connect to (all of) them.
        private List<IAsyncResult> BeginnConnect(string server, int port) {
            List<IAsyncResult> retVal = new List<IAsyncResult>();

            IPHostEntry hostEntry = null;
            hostEntry = Dns.GetHostEntry(server);

            // Nutze die Eigenschaft AddressFamily von IPEndPoint um Konflikte zwischen
            // IPv4 und IPv6zu vermeiden. Gehe dazu die Adressliste mit einer Schleife durch.
            foreach (IPAddress address in hostEntry.AddressList) {
                IPEndPoint ipo = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipo.AddressFamily,
                                               SocketType.Stream,
                                               ProtocolType.Tcp);
                retVal.Add(tempSocket.BeginConnect(ipo, ConnectCallback, tempSocket));
            }
            return retVal;
        }

        private void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                if (socket == null) {     // First one wins, all other are ignored.....
                    socket = (Socket)ar.AsyncState;
                    // Complete the connection.  
                    socket?.EndConnect(ar);
                    Screen.WriteLine("Remote port " + HostName + ":" + Port + " connected");

                    Continue = true;
                    // Start Receiving data by entering a data Buffer and a callback.
                    socket?.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), null);
                } else {
                    Screen.WriteLine("Duplicate connection to host !!!");
                    // If more than one addresss was tried. gracefully close others. (to be tested somehow ;-) .....)
                    ((Socket)ar.AsyncState)?.EndConnect(ar);
                    ((Socket)ar.AsyncState)?.Disconnect(false);
                }
            } catch (Exception ex) {
                socket = null;
                Continue = false;
                Screen.WriteLine("Error connecting " + IfName + " to " + HostName + ":" + Port + " !", ConsoleColor.Red);
                Screen.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                int bytesRead = socket.EndReceive(ar);
                if (bytesRead > 0) {
                    Nextion.ProcessReceivedData(Buffer, bytesRead);
                    // Get next chunk of the data.  
                    socket.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), null);
                } else {
                    // Nothing left -> This gets called only if socket was closed. Either when Disconnect() was called locally or remote connection closed.
                    Screen.WriteLine("Socket to remote port " + HostName + ":" + Port + " closed.");
                    Continue = false;
                    socket?.Disconnect(false);
                    socket = null;
                }
            } catch (Exception e) {
                Screen.WriteLine("Exception in ReceiverCallback: " + e.ToString(), ConsoleColor.Red);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            if (socket != null) {
                if (socket.Connected) {
                    await socket.DisconnectAsync(false, cancellationToken);
                }
            }
        }

        //void ITtyService.SendUart(string line) {
        //    if (IsConnected()) {
        //        Nextion.ConvertAndSendL3CommandLine(line, (d, l) => socket.Send(d, l, SocketFlags.None));
        //        //line += NewLine;
        //        //socket.Send(Encoding.UTF8.GetBytes(line));
        //    }
        //}

        public bool IsConnected() {
            return Continue;
        }

        //void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger) {
        //    throw new NotImplementedException();
        //}

        void ITtyService.SetScreen(IOutputWrapper scr) {
            throw new NotImplementedException();
        }

        void ITtyService.SendUart(byte[] toSend, int len) {
            throw new NotImplementedException();
        }

        bool ITtyService.IsConnected() {
            throw new NotImplementedException();
        }

        void ITtyService.ProcessCommand(string s) {
            throw new NotImplementedException();
        }

        public string GetViewName() {
            throw new NotImplementedException();
        }
    }
}
