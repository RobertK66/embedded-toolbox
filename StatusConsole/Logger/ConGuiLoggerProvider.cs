using System.Collections.Concurrent;
using System.Runtime.Versioning;
using StatusConsole.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace StatusConsole.Logger {
    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("ConGuiConsole")]
    public sealed class ConGuiLoggerProvider : ILoggerProvider {
        private readonly IDisposable _onChangeToken;
        private ConGuiLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, ConGuiLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public ConGuiLoggerProvider(
            IOptionsMonitor<ConGuiLoggerConfiguration> config) {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new ConGuiLogger(name, GetCurrentConfig));

        private ConGuiLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose() {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}