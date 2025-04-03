using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsDivision : CodeEntity, ISchoolEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsClassroomDivision> ClassroomDivisions { get; set; }
    }

    internal class MsDivisionConfiguration : CodeEntityConfiguration<MsDivision>
    {
        public override void Configure(EntityTypeBuilder<MsDivision> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.Divisions)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsDivision_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            base.Configure(builder);
        }
    }
}
