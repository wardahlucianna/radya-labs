using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsVenue : CodeEntity, ITeachingEntity
    {
        public string IdBuilding { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual ICollection<TrTimetablePrefDetail> TimetablePrefDetails { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
    }

    internal class MsVenueConfiguration : CodeEntityConfiguration<MsVenue>
    {
        public override void Configure(EntityTypeBuilder<MsVenue> builder)
        {
            builder.HasOne(x => x.Building)
                .WithMany(x => x.Venues)
                .HasForeignKey(fk => fk.IdBuilding)
                .HasConstraintName("FK_MsVenue_MsBuilding")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
