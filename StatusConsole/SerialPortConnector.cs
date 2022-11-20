using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StatusConsoleApi;

namespace StatusConsole {
    public class SerialPortConnector : SerialPortBase {

        ISerialProtocol protocol;
        //IConfigurationSection protConfig;

        override public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger) {
            base.Initialize(cs, rootConfig, logger);

            // Here we Instanciate the configured protocol for this Serial Connector. If there is an protocol config section declared we pass it on to the 
            // ISerialProtocol constructor.
            string typeName = Config?.GetValue<String>("ProtClass") ?? "dummyClass";

            try {
                String configName = Config?.GetValue<String>("ProtConfig")??"dummyCfg";
                IConfigurationSection pluginConfig = rootConfig.GetSection(configName);
                protocol = PluginSystem.LoadPlugin<ISerialProtocol>(typeName, pluginConfig);
            } catch(Exception ex) {
                throw new ApplicationException("Protocol Class (" + typeName + ") not found for '" + cs.Path + "' ");
            }



            //Type type = null;
            //try {
            //    type = Type.GetType(typeName, true);
            //} catch (Exception ex) {
            //    String pluginPath = AppDomain.CurrentDomain.BaseDirectory + "StatusConsolePlugins.dll";
            //    Assembly plugins = AssemblyLoadContext.Default.LoadFromAssemblyPath(pluginPath);
            //    type = plugins.ExportedTypes.Where(t => t.FullName == typeName).FirstOrDefault();
            //}

            //if (type != null) {
            //    // Create an Instance of the protocol class and pass it its config if available.
            //    IConfigurationSection protConfig = null;
            //    String configName = Config?.GetValue<String>("ProtConfig");
            //    if (configName != null) {
            //        protConfig = rootConfig?.GetSection(configName);
            //    }
            //    protocol = (ISerialProtocol)Activator.CreateInstance(type, new object[] { protConfig });
            //} else {
            //    throw new ApplicationException("Protocol Class ("+typeName+") not found for '" + cs.Path + "' ");
            //}
        }

        override public void Read(SerialPort port) {
            // Avoid blocking the thread;
            // If nothing gets received, we sometimes have to check for the Continuation flag here.
            //if (port.ReadTimeout == -1) {
            //    port.ReadTimeout = 50000;
            //}

            // Set the event Handler to receive the bytes
            port.DataReceived += (s, e) => {
                if (e.EventType == SerialData.Chars) {
                    SerialPort sp = (SerialPort)s;
                    while (sp.BytesToRead > 0) {
                        var b = sp.ReadByte();
                        if (b>=0) {
                            Log?.LogTrace("Rx: {@mycharHex} '{@mychar}'", "0x" + b.ToString("X2"), (b == '\n') ? ' ' : b);
                            protocol.ProcessByte((byte)b);
                        }
                    };
                }
            };

            port.ErrorReceived += (s, e) => {
                Log?.LogError(new EventId(2, "Rx"),"Serial Error: " + e.EventType);
                Continue = false;
            };

            port.PinChanged += (s, e) => {
                Log?.LogInformation(new EventId(3, "Rx"), "Pin Changed: " + e.EventType);
            };

            port.Disposed += (s, e) => {
                Log?.LogInformation(new EventId(4, "Rx"), "Serial Port disoposed ");
                Continue = false;
            };

            // and wait in empty loop until shutdown.
            while (Continue) {
                Thread.Sleep(1000);
                if (!port.IsOpen) {
                    Log?.LogInformation(new EventId(4, "Rx"), "Serial Port was closed.");
                    Continue = false;
                } 
            }
            Log?.LogInformation(new EventId(4, "Rx"), "Terminating reader thread. Try to reconnect with <ESC>.");          
        }


        override public void SetScreen(IOutputWrapper scr) {
            Screen = scr;
            protocol.SetScreen(scr, Log, this);
        }

        override public void ProcessCommand(String s) {
            protocol.ProcessUserInput(s);
            //Port.Write(Encoding.ASCII.GetBytes(s + Port.NewLine), 0, (s + Port.NewLine).Length);
        }

    }
}
