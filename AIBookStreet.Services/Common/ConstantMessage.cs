namespace AIBookStreet.Services.Common
{
    public static class ConstantMessage
    {
        public const string Success = "Success";
        public const string Fail = "Fail";
        public const string NotFound = "Not Found";
        public const string NoContent = "No Content";
        public const string Duplicate = "Data already exists";
        public const string EmptyId = "Id is empty";
        public const string UnauthorizedAccess = "You are not authorized to perform this action";
        public const string DatabaseError = "Database operation failed";

        public static class Common
        {
            public const string EmptyInfo = "Thông tin không được để trống";
            public const string AddSuccess = "Thêm thành công";
            public const string AddFail = "Thêm thất bại";
            public const string UpdateSuccess = "Cập nhật thành công";
            public const string UpdateFail = "Cập nhật thất bại";
            public const string DeleteSuccess = "Xoá thành công";
            public const string DeleteFail = "Xoá thất bại";
            public const string NotFoundForUpdate = "Không tìm thấy để cập nhật";
            public const string NotFoundForDelete = "Không tìm thấy để xoá";
        }

        public static class Validation
        {
            public const string InvalidEmailFormat = "Định dạng email không hợp lệ";
            public const string InvalidPhoneFormat = "Định dạng số điện thoại không hợp lệ";
            public const string InvalidDOB = "Ngày sinh không được ở tương lai";
            public const string PasswordTooShort = "Mật khẩu phải có ít nhất 6 ký tự";
            public const string PasswordTooWeak = "Mật khẩu phải chứa ít nhất một chữ cái viết hoa, một chữ cái viết thường, một chữ số và một ký tự đặc biệt";
            public const string InvalidGender = "Giới tính phải là 'Nam' hoặc 'Nữ'";

        }

        public static class Image
        {
            public const string MainImageSizeExceeded = "Kích thước ảnh chính không được vượt quá 10MB";
            public const string InvalidMainImageFormat = "Định dạng ảnh chính không hợp lệ";
            public const string MainImageUploadFailed = "Tải ảnh chính lên thất bại";
            public const string SubImageSizeExceeded = "Kích thước ảnh phụ không được vượt quá 10MB";
            public const string InvalidSubImageFormat = "Định dạng ảnh phụ không hợp lệ";
            public const string SubImageUploadFailed = "Tải ảnh phụ lên thất bại";
            public const string TooManyImages = "Số lượng ảnh phụ tối đa cho phép là 5";
            public const string ImageProcessingError = "Lỗi xử lý ảnh";

        }

        public static class Book
        {
            public const string EmptyCode = "Mã sách không được để trống";
            public const string EmptyTitle = "Tiêu đề sách không được để trống";
            public const string CodeExists = "Mã sách đã tồn tại";
            public const string InvalidAuthorId = "ID tác giả không hợp lệ";
            public const string InvalidCategoryId = "ID thể loại không hợp lệ";

        }

        public static class User
        {
            public const string EmptyUsername = "Tên đăng nhập không được để trống";
            public const string EmptyPassword = "Mật khẩu không được để trống";
            public const string UsernameOrEmailExists = "Tên đăng nhập hoặc email đã tồn tại";
            public const string UsernameExists = "Tên đăng nhập đã tồn tại";
            public const string EmailExists = "Email đã tồn tại";

        }

        public static class Publisher
        {
            public const string EmptyPublisherName = "Tên nhà xuất bản không được để trống";
            public const string PublisherNameExists = "Tên nhà xuất bản đã tồn tại";

        }

        public static class Store
        {
            public const string EmptyStoreName = "Tên cửa hàng không được để trống";
            public const string StoreNameExists = "Tên cửa hàng đã tồn tại";

        }
    }
}
