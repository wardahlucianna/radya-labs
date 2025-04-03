using System;
using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Student
{
    public class MsStudent : UserKindStudentParentEntity, IDocumentEntity
    {
        //public string IdStudent { get; set; }
        //public string IdBinusian { get; set; }
        public int IdStudentStatus { get; set; }
        public string IdSchool { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }

        public virtual MsSiblingGroup SiblingGroup { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual LtStudentStatus StudentStatus { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }
        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
        public virtual ICollection<TrBLPGroupStudent> BLPGroupStudents { get; set; }
        public virtual ICollection<MsRespondent> Respondents { get; set; }
        public virtual ICollection<TrBLPUpdatedConsentStatus> BLPUpdatedConsentStatuses { get; set; }
        public virtual ICollection<TrBLPEmailSentLog> BLPEmailSentLogs { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<MsDocumentReqApplicant> DocumentReqApplicants { get; set; }
        public virtual MsAdmissionData AdmissionData { get; set; }
    }

    internal class MsStudentConfiguration : UserKindStudentParentEntityConfiguration<MsStudent>
    {
        public override void Configure(EntityTypeBuilder<MsStudent> builder)
        {
            //builder.HasKey(x => x.IdStudent);

            builder.Property(x => x.BinusianEmailAddress)
                .HasMaxLength(200);

            builder.Property(x => x.POB)
                .HasMaxLength(100)
                .IsRequired();

            //builder.Property(x => x.IdBinusian)
            //    .IsRequired()
            //    .HasMaxLength(36);

            builder.HasOne(x => x.StudentStatus)
               .WithMany(y => y.Students)
               .HasForeignKey(fk => fk.IdStudentStatus)
               .HasConstraintName("FK_MsStudent_LtStudentStatus")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.School)
                .WithMany(y => y.Students)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsStudent_MsSchool")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
