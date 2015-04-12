using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace ContactManager.Models
{
    public enum ResourceType
    { 
        None = 0,
        Video = 1,
        Image = 2,
        Document = 3
    }

    public class Resource
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(256)]
        public string Title { get; set; }

        [Required, StringLength(256)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        [Required, Column(TypeName="int")]
        public ResourceType Type { get; set; }

        [StringLength(512)]
        public string RootPath { get; set; }

        public DateTime UploadedDate { get; set; }

        public virtual ApplicationUser UploadedBy { get; set; }

        public void SaveFile(HttpPostedFileBase file)
        {
            string path = "~/Resources/";
            switch (Type)
            {
                case ResourceType.Document:
                    path += "Documents/";
                    break;
                case ResourceType.Image:
                    path += "Images/";
                    break;
                case ResourceType.Video:
                    path += "Video/";
                    break;
            }
            Helpers.CheckPath(path);    //Check path for resource, if does not exist so create it
            path += file.FileName.Insert(file.FileName.LastIndexOf('.'), DateTime.UtcNow.Ticks.ToString());
            file.SaveAs(HostingEnvironment.MapPath(path)); //Save file on server
            switch (Type)
            {
                case ResourceType.Video:
                    if (!Path.GetExtension(path).ToLower().Equals(".mp4")) // if video has extension not equals .mp4
                        Helpers.ConvertVideoToMp4(ref path, file.FileName);// convert vodeo to .mp4
                    Helpers.SaveVideoThumbnail(path);
                    break;
                case ResourceType.Image:                                    //Convert image to JPEG, except gif
                    if (!Path.GetExtension(path).ToLower().Equals(".gif"))
                        Helpers.ConvertImageToJpeg(ref path, file.FileName);
                    Helpers.SaveImageThumbnail(path);
                    break;
            }
            RootPath = path;
        }
    }
}