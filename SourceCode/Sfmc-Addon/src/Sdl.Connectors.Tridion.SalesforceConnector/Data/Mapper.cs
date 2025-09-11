namespace Sdl.Connectors.Tridion.SalesforceConnector.Data
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Common.Contract.ConnectedInfo;
    using ContentService.Contract;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using SalesforceContentService.Models;
    using Utils;

    public static class Mapper
    {
        public static List<IEntity> ToDataExtensionFields(this List<DataExtensionFieldsModel> fields, IEntityIdentity parentIdentity)
        {
            List<IEntity> entities = new List<IEntity>();
            foreach (DataExtensionFieldsModel dataExtensionFieldsModel in fields)
            {
                entities.Add(dataExtensionFieldsModel.MapDataExtensionField(parentIdentity));
            }

            return entities;
        }

        public static List<IEntity> ToFrameWorkEntities(this IEnumerable<ConnectedFileSystemEntry> entries, IEntityIdentity parentIdentity)
        {
            return entries.Select(entry =>
            {
                IEntity entity;
                ContentIdentity contentIdentity = Helper.DecodeIdentityId(entry.Id);
                if (contentIdentity.Context == nameof(ContentType.DataFolder) || contentIdentity.Context == nameof(ContentType.SharedDataFolder))
                {
                    entity = entry.MapSaleforceFolder(parentIdentity);
                }
                else if (contentIdentity.Context == nameof(ContentType.DataExtensionFolder) || contentIdentity.Context == nameof(ContentType.SharedDataExtensionFolder))
                {
                    entity = entry.MapDataExtensionFolder(parentIdentity);
                }
                else
                {
                    entity = null;
                }

                return entity;
            }).ToList();
        }

        public static EventDefinitionItem MapToEventDefinitiontem(this EventDefinitionModel eventDefinition, IEntityIdentity identity)
        {
            IEntityIdentity itemIdentity = new EntityIdentity(eventDefinition?.EventDefinitionKey, StructureType.Leaf)
            {
                NamespaceId = identity.NamespaceId,
                LocaleId = identity.LocaleId,
                Type = nameof(EventDefinitionItem),
            };

            IEntityIdentity parentIdentity = new EntityIdentity(Helper.JourneysId, StructureType.Container)
            {
                NamespaceId = identity.NamespaceId,
                LocaleId = identity.LocaleId,
                Type = nameof(SalesforceFolder),
            };

            EventDefinitionItem eventDefinitiontem = new EventDefinitionItem(itemIdentity, parentIdentity)
            {
                Title = eventDefinition?.Name,
                Name = eventDefinition?.Name,
                DataExtensionId = eventDefinition?.DataExtensionId,
                EventDefinitionKey = eventDefinition?.EventDefinitionKey,
                DataExtensionName = eventDefinition?.DataExtensionName,
            };

            return eventDefinitiontem;
        }

        public static DataExtensionField MapDataExtensionField(this DataExtensionFieldsModel fieldModel, IEntityIdentity identity)
        {
            IEntityIdentity itemIdentity = new EntityIdentity(fieldModel.TridionId, StructureType.Leaf)
            {
                NamespaceId = identity.NamespaceId,
                LocaleId = identity.LocaleId,
                Type = nameof(DataExtensionField)
            };

            IEntityIdentity newParentIdentity;

            if (identity.Type == nameof(DataExtensionFolder))
            {
                newParentIdentity = identity;
            }
            else
            {
                string[] extensionIdInfos = Helper.DecodeDataExtensionId(identity.Id);
                string parentId = Helper.CreateComposedId(extensionIdInfos[2], extensionIdInfos[0]);
                newParentIdentity = new EntityIdentity(parentId, StructureType.Container)
                {
                    NamespaceId = identity.NamespaceId,
                    LocaleId = identity.LocaleId,
                    Type = nameof(DataExtensionFolder)
                };
            }

            DataExtensionField field = new DataExtensionField(itemIdentity, newParentIdentity)
            {
                Title = fieldModel.Name,
                Filename = fieldModel.Name,
                CustomerKey = fieldModel.CustomerKey,
                FieldType = fieldModel.FieldType,
                MaxLength = fieldModel.MaxLength,
                IsPrimaryKey = fieldModel.IsPrimaryKey,
            };

            return field;
        }

        public static Contact CreateContactEntity(IEntityIdentity parentIdentity, string contactKey)
        {
            IEntityIdentity itemIdentity = new EntityIdentity(contactKey, StructureType.Leaf)
            {
                NamespaceId = parentIdentity.NamespaceId,
                LocaleId = parentIdentity.LocaleId,
                Type = nameof(Contact),
            };

            Contact contact = new Contact(itemIdentity, parentIdentity);
            contact.Journeys = new List<ContactJourney>();

            return contact;
        }

        public static List<DataExtensionFieldsModel> SetDataExtensionListIds(this List<DataExtensionFieldsModel> fields, IEntityIdentity parentIdentity)
        {
            List<DataExtensionFieldsModel> results = new List<DataExtensionFieldsModel>();
            foreach (DataExtensionFieldsModel dataExtensionField in fields)
            {
                results.Add(dataExtensionField.SetDataExtensionIdProperty(parentIdentity));
            }

            return results;
        }

        public static DataExtensionFieldsModel SetDataExtensionIdProperty(this DataExtensionFieldsModel field, IEntityIdentity identity)
        {
            string customerkey = field.CustomerKey.Replace("[", "");
            customerkey = customerkey.Replace("]", "");
            StringBuilder tridionId = new StringBuilder();

            if (identity.Type == nameof(DataExtensionFolder))
            {
                ContentIdentity contentIdentity = Helper.DecodeIdentityId(identity.Id);

                tridionId.Append(customerkey);
                tridionId.Append('.');
                tridionId.Append(contentIdentity.Context);
            }
            else
            {
                string[] infos = identity.Id.Split('.');
                tridionId.Append(customerkey);
                tridionId.Append('.');
                tridionId.Append(infos[2]);
            }

            field.TridionId = tridionId.ToString();
            return field;
        }

        public static DataExtensionFolder MapDataExtensionFolder(this ConnectedFileSystemEntry entry, IEntityIdentity parentIdentity, ContentIdentity parentFolder = null)
        {
            IEntityIdentity itemIdentity = new EntityIdentity(entry.Id, StructureType.Container)
            {
                NamespaceId = parentIdentity.NamespaceId,
                LocaleId = parentIdentity.LocaleId,
                Type = nameof(DataExtensionFolder)
            };

            IEntityIdentity newParentIdentity;

            if (parentIdentity.Id == "root")
            {
                newParentIdentity = RootEntity.CreateRootEntityIdentity(parentIdentity.NamespaceId, parentIdentity.LocaleId);
            }
            else
            {
                ContentIdentity contentIdentity = Helper.DecodeIdentityId(parentIdentity.Id);

                if (parentFolder == null)
                {
                    string parentComposedId = Helper.CreateComposedId(contentIdentity.Context, entry.ParentId);
                    newParentIdentity = new EntityIdentity(parentComposedId, StructureType.Container)
                    {
                        NamespaceId = parentIdentity.NamespaceId,
                        LocaleId = parentIdentity.LocaleId,
                        Type = nameof(SalesforceFolder)
                    };
                }
                else
                {
                    string parentComposedId = Helper.CreateComposedId(parentFolder.Context, parentFolder.Id);
                    newParentIdentity = new EntityIdentity(parentComposedId, StructureType.Container)
                    {
                        NamespaceId = parentIdentity.NamespaceId,
                        LocaleId = parentIdentity.LocaleId,
                        Type = nameof(SalesforceFolder)
                    };
                }
            }

            DataExtensionFolder folder = new DataExtensionFolder(itemIdentity, newParentIdentity)
            {
                Title = Path.GetFileNameWithoutExtension(entry.FullName)
            };

            return folder;
        }

        public static SalesforceFolder MapSaleforceFolder(this ConnectedFileSystemEntry entry, IEntityIdentity parentIdentity, bool IsRoot = false, ConnectedFileSystemEntry parentFolder = null)
        {
            IEntityIdentity itemIdentity = new EntityIdentity(entry.Id, StructureType.Container)
            {
                NamespaceId = parentIdentity.NamespaceId,
                LocaleId = parentIdentity.LocaleId,
                Type = nameof(SalesforceFolder)
            };

            IEntityIdentity newParentIdentity;

            if (IsRoot)
            {
                newParentIdentity = RootEntity.CreateRootEntityIdentity(parentIdentity.NamespaceId, parentIdentity.LocaleId);
            }
            else
            {
                ContentIdentity contentIdentity = Helper.DecodeIdentityId(parentIdentity.Id);

                if (parentFolder == null)
                {
                    string parentComposedId = Helper.CreateComposedId(contentIdentity.Context, entry.ParentId);
                    newParentIdentity = new EntityIdentity(parentComposedId, StructureType.Container)
                    {
                        NamespaceId = parentIdentity.NamespaceId,
                        LocaleId = parentIdentity.LocaleId,
                        Type = nameof(SalesforceFolder)
                    };
                }
                else
                {
                    string parentComposedId = Helper.CreateComposedId(contentIdentity.Context, parentFolder.Id);
                    newParentIdentity = new EntityIdentity(parentComposedId, StructureType.Container)
                    {
                        NamespaceId = parentIdentity.NamespaceId,
                        LocaleId = parentIdentity.LocaleId,
                        Type = nameof(SalesforceFolder)
                    };
                }
            }

            SalesforceFolder folder = new SalesforceFolder(itemIdentity, newParentIdentity)
            {
                Title = entry.FullName
            };

            return folder;
        }
    }
}
