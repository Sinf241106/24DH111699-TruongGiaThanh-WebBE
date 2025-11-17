using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models; // Đảm bảo using Models
using System.Data.Entity;
using System.Net;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // HÀM INDEX (Giữ nguyên như cũ)
        public ActionResult Index()
        {
            var viewModel = new HomeViewModel();
            viewModel.FeaturedProducts = db.Products
                                            .Include(p => p.Category)
                                            .OrderBy(p => p.ProductID)
                                            .Take(6)
                                            .ToList();
            viewModel.NewProducts = db.Products
                                        .Include(p => p.Category)
                                        .OrderBy(p => p.ProductID)
                                        .Skip(6)
                                        .Take(8)
                                        .ToList();
            return View(viewModel);
        }


        // -----------------------------------------------------------------
        // --- (ĐÃ SỬA) HÀM DETAILS ĐỂ DÙNG VIEWMODEL MỚI ---
        // -----------------------------------------------------------------
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 1. Tìm sản phẩm chính
            Product mainProduct = db.Products.Find(id);
            if (mainProduct == null)
            {
                return HttpNotFound(); // Không tìm thấy sản phẩm
            }

            // 2. Tìm sản phẩm tương tự (ví dụ: 8 sản phẩm cùng danh mục)
            var similarProducts = db.Products
                                    .Include(p => p.Category)
                                    .Where(p => p.CategoryID == mainProduct.CategoryID && p.ProductID != id) // Cùng danh mục, khác chính nó
                                    .OrderBy(p => p.ProductID)
                                    .Take(8)
                                    .ToList();

            // 3. Tìm sản phẩm bán chạy (ví dụ: 10 sản phẩm mới nhất)
            var bestSellingProducts = db.Products
                                        .Include(p => p.Category)
                                        .OrderByDescending(p => p.ProductID) // Lấy sản phẩm mới nhất
                                        .Take(10) // <-- (ĐÃ SỬA) TĂNG SỐ LƯỢNG SẢN PHẨM LÊN 10
                                        .ToList();

            // 4. Tạo ViewModel và gán dữ liệu
            var viewModel = new ProductDetailViewModel
            {
                MainProduct = mainProduct,
                SimilarProducts = similarProducts,
                BestSellingProducts = bestSellingProducts
            };

            // Trả về View "Details.cshtml" kèm theo ViewModel
            return View(viewModel);
        }
        // -----------------------------------------------------------------


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