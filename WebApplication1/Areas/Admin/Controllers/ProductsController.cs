using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.IO;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private WebApplication1.Models.MyStoreEntities db = new WebApplication1.Models.MyStoreEntities();

        // --- HÀM HELPER ĐỂ LƯU ẢNH AN TOÀN ---
        private string SaveImage(HttpPostedFileBase imageFile)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var originalFileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + originalFileName;
                    var path = Path.Combine(Server.MapPath("~/Content/Uploads"), uniqueFileName);
                    imageFile.SaveAs(path);
                    return "/Content/Uploads/" + uniqueFileName;
                }
                catch (Exception) { return null; }
            }
            return null;
        }

        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                string imageUrl = SaveImage(ImageFile);
                if (imageUrl != null) product.ProductImage = imageUrl;
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                string newImageUrl = SaveImage(ImageFile);
                if (newImageUrl != null) product.ProductImage = newImageUrl;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // -----------------------------------------------------------------
        // --- ĐÃ SỬA LẠI HÀM NÀY ĐỂ BẮT LỖI XÓA ---
        // -----------------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            try
            {
                // 1. Xóa trong Database trước
                db.Products.Remove(product);
                db.SaveChanges(); // Nếu sản phẩm đã bán, lỗi sẽ xảy ra ở dòng này -> nhảy xuống catch

                // 2. Nếu xóa DB thành công thì mới xóa ảnh (để tránh mất ảnh oan)
                if (!string.IsNullOrEmpty(product.ProductImage))
                {
                    var path = Path.Combine(Server.MapPath("~"), product.ProductImage.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // 3. Nếu có lỗi (do dính khóa ngoại với bảng Order), hiển thị thông báo
                ViewBag.Error = "Không thể xóa sản phẩm này vì nó đang tồn tại trong đơn hàng!";
                return View("Delete", product);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}