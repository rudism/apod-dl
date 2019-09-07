using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace apod_dl
{
    public static class Util
    {
        private static Image ResizeImage(Logger logger, Image image, int width, int height)
        {
            if(width == 0 || height == 0) return image;

            logger.Debug($"Resizing image {image.Width}x{image.Height} -> {width}x{height}.");
            var destImage = new Bitmap(width, height);

            try
            {
                var destRect = new Rectangle(0, 0, width, height);

                var hres = image.HorizontalResolution > 0
                    ? image.HorizontalResolution
                    : 72;
                var vres = image.VerticalResolution > 0
                    ? image.VerticalResolution
                    : 72;

                logger.Debug($"Setting output resolution to {hres}x{vres}.");
                destImage.SetResolution(hres, vres);

                using(var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using(var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error("Could not resize image", ex);
            }

            return destImage;
        }

        private static bool NeedsResize(Logger logger, Image image, int? width, int? height, out int newWidth, out int newHeight)
        {
            newWidth = 0;
            newHeight = 0;

            if(!width.HasValue || !height.HasValue)
            {
                logger.Debug("No dimensions given, skipping resize.");
                return false;
            }

            var wratio = width.Value / (double) image.Width;
            var hratio = height.Value / (double) image.Height;

            if(wratio > 1 || hratio > 1)
            {
                logger.Debug($"Image dimension already fits ({image.Width}x{image.Height}), not resizing.");
                return false;
            }

            var useRatio = wratio > hratio ? wratio : hratio;
            newWidth = (int) Math.Round(image.Width * useRatio);
            newHeight = (int) Math.Round(image.Height * useRatio);

            return true;
        }

        public static void SaveImage(Logger logger, string dest, byte[] raw, int? width, int? height)
        {
            using(var mstream = new MemoryStream(raw))
            using(var image = Image.FromStream(mstream))
            {
                int newWidth, newHeight;
                using(var destImage = NeedsResize(logger, image, width, height, out newWidth, out newHeight)
                    ? ResizeImage(logger, image, newWidth, newHeight)
                    : image)
                {
                    try
                    {
                        destImage.Save(dest, ImageFormat.Jpeg);
                    }
                    catch(Exception ex)
                    {
                        logger.Error($"Could not save image {dest}", ex, false);
                    }
                    logger.Log($"Saved image {dest}.");
                }
            }
        }
    }
}
