using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.ViewModel;
using System.Web.Security; // <-- (MỚI) Thêm thư viện này

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

        // -------------------------------------------------
        // --- ĐĂNG KÝ ---
        // -------------------------------------------------
        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
                    return View(model);
                }
                var existingEmail = db.Customers.FirstOrDefault(c => c.CustomerEmail == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                var newUser = new User();
                newUser.Username = model.Username;
                newUser.Password = model.Password; // (Cần mã hóa)
                newUser.UserRole = "C";

                db.Users.Add(newUser);

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

        // -------------------------------------------------
        // --- (ĐÃ SỬA) ĐĂNG NHẬP ---
        // -------------------------------------------------
        // GET: Account/Login
        public ActionResult Login()
        {
            // Trả về View Login.cshtml
            return View();
        }

        // POST: Account/Login (Hàm xử lý khi nhấn nút "Đăng nhập")
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm người dùng trong CSDL (bảng User)
                // (Giả sử bạn chưa mã hóa mật khẩu)
                var user = db.Users.FirstOrDefault(
                    u => u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase) &&
                         u.Password == model.Password
                );

                // 2. Nếu tìm thấy (tài khoản hợp lệ)
                if (user != null)
                {
                    // 3. Tạo "vé" xác thực (Authentication Ticket)
                    FormsAuthentication.SetAuthCookie(user.Username, false); // false = không nhớ

                    // (Tùy chọn) Lưu thông tin Khách hàng vào Session
                    var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
                    if (customer != null)
                    {
                        Session["FullName"] = customer.CustomerName;
                        Session["Username"] = customer.Username;
                    }

                    // 4. Chuyển hướng về trang chủ
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // 5. Nếu sai, báo lỗi
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            // Nếu không hợp lệ (lỗi validation) hoặc đăng nhập sai,
            // trả về View Login kèm thông báo lỗi
            return View(model);
        }

        // -------------------------------------------------
        // --- (MỚI) ĐĂNG XUẤT ---
        // -------------------------------------------------
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut(); // Xóa vé
            Session.Clear(); // Xóa hết Session (như FullName)
            return RedirectToAction("Index", "Home"); // Về trang chủ
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