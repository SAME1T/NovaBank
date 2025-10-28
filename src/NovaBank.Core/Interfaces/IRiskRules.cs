using System;

namespace NovaBank.Core.Interfaces
{
    /// <summary>Risk evaluation rules.</summary>
    public interface IRiskRules
    {
        /// <summary>Determines whether a customer is considered high risk.</summary>
        bool IsHighRiskCustomer(Guid customerId);
    }
}
