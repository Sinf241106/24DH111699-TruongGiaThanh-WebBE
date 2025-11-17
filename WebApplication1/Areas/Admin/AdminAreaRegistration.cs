using System.Web.Mvc;

namespace WebApplication1.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                // Dòng này rất quan trọng, nó báo cho hệ thống
                // chỉ tìm Controller trong thư mục Admin
                new[] { "WebApplication1.Areas.Admin.Controllers" }
            );
        }
    }
}