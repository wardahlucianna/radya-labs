using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Attendance
{
    public class MsCard : AuditIsActiveEntity, IStudentEntity
    {
        public string CardID { get; set; }
        public string BinusianID { get; set; }
        public string Audit_Activity { get; set; }
        public DateTime Audit_Time { get; set; }
        public string Audit_User_Name { get; set; }
        public string Kd_Reason { get; set; }
    }

    internal class MsCardConfiguration : ExceptionActiveEntityConfiguration<MsCard>
    {
        public override void Configure(EntityTypeBuilder<MsCard> builder)
        {
            builder.HasKey(x => x.CardID);

            builder.Property(x => x.CardID)
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(x => x.BinusianID)
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasMaxLength(1)
                .IsRequired();

            builder.Property(x => x.Audit_Activity)
                .HasMaxLength(1)
                .IsRequired();

            builder.Property(x => x.Audit_User_Name)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Kd_Reason)
                .HasMaxLength(2);

            base.Configure(builder);
        }
    }
}
