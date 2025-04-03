using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentEmailAdditionalReceiver : AuditEntity, IStudentEntity
    {
        public string Scenario { get; set; }
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public string AddressType { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsSchool School { get; set; }
    }
    internal class MsStudentEmailAdditionalReceiverConfiguration : AuditEntityConfiguration<MsStudentEmailAdditionalReceiver>
    {
        public override void Configure(EntityTypeBuilder<MsStudentEmailAdditionalReceiver> builder)
        {
            builder.Property(x => x.IdUser)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.AddressType)
              .HasMaxLength(10)
              .IsRequired();

            builder.Property(x => x.Scenario)
                .HasMaxLength(15)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.StudentEmailAdditionalReceivers)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsStudentEmailAdditionalReceiver_MsUser")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.School)
                .WithMany(x => x.StudentEmailAdditionalReceivers)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsStudentEmailAdditionalReceiver_MsSchool")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
