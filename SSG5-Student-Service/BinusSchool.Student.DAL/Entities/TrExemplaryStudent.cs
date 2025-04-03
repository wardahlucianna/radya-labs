using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrExemplaryStudent : AuditEntity, IStudentEntity
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
        public string IdExemplary { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual TrExemplary Exemplary { get; set; }
    }

    internal class TrExemplaryStudentConfiguration : AuditEntityConfiguration<TrExemplaryStudent>
    {
        public override void Configure(EntityTypeBuilder<TrExemplaryStudent> builder)
        {

            builder.Property(x => x.IdHomeroom)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdStudent)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdExemplary)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.ExemplaryStudents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_TrExemplaryStudent_MsHomeroom")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.ExemplaryStudents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrExemplaryStudent_MsStudent")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Exemplary)
                .WithMany(x => x.ExemplaryStudents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExemplary)
                .HasConstraintName("FK_TrExemplaryStudent_TrExemplary")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
