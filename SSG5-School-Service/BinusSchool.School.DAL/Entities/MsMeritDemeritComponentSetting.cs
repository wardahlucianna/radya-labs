using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsMeritDemeritComponentSetting : AuditEntity, ISchoolEntity
    {
        public string IdGrade { get; set; }
        public bool IsUsePointSystem { get; set; }
        public bool IsUseDemeritSystem { get; set; }
        public MsGrade Grade { get; set; }
    }
    internal class MsMeritDemeritComponentSettingConfiguration : AuditEntityConfiguration<MsMeritDemeritComponentSetting>
    {
        public override void Configure(EntityTypeBuilder<MsMeritDemeritComponentSetting> builder)
        {
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.MeritDemeritComponentSettings)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsMeritDemeritComponentSetting_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
