using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ContactManager.Models;
using System.IO;

namespace ContactManager.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [AllowAnonymous]
        public ActionResult Resources()
        {
            ViewBag.Message = "Your resources page.";
            ViewBag.Documents = db.Resources.ToList().Where(it => it.Type == ResourceType.Document);
            ViewBag.Video = db.Resources.ToList().Where(it => it.Type == ResourceType.Video);
            ViewBag.Images = db.Resources.ToList().Where(it => it.Type == ResourceType.Image);
            return View();
        }
        [AllowAnonymous]
        public ActionResult Education()
        {
            ViewBag.Message = "Your education page.";

            return View();
        }
        [AllowAnonymous]
        public ActionResult Admin()
        {
            ViewBag.Message = "Your admin page.";

            return View();
        }
        [AllowAnonymous]
        public ActionResult Azcareers()
        {
            ViewBag.Message = "Your a-z careers page.";

            return View(db.Careers.OrderBy(it=>it.Name));
        }

        [AllowAnonymous]
        public ActionResult Career(int? id)
        {
            Career career = db.Careers.Find(id);
            if (career != null)
            {
                if (career.UseLinkForVideo && !String.IsNullOrEmpty(career.VideoLink))
                {
                    ViewBag.Link = career.VideoLink;
                    ViewBag.Thumb = "";
                }
                else if (career.Video != null)
                {
                    ViewBag.Link = career.Video.RootPath.Replace("~", "").Replace("\\", "/");
                    ViewBag.Thumb = Path.GetDirectoryName(career.Video.RootPath).Replace("~", "").Replace("\\", "/") + "/Thumbs/" + Path.GetFileNameWithoutExtension(career.Video.RootPath) + ".jpeg";
                }
                return View(career);
            }
            else
            {
                return RedirectToAction("Azcareers");
            }
        }
    }
}