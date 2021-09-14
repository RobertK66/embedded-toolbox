using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public interface IUartServices {
        Dictionary<string, IUartService> GetUartServices();
        IUartService GetCurrentService();
        IUartService GetNextService();
    }
}
