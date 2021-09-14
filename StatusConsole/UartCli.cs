using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
    public class UartCli : IUartService {
        private IConfigurationSection _config;
        private SerialPort  _serialPort;
        private bool _continue;
        //Thread reader = null;
        Task reader;

        string IUartService.GetInterfaceName() {
            return _config.Key;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken) {
            _serialPort = new SerialPort();
            _serialPort.PortName = _config?.GetValue<String>("ComName")??"COM1";
            _serialPort.BaudRate = _config?.GetValue<int?>("Baud")??9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Open();
            _serialPort.NewLine = "\r";
            //_serialPort.ReadTimeout = 10;
            Console.WriteLine("Uart "+ _serialPort.PortName + " connected");

            _continue = true;
            reader = Task.Run(() => Read());
            //reader = new Thread(Read);
            //reader.Start();

            return Task.CompletedTask;
        }

        public void Read() {
            // Avoid blocking the thread;
            if (_serialPort.ReadTimeout == -1) {
                _serialPort.ReadTimeout = 500;
            }
            while(_continue) {
                try {
                    char ch = (char)_serialPort.ReadChar();
                    Console.Write(ch);
                } catch(TimeoutException) { }
            }
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            // terminate the reader Task.
            _continue = false;
            await reader;       // reader Task will be finished and execution "awaits it" and continues afterwards. (Without blocking any thread here)
            //reader.Join();    // How to do this in an async manner if Thread is used iso Task !?
            _serialPort.Close();
            Console.WriteLine("Uart " + _serialPort.PortName + " closed.");
        }

        void IUartService.SendUart(string line) {
            _serialPort?.WriteLine(line);
        }

        public void SetConfiguration(IConfigurationSection cs) {
            _config = cs;

        }

    }
}
