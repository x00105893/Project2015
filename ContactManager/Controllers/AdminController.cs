using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ContactManager.Models;
using System.IO;
using NReco.VideoConverter;
using System.Drawing;

namespace ContactManager.Controllers
{
    [Authorize(Roles = "canEdit"), CustomHandleError]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //
        // GET: /Admin/
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Resources(String type)
        {
            if (type != null)
            {
                ResourceType resType = (ResourceType)Enum.Parse(typeof(ResourceType), type);
                switch (resType)
                {
                    case ResourceType.Document:
                        ViewBag.Title = "Documents (PDF)";
                        break;
                    case ResourceType.Image:
                        ViewBag.Title = "Images";
                        break;
                    case ResourceType.Video:
                        ViewBag.Title = "Video";
                        break;
                }
                ViewBag.Type = resType;
                return View(db.Resources.ToList().Where(it => it.Type == resType));
            }
            else
            {
                ViewBag.Title = "Resources";
                ViewBag.Type = ResourceType.None;
                return View(db.Resources.ToList());
            }
        }

        public ActionResult Resource(int? id, String type)
        {
            if (id.HasValue)
            {
                var resource = db.Resources.Find(id.Value);
                ViewBag.Type = resource.Type;
                switch (resource.Type)
                {
                    case ResourceType.Document:
                        ViewBag.Accept = "application/pdf";
                        ViewBag.Title = "Documents (PDF)";
                        break;
                    case ResourceType.Image:
                        ViewBag.Accept = "image/*";
                        ViewBag.Title = "Images";
                        break;
                    case ResourceType.Video:
                        ViewBag.Accept = "video/*";
                        ViewBag.Title = "Video";
                        break;
                }
                return View(resource);
            }
            else
            {
                if (type != null)
                {
                    ResourceType resType = (ResourceType)Enum.Parse(typeof(ResourceType), type);
                    switch (resType)
                    {
                        case ResourceType.Document:
                            ViewBag.Accept = "application/pdf";
                            ViewBag.Title = "Documents (PDF)";
                            break;
                        case ResourceType.Image:
                            ViewBag.Accept = "image/*";
                            ViewBag.Title = "Images";
                            break;
                        case ResourceType.Video:
                            ViewBag.Accept = "video/*";
                            ViewBag.Title = "Video";
                            break;
                    }
                    ViewBag.Type = resType;
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Resource(Resource model, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                ResourceType resType = ResourceType.None;
                if (model.ID == 0)
                {
                    if (file != null)
                    {
                        string path = SaveFile(file, model.Type);
                        Resource resource = new Resource()
                        {
                            Name = model.Name,
                            Description = model.Description,
                            RootPath = path,
                            Title = model.Title,
                            Type = model.Type,
                            UploadedBy = db.Users.ToList().FirstOrDefault(it => it.UserName == User.Identity.Name),
                            UploadedDate = DateTime.UtcNow
                        };
                        db.Resources.Add(resource);
                        db.SaveChanges();
                        resType = resource.Type;
                    }
                    else
                        ModelState.AddModelError("", "Please, choose a file for upload.");
                }
                else
                { 
                    Resource resource = db.Resources.Find(model.ID);
                    if (resource != null)
                    {
                        string path = String.Empty;
                        if (file != null)
                        {
                            DeleteFile(resource.RootPath);
                            DeleteThumbnail(resource.RootPath);
                            path = SaveFile(file, resource.Type);
                        }
                        resource.RootPath = path;
                        resource.Name = model.Name;
                        resource.Description = model.Description;
                        resource.Title = model.Title;
                        resource.UploadedBy = db.Users.ToList().FirstOrDefault(it => it.UserName == User.Identity.Name);
                        resource.UploadedDate = DateTime.UtcNow;
                        db.SaveChanges();
                        resType = resource.Type;
                    }
                }
                if (resType != ResourceType.None)
                    return RedirectToAction("Resources", new { type = resType });
            }

            return View(model);
        }

        public ActionResult DeleteResource(int? id, String type)
        {
            var res = db.Resources.Find(id);
            ResourceType resType = ResourceType.None;
            if (res != null)
            {
                resType = res.Type;
                DeleteFile(res.RootPath);
                DeleteThumbnail(res.RootPath);
                db.Resources.Remove(res);
                db.SaveChanges();
            }
            return RedirectToAction("Resources", new { type = resType });
        }

        private string SaveFile(HttpPostedFileBase file, ResourceType type)
        {
            string path = "~/Resources/";
            switch (type)
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
            CheckPath(path);
            path += file.FileName.Insert(file.FileName.LastIndexOf('.'), DateTime.UtcNow.Ticks.ToString());
            file.SaveAs(Server.MapPath(path));
            switch (type)
            {
                case ResourceType.Video:
                    if (!Path.GetExtension(path).ToLower().Equals(".mp4"))
                        ConvertVideoToMp4(ref path, file.FileName);
                    SaveVideoThumbnail(path);
                    break;
                case ResourceType.Image:
                    if (!Path.GetExtension(path).ToLower().Equals(".gif"))
                        ConvertImageToJpeg(ref path, file.FileName);
                    SaveImageThumbnail(path);
                    break;
            }
            return path;
        }

        private void ConvertVideoToMp4(ref string path, string fileName)
        {
            var ffMpeg = new FFMpegConverter();
            string newPath = Path.GetDirectoryName(path) + "\\"
                + Path.GetFileNameWithoutExtension(fileName) + DateTime.UtcNow.Ticks.ToString() + ".mp4";
            ffMpeg.ConvertMedia(Server.MapPath(path), Server.MapPath(newPath), "mp4");
            System.IO.File.Delete(Server.MapPath(path));
            path = newPath;
        }

        private void ConvertImageToJpeg(ref string path, string fileName)
        {
            Image image = Image.FromFile(Server.MapPath(path));
            string newPath = Path.GetDirectoryName(path) + "\\"
                + Path.GetFileNameWithoutExtension(fileName) + DateTime.UtcNow.Ticks.ToString() + ".jpeg";
            image.Save(Server.MapPath(newPath), System.Drawing.Imaging.ImageFormat.Jpeg);
            image.Dispose();
            System.IO.File.Delete(Server.MapPath(path));
            path = newPath;
        }

        private void SaveVideoThumbnail(string path)
        {
            var ffMpeg = new FFMpegConverter();
            string imagePath = Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileNameWithoutExtension(path) + ".jpeg";
            CheckPath(imagePath);
            ffMpeg.GetVideoThumbnail(Server.MapPath(path), Server.MapPath(imagePath));
        }

        private void SaveImageThumbnail(string path)
        {
            Image image = Image.FromFile(Server.MapPath(path));
            string imagePath = Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileName(path);
            CheckPath(imagePath);
            Size thumbnailSize = GetThumbnailSize(image);
            Image thumb = image.GetThumbnailImage(thumbnailSize.Width, thumbnailSize.Height, null, IntPtr.Zero);
            thumb.Save(Server.MapPath(imagePath));
            image.Dispose();
            thumb.Dispose();
        }

        static Size GetThumbnailSize(Image original)
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

        private void DeleteFile(string path)
        {
            if (System.IO.File.Exists(Server.MapPath(path)))
                System.IO.File.Delete(Server.MapPath(path));
        }

        private void DeleteThumbnail(string path)
        {
            if (System.IO.File.Exists(Server.MapPath(Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileName(path))))
                System.IO.File.Delete(Server.MapPath(Path.GetDirectoryName(path) + "\\Thumbs\\" + Path.GetFileName(path)));
        }

        private void CheckPath(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(Server.MapPath(path))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Server.MapPath(path)));
            }
        }

        public ActionResult Careers()
        {
            return View(db.Careers.ToList());
        }

        [HttpGet]
        public ActionResult Career(int? id)
        {
            MultiSelectList documents = new MultiSelectList(db.Resources.Where(it => it.Type == ResourceType.Document), "ID", "Name");
            ViewBag.Documents = documents;
            List<SelectListItem> videos = new List<SelectListItem>();
            videos.Add(new SelectListItem());
            foreach (var video in db.Resources.Where(it=>it.Type == ResourceType.Video))
            {
                videos.Add(new SelectListItem() { Text = video.Name, Value = video.ID.ToString() });
            }
            ViewBag.Video = videos;
            if (id == null)
                return View(new Career() { ID = 0 });
            else
            {
                Career career = db.Careers.Find(id);
                if (career != null)
                {
                    if (!career.UseLinkForVideo && career.Video != null)
                    {
                        var v = videos.FirstOrDefault(it => it.Value == career.Video.ID.ToString());
                        if (v != null)
                            v.Selected = true;
                        ViewBag.Video = videos;

                    }
                    if(career.Documents != null && career.Documents.Count != 0)
                    {
                        documents = new MultiSelectList(db.Resources.Where(it => it.Type == ResourceType.Document), "ID", "Name", career.Documents.Select(it=>it.ID));
                        ViewBag.Documents = documents;
                    }
                    return View(career);
                }
            }

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Career(Career model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Career career = db.Careers.Find(model.ID);
                if (career != null)
                {
                    career.CareerPath = model.CareerPath;
                    career.Description = model.Description;
                    career.KeySkills = model.KeySkills;
                    career.Name = model.Name;
                    career.UseLinkForVideo = model.UseLinkForVideo;
                    career.VideoLink = model.VideoLink;
                    career.WhatDo = model.WhatDo;
                    career.WhoHire = model.WhoHire;
                    career.ModifiedBy = db.Users.ToList().FirstOrDefault(it => it.UserName == User.Identity.Name);
                    career.ModifiedDate = DateTime.UtcNow;
                    if (!String.IsNullOrEmpty(form["VideoID"]))
                    {
                        var video = db.Resources.Find(Convert.ToInt32(form["VideoID"]));
                        if (video != null)
                            career.Video = video;
                    }
                    else
                    {
                        career.Video = null;
                    }
                    if(!String.IsNullOrEmpty(form["DocumentsID"]))
                    {
                        career.Documents.Clear();
                        string[] ids = form["DocumentsID"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string id in ids)
                        {
                            Resource res = db.Resources.Find(Convert.ToInt32(id));
                            if (res != null) 
                                career.Documents.Add(res);
                        }
                    }
                    else
                    {
                        career.Documents.Clear();
                    }
                    db.SaveChanges();
                    return RedirectToAction("Careers");
                }
                else
                {
                    career = new Career()
                    {
                        CareerPath = model.CareerPath,
                        Description = model.Description,
                        KeySkills = model.KeySkills,
                        Name = model.Name,
                        UseLinkForVideo = model.UseLinkForVideo,
                        VideoLink = model.VideoLink,
                        WhatDo = model.WhatDo,
                        WhoHire = model.WhoHire,
                        ModifiedBy = db.Users.ToList().FirstOrDefault(it => it.UserName == User.Identity.Name),
                        ModifiedDate = DateTime.UtcNow
                    };
                    if (!String.IsNullOrEmpty(form["VideoID"]))
                    {
                        var video = db.Resources.Find(Convert.ToInt32(form["VideoID"]));
                        if (video != null)
                            career.Video = video;
                    }
                    if (!String.IsNullOrEmpty(form["DocumentsID"]))
                    {
                        string[] ids = form["DocumentsID"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string id in ids)
                        {
                            Resource res = db.Resources.Find(Convert.ToInt32(id));
                            if (res != null)
                                career.Documents.Add(res);
                        }
                    }
                    db.Careers.Add(career);
                    db.SaveChanges();
                    return RedirectToAction("Careers");
                }
            }
            return View(model);
        }

        public class ImagesJson
        {
            public string image { get; set; }

            public string thumb { get; set; }

            public string folder { get; set; }
        }
        public ActionResult Images()
        {
            List<ImagesJson> images = new List<ImagesJson>();
            string imagesPath = Server.MapPath("/Resources/Images");
            if (System.IO.Directory.Exists(imagesPath))
            {
                foreach (var imageFile in System.IO.Directory.EnumerateFiles(imagesPath))
                {
                    if(System.IO.File.Exists(Server.MapPath("/Resources/Images/" + Path.GetFileName(imageFile))))
                        images.Add(new ImagesJson()
                        {
                            image = "/Resources/Images/" + Path.GetFileName(imageFile),
                            thumb = System.IO.File.Exists(Server.MapPath("/Resources/Images/Thumbs/" + Path.GetFileName(imageFile))) ? "/Resources/Images/Thumbs/" + Path.GetFileName(imageFile) :
                                "/Resources/Images/" + Path.GetFileName(imageFile)
                        });
                }
            }
            return Json(images, JsonRequestBehavior.AllowGet);
        }
    }
}
