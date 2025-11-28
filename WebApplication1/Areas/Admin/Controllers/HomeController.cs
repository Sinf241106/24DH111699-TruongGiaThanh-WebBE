using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private WebApplication1.Models.MyStoreEntities db = new WebApplication1.Models.MyStoreEntities();

        // ----------------------------------------------------
        // --- (ĐÃ SỬA) HÀM INDEX: CHUYỂN HƯỚNG VỀ DASHBOARD ---
        // ----------------------------------------------------
        public ActionResult Index()
        {
            // Khi vào Admin, tự động nhảy sang trang Dashboard (Biểu đồ)
            return RedirectToAction("Dashboard");
        }

        // ----------------------------------------------------
        // --- (MỚI) HÀM DASHBOARD: CHỨA SỐ LIỆU GIẢ ---
        // ----------------------------------------------------
        public ActionResult Dashboard()
        {
            // Số liệu giả định (Bạn có thể sửa số ở đây cho đẹp)
            ViewBag.TotalRevenue = 4591854925; // Doanh thu
            ViewBag.TotalOrders = 15;          // Đơn hàng
            ViewBag.TotalCustomers = 8;       // Khách hàng
            ViewBag.TotalProducts = 27;        // Sản phẩm

            // Trả về View Dashboard.cshtml mà chúng ta đã tạo
            return View();
        }

        // ----------------------------------------------------
        // --- CÁC HÀM CŨ (GIỮ NGUYÊN ĐỂ KHÔNG LỖI) ---
        // ----------------------------------------------------
        public ActionResult Search(string searchString, int? page)
        {
            var products = from p in db.Products select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString));
            }
            ViewBag.SearchKey = searchString;

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