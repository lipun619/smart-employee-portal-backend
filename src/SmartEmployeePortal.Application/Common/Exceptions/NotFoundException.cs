namespace SmartEmployeePortal.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist in the database.
/// The API layer catches this and returns HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' with key '{key}' was not found.")
    {
    }
}
