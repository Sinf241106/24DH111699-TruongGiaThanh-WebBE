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

        // =========================================================
        // HÀM HỖ TRỢ: LƯU ẢNH VÀO SERVER
        // =========================================================
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

        // =========================================================
        // 1. TRANG DANH SÁCH (INDEX)
        // =========================================================
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        // =========================================================
        // 2. BẬT/TẮT TRẠNG THÁI NHANH (AJAX)
        // =========================================================
        [HttpPost]
        public JsonResult ToggleActive(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                db.SaveChanges();
                return Json(new { success = true, isActive = product.IsActive });
            }
            return Json(new { success = false });
        }

        // =========================================================
        // 3. CHI TIẾT SẢN PHẨM (DETAILS)
        // =========================================================
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // =========================================================
        // 4. TẠO MỚI (CREATE)
        // =========================================================
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,Quantity,IsActive")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                string imageUrl = SaveImage(ImageFile);
                if (imageUrl != null) product.ProductImage = imageUrl;

                
                if (product.IsActive == null)
                {
                    product.IsActive = true;
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // =========================================================
        // 5. CHỈNH SỬA (EDIT)
        // =========================================================
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
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,Quantity,IsActive")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.Find(product.ProductID);
                if (existingProduct != null)
                {
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.ProductPrice = product.ProductPrice;
                    existingProduct.ProductDescription = product.ProductDescription;
                    existingProduct.CategoryID = product.CategoryID;

                    // Cập nhật Số lượng và Trạng thái
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.IsActive = product.IsActive;

                    string newImageUrl = SaveImage(ImageFile);
                    if (newImageUrl != null) existingProduct.ProductImage = newImageUrl;

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // =========================================================
        // 6. XÓA (DELETE)
        // =========================================================
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            try
            {
                db.Products.Remove(product);
                db.SaveChanges();
                if (!string.IsNullOrEmpty(product.ProductImage))
                {
                    var path = Path.Combine(Server.MapPath("~"), product.ProductImage.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.Error = "Không thể xóa sản phẩm này vì nó đang có trong đơn hàng!";
                return View("Delete", product);
            }
        }

        // =========================================================
        // [BƯỚC 1] HÀM NÂNG CẤP DATABASE (TẠO CỘT MỚI)
        // =========================================================
        // =========================================================
        // [BƯỚC 1] HÀM NÂNG CẤP DATABASE (PHIÊN BẢN SỬA LỖI TÊN BẢNG)
        // =========================================================
        public ActionResult UpgradeDatabase()
        {
            try
            {
                // MÌNH ĐÃ ĐỔI 'Products' THÀNH 'Product' (Bỏ chữ s)

                // 1. Thêm cột Quantity
                string sql1 = @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'Quantity')
                                BEGIN ALTER TABLE Product ADD Quantity INT DEFAULT 50 WITH VALUES; END";

                // 2. Thêm cột IsActive
                string sql2 = @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'IsActive')
                                BEGIN ALTER TABLE Product ADD IsActive BIT DEFAULT 1 WITH VALUES; END";

                db.Database.ExecuteSqlCommand(sql1);
                db.Database.ExecuteSqlCommand(sql2);

                return Content("✅ THÀNH CÔNG! Đã tìm thấy bảng 'Product' và thêm cột xong. Mời bạn làm tiếp BƯỚC 2 (FixData).");
            }
            catch (Exception ex)
            {
                return Content("❌ VẪN LỖI: " + ex.Message + ". \n\n Bạn hãy mở Server Explorer lên xem chính xác tên bảng của bạn là gì nhé!");
            }
        }

        // =========================================================
        // [BƯỚC 2] HÀM SỬA LỖI HẾT HÀNG (NẠP DỮ LIỆU)
        // =========================================================
        public ActionResult FixData()
        {
            var products = db.Products.ToList();
            Random rand = new Random();
            foreach (var item in products)
            {
                // Random 10-100 cuốn để hết hàng
                item.Quantity = rand.Next(10, 100);
                // Bật trạng thái bán
                item.IsActive = true;
            }
            db.SaveChanges();

            // Quay về danh sách
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}