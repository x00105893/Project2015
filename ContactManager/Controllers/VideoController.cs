using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ContactManager.Controllers
{
    public class VideoResult : ActionResult
    {
        /// <summary>
        /// Physical path for video
        /// </summary>
        public string VideoFilePath { get; set; }

        public VideoResult(string path) : base()
        {
            this.VideoFilePath = path;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", " "));
            var file = new FileInfo(VideoFilePath);
            //Check the file exist,  it will be written into the response 
            if (file.Exists)
            {
                var stream = file.OpenRead();
                var bytesinfile = new byte[stream.Length];
                stream.Read(bytesinfile, 0, (int)file.Length);
                context.HttpContext.Response.BinaryWrite(bytesinfile);
            } 
        }
    }

    public class VideoController : Controller
    {
        public VideoResult Video(string path)
        {
            return new VideoResult(path);
        }
        public ActionResult _Video()
        {
            return View();
        }
	}
}