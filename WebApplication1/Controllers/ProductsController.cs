using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebApplication1.Models; // <-- SỬA NAMESPACE MODEL
using PagedList;

namespace WebApplication1.Controllers // <-- SỬA NAMESPACE CONTROLLER
{
    public class ProductsController : Controller
    {
        // SỬA TÊN CONTEXT CHÍNH XÁC
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Products (Đây là trang Bán Hàng)
        public ActionResult Index(int? page)
        {
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var products = db.Products.OrderByDescending(p => p.ProductID);
            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products
                                 .Include(p => p.Category)
                                 .FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var sidebarProducts = db.Products
                                         .Where(p => p.ProductID != id)
                                         .OrderByDescending(p => p.ProductID)
                                         .Take(3)
                                         .ToList();
            ViewBag.SidebarProducts = sidebarProducts;
            return View(product);
        }

        // Hàm Dispose
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