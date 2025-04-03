using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionHeader : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }  
        public string IdStudent { get; set; }   
        public string IdStatusOverall { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsServiceAsActionStatus StatusOverall { get; set; }

        public virtual ICollection<TrServiceAsActionForm> ServiceAsActionForms { get; set; }
    }

    internal class TrExperienceHeaderConfiguration : AuditEntityConfiguration<TrServiceAsActionHeader>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionHeader> builder)
        {
            builder.Property(x => x.IdAcademicYear).IsRequired().HasMaxLength(36);

            builder.Property(x => x.IdStudent).IsRequired().HasMaxLength(36);

            builder.Property(x => x.IdStatusOverall).IsRequired().HasMaxLength(36);

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.ServiceAsActionHeaders)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrExperienceHeader_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.ServiceAsActionHeaders)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrExperienceHeader_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.StatusOverall)
                .WithMany(x => x.ServiceAsActionHeaders)
                .HasForeignKey(fk => fk.IdStatusOverall)
                .HasConstraintName("FK_TrExperienceHeader_MsExperienceStatus")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
