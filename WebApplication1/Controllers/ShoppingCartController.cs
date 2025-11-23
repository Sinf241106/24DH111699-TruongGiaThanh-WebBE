using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Data.Entity;
using System.Net;

namespace WebApplication1.Controllers
{
    public class ShoppingCartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        private const string CartSession = "CartSession";

        // GET: ShoppingCart
        public ActionResult Index()
        {
            var cart = (List<CartItem>)Session[CartSession];
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            // Xóa voucher cũ nếu người dùng quay lại giỏ hàng
            Session["VoucherDiscount"] = null;
            return View(cart);
        }

        // -------------------------------------------------
        // (GET) AddToCart - ĐÃ SỬA: BẮT BUỘC ĐĂNG NHẬP
        // -------------------------------------------------
        public ActionResult AddToCart(int id)
        {
            // [MỚI] Kiểm tra đăng nhập ngay đầu hàm
            if (Session["Username"] == null)
            {
                // Chưa đăng nhập -> Chuyển sang trang Login
                return RedirectToAction("Login", "Account");
            }

            var cart = (List<CartItem>)Session[CartSession];
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            var existingItem = cart.FirstOrDefault(item => item.ProductID == id);
            int quantity = 1;

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var product = db.Products.Find(id);
                if (product != null)
                {
                    cart.Add(new CartItem(product, quantity));
                }
            }
            Session[CartSession] = cart;
            return RedirectToAction("Index");
        }

        // -------------------------------------------------
        // (POST) AddToCart - ĐÃ SỬA: BẮT BUỘC ĐĂNG NHẬP
        // -------------------------------------------------
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            // [MỚI] Kiểm tra đăng nhập ngay đầu hàm
            if (Session["Username"] == null)
            {
                // Chưa đăng nhập -> Chuyển sang trang Login
                return RedirectToAction("Login", "Account");
            }

            var cart = (List<CartItem>)Session[CartSession];
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            var existingItem = cart.FirstOrDefault(item => item.ProductID == id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var product = db.Products.Find(id);
                if (product != null)
                {
                    cart.Add(new CartItem(product, quantity));
                }
            }
            Session[CartSession] = cart;
            return RedirectToAction("Index");
        }

        // RemoveFromCart
        public ActionResult RemoveFromCart(int id)
        {
            var cart = (List<CartItem>)Session[CartSession];
            if (cart != null)
            {
                var itemToRemove = cart.FirstOrDefault(item => item.ProductID == id);
                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);
                }
                Session[CartSession] = cart;
            }
            return RedirectToAction("Index");
        }

        // UpdateCart
        [HttpPost]
        public JsonResult UpdateCart(int id, int quantity)
        {
            var cart = (List<CartItem>)Session[CartSession];
            if (cart != null)
            {
                var itemToUpdate = cart.FirstOrDefault(item => item.ProductID == id);
                if (itemToUpdate != null)
                {
                    itemToUpdate.Quantity = quantity;
                }
                Session[CartSession] = cart;
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // -------------------------------------------------
        // --- HÀM KIỂM TRA VOUCHER ---
        // -------------------------------------------------
        [HttpPost]
        public JsonResult ApplyVoucher(string voucherCode)
        {
            var cart = (List<CartItem>)Session[CartSession];
            decimal totalAmount = (cart != null) ? cart.Sum(c => c.TotalPrice) : 0;
            decimal discountAmount = 0;

            // 1. Kiểm tra mã voucher
            if (voucherCode.Equals("ILoveSinfStore", StringComparison.OrdinalIgnoreCase))
            {
                // 2. Tính 20% giảm giá
                discountAmount = totalAmount * 0.20m; // 20%

                // 3. Lưu tiền giảm giá vào Session
                Session["VoucherDiscount"] = discountAmount;

                decimal newTotal = totalAmount - discountAmount;

                // 4. Trả về kết quả (đã định dạng tiền)
                return Json(new
                {
                    success = true,
                    discount = String.Format("{0:N0}", discountAmount),
                    newTotal = String.Format("{0:N0}", newTotal),
                    message = "Áp dụng voucher thành công!"
                });
            }
            else
            {
                // 5. Nếu voucher sai, xóa Session cũ
                Session["VoucherDiscount"] = null;
                return Json(new
                {
                    success = false,
                    message = "Voucher không hợp lệ."
                });
            }
        }


        // GET: ShoppingCart/Checkout
        public ActionResult Checkout()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "ShoppingCart") });
            }
            var username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(customer);
        }

        // POST: ShoppingCart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(FormCollection form) // Nhận form
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = (List<CartItem>)Session[CartSession];
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);

            // 3. Tạo Đơn hàng (Order) mới
            Order newOrder = new Order();
            newOrder.CustomerID = customer.CustomerID;
            newOrder.OrderDate = DateTime.Now;

            // TÍNH TOÁN TỔNG TIỀN CÓ VOUCHER
            decimal totalAmount = cart.Sum(c => c.TotalPrice);
            // Lấy tiền giảm giá từ Session (nếu có thì dùng, không thì là 0)
            decimal discount = (Session["VoucherDiscount"] as decimal?) ?? 0;
            newOrder.TotalAmount = totalAmount - discount; // Lưu giá cuối cùng

            newOrder.PaymentStatus = "Chưa thanh toán";
            newOrder.AddressDelivery = customer.CustomerAddress;

            db.Orders.Add(newOrder);

            // 4. Thêm Chi tiết đơn hàng (OrderDetail)
            foreach (var item in cart)
            {
                OrderDetail detail = new OrderDetail();
                detail.Order = newOrder;
                detail.ProductID = item.ProductID;
                detail.Quantity = item.Quantity;
                detail.UnitPrice = item.ProductPrice;
                db.OrderDetails.Add(detail);
            }

            // 5. Lưu tất cả thay đổi vào CSDL
            db.SaveChanges();

            // 6. Xóa giỏ hàng VÀ voucher
            Session[CartSession] = null;
            Session["VoucherDiscount"] = null;

            // 7. Chuyển đến trang "Đặt hàng thành công"
            return RedirectToAction("OrderSuccess");
        }

        // GET: ShoppingCart/OrderSuccess
        public ActionResult OrderSuccess()
        {
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