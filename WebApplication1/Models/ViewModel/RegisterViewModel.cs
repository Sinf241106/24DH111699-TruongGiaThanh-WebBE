using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModel
{
    // Class này chứa các trường dữ liệu cho Form Đăng ký
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Tên đăng nhập.")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(30, ErrorMessage = "Tên đăng nhập không quá 30 ký tự.")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[0-9]).*$", ErrorMessage = "Tên đăng nhập phải có ít nhất 1 chữ hoa và 1 chữ số")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [RegularExpression("^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()_\\-+=]).*$", ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ số, và 1 ký tự đặc biệt (ví dụ: @, #, $, ...)")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không trùng khớp.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Họ và tên.")]
        [Display(Name = "Họ tên")]
        [StringLength(30, ErrorMessage = "Họ tên không quá 30 ký tự.")]
        [RegularExpression("^[^0-9]*$", ErrorMessage = "Họ tên không được chứa số.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [Display(Name = "Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 số.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Số điện thoại chỉ được chứa số.")]
        public string Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        // --- (ĐÃ SỬA) ---
        [StringLength(40, ErrorMessage = "Địa chỉ không quá 40 ký tự.")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*[0-9]).*$", ErrorMessage = "Địa chỉ phải bao gồm cả chữ và số.")]
        public string Address { get; set; }
    }
}