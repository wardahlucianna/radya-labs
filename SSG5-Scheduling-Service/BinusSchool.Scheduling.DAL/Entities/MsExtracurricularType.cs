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
    public class MsExtracurricularType : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public bool IsDefault { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsExtracurricular> Extracurriculars { get; set; }
    }

    internal class MsExtracurricularTypeConfiguration : CodeEntityConfiguration<MsExtracurricularType>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularType> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36);

            builder.HasOne(x => x.School)
                .WithMany(x => x.ExtracurricularTypes)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsExtracurricularType_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
