using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using PagedList;
using System.Data.Entity;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // ---------------------------------------------------------
        // --- TRANG CHỦ (INDEX) - CẤU TRÚC 3 BOX ---
        // ---------------------------------------------------------
        public ActionResult Index(int? page, string searchString, decimal? minPrice, decimal? maxPrice)
        {
            // 1. Tạo truy vấn cơ bản
            IQueryable<Product> products = db.Products.Include(p => p.Category).OrderByDescending(p => p.ProductID);

            // 2. Logic Tìm kiếm
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString) || p.ProductDescription.Contains(searchString));
            }
            if (minPrice.HasValue) products = products.Where(p => p.ProductPrice >= minPrice.Value);
            if (maxPrice.HasValue) products = products.Where(p => p.ProductPrice <= maxPrice.Value);

            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            // --- PHÂN TRANG (BOX 3: TẤT CẢ SÁCH) ---
            int pageSize = 15; // 15 sách mỗi trang
            int pageNumber = (page ?? 1);

            // --- LẤY DỮ LIỆU CHO CÁC BOX ---

            // Box 1: Sách Mới (Top 10 mới nhất)
            ViewBag.NewProducts = db.Products.OrderByDescending(p => p.ProductID).Take(10).ToList();

            // Box 2: Sách Giá Tốt (Top 10 rẻ nhất)
            ViewBag.CheapProducts = db.Products.OrderBy(p => p.ProductPrice).Take(10).ToList();

            // Trả về danh sách chính (Box 3)
            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // ---------------------------------------------------------
        // --- TRANG CHI TIẾT (DETAILS) - ĐÃ FIX LỖI TRỐNG SẢN PHẨM ---
        // ---------------------------------------------------------
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            // 1. Thử lấy sản phẩm CÙNG DANH MỤC trước (Ưu tiên)
            var similarProducts = db.Products
                                    .Where(p => p.CategoryID == product.CategoryID && p.ProductID != id)
                                    .Take(5)
                                    .ToList();

            // 2. [FIX] LOGIC LẤP ĐẦY KHOẢNG TRỐNG
            // Nếu không tìm thấy cuốn nào cùng loại (List rỗng), thì lấy đại 5 cuốn khác
            if (similarProducts.Count == 0)
            {
                similarProducts = db.Products
                                    .Where(p => p.ProductID != id) // Trừ cuốn đang xem ra
                                    .OrderBy(x => Guid.NewGuid()) // (Mẹo) Lấy ngẫu nhiên
                                    .Take(5)
                                    .ToList();
            }

            // 3. Lấy sản phẩm bán chạy (Cho cột bên phải)
            var bestSelling = db.Products.OrderByDescending(p => p.ProductID).Take(10).ToList();

            var viewModel = new ProductDetailViewModel
            {
                MainProduct = product,
                SimilarProducts = similarProducts, // Giờ chắc chắn sẽ có dữ liệu
                BestSellingProducts = bestSelling
            };
            return View(viewModel);
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
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}