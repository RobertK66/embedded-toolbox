using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenLib {
    public interface IConOutput {
        void WriteLine(string v, ConsoleColor? col = null);
        void Write(string v, ConsoleColor? col = null);
        void WriteData(byte[] buffer, int bytesRead);
    }
}
