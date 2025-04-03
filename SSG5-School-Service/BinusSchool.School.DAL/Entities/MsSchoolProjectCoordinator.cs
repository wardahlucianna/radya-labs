using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsSchoolProjectCoordinator : AuditEntity, ISchoolEntity
    {
        public string IdBinusian { get; set; }
        public string IdSchool { get; set; }
        public string Remarks { get; set; }
        public string PhotoLink { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsSchoolProjectCoordinatorConfiguration : AuditEntityConfiguration<MsSchoolProjectCoordinator>
    {
        public override void Configure(EntityTypeBuilder<MsSchoolProjectCoordinator> builder)
        {
            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36);

            builder.Property(x => x.Remarks)
                .HasMaxLength(128);

            builder.Property(x => x.PhotoLink)
                .HasMaxLength(4000)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.SchoolProjectCoordinators)
                .HasForeignKey(x => x.IdBinusian)
                .HasConstraintName("FK_MsSchoolProjectCoordinator_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.SchoolProjectCoordinators)
                .HasForeignKey(x => x.IdSchool)
                .HasConstraintName("FK_MsSchoolProjectCoordinator_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
