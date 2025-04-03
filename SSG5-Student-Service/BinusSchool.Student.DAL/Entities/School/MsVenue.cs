using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsVenue : CodeEntity, IStudentEntity
    {
        public string IdBuilding { get; set; }
        [Required]
        public int Capacity { get; set; }
        public virtual MsBuilding Building { get; set; }
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

            /*builder.Property(x => x.IdBuilding)
                .HasMaxLength(36)
                .IsRequired();*/

            builder.Property(x => x.Capacity).HasComment("kapasitas dari venue tersebut");

            //builder.Property(x => x.Capacity).HasConversion<string>();

            base.Configure(builder);
        }
    }
}
