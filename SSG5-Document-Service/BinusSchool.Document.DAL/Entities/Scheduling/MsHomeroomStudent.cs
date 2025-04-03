using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Scheduling
{
    public class MsHomeroomStudent : AuditEntity, IDocumentEntity
    {
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
        public int Semester { get; set; }
        public Gender Gender { get; set; }
        public string Religion { get; set; }

        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual ICollection<TrBLPGroupStudent> BLPGroupStudents { get; set; }
        public virtual ICollection<TrBLPUpdatedConsentStatus> BLPUpdatedConsentStatuses { get; set; }
    }
    internal class MsHomeroomStudentConfiguration : AuditEntityConfiguration<MsHomeroomStudent>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomStudent> builder)
        {

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36);

            builder.Property(x => x.Religion)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdHomeroom)
                .HasMaxLength(36);

            builder.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(6)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomStudent_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsHomeroomStudent_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
