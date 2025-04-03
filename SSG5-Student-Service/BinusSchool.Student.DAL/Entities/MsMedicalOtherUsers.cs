using System;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.Formula.Functions;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsMedicalOtherUsers : AuditEntity, IStudentEntity
    {
        public string MedicalOtherUsersName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsMedicalOtherUsersConfiguration : AuditEntityConfiguration<MsMedicalOtherUsers>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalOtherUsers> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalOtherUsers)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsMedicalOtherUsers")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.MedicalOtherUsersName)
                .HasMaxLength(100);

            builder.Property(x => x.PhoneNumber)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
