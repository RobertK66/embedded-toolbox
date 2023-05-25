using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Logger {

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Configuration;

    public static class ColorConsoleLoggerExtensions {
        public static ILoggingBuilder AddConGuiLogger(
            this ILoggingBuilder builder) {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, ConGuiLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <ConGuiLoggerConfiguration, ConGuiLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddConGuiLogger(
            this ILoggingBuilder builder,
            Action<ConGuiLoggerConfiguration> configure) {
            builder.AddConGuiLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }

    //internal class ConGuiLoggerExtensions {
    //}
}
