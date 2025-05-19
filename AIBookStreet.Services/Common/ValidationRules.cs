using System.Text.RegularExpressions;

namespace AIBookStreet.Services.Common
{
    public static class ValidationRules
    {
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            // Username chỉ được chứa chữ cái, số và dấu gạch dưới
            // Độ dài từ 3-50 ký tự
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,50}$");
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // Kiểm tra định dạng email
            return Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Kiểm tra khoảng trắng
            if (password.Contains(" "))
                return false;

            // Mật khẩu phải có ít nhất 8 ký tự
            // Chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt
            // Không chứa khoảng trắng
            // Độ dài tối đa 50 ký tự
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z\s])[^\s]{8,50}$");
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            // Số điện thoại phải bắt đầu bằng 0 và có 10 số
            return Regex.IsMatch(phone, @"^0\d{9}$");
        }

        public static bool IsValidFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return false;

            // Họ tên chỉ được chứa chữ cái, dấu cách và dấu gạch ngang
            // Độ dài từ 2-100 ký tự
            return Regex.IsMatch(fullName, @"^[a-zA-ZÀ-ỹ\s-]{2,100}$");
        }
    }
} 