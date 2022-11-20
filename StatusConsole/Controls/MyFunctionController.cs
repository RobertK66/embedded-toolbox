using ConsoleGUI.Input;
using StatusConsoleApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace StatusConsole.Controls {
    public class MyFunctionController : IInputListener {
        private IConfigurableServices services;
        private Dictionary<string, MyUartScreen> screens = new Dictionary<string, MyUartScreen>();

        public MyFunctionController(IConfigurableServices services) {
            this.services = services;
        }

        public void OnInput(InputEvent inputEvent) {
            if (inputEvent.Key.Key == ConsoleKey.Escape) {
                ITtyService uartService = services.GetCurrentService();
                CancellationToken ct = new();
                Task t = uartService.StopAsync(ct);
                t.Wait();
                MyUartScreen scr;
                if (screens.TryGetValue(uartService.GetInterfaceName(), out scr)) { 
                    scr.Clear();
                }
                t = uartService.StartAsync(ct);
                t.Wait();
            } 
        }

        internal void AddUartScreen(string name, MyUartScreen uartScreen) {
            screens.Add(name, uartScreen);  
        }
    }
}
