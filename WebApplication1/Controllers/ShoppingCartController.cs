using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

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
            if (cart == null) { cart = new List<CartItem>(); }
            return View(cart);
        }

        // =================================================
        // 1. ADD TO CART (ĐÃ SỬA: CHẶN NẾU SẢN PHẨM ẨN)
        // =================================================
        public ActionResult AddToCart(int id)
        {
            // 1. Kiểm tra sản phẩm trong DB trước
            var product = db.Products.Find(id);

            // [QUAN TRỌNG] Nếu SP không tồn tại HOẶC đang bị ẨN (IsActive = false)
            if (product == null || product.IsActive == false)
            {
                // Đá về trang chủ, không cho mua
                return RedirectToAction("Index", "Home");
            }

            // 2. Kiểm tra đăng nhập
            if (Session["Username"] == null) return RedirectToAction("Login", "Account");

            var cart = (List<CartItem>)Session[CartSession] ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(item => item.ProductID == id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                // Thêm sản phẩm vào giỏ
                cart.Add(new CartItem(product, 1));
            }
            Session[CartSession] = cart;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            // Logic tương tự cho POST
            var product = db.Products.Find(id);
            if (product == null || product.IsActive == false) return RedirectToAction("Index", "Home");

            if (Session["Username"] == null) return RedirectToAction("Login", "Account");

            var cart = (List<CartItem>)Session[CartSession] ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(item => item.ProductID == id);

            if (existingItem != null)
                existingItem.Quantity += quantity;
            else
                cart.Add(new CartItem(product, quantity));

            Session[CartSession] = cart;
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = (List<CartItem>)Session[CartSession];
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.ProductID == id);
                if (item != null) cart.Remove(item);
                Session[CartSession] = cart;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult UpdateCart(int id, int quantity)
        {
            var cart = (List<CartItem>)Session[CartSession];
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.ProductID == id);
                if (item != null) item.Quantity = quantity;
                Session[CartSession] = cart;
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult ApplyVoucher(string voucherCode)
        {
            var cart = (List<CartItem>)Session[CartSession];
            decimal total = (cart != null) ? cart.Sum(c => c.TotalPrice) : 0;

            if (voucherCode.Equals("ILoveSinfStore", StringComparison.OrdinalIgnoreCase))
            {
                decimal discount = total * 0.20m;
                Session["VoucherDiscount"] = discount;
                return Json(new { success = true, discount = String.Format("{0:N0}", discount), newTotal = String.Format("{0:N0}", total - discount), message = "Áp dụng thành công!" });
            }
            else
            {
                Session["VoucherDiscount"] = null;
                return Json(new { success = false, message = "Voucher không hợp lệ." });
            }
        }

        public ActionResult Checkout()
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout") });

            var username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);

            if (customer == null) return RedirectToAction("Index", "Home");

            var cart = (List<CartItem>)Session[CartSession];
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(FormCollection form)
        {
            if (Session["Username"] == null) return RedirectToAction("Login", "Account");
            return RedirectToAction("Payment");
        }

        public ActionResult Payment()
        {
            if (Session["Username"] == null) return RedirectToAction("Login", "Account");

            var cart = (List<CartItem>)Session[CartSession];
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            decimal totalAmount = cart.Sum(item => item.TotalPrice);
            decimal discount = (Session["VoucherDiscount"] as decimal?) ?? 0;
            decimal finalTotal = totalAmount - discount;

            ViewBag.FinalTotal = finalTotal;
            ViewBag.OrderCode = "DH" + DateTime.Now.ToString("ddHHmmss");

            return View();
        }

        // =================================================
        // 2. CONFIRM PAYMENT (ĐÃ XÓA TRỪ KHO)
        // =================================================
        [HttpPost]
        public ActionResult ConfirmPayment(string paymentMethod)
        {
            if (Session["Username"] == null) return RedirectToAction("Login", "Account");
            var cart = (List<CartItem>)Session[CartSession];
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            var username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);

            // A. Lưu Order
            Order newOrder = new Order();
            newOrder.CustomerID = customer.CustomerID;
            newOrder.OrderDate = DateTime.Now;
            newOrder.AddressDelivery = customer.CustomerAddress;

            decimal totalAmount = cart.Sum(c => c.TotalPrice);
            decimal discount = (Session["VoucherDiscount"] as decimal?) ?? 0;
            newOrder.TotalAmount = totalAmount - discount;

            if (paymentMethod == "BANKING")
                newOrder.PaymentStatus = "Chờ duyệt (Chuyển khoản)";
            else
                newOrder.PaymentStatus = "COD (Chưa thanh toán)";

            db.Orders.Add(newOrder);
            db.SaveChanges();

            // B. Lưu OrderDetail (KHÔNG TRỪ KHO NỮA)
            foreach (var item in cart)
            {
                OrderDetail detail = new OrderDetail();
                detail.OrderID = newOrder.OrderID;
                detail.ProductID = item.ProductID;
                detail.Quantity = item.Quantity;
                detail.UnitPrice = item.ProductPrice;
                db.OrderDetails.Add(detail);

                // --- ĐÃ XÓA ĐOẠN TRỪ KHO Ở ĐÂY ---
            }

            db.SaveChanges();

            Session[CartSession] = null;
            Session["VoucherDiscount"] = null;

            return RedirectToAction("OrderSuccess");
        }

        public ActionResult OrderSuccess()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}