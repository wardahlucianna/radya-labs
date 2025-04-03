using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsSubjectLevel : CodeEntity, ITeachingEntity
    {
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }

        public virtual ICollection<MsSubjectMappingSubjectLevel> SubjectMappingSubjectLevels { get; set; }
    }

    internal class MsSubjectLevelConfiguration : CodeEntityConfiguration<MsSubjectLevel>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectLevel> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectLevels)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSubjectLevel_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
