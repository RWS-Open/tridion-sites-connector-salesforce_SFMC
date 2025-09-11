namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;
    using global::Tridion.ConnectorFramework.Contracts;

    [Schema("SalesforceFolder", "Salesforce folder")]
    public class SalesforceFolder : ContentFolderBase
    {
        public SalesforceFolder()
        { }

        public SalesforceFolder(IEntityIdentity identity, IEntityIdentity parentIdentity)
        {
            base.Identity = identity;

            base.ParentIdentity = parentIdentity.Id == identity.Id ? RootEntity.CreateRootEntityIdentity(identity.NamespaceId, identity.LocaleId) : parentIdentity;
        }
    }
}
