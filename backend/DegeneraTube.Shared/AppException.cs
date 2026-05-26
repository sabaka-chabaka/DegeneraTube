namespace DegeneraTube.Shared;

public class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object id)
        : base($"{entity} with id '{id}' not found.", 404)
    {
    }

    public NotFoundException(string message)
        : base(message, 404)
    {
    }
}

public class UnauthorizedException() : AppException("Unauthorized.", 401);

public class ForbiddenException() : AppException("Forbidden.", 403);

public class ValidationException(IDictionary<string, string[]> errors)
    : AppException("One or more validation errors occurred.", 422)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors.AsReadOnly();
}
 
public class ConflictException(string message) : AppException(message, 409);
