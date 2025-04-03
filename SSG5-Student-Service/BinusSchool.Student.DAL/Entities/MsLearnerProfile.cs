using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsLearnerProfile : AuditEntity, IStudentEntity
    {        
        public string Name { get; set; }
        public LearnerProfile Type { get; set; }
        public ICollection<TrLearningGoalStudent> LearningGoalStudent { get; set; }
    }

    internal class MsLearnerProfilekConfiguration : AuditEntityConfiguration<MsLearnerProfile>
    {
        public override void Configure(EntityTypeBuilder<MsLearnerProfile> builder)
        {        
            builder.Property(x => x.Name)
                .IsRequired(true)
                .HasMaxLength(100);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired(true);

            base.Configure(builder); 

        }
    }

}
