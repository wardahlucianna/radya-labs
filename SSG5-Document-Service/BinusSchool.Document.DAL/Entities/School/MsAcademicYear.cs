using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.School
{
    public class MsAcademicYear : CodeEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public int OrderNumber { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsLevel> Levels { get; set; }
        public virtual ICollection<TrBLPGroupStudent> BLPGroupStudents { get; set; }
        public virtual ICollection<MsSurveyQuestion> SurveyQuestions { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<MsDocumentReqType> StartAYDocumentReqTypes { get; set; }
        public virtual ICollection<MsDocumentReqType> EndAYDocumentReqTypes { get; set; }
        public virtual ICollection<TrDocumentReqApplicantDetail> DocumentReqApplicantDetails { get; set; }
        public virtual ICollection<MsEventType> EventTypes { get; set; }
        public virtual ICollection<TrEvent> Events { get; set; }
    }
    internal class MsAcademicYearConfiguration : CodeEntityConfiguration<MsAcademicYear>
    {
        public override void Configure(EntityTypeBuilder<MsAcademicYear> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.AcademicYears)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsAcademicYear_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
