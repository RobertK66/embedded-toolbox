using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.L3 {
    public class L3PackageRxEventArgs {
        public byte[] l3Data { get; set; }
        public int l3bytes { get; set; }
    }


    public interface IL3Protocol {
        // API for TX (L3 Package -> L2 -> ComPort/Socket)
        //void SendL3Package(byte[] l3Data, int l3bytes);
        void ConvertAndSendL3CommandLine(String commandLine, Action<byte[], int> SendL2Data);

        // L3 Rx Implementation to detect L3 Protocol
        void ProcessReceivedData(byte[] bytes, int bytesReceivedL2);

        event Action<object, L3PackageRxEventArgs> L3PackageReceived;

 
    }
}
