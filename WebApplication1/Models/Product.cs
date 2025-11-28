namespace WebApplication1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations; // Thêm thư viện này

    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
            // Mặc định trạng thái là True (Đang bán) khi tạo mới
            this.IsActive = true;
        }

        public int ProductID { get; set; }
        public int CategoryID { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string ProductName { get; set; }

        [Display(Name = "Mô tả")]
        public string ProductDescription { get; set; }

        // --- RÀNG BUỘC GIÁ TIỀN ---
        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Vui lòng nhập giá tiền")]
        [Range(1000, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 1,000đ")]
        [DataType(DataType.Currency)]
        public decimal ProductPrice { get; set; }

        public string ProductImage { get; set; }

        [Display(Name = "Số lượng")]
        public Nullable<int> Quantity { get; set; }

        [Display(Name = "Trạng thái")]
        public Nullable<bool> IsActive { get; set; }

        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}