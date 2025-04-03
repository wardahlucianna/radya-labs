using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentMeritApprovalHs : AuditEntity, IStudentEntity
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
        public string IdUserApproved3{ get; set; }
        /// <summary>
        /// Note will be filled if column IsApproved is false
        /// </summary>
        public string Note3 { get; set; }
        public RequestType RequestType { get; set; }
        public string Reason { get; set; }
        public string RequestReason { get; set; }
        public string Status { get; set; }
        public string IdTrEntryMeritStudent { get; set; }

        public virtual MsUser User1 { get; set; }
        public virtual MsUser User2 { get; set; }
        public virtual MsUser User3 { get; set; }
        public virtual TrEntryMeritStudent EntryMeritStudent { get; set; }
    }

    internal class HsStudentMeritApprovalConfiguration : AuditEntityConfiguration<TrStudentMeritApprovalHs>
    {
        public override void Configure(EntityTypeBuilder<TrStudentMeritApprovalHs> builder)
        {
            builder.Property(x => x.Note1).HasMaxLength(450);
            builder.Property(x => x.Note2).HasMaxLength(450);
            builder.Property(x => x.Note3).HasMaxLength(450);

            builder.HasOne(x => x.User1)
            .WithMany(x => x.StudentMeritApprovals1)
            .HasForeignKey(fk => fk.IdUserApproved1)
            .HasConstraintName("FK_HsStudentMeritApproval_Msuser_IdUserApproved1")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.User2)
            .WithMany(x => x.StudentMeritApprovals2)
            .HasForeignKey(fk => fk.IdUserApproved2)
            .HasConstraintName("FK_HsStudentMeritApproval_Msuser_IdUserApproved2")
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User3)
            .WithMany(x => x.StudentMeritApprovals3)
            .HasForeignKey(fk => fk.IdUserApproved3)
            .HasConstraintName("FK_HsStudentMeritApproval_Msuser_IdUserApproved3")
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EntryMeritStudent)
            .WithMany(x => x.StudentMeritApproval)
            .HasForeignKey(fk => fk.IdTrEntryMeritStudent)
            .HasConstraintName("FK_HsStudentMeritApproval_TrEntryMeritStudent")
            .OnDelete(DeleteBehavior.Restrict).IsRequired();

            base.Configure(builder);
        }
    }
}
