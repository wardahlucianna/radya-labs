using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentExitStatus : AuditEntity, IStudentEntity
    {
        public string IdStudentExit { get; set; }
        public StatusExitStudent Status { get; set; }
        public string Note { get; set; }
        public virtual TrStudentExit StudentExit { get; set; }

    }

    internal class TrStudentExitStatusConfiguration : AuditEntityConfiguration<TrStudentExitStatus>
    {
        public override void Configure(EntityTypeBuilder<TrStudentExitStatus> builder)
        {
            builder.HasOne(x => x.StudentExit)
              .WithMany(y => y.StudentExitStatuses)
              .HasForeignKey(fk => fk.IdStudentExit)
              .HasConstraintName("FK_TrStudentExitStatus_TrStudentExit")
              .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status).HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (StatusExitStudent)Enum.Parse(typeof(StatusExitStudent), valueFromDb))
               .IsRequired();

            builder.Property(x => x.Note)
               .HasMaxLength(1054);

            base.Configure(builder);
        }

    }
}
