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
            var customers = db.Customers.ToList();
            return View(customers);
        }

        public ActionResult AutoInsert()
        {
            // Danh sách 3 thành viên
            var teamMembers = new[]
            {
                new { ID = "24DH111699", Name = "Trương Gia Thành", Phone = "0933490411", Email = "dogadoniss@gmail.com", Address = "291 Trần Hưng Đạo" },
                new { ID = "24DH111453", Name = "Nguyễn Hoàng Phúc", Phone = "0901234567", Email = "phuc@gmail.com", Address = "Đồng Nai" },
                new { ID = "24DH112726", Name = "Nguyễn Trần Duy Long", Phone = "0901234568", Email = "long@gmail.com", Address = "Bình Dương" }
            };

            foreach (var mem in teamMembers)
            {
                // 1. TẠO TÀI KHOẢN (USER)
                var userExists = db.Users.FirstOrDefault(u => u.Username == mem.ID);
                if (userExists == null)
                {
                    var newUser = new User();
                    newUser.Username = mem.ID;
                    newUser.Password = "123456";

                    // [QUAN TRỌNG] Đã sửa thành chữ "C" (1 ký tự) để không bị lỗi
                    newUser.UserRole = "C";

                    db.Users.Add(newUser);
                    db.SaveChanges();
                }

                // 2. TẠO KHÁCH HÀNG (CUSTOMER)
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

            // Chạy xong quay về danh sách luôn
            return RedirectToAction("Index");
        }
    }
}