using System.Collections.Generic;
using WebApplication1.Models; // Đảm bảo using đúng namespace Models của bạn

namespace WebApplication1.Models
{
    public class ProductDetailViewModel
    {
        // 1. Sản phẩm chính đang xem
        public Product MainProduct { get; set; }

        // 2. Danh sách sản phẩm cuộn ngang ở dưới (Sản phẩm tương tự)
        public IEnumerable<Product> SimilarProducts { get; set; }

        // 3. Danh sách sản phẩm ở cột phải (Sản phẩm bán chạy)
        public IEnumerable<Product> BestSellingProducts { get; set; }
    }
}