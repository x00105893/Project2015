using System.Web;
using System.Web.Mvc;

namespace ContactManager
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
            //filters.Add(new System.Web.Mvc.AuthorizeAttribute());
        }
    }
}
