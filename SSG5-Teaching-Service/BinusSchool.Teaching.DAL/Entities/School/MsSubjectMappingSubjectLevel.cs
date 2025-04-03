using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsSubjectMappingSubjectLevel : AuditEntity, ITeachingEntity
    {
        
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public bool IsDefault { get; set; }

        public virtual MsSubject Subject { get; set; }
        public virtual MsSubjectLevel SubjectLevel { get; set; }
        public virtual ICollection<TrLessonPlan> LessonPlans { get; set; }
    }

    internal class MsSubjectMappingSubjectLevelConfiguration : AuditEntityConfiguration<MsSubjectMappingSubjectLevel>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectMappingSubjectLevel> builder)
        {
            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectMappingSubjectLevels)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsSubjectMappingSubjectLevel_MsSubject")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectLevel)
               .WithMany(x => x.SubjectMappingSubjectLevels)
               .HasForeignKey(fk => fk.IdSubjectLevel)
               .HasConstraintName("FK_MsSubjectMappingSubjectLevel_MsSubjectLevel")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
