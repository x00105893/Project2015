using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContactManager
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Logger.Error(filterContext.Exception.Message, filterContext.Exception);
            base.OnException(filterContext);
        }
    }
}