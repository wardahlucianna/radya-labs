using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.UserDb.Entities.School;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrLogQueueMessage : AuditEntity, IUserEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsDone { get; set; }
        public bool IsError { get; set; }
        public bool IsProcess { get; set; }
        public string ErrorMessage { get; set; }
        public string IdSchool { get; set; }
        public MsSchool School { get; set; }
    }

    internal class TrLogQueueMessageConfiguration : AuditEntityConfiguration<TrLogQueueMessage>
    {
        public override void Configure(EntityTypeBuilder<TrLogQueueMessage> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.LogQueueMessages)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_TrLogQueueMessage_MsSchools")
                .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired();

            base.Configure(builder);
        }
    }
}
