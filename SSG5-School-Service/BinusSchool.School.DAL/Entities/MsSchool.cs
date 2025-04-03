using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsSchool : AuditEntity, ISchoolEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Logo {get;set;}
        public string Telephone { get; set; }
        public string Ext { get; set; }
        public virtual ICollection<MsAcademicYear> AcademicYears { get; set; }
        public virtual ICollection<MsBuilding> Buildings { get; set; }
        public virtual ICollection<MsCurriculum> Curriculums { get; set; }
        public virtual ICollection<MsDivision> Divisions { get; set; }
        public virtual ICollection<MsClassroom> Classrooms { get; set; }
        public virtual ICollection<MsSessionSet> SessionSets { get; set; }
        public virtual ICollection<MsSubjectType> SubjectTypes { get; set; }
        public virtual ICollection<MsSubjectGroup> SubjectGroups { get; set; }
        public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<MsLevelOfInteraction> LevelOfInteractions { get; set; }
        public virtual ICollection<MsFeatureSchool> FeatureSchools { get; set; }
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
        public virtual ICollection<MsTextbookSettingApproval> TextbookSettingApprovals { get; set; }
        public virtual ICollection<MsBanner> Banners { get; set; }
        public virtual ICollection<LtRole> Roles { get; set; }
        public virtual ICollection<MsVenueReservationRule> VenueReservationRules { get; set; }
        public virtual ICollection<MsReservationOwner> ReservationOwners { get; set; }
        public virtual ICollection<LtVenueType> VenueTypes { get; set; }
        public virtual ICollection<MsEquipmentType> EquipmentTypes { get; set; }
        public virtual ICollection<MsProjectFeedback> ProjectFeedbacks { get; set; }
        public virtual ICollection<MsSchoolProjectCoordinator> SchoolProjectCoordinators { get; set; }
    }

    internal class MsSchoolConfiguration : AuditEntityConfiguration<MsSchool>
    {
        public override void Configure(EntityTypeBuilder<MsSchool> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Logo)
            .HasMaxLength(900);

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
