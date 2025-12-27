namespace NovaBank.Core.Exceptions;

/// <summary>Entity not found exception.</summary>
public class NotFoundException : DomainException
{
    public NotFoundException() { }
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, Exception inner) : base(message, inner) { }
}

