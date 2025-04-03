using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Teaching
{
    public class LtPosition : CodeEntity, IStudentEntity
    {
        public int PositionOrder { get; set; }
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
        public virtual ICollection<MsCounselor> Counselors { get; set; }
    }

    internal class LtPositionConfiguration : CodeEntityConfiguration<LtPosition>
    {
        public override void Configure(EntityTypeBuilder<LtPosition> builder)
        {
            builder.HasIndex(x => x.Code)
                .IsUnique();
            
            base.Configure(builder);
        }
    }
}
