using System.Web.Mvc;

namespace ContactManager.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Error()
        {
            return View();
        }

        public ActionResult NoPageFound(string aspxerrorpath)
        {
            ViewBag.Url = Request.Url.Host + aspxerrorpath;
            Response.StatusCode = 404;
            return View();
        }
	}
}