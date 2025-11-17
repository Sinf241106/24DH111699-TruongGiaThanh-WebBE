using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models; // <-- SỬA NAMESPACE MODEL
using System.IO;

// <-- SỬA NAMESPACE CONTROLLER
namespace WebApplication1.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        // SỬA NAMESPACE CHO KHAI BÁO CONTEXT
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

                    // Đảm bảo thư mục Content/Uploads tồn tại
                    var path = Path.Combine(Server.MapPath("~/Content/Uploads"), uniqueFileName);
                    imageFile.SaveAs(path);
                    return "/Content/Uploads/" + uniqueFileName;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
        // --- KẾT THÚC HÀM HELPER ---


        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Gọi hàm lưu ảnh
                string imageUrl = SaveImage(ImageFile);
                if (imageUrl != null)
                {
                    product.ProductImage = imageUrl; // Gán đường dẫn vào model
                }

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

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Gọi hàm lưu ảnh MỚI
                string newImageUrl = SaveImage(ImageFile);
                if (newImageUrl != null)
                {
                    // Nếu có ảnh mới, cập nhật đường dẫn
                    product.ProductImage = newImageUrl;
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // -----------------------------------------------------------------
        // --- HÀM DELETE (GET) ĐÃ ĐƯỢC THÊM VÀO ---
        // -----------------------------------------------------------------
        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            // Trả về View Delete.cshtml để xác nhận xóa
            return View(product);
        }

        // -----------------------------------------------------------------
        // --- HÀM DELETE (POST) ĐÃ ĐƯỢC THÊM VÀO ---
        // -----------------------------------------------------------------
        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);

            // Thêm logic xóa file ảnh khỏi server
            if (!string.IsNullOrEmpty(product.ProductImage))
            {
                try
                {
                    // Lấy đường dẫn vật lý (vd: C:\Projects\...\Content\Uploads\image.jpg)
                    // Chú ý: TrimStart('/') để tránh lỗi Path.Combine
                    var path = Path.Combine(Server.MapPath("~"), product.ProductImage.TrimStart('/'));

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                catch (Exception)
                {
                    // Có thể log lỗi ở đây, nhưng vẫn tiếp tục xóa database
                }
            }

            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        // -----------------------------------------------------------------


        // Cần thêm hàm Dispose để giải phóng tài nguyên
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