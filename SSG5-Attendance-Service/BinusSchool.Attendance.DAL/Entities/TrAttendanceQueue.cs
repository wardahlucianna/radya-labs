using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceQueue : AuditEntity, IAttendanceEntity
    {
        public string TriggerFrom { get; set; }
        public string QueueName { get; set; }
        public string Data { get; set; }
        public bool IsExecuted { get; set; }
        public DateTime? StartExecutedDate { get; set; }
        public DateTime? EndExecutedDate { get; set; }
        public bool Status { get; set; }
        public string Description { get; set; }

    }

    internal class TrAttendanceQueueConfiguration : AuditEntityConfiguration<TrAttendanceQueue>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceQueue> builder)
        {
            builder.Property(x => x.TriggerFrom)
            .HasMaxLength(200)
            .IsRequired();

            builder.Property(x => x.QueueName)
              .HasMaxLength(128)
              .IsRequired();

            builder.Property(x => x.Data)
              .IsRequired();

            builder.Property(x => x.StartExecutedDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.EndExecutedDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.Description)
               .HasMaxLength(200);

            base.Configure(builder);
        }
    }

}
