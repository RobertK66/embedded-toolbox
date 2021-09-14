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
        private SerialPort  _serialPort;
        private bool _continue;
        Thread reader = null;

     Task IHostedService.StartAsync(CancellationToken cancellationToken) {

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM30";
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Open();
            _serialPort.NewLine = "\r";
            //_serialPort.ReadTimeout = 10;
            Console.WriteLine("Uart connected");

            _continue = true;
            //readerTask = Task.Run(() => Read());
            reader = new Thread(Read);
            reader.Start();

            return Task.CompletedTask;
        }

        public void Read() {
            // Avoid blocking the thread;
            if (_serialPort.ReadTimeout == -1) {
                _serialPort.ReadTimeout = 10;
            }
            while(_continue) {
                try {
                    char ch = (char)_serialPort.ReadChar();
                    Console.Write(ch);
                } catch(TimeoutException) { }
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            _continue = false;
            //readerTask.Wait();
            reader.Join();
            _serialPort.Close();
            return Task.CompletedTask;
        }

        void IUartService.SendUart(string line) {
            _serialPort?.WriteLine(line);
        }
    }
}
