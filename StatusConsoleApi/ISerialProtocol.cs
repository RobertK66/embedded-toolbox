using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace StatusConsoleApi
{
    public interface ISerialProtocol {
        void ProcessByte(byte b);


        void ProcessUserInput(String cmd);
        void SetScreen(IOutputWrapper screen, ILogger log, ITtyService tty, string cmdTermination);
    }
}