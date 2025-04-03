using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularSupportDoc : AuditEntity, ISchedulingEntity
    {
        public string Name { get; set; }
        public bool ShowToParent { get; set; }
        public bool ShowToStudent { get; set; }
        public bool Status { get; set; }

        public string FileName { get; set; }
        public decimal FileSize { get; set; }
        public virtual ICollection<MsExtracurricularSupportDocGrade> ExtracurricularSupportDocGrades { get; set; }
    }

    internal class MsExtracurricularSupportDocConfiguration : AuditEntityConfiguration<MsExtracurricularSupportDoc>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularSupportDoc> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();


            builder.Property(x => x.FileName)
                .HasMaxLength(250);

            builder.Property(x => x.FileSize)
                .HasColumnType("DECIMAL(7,2)");

            base.Configure(builder);
        }
    }
}
