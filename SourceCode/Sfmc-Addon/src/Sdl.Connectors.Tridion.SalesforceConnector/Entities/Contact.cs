namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.Remoting.Contracts;

    [Schema("Contact", "Contact")]
    public class Contact : EntityBase
    {
        public Contact()
        { }

        public Contact(IEntityIdentity identity, IEntityIdentity parentIdentity)
        {
            base.Identity = identity;

            base.ParentIdentity = parentIdentity;
        }

        [SchemaField(false, true, "")]
        public virtual List<ContactJourney> Journeys { get; set; }
    }

    [Schema("ContactJourney", "Contact Journey")]
    public class ContactJourney : IValueObject
    {
        [SchemaField(false,true, "Name")]
        public virtual string Name { get; set; }

        [SchemaField(false, true, "Event Definitions")]
        public virtual List<EventDefinition> EventDefinitions { get; set; }

        [SchemaField(false, true, "Contact Informations")]
        public virtual IList<ContactData> ContactData { get; set; }
    }

    [Schema("EventDefinition", "Event Definition")]
    public class EventDefinition : IValueObject
    {
        [SchemaField(false, true, "Name")]
        public virtual string Name { get; set; }

        [SchemaField(false, true, "Key")]
        public virtual string Key { get; set; }
    }

    [Schema("ContactData", "Contact Data")]
    public class ContactData : IValueObject
    {
        [SchemaField(Required: false, Readonly: false, Description: "Name")]
        public virtual string Name { get; set; }

        [SchemaField(Required: false, Readonly: false, Description: "Value")]
        public virtual string Value { get; set; }
    }
}
