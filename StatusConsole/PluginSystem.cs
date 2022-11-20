using Microsoft.Extensions.Configuration;
using StatusConsoleApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    internal class PluginSystem {
        private static bool loaded = false;
        internal static List<Assembly> LoadedAssemblies = new List<Assembly>();

        internal static T LoadPlugin<T>(string typeName, IConfigurationSection config) {
            T newInstance;

            if (!loaded) {
                LoadPluginAssemblies();
            }

            Type type = null;
            try {
                type = Type.GetType(typeName);
                if (type == null) { 
                    foreach(Assembly assembly in LoadedAssemblies) {
                        type = assembly.ExportedTypes.Where(t => t.FullName == typeName).FirstOrDefault();
                        if (type != null) {
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                throw new ApplicationException("Error loading plugin Assemblies", ex);
            }

            if (type != null) {
                newInstance = (T)Activator.CreateInstance(type, new object[] { config });
            } else {
                throw new ApplicationException("Protocol Class (" + typeName + ") not found");
            }

            return newInstance;
        }

        private static void LoadPluginAssemblies() {
            // TODO: search all real plugins (not build with project dependencies) ....
            LoadedAssemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(AppDomain.CurrentDomain.BaseDirectory + "StatusConsolePlugins.dll"));
            LoadedAssemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(AppDomain.CurrentDomain.BaseDirectory + "ClimbPlugins.dll"));
            loaded = true;
        }
    }
}
