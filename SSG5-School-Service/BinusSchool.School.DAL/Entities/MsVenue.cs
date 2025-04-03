using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsVenue : CodeEntity, ISchoolEntity
    {
        public string IdBuilding { get; set; }
        [Required]
        public int Capacity { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
        public virtual ICollection<MsVenueEquipment> VenueEquipments { get; set; }

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

            builder.Property(x => x.Capacity).HasComment("kapasitas dari venue tersebut");

            //builder.Property(x => x.Capacity).HasConversion<string>();

            base.Configure(builder);
        }
    }
}
