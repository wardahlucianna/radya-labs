using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrCounselingServicesEntryConcern : AuditEntity, IStudentEntity
    {
        public string IdCounselingServicesEntry { get; set; }
        public string IdConcernCategory { get; set; }

        public virtual TrCounselingServicesEntry CounselingServicesEntry { get; set; }
        public virtual MsConcernCategory ConcernCategory { get; set; }
    }

    internal class TrCounselingServicesEntryConcernConfiguration : AuditEntityConfiguration<TrCounselingServicesEntryConcern>
    {
        public override void Configure(EntityTypeBuilder<TrCounselingServicesEntryConcern> builder)
        {
            builder.HasOne(x => x.CounselingServicesEntry)
             .WithMany(x => x.CounselingServicesEntryConcern)
             .HasForeignKey(fk => fk.IdCounselingServicesEntry)
             .HasConstraintName("FK_TrCounselingServicesEntryConcern_TrCounselingServicesEntry")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.ConcernCategory)
             .WithMany(x => x.CounselingServicesEntryConcern)
             .HasForeignKey(fk => fk.IdConcernCategory)
             .HasConstraintName("FK_TrCounselingServicesEntryConcern_MsConcernCategory")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
