using ContactManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContactManager.Controllers
{
    public class FileController : Controller
    {
        //
        // GET: /File/

        public ActionResult Download(int? id)
        {
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var resource = db.Resources.Find(id);
                if (resource == null || !System.IO.File.Exists(Server.MapPath(resource.RootPath)))
                {
                    return Content("File doesn't exist.");
                }
                else
                {
                    string extension = Path.GetExtension(resource.RootPath);
                    byte[] array = System.IO.File.ReadAllBytes(Server.MapPath(resource.RootPath));
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + resource.Name + extension);
                    return new FileContentResult(array, "application/octet-stream");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

    }
}
