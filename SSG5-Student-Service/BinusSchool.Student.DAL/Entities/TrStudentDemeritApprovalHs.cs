using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentDemeritApprovalHs : AuditEntity, IStudentEntity
    {
        public bool IsApproved1 { get; set; }
        /// <summary>
        ///IdUser who approves
        /// </summary>
        public string IdUserApproved1 { get; set; }
        /// <summary>
        /// Note will be filled if column IsApproved is false
        /// </summary>
        public string Note1 { get; set; }
        public bool IsApproved2 { get; set; }
        /// <summary>
        ///IdUser who approves
        /// </summary>
        public string IdUserApproved2 { get; set; }
        /// <summary>
        /// Note will be filled if column IsApproved is false
        /// </summary>
        public string Note2 { get; set; }
        public bool IsApproved3 { get; set; }
        /// <summary>
        ///IdUser who approves
        /// </summary>
        public string IdUserApproved3 { get; set; }
        /// <summary>
        /// Note will be filled if column IsApproved is false
        /// </summary>
        public string Note3 { get; set; }
        public string Reason { get; set; }
        public string RequestReason { get; set; }
        public string Status { get; set; }
        public RequestType RequestType { get; set; }
        public string IdTrEntryDemeritStudent { get; set; }
        public virtual MsUser User1 { get; set; }
        public virtual MsUser User2 { get; set; }
        public virtual MsUser User3 { get; set; }
        public virtual TrEntryDemeritStudent EntryDemeritStudent { get; set; }
    }

    internal class HsStudentDemeritApprovalConfiguration : AuditEntityConfiguration<TrStudentDemeritApprovalHs>
    {
        public override void Configure(EntityTypeBuilder<TrStudentDemeritApprovalHs> builder)
        {
            builder.Property(x => x.Note1).HasMaxLength(450);
            builder.Property(x => x.Note2).HasMaxLength(450);
            builder.Property(x => x.Note3).HasMaxLength(450);

            builder.HasOne(x => x.User1)
            .WithMany(x => x.StudentDemeritApprovals1)
            .HasForeignKey(fk => fk.IdUserApproved1)
            .HasConstraintName("FK_HsStudentDemeritApproval_Msuser_IdUserApproved1")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.User2)
           .WithMany(x => x.StudentDemeritApprovals2)
           .HasForeignKey(fk => fk.IdUserApproved2)
           .HasConstraintName("FK_HsStudentDemeritApproval_Msuser_IdUserApproved2")
           .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User3)
           .WithMany(x => x.StudentDemeritApprovals3)
           .HasForeignKey(fk => fk.IdUserApproved3)
           .HasConstraintName("FK_HsStudentDemeritApproval_Msuser_IdUserApproved3")
           .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EntryDemeritStudent)
           .WithMany(x => x.StudentDemeritApproval)
           .HasForeignKey(fk => fk.IdTrEntryDemeritStudent)
           .HasConstraintName("FK_HsStudentMeritApproval_TrEntryDemeritStudent")
           .OnDelete(DeleteBehavior.Restrict).IsRequired();

            base.Configure(builder);
        }
    }
}
