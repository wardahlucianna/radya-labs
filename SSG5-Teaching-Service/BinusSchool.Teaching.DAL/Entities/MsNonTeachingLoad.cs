using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsNonTeachingLoad : AuditNoUniqueEntity, ITeachingEntity
    {
        public string Id { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdAcademicYear { get; set; }
        public AcademicType Category { get; set; }
        public int Load { get; set; }
        public string Parameter { get; set; }

        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual ICollection<TrNonTeachingLoad> TrNonTeachingLoads { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class MsNonTeachingLoadConfiguration : AuditNoUniqueEntityConfiguration<MsNonTeachingLoad>
    {
        public override void Configure(EntityTypeBuilder<MsNonTeachingLoad> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(MsNonTeachingLoad).Name)
                .HasMaxLength(36);


            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.NonTeachingLoads)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsNonTeachingLoad_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.Category)
                .IsRequired();
                
            builder.Property(x => x.Load)
                .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
                .WithMany(x => x.NonTeachingLoads)
                .HasForeignKey(fk => fk.IdTeacherPosition)
                .HasConstraintName("FK_MsNonTeachingLoad_MsTeacherPosition")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
