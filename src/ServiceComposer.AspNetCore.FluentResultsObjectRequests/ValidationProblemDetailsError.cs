using FluentResults;

namespace ServiceComposer.AspNetCore.FluentResultsObjectRequests
{
    public class RequestValidationError : Error
    {
        public const string PropertyNameKey = "PropertyName";
        public RequestValidationError(string message, string propertyName = null)
            : base(message)
        {
            WithMetadata(PropertyNameKey, propertyName ?? string.Empty);
        }
    }
}
