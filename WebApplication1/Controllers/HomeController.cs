using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models; // Đảm bảo tên Project của bạn đúng
using PagedList;
using System.Data.Entity;
using System.Collections.Generic; // Cần thêm cái này để dùng List

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // --- TRANG CHỦ ---
        public ActionResult Index(int? page, string searchString, decimal? minPrice, decimal? maxPrice)
        {
            // 1. QUERY CHO DANH SÁCH CHÍNH (CÓ PHÂN TRANG)
            IQueryable<Product> products = db.Products.Include(p => p.Category).OrderByDescending(p => p.ProductID);

            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString) || p.ProductDescription.Contains(searchString));
            }
            if (minPrice.HasValue) products = products.Where(p => p.ProductPrice >= minPrice.Value);
            if (maxPrice.HasValue) products = products.Where(p => p.ProductPrice <= maxPrice.Value);

            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            // Số lượng sản phẩm ở khung trên (Danh sách chính)
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            // 2. QUERY CHO KHUNG MỚI (TOP 10 SẢN PHẨM MỚI NHẤT)
            // Lấy 10 cái, sắp xếp ID giảm dần -> lưu vào ViewBag
            ViewBag.NewProducts = db.Products.OrderByDescending(p => p.ProductID).Take(10).ToList();

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // --- CÁC HÀM KHÁC GIỮ NGUYÊN ---
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            var similarProducts = db.Products.Where(p => p.CategoryID == product.CategoryID && p.ProductID != id).Take(4).ToList();

            // --- SỬA DÒNG NÀY ---
            // Đổi Take(4) thành Take(10) để lấy 10 sản phẩm -> Danh sách dài ra -> Sẽ hiện thanh cuộn
            var bestSelling = db.Products.OrderByDescending(p => p.ProductID).Take(10).ToList();

            var viewModel = new ProductDetailViewModel
            {
                MainProduct = product,
                SimilarProducts = similarProducts,
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