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
    public class MsExtracurricularAttPresentStat : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricularStatusAtt { get; set; }
        public string IdSchool { get; set; }
        public string LevelCode { get; set; }
        public bool IsPresent { get; set; }

        public virtual LtExtracurricularStatusAtt ExtracurricularStatusAtt { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsExtracurricularAttPresentStatConfiguration : AuditEntityConfiguration<MsExtracurricularAttPresentStat>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularAttPresentStat> builder)
        {
            builder.HasOne(x => x.ExtracurricularStatusAtt)
                .WithMany(x => x.ExtracurricularAttPresentStats)
                .HasForeignKey(fk => fk.IdExtracurricularStatusAtt)
                .HasConstraintName("FK_MsExtracurricularAttPresentStat_LtExtracurricularStatusAtt")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.ExtracurricularAttPresentStats)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsExtracurricularAttPresentStat_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.LevelCode)
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
