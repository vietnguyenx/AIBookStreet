namespace AIBookStreet.Services.Common
{
    public class ConstantMessage
    {
        public const string Success = "Success";
        public const string Fail = "Fail";
        public const string NotFound = "Not Found";
        public const string NoContent = "No Content";
        public const string Duplicate = "Data was exist";
        public const string EmptyId = "Id is empty";

        public static class Book
        {
            public const string EmptyInfo = "Book information cannot be empty";
            public const string EmptyCode = "Book code cannot be empty";
            public const string EmptyTitle = "Book title cannot be empty";
            public const string CodeExists = "Book code already exists";
            public const string InvalidAuthorId = "Invalid author ID";
            public const string InvalidCategoryId = "Invalid category ID";
            public const string MainImageSizeExceeded = "Main image size must not exceed 10MB";
            public const string InvalidMainImageFormat = "Invalid main image format";
            public const string MainImageUploadFailed = "Failed to upload main image";
            public const string SubImageSizeExceeded = "Sub image size must not exceed 10MB";
            public const string InvalidSubImageFormat = "Invalid sub image format";
            public const string SubImageUploadFailed = "Failed to upload sub image";
            public const string AddSuccess = "Book added successfully";
            public const string AddFail = "Failed to add book";
            public const string UpdateSuccess = "Book updated successfully";
            public const string UpdateFail = "Failed to update book";
            public const string DeleteSuccess = "Book deleted successfully";
            public const string DeleteFail = "Failed to delete book";
            public const string NotFoundForUpdate = "Book not found for update";
            public const string NotFoundForDelete = "Book not found for deletion";
        }

        public static class User
        {
            public const string EmptyInfo = "User information cannot be empty";
            public const string EmptyUsername = "Username cannot be empty";
            public const string EmptyPassword = "Password cannot be empty";
            public const string UsernameOrEmailExists = "Username or email already exists";
            public const string UsernameExists = "Username already exists";
            public const string EmailExists = "Email already exists";
            public const string MainImageSizeExceeded = "Main image size must not exceed 10MB";
            public const string InvalidMainImageFormat = "Invalid main image format";
            public const string MainImageUploadFailed = "Failed to upload main image";
            public const string SubImageSizeExceeded = "Sub image size must not exceed 10MB";
            public const string InvalidSubImageFormat = "Invalid sub image format";
            public const string SubImageUploadFailed = "Failed to upload sub image";
            public const string AddSuccess = "User added successfully";
            public const string AddFail = "Failed to add user";
            public const string UpdateSuccess = "User updated successfully";
            public const string UpdateFail = "Failed to update user";
            public const string DeleteSuccess = "User deleted successfully";
            public const string DeleteFail = "Failed to delete user";
            public const string NotFoundForUpdate = "User not found for update";
            public const string NotFoundForDelete = "User not found for deletion";
            public const string InvalidEmailFormat = "Invalid email format";
            public const string InvalidPhoneFormat = "Invalid phone number format";
            public const string InvalidDOB = "Date of birth cannot be in the future";
            public const string PasswordTooShort = "Password must be at least 6 characters long";
            public const string PasswordTooWeak = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character";
            public const string InvalidGender = "Gender must be either 'Male' or 'Female'";
            public const string TooManyImages = "Maximum number of additional images allowed is 5";
            public const string DatabaseError = "Database operation failed";
            public const string ImageProcessingError = "Error processing image";
            public const string UnauthorizedAccess = "You are not authorized to perform this action";
        }
    }
} 