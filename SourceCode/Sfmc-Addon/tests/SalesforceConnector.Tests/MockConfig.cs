namespace SalesforceConnector.Tests
{
    using Sdl.Connectors.Tridion.SalesforceConnector.Configuration;

    public class MockConfig : ISalesforceConfiguration
    {
        public virtual string ClientId { get; set; } = "w0wq3alxyzulv34hn57cleqi";

        public virtual string ClientSecret { get; set; } = "aXSaPuzktatXNsfY2zg7IB8J";

        public virtual string HostUrl { get; set; } = "https://mczhbl9dtbdd8778yh71c46x8p-4.auth.marketingcloudapis.com";

        public virtual string AccountId { get; set; } = "510004466";
    }
}
