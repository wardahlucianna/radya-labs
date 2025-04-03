using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsEmailRecepient : AuditEntity, ISchedulingEntity
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdBinusian { get; set; }
        public bool IsCC { get; set; }
        public TypeEmailRecepient Type { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
    }
    internal class MsEmailRecepientConfiguration : AuditEntityConfiguration<MsEmailRecepient>
    {
        public override void Configure(EntityTypeBuilder<MsEmailRecepient> builder)
        {
            builder.Property(e => e.Type)
                .HasMaxLength(maxLength: 100)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (TypeEmailRecepient)Enum.Parse(typeof(TypeEmailRecepient), valueFromDb))
                .IsRequired();

            builder.HasOne(x => x.Role)
               .WithMany(x => x.EmailRecepients)
               .HasForeignKey(fk => fk.IdRole)
               .HasConstraintName("FK_MsEmailRecepient_LtRole")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
               .WithMany(x => x.EmailRecepients)
               .HasForeignKey(fk => fk.IdTeacherPosition)
               .HasConstraintName("FK_MsEmailRecepient_MsTeacherPosition")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Staff)
               .WithMany(x => x.EmailRecepients)
               .HasForeignKey(fk => fk.IdBinusian)
               .HasConstraintName("FK_MsEmailRecepient_MsStaff")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
    
}
