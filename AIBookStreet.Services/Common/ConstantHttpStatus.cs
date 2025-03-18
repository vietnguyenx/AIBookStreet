namespace AIBookStreet.Services.Common
{
    public class ConstantHttpStatus
    {
        // 2xx Success
        public const int OK = 200;
        public const int CREATED = 201;
        public const int ACCEPTED = 202;
        public const int NO_CONTENT = 204;

        // 4xx Client Errors
        public const int BAD_REQUEST = 400;
        public const int UNAUTHORIZED = 401;
        public const int FORBIDDEN = 403;
        public const int NOT_FOUND = 404;
        public const int METHOD_NOT_ALLOWED = 405;
        public const int CONFLICT = 409;
        public const int UNPROCESSABLE_ENTITY = 422;

        // 5xx Server Errors
        public const int INTERNAL_SERVER_ERROR = 500;
        public const int NOT_IMPLEMENTED = 501;
        public const int BAD_GATEWAY = 502;
        public const int SERVICE_UNAVAILABLE = 503;
    }
} 