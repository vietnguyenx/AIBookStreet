namespace AIBookStreet.API.RequestModel
{
    public class PaginatedRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortField { get; set; }

        public int? SortOrder { get; set; }

        public PaginatedRequest(int pageNumber, int pageSize, string? sortField, int? sortOrder)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortField = sortField;
            SortOrder = sortOrder;
        }
    }

    public class PaginatedRequest<T> : PaginatedRequest where T : class
    {
        public T? Result { get; set; }

        public PaginatedRequest(T? Result, int pageNumber, int pageSize, string? sortField, int? sortOrder) : base(pageNumber, pageSize, sortField, sortOrder)
        {
            this.Result = Result;
        }
    }
}
