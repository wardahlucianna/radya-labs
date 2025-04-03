using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsBuilding : CodeEntity, ITeachingEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsVenue> Venues { get; set; }
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
