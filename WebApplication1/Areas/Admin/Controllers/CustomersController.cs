using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index()
        {
            // 1. Danh sách tên những người bạn muốn hiển thị
            // (Chỉ hiển thị Trương Gia Thành và Nguyễn Hoàng Phúc)
            var allowedNames = new List<string>
            {
                "Trương Gia Thành",
                "Nguyễn Hoàng Phúc"
            };

            // 2. Lọc dữ liệu từ Database
            // Chỉ lấy những khách hàng có tên nằm trong danh sách allowedNames
            var customers = db.Customers
                              .Where(c => allowedNames.Contains(c.CustomerName))
                              .ToList();

            return View(customers);
        }

        // Hàm AutoInsert để thêm dữ liệu (Giữ nguyên logic cũ để bạn có thể dùng nếu cần)
        public ActionResult AutoInsert()
        {
            var teamMembers = new[]
            {
                new { ID = "24DH111699", Name = "Trương Gia Thành", Phone = "0933490411", Email = "dogadoniss@gmail.com", Address = "291 Trần Hưng Đạo" },
                new { ID = "24DH111453", Name = "Nguyễn Hoàng Phúc", Phone = "0901234567", Email = "phuc@gmail.com", Address = "Đồng Nai" },
                // (Bạn Duy Long vẫn được thêm vào DB nhưng sẽ bị ẩn ở trang Index do bộ lọc ở trên)
                new { ID = "24DH112726", Name = "Nguyễn Trần Duy Long", Phone = "0901234568", Email = "long@gmail.com", Address = "Bình Dương" }
            };

            foreach (var mem in teamMembers)
            {
                // 1. TẠO USER
                var userExists = db.Users.FirstOrDefault(u => u.Username == mem.ID);
                if (userExists == null)
                {
                    var newUser = new User();
                    newUser.Username = mem.ID;
                    newUser.Password = "123456";
                    newUser.UserRole = "C"; // Role 1 ký tự

                    db.Users.Add(newUser);
                    db.SaveChanges();
                }

                // 2. TẠO CUSTOMER
                var custExists = db.Customers.FirstOrDefault(c => c.Username == mem.ID);
                if (custExists == null)
                {
                    var newCust = new Customer();
                    newCust.Username = mem.ID;
                    newCust.CustomerName = mem.Name;
                    newCust.CustomerPhone = mem.Phone;
                    newCust.CustomerEmail = mem.Email;
                    newCust.CustomerAddress = mem.Address;

                    db.Customers.Add(newCust);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }
    }
}