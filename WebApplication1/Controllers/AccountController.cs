using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.ViewModel;
using System.Web.Security;
using System.Data.Entity; // Bắt buộc có để dùng .Include()

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // ==========================================
        // 1. ĐĂNG NHẬP (LOGIN)
        // ==========================================
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(
                    u => u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase) &&
                         u.Password == model.Password
                );

                if (user != null)
                {
                    // Tạo cookie xác thực
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    // Lưu thông tin Customer vào Session để dùng sau này
                    var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
                    if (customer != null)
                    {
                        Session["FullName"] = customer.CustomerName;
                        Session["Username"] = customer.Username;
                        Session["CustomerID"] = customer.CustomerID; // Quan trọng để lấy đơn hàng
                    }

                    // Nếu có Url trả về (ví dụ từ trang thanh toán chuyển qua)
                    if (Request.QueryString["ReturnUrl"] != null)
                    {
                        return Redirect(Request.QueryString["ReturnUrl"]);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
        }

        // ==========================================
        // 2. ĐĂNG XUẤT (LOGOUT)
        // ==========================================
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Clear(); // Xóa sạch session
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 3. ĐĂNG KÝ (REGISTER)
        // ==========================================
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng username
                var existingUser = db.Users.FirstOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
                    return View(model);
                }

                // Kiểm tra trùng email
                var existingEmail = db.Customers.FirstOrDefault(c => c.CustomerEmail == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                // Tạo User mới
                var newUser = new User();
                newUser.Username = model.Username;
                newUser.Password = model.Password;
                newUser.UserRole = "C"; // C = Customer
                db.Users.Add(newUser);

                // Tạo Customer mới
                var newCustomer = new Customer();
                newCustomer.CustomerName = model.FullName;
                newCustomer.CustomerEmail = model.Email;
                newCustomer.CustomerPhone = model.Phone;
                newCustomer.CustomerAddress = model.Address;
                newCustomer.Username = newUser.Username;
                db.Customers.Add(newCustomer);

                db.SaveChanges();

                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        // ==========================================
        // 4. XEM LỊCH SỬ ĐƠN HÀNG (MY ORDERS)
        // ==========================================
        public ActionResult MyOrders()
        {
            // Kiểm tra session, nếu mất session bắt đăng nhập lại
            if (Session["CustomerID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerID"];

            // Lấy danh sách đơn hàng kèm chi tiết sản phẩm và thông tin khách
            var myOrders = db.Orders
                             .Include("OrderDetails.Product") // Load chi tiết sp
                             .Include("Customer")             // Load thông tin khách (để hiện email/sđt)
                             .Where(o => o.CustomerID == customerId)
                             .OrderByDescending(o => o.OrderID) // Đơn mới nhất lên đầu
                             .ToList();

            return View(myOrders);
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