using System;

namespace StatusConsoleApi {
    public interface IOutputWrapper {
        void WriteLine(string v);
        void WriteLine(string v, ConsoleColor red);
        void WriteData(byte[] buffer, int bytesRead);
        void Write(string v);
    }
}