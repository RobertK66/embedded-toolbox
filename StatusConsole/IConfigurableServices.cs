using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public interface IConfigurableServices {
        Dictionary<string, ITtyService> GetTtyServices();
        ITtyService GetCurrentService();
        ITtyService GetNextService();
    }
}
