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

            Type type = null;
            try {
                type = Type.GetType(typeName);
                if (type == null) {
                    if (!loaded) {
                        LoadPluginAssemblies();
                    }
                    foreach (Assembly assembly in LoadedAssemblies) {
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
            
            // The plugin dlls build with this project can be loaded by Name (even if the app is distributed as single exe file!)
            LoadedAssemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("StatusConsolePlugins")));
            LoadedAssemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("ClimbPlugins")));

            // TODO: search all real extension plugin dlls ion some configured(?) directory ........
            // LoadedAssemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(""+"ClimbPlugins"));

            loaded = true;
        }
    }
}
