using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtLockerPosition : AuditEntity, IStudentEntity
    {
        public bool LockerPosition { get; set; }
        public string PositionName { get; set; }
        public virtual ICollection<MsLocker> Lockers { get; set; }
    }

    internal class LtLockerPositionConfiguration : AuditEntityConfiguration<LtLockerPosition>
    {
        public override void Configure(EntityTypeBuilder<LtLockerPosition> builder)
        {
            builder.Property(x => x.PositionName)
                .HasMaxLength(50)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
