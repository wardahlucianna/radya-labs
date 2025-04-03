using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrPersonalWellBeing : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string ArticleName { get; set; }
        public PersonalWellBeingFor For {get; set; }
        public string Link {get; set; }
        public string Description { get; set; }
        public bool NotifRecipient { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }

        public virtual ICollection<TrPersonalWellBeingLevel> PersonalWellBeingLevel { get; set; } 
        public virtual ICollection<TrPersonalWellBeingAttachment> PersonalWellBeingAttachment { get; set; } 
    }

    internal class TrPersonalWellBeingConfiguration : AuditEntityConfiguration<TrPersonalWellBeing>
    {
        public override void Configure(EntityTypeBuilder<TrPersonalWellBeing> builder)
        {
            builder.Property(p => p.ArticleName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.For).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(p => p.Link).HasMaxLength(450);
            builder.Property(p => p.Description).HasMaxLength(450);

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.PersonalWellBeing)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_TrPersonalWellBeing_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
