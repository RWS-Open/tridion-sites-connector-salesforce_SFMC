namespace SalesforceConnector.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Sdl.Connectors.Tridion.SalesforceConnector.Data;
    using Sdl.Connectors.Tridion.SalesforceConnector.Entities;
    using Sdl.Connectors.Tridion.SalesforceConnector.Utils;
    using Tridion.ConnectorFramework.Connector.SDK;
    using Tridion.ConnectorFramework.Contracts;
    using Xunit;

    public class ContextIntegrationTests
    {
        private readonly MockConfig config = new MockConfig();
        private readonly MockLogger logger = new MockLogger();
        private readonly SalesforceContext context;

        public ContextIntegrationTests()
        {
            this.context = new SalesforceContext(this.config, this.logger);
        }

        [Fact]
        public async void GetRootContent_ValidItem_Succes()
        {
            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntityIdentity parentIdentity = RootEntity.CreateRootEntityIdentity("test", "test");

            IList<IEntity> entities = await this.context.GetRootContent(parentIdentity).ConfigureAwait(false);

            Assert.NotNull(entities);
            Assert.NotEqual(0, (double)entities.Count);
        }

        [Fact]
        public async void GetContentList_ValidItem_Succes()
        {
            await this.context.EnsureAuthentication().ConfigureAwait(false);
            IEntityIdentity identity = RootEntity.CreateRootEntityIdentity("test", "test");
            IList<IEntity> rootEntities = await this.context.GetRootContent(identity).ConfigureAwait(false);

            IEntityIdentity testIdentity = new EntityIdentity("EventDefinitions", StructureType.Container);

            var entities = await this.context.GetContentList(testIdentity).ConfigureAwait(false);

            Assert.NotNull(entities);
            Assert.NotEqual(0, (double)entities.Count);
        }

        [Fact]
        public async void GetActiveEventDefinitions_Succes()
        {
            IEntityIdentity identity = new EntityIdentity(Helper.JourneysId, StructureType.Container){};

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            var entities = await this.context.GetActiveEventDefinitions(identity).ConfigureAwait(false);

            Assert.NotNull(entities);
        }

        [Fact]
        public async void GetDataExtensionFieldInfo_ValidDataExtension_Succes()
        {
            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntityIdentity parentIdentity = new EntityIdentity("4E786497-4A72-46F1-A56F-BBB18D352815.field3~SharedDataExtensionFolder", StructureType.Leaf) { Type = nameof(DataExtensionField) };

            IEntity entities = await this.context.GetDataExtensionFieldInfo(parentIdentity).ConfigureAwait(false);

            Assert.NotNull(entities);
        }

        [Fact]
        public async void GetFolderInfo_ValidItem_Succes()
        {
            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntityIdentity parentIdentity = new EntityIdentity(Helper.JourneysId, StructureType.Container) { Type = nameof(DataExtensionFolder) };

            IEntity entities = await this.context.GetFolderInfo(parentIdentity).ConfigureAwait(false);

            Assert.NotNull(entities);
            Assert.Equal(entities.Identity.Id, parentIdentity.Id);
        }

        [Fact]
        public async void FireEvent_ValidItem_Succes()
        {
            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntityIdentity parentIdentity = new EntityIdentity("72413F24-02B5-4E98-87C9-963E633C66A4", StructureType.Container) { Type = nameof(DataExtensionFolder) };
            IEntity entity = new JourneyEventTrigger() {Values = new List<JourneyEventTriggerItem>()};

            JourneyEventTrigger record = new JourneyEventTrigger();

            record.Values = new List<JourneyEventTriggerItem>();
            record.Values.Add(new JourneyEventTriggerItem
            {
                Name = "Email",
                Value = "sdltestemail9@gmail.com"
            });

            record.Values.Add(new JourneyEventTriggerItem
            {
                Name = "First Name",
                Value = "Test"
            });

            entity.SetPropertyValue("EventDefinitionKey", "DEAudience-2ce1017b-44d3-1128-91d5-dc91a9984d91");
            entity.SetPropertyValue("Values", record.Values);

            await this.context.FireEventJourney(parentIdentity, entity).ConfigureAwait(false);
        }

        [Fact]
        public async void GetContactInfo_ValidContactKey_ReturnValidList()
        {
            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntityIdentity parentIdentity = new EntityIdentity("sven.bengtsson@company.com", StructureType.Container);

            IEntity contactInfo = await this.context.GetContactInfo(parentIdentity).ConfigureAwait(false);

            Assert.NotNull(contactInfo);
        }
    }
}
