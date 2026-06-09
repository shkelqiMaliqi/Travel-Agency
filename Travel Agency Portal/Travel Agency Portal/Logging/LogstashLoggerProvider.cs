using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Travel_Agency_Portal.Logging;

public sealed class LogstashLoggerProvider : ILoggerProvider
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _applicationName;

    public LogstashLoggerProvider(string host, int port, string applicationName)
    {
        _host = host;
        _port = port;
        _applicationName = applicationName;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LogstashLogger(_host, _port, _applicationName, categoryName);
    }

    public void Dispose()
    {
    }

    private sealed class LogstashLogger : ILogger
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _applicationName;
        private readonly string _categoryName;

        public LogstashLogger(string host, int port, string applicationName, string categoryName)
        {
            _host = host;
            _port = port;
            _applicationName = applicationName;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var entry = new
            {
                timestamp = DateTimeOffset.UtcNow,
                application = _applicationName,
                level = logLevel.ToString(),
                category = _categoryName,
                eventId = eventId.Id,
                eventName = eventId.Name,
                message = formatter(state, exception),
                exception = exception?.ToString()
            };

            try
            {
                using var client = new TcpClient();
                client.SendTimeout = 500;
                client.ReceiveTimeout = 500;
                client.Connect(_host, _port);

                var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entry) + "\n");
                using var stream = client.GetStream();
                stream.Write(payload);
            }
            catch
            {
                // Logging must never break request handling.
            }
        }
    }
}
