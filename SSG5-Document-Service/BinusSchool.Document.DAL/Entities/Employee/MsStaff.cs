using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsStaff : AuditNoUniqueEntity, IDocumentEntity
    {
        public string IdBinusian { get; set; }    
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string IdSchool { get; set; }
        public int IdStaffStatus { get; set; }
        public string BinusianEmailAddress { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsDocumentReqApprover> DocumentReqApprovers { get; set; }
        public virtual ICollection<MsDocumentReqDefaultPIC> DocumentReqDefaultPICs { get; set; }
        public virtual ICollection<MsDocumentReqApplicant> DocumentReqApplicants { get; set; }
        public virtual ICollection<TrDocumentReqApplicantDetail> DocumentReqApplicantDetails { get; set; }
        public virtual ICollection<TrDocumentReqStatusTrackingHistory> DocumentReqStatusTrackingHistories { get; set; }
        public virtual ICollection<TrDocumentReqPIC> DocumentReqPICs { get; set; }
        public virtual ICollection<TrDocumentReqPaymentManual> DocumentReqPaymentManuals { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
    }

    internal class MsStaffConfiguration : AuditNoUniqueEntityConfiguration<MsStaff>
    {
        public override void Configure(EntityTypeBuilder<MsStaff> builder)
        {
            builder.HasKey(x => x.IdBinusian);
          
            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);    

            builder.Property(x => x.ShortName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();
         
            builder.Property(x => x.LastName)
               .HasMaxLength(100);

            builder.Property(x => x.IdSchool)
               .HasMaxLength(36);

            builder.Property(x => x.BinusianEmailAddress)
               .HasMaxLength(60);

            builder.HasOne(x => x.School)
                .WithMany(x => x.Staffs)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsStaff_MsSchool")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
