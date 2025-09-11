namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Configuration
{
    using global::Tridion.Remoting.Contracts;

    public class LocaleMapping : IValueObject
    {
        public virtual string[] PublicationIds { get; set; }

        public virtual string Locale { get; set; }
    }
}
