using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrEntryMeritStudent : AuditEntity, IStudentEntity
    {
        public string IdHomeroomStudent { get; set; }
        public string IdMeritDemeritMapping { get; set; }
        public int Point { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Column reason will be filled if Request Type edit or deleted
        /// </summary>
        public string Reason { get; set; }
        public RequestType RequestType { get; set; }
        public string RequestReason { get; set; }
        public bool IsHasBeenApproved { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }
        public string IdParentEntryMeritStudent { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime DateMerit { get; set; }
        public string MeritUserCreate { get; set; }
        public string IdFocusArea { get; set; }
        public EntryMeritStudentType Type { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsMeritDemeritMapping MeritDemeritMapping { get; set; }
        public virtual TrEntryMeritStudent Parent { get; set; }
        public virtual MsFocusArea FocusArea { get; set; }
        public virtual MsUser UserCraete { get; set; }
        public virtual ICollection<TrEntryMeritStudent> Childs { get; set; }
        public virtual ICollection<TrStudentMeritApprovalHs> StudentMeritApproval { get; set; }
        public virtual ICollection<TrEntryMeritStudentEvidance> EntryMeritStudentEvidances { get; set; 
        }
    }
    internal class TrStudentMeritDemeritConfiguration : AuditEntityConfiguration<TrEntryMeritStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEntryMeritStudent> builder)
        {
            builder.Property(x => x.Note)
                .HasMaxLength(450)
                .HasComment("Note for data merit demerit");

            builder.Property(x => x.MeritUserCreate)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasMaxLength(450)
                .HasComment("Reason for data merit demerit if data will be edit or deleted");

            builder.Property(x => x.RequestReason)
               .HasMaxLength(450)
               .HasComment("Request Reason for data merit demerit if data will be edit or deleted");

            builder.Property(e => e.RequestType)
            .HasConversion<string>()
            .HasMaxLength(8)
            .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
            .WithMany(x => x.EntryMeritStudents)
            .HasForeignKey(fk => fk.IdHomeroomStudent)
            .HasConstraintName("FK_TrEntryMeritStudent_MsHomeroomStudent")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.MeritDemeritMapping)
            .WithMany(x => x.EntryMeritStudents)
            .HasForeignKey(fk => fk.IdMeritDemeritMapping)
            .HasConstraintName("FK_TrEntryMeritStudent_MsMeritDemeritMapping")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Parent)
               .WithMany(x => x.Childs)
               .HasForeignKey(fk => fk.IdParentEntryMeritStudent)
               .HasConstraintName("FK_TrEntryMeritStudent_TrEntryMeritStudent")
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.FocusArea)
               .WithMany(x => x.EntryMeritStudents)
               .HasForeignKey(fk => fk.IdFocusArea)
               .HasConstraintName("FK_TrEntryMeritStudent_MsFocusArea")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UserCraete)
            .WithMany(x => x.EntryMeritStudents)
            .HasForeignKey(fk => fk.MeritUserCreate)
            .HasConstraintName("FK_TrEntryMeritStudent_MsUserCreate")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.Property(e => e.Type).HasMaxLength(maxLength: 20)
                .HasConversion(valueToDb =>
                        valueToDb.ToString(),
                    valueFromDb =>
                        (EntryMeritStudentType)Enum.Parse(typeof(EntryMeritStudentType), valueFromDb));

            base.Configure(builder);
        }
    }
}
