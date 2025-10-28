using System;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Interfaces
{
    /// <summary>Domain service contract for customer-related invariants.</summary>
    public interface ICustomerDomainService
    {
        /// <summary>Ensures a NationalId is unique within the system.</summary>
        bool EnsureUniqueNationalId(NationalId nationalId);
    }
}
