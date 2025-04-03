using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsCounselorOption : CodeEntity, IStudentEntity
    {
        public string IdSchool { get; set; }
        public string Value { get; set; }
        public virtual MsSchool School { get; set; }
    }
    internal class MsMessageOptionConfiguration : CodeEntityConfiguration<MsCounselorOption>
    {
        public override void Configure(EntityTypeBuilder<MsCounselorOption> builder)
        {
            builder.HasOne(x => x.School)
               .WithMany(x => x.CounselorOption)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsCounselorOption_MsSchool")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
            base.Configure(builder);
        }
    }
}
