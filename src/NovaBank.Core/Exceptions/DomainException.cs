using System;

namespace NovaBank.Core.Exceptions
{
    /// <summary>Base domain exception type.</summary>
    public class DomainException : Exception
    {
        public DomainException() { }
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception inner) : base(message, inner) { }
    }
}
