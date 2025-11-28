using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Data.Entity;
using System.Net;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // =========================================================
        // 1. DANH SÁCH ĐƠN HÀNG
        // =========================================================
        // GET: Admin/Orders
        public ActionResult Index()
        {
            // Lấy danh sách đơn hàng, kèm thông tin khách hàng
            // Sắp xếp: Đơn mới nhất lên đầu
            var orders = db.Orders
                           .Include(o => o.Customer)
                           .OrderByDescending(o => o.OrderDate)
                           .ToList();

            return View(orders);
        }

        // =========================================================
        // 2. CHI TIẾT ĐƠN HÀNG
        // =========================================================
        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Lấy đơn hàng kèm Customer và danh sách sản phẩm
            var order = db.Orders
                          .Include("Customer")
                          .Include("OrderDetails.Product")
                          .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return HttpNotFound();

            return View(order);
        }

        // =========================================================
        // 3. [MỚI] XÁC NHẬN THANH TOÁN (MARK AS PAID)
        // =========================================================
        [HttpPost] // Chỉ nhận yêu cầu POST để bảo mật
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsPaid(int id)
        {
            // Tìm đơn hàng theo ID
            var order = db.Orders.Find(id);

            if (order != null)
            {
                // Chỉ xử lý nếu đơn hàng CHƯA thanh toán
                if (order.PaymentStatus != "Đã thanh toán")
                {
                    // Cập nhật trạng thái
                    order.PaymentStatus = "Đã thanh toán";

                    // Lưu vào CSDL
                    db.SaveChanges();

                    // Gửi thông báo thành công ra giao diện (nhớ thêm code hiển thị trong View)
                    TempData["Success"] = $"Đã xác nhận thanh toán thành công cho đơn #{id}!";
                }
            }

            // Quay lại trang danh sách để thấy thay đổi
            return RedirectToAction("Index");
        }

        // Giải phóng tài nguyên
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}