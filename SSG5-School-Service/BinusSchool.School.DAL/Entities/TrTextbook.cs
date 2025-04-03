using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Employee;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrTextbook : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdBinusianCreated { get; set; }
        public string IdTextbookSubjectGroup { get; set; }
        public string IdSubject { get; set; }
        public string IdPathway { get; set; }
        public TextbookPreparationBookType BookType { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public decimal Weight { get; set; }
        public string NoSKU { get; set; }
        public bool IsRegion { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsCountinuity { get; set; }
        public bool IsAvailableStatus { get; set; }
        public string Supplier { get; set; }
        public string Location { get; set; }
        public string LastModif { get; set; }
        public string Vendor { get; set; }
        public int OriginalPrice { get; set; }
        public int PriceAfterDiscount { get; set; }
        public string Note { get; set; }
        public string IdBinusianApproval1 { get; set; }
        public string IdBinusianApproval2 { get; set; }
        public string IdBinusianApproval3 { get; set; }
        public string ApprovalNote1 { get; set; }
        public string ApprovalNote2 { get; set; }
        public string ApprovalNote3 { get; set; }
        public TextbookPreparationStatus Status { get; set; }
        public bool IsApproval1 { get; set; }
        public bool IsApproval2 { get; set; }
        public bool IsApproval3 { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStaff StaffCreate { get; set; }
        public virtual MsTextbookSubjectGroup TextbookSubjectGroup { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsPathway Pathway { get; set; }
        public virtual MsStaff StaffApproval1 { get; set; }
        public virtual MsStaff StaffApproval2 { get; set; }
        public virtual MsStaff StaffApproval3 { get; set; }
        public virtual ICollection<TrTextbookAttachment> TextbookAttachments { get; set; }
    }

    internal class TrTextbookConfiguration : AuditEntityConfiguration<TrTextbook>
    {
        public override void Configure(EntityTypeBuilder<TrTextbook> builder)
        {
            builder.Property(e => e.BookType).HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (TextbookPreparationBookType)Enum.Parse(typeof(TextbookPreparationBookType), valueFromDb))
               .IsRequired();

            builder.Property(x => x.ISBN)
               .HasMaxLength(50)
               .IsRequired();

            builder.Property(x => x.Title)
               .HasMaxLength(250)
               .IsRequired();

            builder.Property(x => x.Author)
               .HasMaxLength(100)
               .IsRequired();

            builder.Property(x => x.Publisher)
               .HasMaxLength(100);

            builder.Property(x => x.NoSKU)
               .HasMaxLength(50);

            builder.Property(x => x.Supplier)
               .HasMaxLength(100);

            builder.Property(x => x.Location)
               .HasMaxLength(1025);

            builder.Property(x => x.LastModif)
               .HasMaxLength(100);

            builder.Property(x => x.Vendor)
               .HasMaxLength(100);

            builder.Property(x => x.Note)
               .HasMaxLength(1025);

            builder.Property(x => x.ApprovalNote1)
               .HasMaxLength(1025);

            builder.Property(x => x.ApprovalNote2)
               .HasMaxLength(1025);

            builder.Property(x => x.ApprovalNote3)
               .HasMaxLength(1025);

            builder.Property(e => e.Status).HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (TextbookPreparationStatus)Enum.Parse(typeof(TextbookPreparationStatus), valueFromDb))
               .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Textbooks)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrTextbookEntry_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.StaffCreate)
                .WithMany(x => x.TextbooksUserCreate)
                .HasForeignKey(fk => fk.IdBinusianCreated)
                .HasConstraintName("FK_TrTextbookEntry_MsStaffCreate")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.TextbookSubjectGroup)
                .WithMany(x => x.Textbooks)
                .HasForeignKey(fk => fk.IdTextbookSubjectGroup)
                .HasConstraintName("FK_TrTextbookEntry_MsTextbookSubjectGroup")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.Textbooks)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_TrTextbookEntry_MsSubject")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Pathway)
                .WithMany(x => x.Textbooks)
                .HasForeignKey(fk => fk.IdPathway)
                .HasConstraintName("FK_TrTextbookEntry_MsPathway")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.StaffApproval1)
              .WithMany(x => x.TextbooksUserApproval1)
              .HasForeignKey(fk => fk.IdBinusianApproval1)
              .HasConstraintName("FK_TrTextbookEntry_MsStaffApproval1")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.StaffApproval2)
              .WithMany(x => x.TextbooksUserApproval2)
              .HasForeignKey(fk => fk.IdBinusianApproval2)
              .HasConstraintName("FK_TrTextbookEntry_MsStaffApproval2")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.StaffApproval3)
              .WithMany(x => x.TextbooksUserApproval3)
              .HasForeignKey(fk => fk.IdBinusianApproval3)
              .HasConstraintName("FK_TrTextbookEntry_MsStaffApproval3")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
