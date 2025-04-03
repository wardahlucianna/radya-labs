using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.DocumentDb.Entities.User;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsSchool : AuditEntity, IDocumentEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Logo { get; set; }
        public string Telephone { get; set; }
        public string Ext { get; set; }

        public virtual ICollection<MsAcademicYear> AcademicYears { get; set; }
        public virtual ICollection<MsStudent> Students { get; set; }
        public virtual ICollection<MsClassroom> Classrooms { get; set; }
        public virtual ICollection<MsSurveySection> SurveySections { get; set; }
        public virtual ICollection<MsBLPSetting> BLPEmails { get; set; }
        public virtual ICollection<LtBLPStatus> BLPStatuses { get; set; }
        public virtual ICollection<LtRole> Roles { get; set; }
        public virtual ICollection<MsSourceData> SourceDatas { get; set; }
        public virtual ICollection<MsDocCategory> DocCategories { get; set; }
        public virtual ICollection<MsForm> Forms { get; set; }
        public virtual ICollection<MsBuilding> Buildings { get; set; }
        public virtual ICollection<MsStaff> Staffs { get; set; }
        public virtual ICollection<MsDocumentReqType> DocumentReqTypes { get; set; }
        public virtual ICollection<MsDocumentReqApprover> DocumentReqApprovers { get; set; }
        public virtual ICollection<MsDocumentReqCollectionVenue> DocumentReqCollectionVenues { get; set; }
        public virtual ICollection<LtDocumentReqPaymentMethod> LtDocumentReqPaymentMethods { get; set; }
        public virtual ICollection<MsDocumentReqOptionCategory> DocumentReqOptionCategories { get; set; }
        public virtual ICollection<MsDocumentReqApplicant> DocumentReqApplicants { get; set; }
    }

    internal class MsSchoolConfiguration : AuditEntityConfiguration<MsSchool>
    {
        public override void Configure(EntityTypeBuilder<MsSchool> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.Logo)
            .HasMaxLength(900);

            base.Configure(builder);
        }
    }
}
