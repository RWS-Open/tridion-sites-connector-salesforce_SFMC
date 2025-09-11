namespace Sdl.Connectors.Tridion.SalesforceConnector.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Data;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Content;
    using global::Tridion.ConnectorFramework.Contracts.Content.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Exception;
    using global::Tridion.ConnectorFramework.Contracts.Logging;
    using Utils;

    public class DownloadBinaryService : IDownloadBinaryCapability
    {
        private readonly ISalesforceContext context;
        private readonly IConnectorLogger logger;
        private readonly ICreateThumbnailCapability createThumbnailCapability;

        public DownloadBinaryService(ISalesforceContext context, IConnectorLogger logger, ICreateThumbnailCapability createThumbnailCapability)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (createThumbnailCapability != null)
            {
                this.createThumbnailCapability = createThumbnailCapability;
            }
        }

        public async Task<IBinary> DownloadBinary(IBinaryReference binaryReference, IBinaryDownloadOptions options = null, IConnectorContext context = null)
        {
            Binary binary = new Binary();

            this.logger.LogInfo($"{this.GetType().Name}.{nameof(this.DownloadBinary)} Binary reference id: {binaryReference.Id} ");

            if (string.IsNullOrEmpty(binaryReference.Id) || string.IsNullOrEmpty(binaryReference.Type))
            {
                return binary;
            }

            int maxWidth = 0;
            int maxHeight = 0;

            if (options is IMultimediaDownloadOptions multimediaOptions)
            {
                maxWidth = multimediaOptions.MaxWidth;
                maxHeight = multimediaOptions.MaxHeight;
            }

            BinaryReferenceInfo binaryReferenceInfo = this.DecodeBinaryReferenceType(binaryReference);

            switch (binaryReferenceInfo.EntityType)
            {
                case nameof(ContextType.Icon):
                    {
                        int iconSize = maxWidth == 0 ? maxHeight : maxWidth;
                        string fileName = this.GetIconFileName(binaryReference.Id, iconSize);
                        var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                        binary.Stream = stream;
                        binary.ContentLength = (int)new FileInfo(fileName).Length;
                        binary.ContentType = binaryReferenceInfo.ContentType;
                        break;
                    }
                default:
                    {
                        this.logger.LogError($"{this.GetType().Name}.{nameof(this.DownloadBinary)}. No recognized binary reference: {binaryReference}");
                        throw new DownloadBinaryException("No recognized binary reference: " + binaryReference);
                    }
            }

            return binary;
        }

        #region Private Methods

        private string GetFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!File.Exists(path))
            {
                return Path.Combine(Directory.GetParent(this.GetType().Assembly.Location).FullName, "Images", path);
            }

            return path;
        }

        private string GetIconFileName(string name, int size)
        {
            string fileName = Path.GetFileNameWithoutExtension(name);
            string ext = Path.GetExtension(name);

            string newFileName = name?.Replace(fileName + ext, string.Empty);

            if (size <= 16)
            {
                newFileName += $"{fileName}_16{ext}";
            }
            else if (size <= 32)
            {
                newFileName += $"{fileName}_32{ext}";
            }
            else
            {
                newFileName += $"{fileName}_48{ext}";
            }

            newFileName = this.GetFilePath(newFileName);

            return newFileName;
        }

        private BinaryReferenceInfo DecodeBinaryReferenceType(IBinaryReference binaryReference)
        {
            BinaryReferenceInfo binaryReferenceInfo;

            if (binaryReference.Type == nameof(ContextType.Icon))
            {
                binaryReferenceInfo = new BinaryReferenceInfo
                {
                    EntityType = nameof(ContextType.Icon),
                    ContentType = "image/jpg"
                };
            }
            else
            {
                binaryReferenceInfo = Helper.DecodeBinaryReference(binaryReference.Type);
            }

            return binaryReferenceInfo;
        }

        #endregion Private Methods
    }
}
