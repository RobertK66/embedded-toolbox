using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatusConsoleApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace StatusConsole {

    public class DefaultCli: ISerialProtocol {
        private const int C_BYTESPERLINE = 16;
        private IConfigurationSection? Config;
        private IOutputWrapper? Screen;
        private ILogger? Log;
        private ITtyService? tty;

        Task Timeout;
        //volatile int SendCount = 0;
        private volatile byte[] buffer = new byte[C_BYTESPERLINE];
        private volatile int idx = 0;
     
        // This constructor is called by the 'plugin System' when somb. is configured to use this 'protocol Class'
        public DefaultCli(IConfigurationSection config) {
            this.Config = config;
        }

        public void SetScreen(IOutputWrapper screen, Microsoft.Extensions.Logging.ILogger log, ITtyService tty) {
            this.Screen = screen;
            this.Log = log;
            this.tty = tty;
            Timeout = Task.Run(() => TmoTimer());
        }

        private void TmoTimer() {
            //byte[] copybuffer = new byte[100];
            while (true) {
                try {
                    Thread.Sleep(200);
                    if (idx>0) {
                        Screen?.WriteLine(BufferToLine(buffer, idx));
                        idx = 0;
                    }
                } catch { }
            }

        }

        private string BufferToLine(byte[] buffer, int idx) {
            String line = "";
            for (int i = 0; i < idx; i++) {
                line += " " + buffer[i].ToString("X2");
            }
            for (int i = idx; i < C_BYTESPERLINE; i++) {
                buffer[i] = (byte)'?';
                line += " ..";
            }
            line += "   " + Encoding.ASCII.GetString(buffer, 0, C_BYTESPERLINE).Replace("?", ".").Replace("\0",".");
            return line;
        }

        // UART Incomming Byte protocol
        public void ProcessByte(byte b) {
            buffer[idx++] = b;
            if(idx>=buffer.Length) {
                Screen?.WriteLine(BufferToLine(buffer, idx));
                idx = 0;
            }
        }

        //G/TUI inputs from Users Command Line
        public void ProcessUserInput(string cmd) {
            tty?.SendUart(Encoding.ASCII.GetBytes(cmd + "\n"), cmd.Length + 1);
        }

    }       
}
