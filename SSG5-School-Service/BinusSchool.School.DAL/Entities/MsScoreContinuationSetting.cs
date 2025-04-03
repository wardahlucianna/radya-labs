using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsScoreContinuationSetting : AuditEntity, ISchoolEntity
    {
        public string IdGrade { get; set; }
        public MeritDemeritCategory Category { get; set; }
        public ScoreContinueOption ScoreContinueOption { get; set; }
        public int? Score { get; set; }
        public ScoreContinueEvery ScoreContinueEvery { get; set; }
        public OperationOption Operation { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class MsScoreContinuationSettingConfiguration : AuditEntityConfiguration<MsScoreContinuationSetting>
    {
        public override void Configure(EntityTypeBuilder<MsScoreContinuationSetting> builder)
        {
            builder.Property(x => x.Category)
               .HasConversion<string>()
               .HasMaxLength(25)
               .IsRequired();

            builder.Property(x => x.ScoreContinueOption)
                 .HasConversion<string>()
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(x => x.ScoreContinueEvery)
                 .HasConversion<string>()
                .HasMaxLength(12)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.ScoreContinuationSettings)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsScoreContinuationSetting_MsGrade")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Property(e => e.Operation).HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (OperationOption)Enum.Parse(typeof(OperationOption), valueFromDb))
               .IsRequired();

            base.Configure(builder);
        }
    }
}
