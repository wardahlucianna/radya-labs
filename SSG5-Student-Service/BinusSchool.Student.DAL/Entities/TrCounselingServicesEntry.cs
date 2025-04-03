using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrCounselingServicesEntry : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdCounselor { get; set; }
        public string IdCounselingCategory { get; set; }
        public string IdStudent { get; set; }
        public CounselingWith CounselingWith { get; set; }
        public DateTime DateTime { get; set; }
        public string ReferredBy { get; set; }
        public string BriefReport { get; set; }
        public string FollowUp { get; set; }
        public string FatherIdParent { get; set; }
        public string MotherIdParent { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsCounselingCategory CounselingCategory { get; set; }
        public virtual MsCounselor Counselor { get; set; }
        public virtual MsParent ParentFather { get; set; }
        public virtual MsParent ParentMother { get; set; }
        public virtual ICollection<TrCounselingServicesEntryConcern> CounselingServicesEntryConcern { get; set; }
        public virtual ICollection<TrCounselingServicesEntryAttachment> CounselingServicesEntryAttachment { get; set; }
    }

    internal class TrCounselingServicesEntryConfiguration : AuditEntityConfiguration<TrCounselingServicesEntry>
    {
        public override void Configure(EntityTypeBuilder<TrCounselingServicesEntry> builder)
        {
            
            builder.Property(x => x.DateTime).IsRequired();
            builder.Property(x => x.CounselingWith).HasConversion<string>().HasMaxLength(15).IsRequired();
            builder.Property(x => x.ReferredBy).HasMaxLength(450);
            builder.Property(x => x.FollowUp).HasMaxLength(450);

            builder.HasOne(x => x.Student)
             .WithMany(x => x.CounselingServicesEntry)
             .HasForeignKey(fk => fk.IdStudent)
             .HasConstraintName("FK_TrCounselingServicesEntry_MsStudent")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.CounselingCategory)
             .WithMany(x => x.CounselingServicesEntry)
             .HasForeignKey(fk => fk.IdCounselingCategory)
             .HasConstraintName("FK_TrCounselingServicesEntry_MsCounselingCategory")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.ParentFather)
             .WithMany(x => x.CounselingServicesEntryFather)
             .HasForeignKey(fk => fk.FatherIdParent)
             .HasConstraintName("FK_TrCounselingServicesEntry_MsParentFather")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ParentMother)
             .WithMany(x => x.CounselingServicesEntryMother)
             .HasForeignKey(fk => fk.MotherIdParent)
             .HasConstraintName("FK_TrCounselingServicesEntry_MsParentMother")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.CounselingServicesEntry)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_TrCounselingServicesEntry_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Counselor)
             .WithMany(x => x.CounselingServicesEntry)
             .HasForeignKey(fk => fk.IdCounselor)
             .HasConstraintName("FK_TrCounselingServicesEntry_MsCounselor")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
