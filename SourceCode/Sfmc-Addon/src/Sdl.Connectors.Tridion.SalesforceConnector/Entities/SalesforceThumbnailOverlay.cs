namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using System.IO;
    using global::Tridion.ConnectorFramework.Contracts.Content;

    public class SalesforceThumbnailOverlay : IThumbnailOverlay
    {
        public virtual int X { get; set; }

        public virtual int Y { get; set; }

        public virtual int Width { get; set; }

        public virtual int Height { get; set; }

        public virtual Stream Image { get; set; }
    }
}
