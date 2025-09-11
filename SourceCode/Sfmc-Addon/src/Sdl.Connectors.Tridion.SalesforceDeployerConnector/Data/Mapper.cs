namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Data
{
    using System;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using SalesforceContentService.Models;

    public static class Mapper
    {
        public static SalesforceAsset ToSalesforceAsset(this SalesforceMarketingCloudAsset asset, string entityType, IEntityIdentity parentIdentity)
        {
            var identity = new EntityIdentity(asset.Id, StructureType.Leaf)
            {
                NamespaceId = parentIdentity.NamespaceId,
                LocaleId = parentIdentity.LocaleId,
                Type = entityType
            };

            return new SalesforceAsset(identity, parentIdentity);
        }

        public static IEntity ToAsset(this SalesforceMarketingCloudAsset asset, IEntityIdentity parentEntityIdentity, string entityType)
        {
            switch (entityType)
            {
                case nameof(DownloadContextType.CodeSnippet):
                    {
                        return asset.ToSalesforceCodeSnippet(parentEntityIdentity);
                    }
                case nameof(DownloadContextType.Image):
                    {
                        return asset.ToImage(parentEntityIdentity);
                    }
                default:
                    throw new ArgumentOutOfRangeException($"Unknown type");
            }
        }

        public static SalesforceCodeSnippet ToSalesforceCodeSnippet(this SalesforceMarketingCloudAsset asset, IEntityIdentity parentIdentity)
        {
            EntityIdentity identity = new EntityIdentity(asset.Id, StructureType.Leaf);
            IEntityIdentity newParentIdentity;
            if (parentIdentity != null)
            {
                identity.NamespaceId = parentIdentity.NamespaceId;
                identity.LocaleId = parentIdentity.LocaleId;
                identity.Type = nameof(DownloadContextType.CodeSnippet);
                newParentIdentity = parentIdentity;
            }
            else
            {
                newParentIdentity = new EntityIdentity(asset.Category.Id, StructureType.Container);
            }

            
            SalesforceCodeSnippet codeSnippet = new SalesforceCodeSnippet(identity, newParentIdentity)
            {
                Title = asset.Name,
                ContentType = "text/html",
                Content = asset.Content
            };

            return codeSnippet;
        }

        public static SalesforceImage ToImage(this SalesforceMarketingCloudAsset asset, IEntityIdentity parentEntityIdentity)
        {
            EntityIdentity identity = new EntityIdentity(asset.Id, StructureType.Leaf);
            IEntityIdentity newParentIdentity;
            if (parentEntityIdentity != null)
            {
                identity.NamespaceId = parentEntityIdentity.NamespaceId;
                identity.LocaleId = parentEntityIdentity.LocaleId;
                identity.Type = nameof(DownloadContextType.Image);
                newParentIdentity = parentEntityIdentity;
            }
            else
            {
                newParentIdentity = new EntityIdentity(asset.Category.Id, StructureType.Container);
            }

            SalesforceImage image = new SalesforceImage(identity, newParentIdentity)
            {
                ContentType = $"image/{asset.FileProperties.Extension}",
                Title = asset.Name,
                PublishedUrl = asset.FileProperties.PublishedUrl,
                FileContent = asset.File
            };

            return image;
        }
    }
}
