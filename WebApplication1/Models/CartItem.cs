using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models // <-- ĐÃ SỬA NAMESPACE
{
    // Lớp này dùng để lưu 1 món hàng trong giỏ
    public class CartItem
    {
        // 1. Thông tin sản phẩm
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }

        // 2. Số lượng
        public int Quantity { get; set; }

        // 3. Thành tiền (tự tính)
        public decimal TotalPrice
        {
            get { return ProductPrice * Quantity; }
        }

        // Hàm khởi tạo mặc định (Cần thiết cho Session/Serialization)
        public CartItem() { }

        // Hàm khởi tạo (Constructor) để tạo mới một món hàng
        // Lưu ý: Product phải là lớp Model Product của bạn
        public CartItem(Product product, int quantity)
        {
            this.ProductID = product.ProductID;
            this.ProductName = product.ProductName;
            this.ProductImage = product.ProductImage;
            this.ProductPrice = product.ProductPrice;
            this.Quantity = quantity;
        }
    }
}