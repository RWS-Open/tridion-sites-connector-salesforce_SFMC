using System;

namespace SalesforceConnector.Tests
{
    using Tridion.ConnectorFramework.Contracts.Logging;

    public class MockLogger : IConnectorLogger
    {
        public ConnectorLogLevel Level { get; } = ConnectorLogLevel.Debug;

        public void LogTrace(string message)
        {
            Console.WriteLine(message);
        }

        public void LogDebug(string message)
        {
            Console.WriteLine(message);
        }

        public void LogInfo(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public void LogCritical(string message)
        {
            Console.WriteLine(message);
        }
    }
}
