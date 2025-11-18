using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models; // Đảm bảo using Models
using PagedList; // <-- Thư viện để phân trang
using System.Data.Entity; // <-- Thêm cái này để dùng .Include()

namespace WebApplication1.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // -------------------------------------------------
        // --- (ĐÃ SỬA) HÀM INDEX ĐỂ THÊM TÌM KIẾM ---
        // -------------------------------------------------
        // GET: Products
        public ActionResult Index(int? page, string searchString, decimal? minPrice, decimal? maxPrice)
        {
            // 1. Lấy tất cả sản phẩm
            // (Dùng IQueryable để có thể xây dựng truy vấn)
            IQueryable<Product> allProducts = db.Products.Include(p => p.Category)
                                                        .OrderBy(p => p.ProductID);

            // 2. Lọc theo chuỗi tìm kiếm (tên HOẶC mô tả)
            if (!String.IsNullOrEmpty(searchString))
            {
                allProducts = allProducts.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.ProductDescription.Contains(searchString)
                );
            }

            // 3. Lọc theo giá tối thiểu
            if (minPrice.HasValue)
            {
                allProducts = allProducts.Where(p => p.ProductPrice >= minPrice.Value);
            }

            // 4. Lọc theo giá tối đa
            if (maxPrice.HasValue)
            {
                allProducts = allProducts.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            // 5. (QUAN TRỌNG) Lưu lại các giá trị tìm kiếm để hiển thị lại trên View
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            // 6. Cài đặt phân trang
            int pageSize = 8; // 8 sản phẩm một trang
            int pageNumber = (page ?? 1);

            // 7. Trả về View với danh sách ĐÃ LỌC và phân trang
            return View(allProducts.ToPagedList(pageNumber, pageSize));
        }


        // GET: Products/Details/5 (Hàm này của bạn đã có, giữ nguyên)
        public ActionResult Details(int? id)
        {
            // ... (Giữ nguyên code của hàm Details)
            return View();
        }

        // Dispose
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