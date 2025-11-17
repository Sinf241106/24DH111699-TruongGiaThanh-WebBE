using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models; // Đảm bảo using đúng namespace Models của bạn

namespace WebApplication1.Models // Hoặc WebApplication1.ViewModels nếu bạn tạo thư mục riêng
{
    public class HomeViewModel
    {
        // Danh sách cho hàng 1 (chỉ 6 sản phẩm)
        public IEnumerable<Product> FeaturedProducts { get; set; }

        // Danh sách cho hàng 2 (chỉ 8 sản phẩm)
        public IEnumerable<Product> NewProducts { get; set; }

        // Bạn có thể thêm hàng 3 ở đây, ví dụ:
        // public IEnumerable<Product> BestSellingProducts { get; set; }
    }
}