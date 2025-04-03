using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsTextbookSubjectGroupDetail : AuditEntity, ISchoolEntity
    {
        public string IdTextbookSubjectGroup { get; set; }
        public string IdSubject { get; set; }
        public virtual MsTextbookSubjectGroup TextbookSubjectGroup { get; set; }
        public virtual MsSubject Subject { get; set; }
    }

    internal class MsTextbookSubjectGroupDetailConfiguration : AuditEntityConfiguration<MsTextbookSubjectGroupDetail>
    {
        public override void Configure(EntityTypeBuilder<MsTextbookSubjectGroupDetail> builder)
        {
            builder.HasOne(x => x.TextbookSubjectGroup)
                .WithMany(x => x.TextbookSubjectGroupDetails)
                .HasForeignKey(fk => fk.IdTextbookSubjectGroup)
                .HasConstraintName("FK_MsTextbookSubjectGroupDetail_MsTextbookSubjectGroup")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.TextbookSubjectGroupDetails)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsTextbookSubjectGroupDetail_MsSubject")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
