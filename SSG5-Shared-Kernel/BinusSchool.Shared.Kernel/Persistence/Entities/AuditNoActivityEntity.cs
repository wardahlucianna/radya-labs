using System;
using BinusSchool.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Entities
{
    public class AuditNoActivityEntity
    {
    }

    public class AuditNoActivityEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : AuditNoActivityEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {

        }
    }
}
