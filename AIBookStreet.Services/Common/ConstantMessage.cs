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
    }
} 