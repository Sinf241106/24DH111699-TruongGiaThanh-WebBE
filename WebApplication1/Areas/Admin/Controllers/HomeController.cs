using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using WebApplication1.Models; // <-- SỬA NAMESPACE MODEL
// Đảm bảo bạn có file Context Database tên là MyStoreEntities1 trong Models

namespace WebApplication1.Areas.Admin.Controllers // <-- SỬA NAMESPACE CONTROLLER
{
    public class HomeController : Controller
    {
        // (SỬA LỖI 1: Đổi tên kết nối thành MyStoreEntities1)
        // SỬA NAMESPACE TRONG KHAI BÁO CONTEXT
        private WebApplication1.Models.MyStoreEntities db = new WebApplication1.Models.MyStoreEntities();

        // --- (ĐÃ SỬA) HÀM INDEX SẼ TỰ ĐỘNG CHUYỂN HƯỚNG ---
        public ActionResult Index()
        {
            // Tự động chuyển đến trang Quản lý Sản phẩm
            // để tránh lỗi "Object reference not set"
            return RedirectToAction("Index", "Products");
        }
        // --- HẾT SỬA ---


        // --- HÀM TÌM KIẾM (ĐÃ SỬA) ---
        public ActionResult Search(string searchString, int? page) // Thêm 'page'
        {
            var products = from p in db.Products
                           select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                // (SỬA LỖI 2: Xóa ProductDescription vì nó không tồn tại trong Model)
                products = products.Where(p =>
                    p.ProductName.Contains(searchString)
                );
            }
            ViewBag.SearchKey = searchString;

            // Thêm phân trang cho Search
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View("SearchResults", products.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}