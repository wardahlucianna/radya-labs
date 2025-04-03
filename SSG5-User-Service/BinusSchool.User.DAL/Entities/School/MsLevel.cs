using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsLevel : CodeEntity, IUserEntity
    {
        public string IdAcademicYear { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsAcademicYear MsAcademicYear { get; set; }
        public virtual ICollection<MsGrade> MsGrades { get; set; }
        public virtual ICollection<TrMessageForGrade> MessageForGrades { get; set; }
    }

    internal class MsLevelConfiguration : CodeEntityConfiguration<MsLevel>
    {
        public override void Configure(EntityTypeBuilder<MsLevel> builder)
        {
            builder.HasOne(x => x.MsAcademicYear)
                .WithMany(x => x.MsLevels)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
