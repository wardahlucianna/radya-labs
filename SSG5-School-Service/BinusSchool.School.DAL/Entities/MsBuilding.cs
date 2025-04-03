using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsBuilding : CodeEntity, ISchoolEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsVenue> Venues { get; set; }
        public virtual ICollection<MsFloor> Floors { get; set; }
        public virtual ICollection<MsLocker> Lockers { get; set; }
    }

    internal class MsBuildingConfiguration : CodeEntityConfiguration<MsBuilding>
    {
        public override void Configure(EntityTypeBuilder<MsBuilding> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.Buildings)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsBuilding_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
