using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentFreezeMeritDemerit : AuditEntity,IStudentEntity
    {
        public string IdHomeroomStudent { get; set; }
        public bool IsFreeze { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
    }

    internal class MsStudentFreezeMeritDemeritConfiguration : AuditEntityConfiguration<MsStudentFreezeMeritDemerit>
    {
        public override void Configure(EntityTypeBuilder<MsStudentFreezeMeritDemerit> builder)
        {
            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.StudentFreezeMeritDemerits)
                .IsRequired()
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_MsStudentFreezeMeritDemerit_MsFreeze")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
