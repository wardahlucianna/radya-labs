using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrEntryDemeritStudent: AuditEntity, IStudentEntity
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
        public string IdParentEntryDemeritStudent { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime DateDemerit { get; set; }
        public string DemeritUserCreate { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsMeritDemeritMapping MeritDemeritMapping { get; set; }
        public virtual TrEntryDemeritStudent Parent { get; set; }
        public virtual ICollection<TrEntryDemeritStudent> Childs { get; set; }
        public virtual ICollection<TrStudentDemeritApprovalHs> StudentDemeritApproval { get; set; }
    }
    internal class TrEntryDemeritStudentConfiguration : AuditEntityConfiguration<TrEntryDemeritStudent>
    {
        public override void Configure(EntityTypeBuilder<TrEntryDemeritStudent> builder)
        {
            builder.Property(x => x.Note)
                .HasMaxLength(450)
                .HasComment("Note for data merit demerit");

            builder.Property(x => x.Reason)
                .HasMaxLength(450)
                .HasComment("Reason for data merit demerit if data will be edit or deleted");

            builder.Property(x => x.DemeritUserCreate)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.RequestReason)
               .HasMaxLength(450)
               .HasComment("Request Reason for data merit demerit if data will be edit or deleted");

            builder.Property(e => e.RequestType)
            .HasConversion<string>()
            .HasMaxLength(6)
            .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
            .WithMany(x => x.EntryDemeritStudents)
            .HasForeignKey(fk => fk.IdHomeroomStudent)
            .HasConstraintName("FK_TrEntryDemeritStudent_MsHomeroomStudent")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();


            builder.HasOne(x => x.MeritDemeritMapping)
            .WithMany(x => x.EntryDemeritStudents)
            .HasForeignKey(fk => fk.IdMeritDemeritMapping)
            .HasConstraintName("FK_TrEntryDemeritStudent_MsMeritDemeritMapping")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Parent)
              .WithMany(x => x.Childs)
              .HasForeignKey(fk => fk.IdParentEntryDemeritStudent)
              .HasConstraintName("FK_TrEntryDemeritStudent_TrEntryDemeritStudent")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
