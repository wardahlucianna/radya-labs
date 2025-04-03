using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrMessageForGrade : AuditEntity, IUserEntity
    {
        public string IdMessageFor { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public int? Semester { get; set; }
        public virtual TrMessageFor MessageFor { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
    }

    internal class TrMessageForGradeConfiguration : AuditEntityConfiguration<TrMessageForGrade>
    {
        public override void Configure(EntityTypeBuilder<TrMessageForGrade> builder)
        {
            builder.HasOne(x => x.MessageFor)
               .WithMany(x => x.MessageForGrades)
               .HasForeignKey(fk => fk.IdMessageFor)
               .HasConstraintName("FK_TrMessageForGrade_TrMessageFor")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Level)
               .WithMany(x => x.MessageForGrades)
               .HasForeignKey(fk => fk.IdLevel)
               .HasConstraintName("FK_TrMessageForGrade_MsLevel")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Grade)
               .WithMany(x => x.MessageForGrades)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_TrMessageForGrade_MsGrade")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Homeroom)
               .WithMany(x => x.MessageForGrades)
               .HasForeignKey(fk => fk.IdHomeroom)
               .HasConstraintName("FK_TrMessageForGrade_MsHomeroom")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
