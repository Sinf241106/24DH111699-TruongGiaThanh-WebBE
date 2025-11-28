using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Save image and return virtual path (or null on failure)
        private string SaveImage(HttpPostedFileBase imageFile)
        {
            if (imageFile == null || imageFile.ContentLength == 0)
                return null;

            try
            {
                var uploadsDir = Server.MapPath("~/Content/Uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var originalFileName = Path.GetFileName(imageFile.FileName);
                var uniqueFileName = Guid.NewGuid().ToString("N") + "_" + originalFileName;
                var path = Path.Combine(uploadsDir, uniqueFileName);

                imageFile.SaveAs(path);

                // Return path usable with Url.Content
                return "/Content/Uploads/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SaveImage error: " + ex);
                return null;
            }
        }

        // GET: Admin/Categories
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Admin/Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category, HttpPostedFileBase ImageFile)
        {
            // fallback to Request.Files if needed
            var fileToSave = ImageFile;
            if ((fileToSave == null || fileToSave.ContentLength == 0) && Request.Files.Count > 0)
            {
                var posted = Request.Files["ImageFile"];
                if (posted != null && posted.ContentLength > 0) fileToSave = posted;
            }

            if (ModelState.IsValid)
            {
                if (fileToSave != null && fileToSave.ContentLength > 0)
                {
                    var imageUrl = SaveImage(fileToSave);
                    if (imageUrl == null)
                    {
                        ModelState.AddModelError("ImageFile", "Image upload failed. Check folder permissions and server logs.");
                        return View(category);
                    }
                    category.ImageUrl = imageUrl;
                }

                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, HttpPostedFileBase ImageFile, [Bind(Include = "CategoryName")] Category categoryFormData)
        {
            var categoryToUpdate = db.Categories.Find(id);
            if (categoryToUpdate == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                categoryToUpdate.CategoryName = categoryFormData.CategoryName;

                var fileToSave = ImageFile;
                if ((fileToSave == null || fileToSave.ContentLength == 0) && Request.Files.Count > 0)
                {
                    var posted = Request.Files["ImageFile"];
                    if (posted != null && posted.ContentLength > 0) fileToSave = posted;
                }

                if (fileToSave != null && fileToSave.ContentLength > 0)
                {
                    var newImageUrl = SaveImage(fileToSave);
                    if (newImageUrl == null)
                    {
                        ModelState.AddModelError("ImageFile", "Image upload failed. Check folder permissions and server logs.");
                        return View(categoryToUpdate);
                    }

                    // delete old image file (optional)
                    try
                    {
                        if (!string.IsNullOrEmpty(categoryToUpdate.ImageUrl))
                        {
                            var oldPath = Server.MapPath(categoryToUpdate.ImageUrl);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                    }
                    catch { }

                    categoryToUpdate.ImageUrl = newImageUrl;
                }

                db.Entry(categoryToUpdate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(categoryToUpdate);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            // --- [MỚI] KIỂM TRA RÀNG BUỘC: KHÔNG CHO XÓA NẾU CÒN SẢN PHẨM ---
            // Đếm số sản phẩm thuộc danh mục này
            int productCount = db.Products.Count(p => p.CategoryID == id);

            if (productCount > 0)
            {
                // Nếu có sản phẩm, báo lỗi và không xóa
                // Sử dụng TempData để truyền thông báo lỗi sang View Index
                TempData["Error"] = $"Không thể xóa danh mục '{category.CategoryName}' vì đang có {productCount} sản phẩm thuộc danh mục này. Vui lòng xóa sản phẩm trước.";
                return RedirectToAction("Index");
            }

            // Nếu không có sản phẩm nào, tiến hành xóa ảnh danh mục (nếu có)
            try
            {
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    var physicalPath = Server.MapPath(category.ImageUrl);
                    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
                }
            }
            catch { }

            // Xóa danh mục
            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa danh mục thành công!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}