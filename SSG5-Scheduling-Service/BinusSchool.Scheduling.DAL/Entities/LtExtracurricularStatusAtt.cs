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
    public class LtExtracurricularStatusAtt : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public bool IsPresent { get; set; }
        public bool NeedReason { get; set; }
        public bool? IsVisible { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrExtracurricularAttendanceEntry> ExtracurricularAttendanceEntries { get; set; }
        public virtual ICollection<MsExtracurricularAttPresentStat> ExtracurricularAttPresentStats { get; set; }
    }
    internal class LtExtracurricularStatusAttConfiguration : AuditEntityConfiguration<LtExtracurricularStatusAtt>
    {
        public override void Configure(EntityTypeBuilder<LtExtracurricularStatusAtt> builder)
        {
            builder.Property(p => p.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.ExtracurricularStatusAtts)
                .IsRequired()
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_LtExtracurricularStatusAtt_MsSchool")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }

}
