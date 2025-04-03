using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularGroup : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsExtracurricular> Extracurriculars { get; set; }
    }

    internal class MsExtracurricularGroupConfiguration : AuditEntityConfiguration<MsExtracurricularGroup>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularGroup> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36);

            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Status)              
                .IsRequired();

            builder.HasOne(x => x.School)
              .WithMany(x => x.ExtracurricularGroups)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsExtracurricularGroup_MsSchool")
              .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
