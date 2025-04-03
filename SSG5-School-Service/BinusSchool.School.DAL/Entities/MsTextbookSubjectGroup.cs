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
    public class MsTextbookSubjectGroup : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public string SubjectGroupName { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsTextbookSubjectGroupDetail> TextbookSubjectGroupDetails { get; set; }
        public virtual ICollection<TrTextbook> Textbooks { get; set; }
    }

    internal class MsTextbookSubjectGroupConfiguration : AuditEntityConfiguration<MsTextbookSubjectGroup>
    {
        public override void Configure(EntityTypeBuilder<MsTextbookSubjectGroup> builder)
        {
            builder.Property(x => x.SubjectGroupName)
               .HasMaxLength(100)
               .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.TextbookSubjectGroups)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsTextbookSubjectGroup_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }

    }
}
