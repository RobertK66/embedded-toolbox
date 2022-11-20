using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace StatusConsoleApi
{
    public interface ISerialProtocol {
        void ProcessByte(byte b);
        void ProcessCommand(String cmd);

        void SetScreen(IConfigurationSection debugConfig, IOutputWrapper screen, ILogger log, ITtyService tty);
    }
}