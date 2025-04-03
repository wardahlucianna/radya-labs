using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrExemplary : AuditEntity, IStudentEntity
    {
        public DateTime PostedDate { get; set; }
        public DateTime? ExemplaryDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdExemplaryCategory { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrExemplaryLikes> ExemplaryLikes { get; set; }
        public virtual ICollection<TrExemplaryAttachment> ExemplaryAttachments { get; set; }
        public virtual ICollection<TrExemplaryStudent> ExemplaryStudents { get; set; }
        public virtual ICollection<TrExemplaryValue> TrExemplaryValues { get; set; }
        public virtual LtExemplaryCategory LtExemplaryCategory { get; set; }
    }

    internal class TrExemplaryConfiguration : AuditEntityConfiguration<TrExemplary>
    {
        public override void Configure(EntityTypeBuilder<TrExemplary> builder)
        {

            builder.Property(x => x.PostedDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.ExemplaryDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.Title)
               .HasMaxLength(100);

            builder.Property(x => x.Description)
               .HasMaxLength(4000);

            builder.Property(x => x.IdAcademicYear)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Exemplaries)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrExemplary_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.LtExemplaryCategory)
                .WithMany(x => x.TrExemplaries)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExemplaryCategory)
                .HasConstraintName("FK_TrExemplary_LtExemplaryCategory")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
