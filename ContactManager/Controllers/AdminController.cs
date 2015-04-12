using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ContactManager.Models;
using System.IO;
using NReco.VideoConverter;
using System.Drawing;
using Newtonsoft.Json;

namespace ContactManager.Controllers
{
    [Authorize(Roles = "canEdit"), CustomHandleError]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Return view for Admin page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        #region Resources
        /// <summary>
        /// Return view for list of resources by type
        /// </summary>
        /// <param name="type">Name of resources type</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return view for editing or creating Resource entity
        /// </summary>
        /// <param name="id">Primary key for resource. Used for editing. For creating mode null</param>
        /// <param name="type">Name of resource type</param>
        /// <returns></returns>
        public ActionResult Resource(int? id, String type)
        {
            if (id.HasValue)
            {
                var resource = db.Resources.Find(id.Value);
                ViewBag.Type = resource.Type;
                switch (resource.Type)                          // set title and filter for upload control by resource type
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

        /// <summary>
        /// Post action for saving resource
        /// </summary>
        /// <param name="model">Data from form</param>
        /// <param name="file">File for resource</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Resource(Resource model, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                ResourceType resType = ResourceType.None;
                if (model.ID == 0)
                {
                    if (file != null)                   // Create new resource
                    {
                        Resource resource = new Resource()
                        {
                            Name = model.Name,
                            Description = model.Description,
                            Title = model.Title,
                            Type = model.Type,
                            UploadedBy = db.Users.ToList().FirstOrDefault(it => it.UserName == User.Identity.Name),
                            UploadedDate = DateTime.UtcNow
                        };
                        resource.SaveFile(file);        // Save file for resource
                        db.Resources.Add(resource);
                        db.SaveChanges();
                        resType = resource.Type;
                    }
                    else
                        ModelState.AddModelError("", "Please, choose a file for upload.");
                }
                else                                    // Update existing resource
                {
                    Resource resource = db.Resources.Find(model.ID);
                    if (resource != null)
                    {
                        if (file != null)
                        {
                            Helpers.DeleteFile(resource.RootPath);
                            Helpers.DeleteThumbnail(resource.RootPath);
                            resource.SaveFile(file);
                        }
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
                else
                    return RedirectToAction("Index");
            }

            return View(model);
        }

        /// <summary>
        /// Delete resource by id
        /// </summary>
        /// <param name="id">Primary key for resource</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult DeleteResource(int? id, String type)
        {
            var res = db.Resources.Find(id);
            ResourceType resType = ResourceType.None;
            if (res != null)
            {
                resType = res.Type;
                Helpers.DeleteFile(res.RootPath);
                Helpers.DeleteThumbnail(res.RootPath);
                db.Resources.Remove(res);
                db.SaveChanges();
            }
            return RedirectToAction("Resources", new { type = resType });
        }
        #endregion

        #region Careers
        /// <summary>
        /// Return View for list of careers
        /// </summary>
        /// <returns></returns>
        public ActionResult Careers()
        {
            return View(db.Careers.ToList());
        }

        /// <summary>
        /// Return View for editing or creating career
        /// </summary>
        /// <param name="id">Primary key for career</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Career(int? id)
        {
            MultiSelectList documents = new MultiSelectList(db.Resources.Where(it => it.Type == ResourceType.Document), "ID", "Name");  // Get documets list
            ViewBag.Documents = documents;
            List<SelectListItem> videos = new List<SelectListItem>();
            videos.Add(new SelectListItem());
            foreach (var video in db.Resources.Where(it => it.Type == ResourceType.Video))                // Get video list
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

        /// <summary>
        /// Save career data
        /// </summary>
        /// <param name="model">Data from form</param>
        /// <param name="form">Form data collection</param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult Career(Career model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Career career = db.Careers.Find(model.ID);
                if (career != null)                             // Update existing career
                {
                    career.Update(career);
                    career.ModifiedBy = db.Users.ToList().FirstOrDefault(it => it.UserName == User.Identity.Name);
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
                    if (!String.IsNullOrEmpty(form["DocumentsID"]))
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
                else                        // Create new career
                {
                    career = new Career(model)
                    {
                        ModifiedBy = db.Users.ToList().FirstOrDefault(it => it.UserName == User.Identity.Name),
                        Documents = new List<Resource>()
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

        /// <summary>
        /// Delete career by id
        /// </summary>
        /// <param name="id">Primary key for career</param>
        /// <returns></returns>
        public ActionResult DeleteCareer(int? id)
        {
            var career = db.Careers.Find(id);
            if (career != null)
            {
                db.Careers.Remove(career);
            }
            return RedirectToAction("Careers");
        }
        #endregion

        #region Tests
        public ActionResult Tests()
        {
            return View(db.Tests.ToList());
        }

        [HttpGet]
        public ActionResult Test(int? id)
        {
            List<SelectListItem> careers = new List<SelectListItem>();
            careers.Add(new SelectListItem());
            foreach (var career in db.Careers)
            {
                careers.Add(new SelectListItem() { Text = career.Name, Value = career.ID.ToString() });
            }
            ViewBag.Careers = careers;
            if (id.HasValue)
            {
                var test = db.Tests.Find(id);
                string g = JsonConvert.SerializeObject(test.Questions);
                if (test != null)
                {
                    if(test.Career != null)
                    {
                        var career = careers.FirstOrDefault(it => it.Value == test.Career.ID.ToString());
                        if (career != null)
                            career.Selected = true;
                        ViewBag.Careers = careers;
                    }
                    return View(test);
                }
            }
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Test(Test model, FormCollection form)
        {
            if (model.ID != 0)
            {
                Test test = db.Tests.Find(model.ID);
                test.Title = model.Title;
                test.ModifiedDate = DateTime.UtcNow;
                foreach (var question in test.Questions)
                {
                    db.QuestionsOptions.RemoveRange(question.Options);
                }
                db.Questions.RemoveRange(test.Questions);
                test.Questions = GetQuestions(form["questions"]);
                if (!String.IsNullOrEmpty(form["CareerID"]))
                {
                    var career = db.Careers.Find(Convert.ToInt32(form["CareerID"]));
                    if (career != null)
                        test.Career = career;
                }
                else
                    test.Career = null;
                db.SaveChanges();
                return RedirectToAction("Tests");
            }
            else
            {
                Test test = new Test()
                {
                    Title = model.Title,
                    ModifiedDate = DateTime.UtcNow
                };
                test.Questions = GetQuestions(form["questions"]);
                if (!String.IsNullOrEmpty(form["CareerID"]))
                {
                    var career = db.Careers.Find(Convert.ToInt32(form["CareerID"]));
                    if (career != null)
                        test.Career = career;
                }
                db.Tests.Add(test);
                db.SaveChanges();
                return RedirectToAction("Tests");
            }
        }

        private List<Question> GetQuestions(string questionsJSON)
        {
            List<Question> questionsList = new List<Question>();
            dynamic questions = JsonConvert.DeserializeObject<List<Question>>(questionsJSON);
            foreach (var question in questions)
            {
                if (question == null)
                    continue;
                Question q = new Question()
                {
                    Text = question.Text,
                    HelpText = question.HelpText,
                    Type = question.Type,
                    Options = new List<QuestionOption>()
                };
                foreach (var option in question.Options)
                {
                    QuestionOption op = new QuestionOption()
                    {
                        Text = ((string)option.Text).Replace("\"","\\\""),
                        IsRight = option.IsRight
                    };
                    q.Options.Add(op);
                }
                questionsList.Add(q);
            }
            return questionsList;
        }

        public ActionResult DeleteTest(int? id)
        {
            return RedirectToAction("Tests");
        }
        #endregion
        /// <summary>
        /// Class for imagebromser plugin used for preview images
        /// </summary>
        public class ImagesJson
        {
            public string image { get; set; }

            public string thumb { get; set; }

            public string folder { get; set; }
        }

        /// <summary>
        /// Return list of ImageJson for preview
        /// </summary>
        /// <returns></returns>
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
