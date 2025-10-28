using System;
using System.ComponentModel.DataAnnotations;

namespace NovaBank.Core.Abstractions
{
    /// <summary>
    /// Base aggregate/entity with identifier and auditing fields.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>Unique identifier.</summary>
        public Guid Id { get; protected set; } = Guid.NewGuid();

        /// <summary>UTC creation timestamp.</summary>
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        /// <summary>UTC update timestamp.</summary>
        public DateTime? UpdatedAt { get; protected set; }

        /// <summary>Concurrency token for EF Core optimistic concurrency.</summary>
        [Timestamp]
        public byte[]? RowVersion { get; protected set; }

        /// <summary>Update the UpdatedAt field to current UTC time.</summary>
        protected void TouchUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
