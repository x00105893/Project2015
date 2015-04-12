using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace ContactManager
{
    public static class Helpers
    {
        /// <summary>
        /// Convert video to MP4
        /// </summary>
        /// <param name="path">path to video</param>
        /// <param name="fileName">name of video</param>
        public static void ConvertVideoToMp4(ref string path, string fileName)
        {
            var ffMpeg = new FFMpegConverter();
            string newPath = Path.GetDirectoryName(path) + "\\"
                + Path.GetFileNameWithoutExtension(fileName) + DateTime.UtcNow.Ticks.ToString() + ".mp4";
            ffMpeg.ConvertMedia(HostingEnvironment.MapPath(path), HostingEnvironment.MapPath(newPath), "mp4");
            System.IO.File.Delete(HostingEnvironment.MapPath(path));
            path = newPath;
        }

        /// <summary>
        /// Conver imag to JPEG
        /// </summary>
        /// <param name="path">path for image</param>
        /// <param name="fileName">name of image</param>
        public static void ConvertImageToJpeg(ref string path, string fileName)
        {
            Image image = Image.FromFile(HostingEnvironment.MapPath(path));
            string newPath = Path.GetDirectoryName(path) + "\\"
                + Path.GetFileNameWithoutExtension(fileName) + DateTime.UtcNow.Ticks.ToString() + ".jpeg";
            image.Save(HostingEnvironment.MapPath(newPath), System.Drawing.Imaging.ImageFormat.Jpeg);
            image.Dispose();
            System.IO.File.Delete(HostingEnvironment.MapPath(path));
            path = newPath;
        }

        /// <summary>
        /// Save thumbnail for video
        /// </summary>
        /// <param name="path">path to video</param>
        public static void SaveVideoThumbnail(string path)
        {
            var ffMpeg = new FFMpegConverter();
            string imagePath = Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileNameWithoutExtension(path) + ".jpeg";
            CheckPath(imagePath);
            ffMpeg.GetVideoThumbnail(HostingEnvironment.MapPath(path), HostingEnvironment.MapPath(imagePath));
        }

        /// <summary>
        /// Save thumbnail for image
        /// </summary>
        /// <param name="path">path to image</param>
        public static void SaveImageThumbnail(string path)
        {
            Image image = Image.FromFile(HostingEnvironment.MapPath(path));
            string imagePath = Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileName(path);
            CheckPath(imagePath);
            Size thumbnailSize = GetThumbnailSize(image);
            Image thumb = image.GetThumbnailImage(thumbnailSize.Width, thumbnailSize.Height, null, IntPtr.Zero);
            thumb.Save(HostingEnvironment.MapPath(imagePath));
            image.Dispose();
            thumb.Dispose();
        }

        /// <summary>
        /// Get optimal thumbnail size
        /// </summary>
        /// <param name="original">Image</param>
        /// <returns></returns>
        private static Size GetThumbnailSize(Image original)
        {
            const int maxPixels = 200;

            int originalWidth = original.Width;
            int originalHeight = original.Height;

            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }

        /// <summary>
        /// Delete file from server
        /// </summary>
        /// <param name="path">path to file</param>
        public static void DeleteFile(string path)
        {
            if (System.IO.File.Exists(HostingEnvironment.MapPath(path)))
                System.IO.File.Delete(HostingEnvironment.MapPath(path));
        }

        /// <summary>
        /// Delete thumbnail for file
        /// </summary>
        /// <param name="path">path to file</param>
        public static void DeleteThumbnail(string path)
        {
            if (System.IO.File.Exists(HostingEnvironment.MapPath(Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileName(path))))
                System.IO.File.Delete(HostingEnvironment.MapPath(Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileName(path)));
        }

        /// <summary>
        /// Check path and create if doesn't exist
        /// </summary>
        /// <param name="path">Path</param>
        public static void CheckPath(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(HostingEnvironment.MapPath(path))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(HostingEnvironment.MapPath(path)));
            }
        }
    }
}