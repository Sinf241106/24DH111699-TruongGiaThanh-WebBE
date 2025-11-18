using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.ViewModel;
using System.Web.Security;
using System.Data.Entity;

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

        // --- HÀM LOGIN (POST) ---
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
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
                    if (customer != null)
                    {
                        Session["FullName"] = customer.CustomerName;
                        Session["Username"] = customer.Username;
                        Session["CustomerID"] = customer.CustomerID;
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

        // -------------------------------------------------
        // --- (ĐÃ SỬA) HÀM XEM ĐƠN HÀNG (BILL) ---
        // -------------------------------------------------
        // GET: Account/MyOrders
        public ActionResult MyOrders()
        {
            if (Session["CustomerID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerID"];

            // 3. Lấy tất cả đơn hàng của khách hàng này
            // (ĐÃ SỬA) Thêm OrderByDescending(o => o.OrderDate)
            // (VÀ SỬA LẠI Include)
            var myOrders = db.Orders
                             .Include("OrderDetails.Product")
                             .Include("Customer")
                             .Where(o => o.CustomerID == customerId)
                             .OrderByDescending(o => o.OrderDate) // <-- SẮP XẾP MỚI NHẤT LÊN ĐẦU
                             .ToList();

            return View(myOrders);
        }

        // --- CÁC HÀM KHÁC ---
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

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
                newUser.Password = model.Password;
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