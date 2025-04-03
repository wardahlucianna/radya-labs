using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsMeritDemeritMapping : AuditEntity, IStudentEntity
    {
        public string IdGrade { get; set; }
        public MeritDemeritCategory Category { get; set; }
        public string IdLevelOfInteraction { get; set; }
        public string DisciplineName { get; set; }
        public int? Point { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsLevelOfInteraction LevelOfInteraction { get; set; }
        public virtual ICollection<TrEntryMeritStudent> EntryMeritStudents { get; set; }
        public virtual ICollection<TrEntryDemeritStudent> EntryDemeritStudents { get; set; }
    }

    internal class MsMeritDemeritMappingConfiguration : AuditEntityConfiguration<MsMeritDemeritMapping>
    {
        public override void Configure(EntityTypeBuilder<MsMeritDemeritMapping> builder)
        {
            builder.Property(x => x.DisciplineName)
                .HasMaxLength(250)
                .IsRequired();
            builder.Property(x=>x.Category)
                .HasConversion<string>()
                .HasMaxLength(25)
                .IsRequired();
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.MeritDemeritMappings)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsMeritDemeritMapping_MsGrade")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.LevelOfInteraction)
               .WithMany(x => x.MeritDemeritMappings)
               .HasForeignKey(fk => fk.IdLevelOfInteraction)
               .HasConstraintName("FK_MsMeritDemeritMapping_MsLevelOfInteraction")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
