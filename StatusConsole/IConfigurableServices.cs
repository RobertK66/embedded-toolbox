using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public interface IConfigurableServices : IEnumerable<ITtyService>, IHostedService {
        Dictionary<string, ITtyService> GetTtyServices();
        ITtyService GetCurrentService();
        ITtyService GetNextService();
    }
}
