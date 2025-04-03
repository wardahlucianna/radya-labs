using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrTappingTransaction : AuditNoActivityEntity, IAttendanceEntity
    {
        public string DeviceID { get; set; }
        public string CardUID { get; set; }
        public DateTime TransactionTime { get; set; }
        public string Binusian_id { get; set; }
        public string IdTappingTransaction { get; set; }

        public virtual MsCard Card { get; set; }
    }

    internal class TrTappingTransactionConfiguration : AuditNoActivityEntityConfiguration<TrTappingTransaction>
    {
        public override void Configure(EntityTypeBuilder<TrTappingTransaction> builder)
        {
            builder.HasKey(x => x.DeviceID);

            builder.Property(x => x.DeviceID)
                .HasMaxLength(50);

            builder.Property(x => x.CardUID)
                .HasMaxLength(50);

            builder.Property(x => x.Binusian_id)
                .HasMaxLength(16);

            builder.Property(x => x.Binusian_id)
                .HasMaxLength(36);

            builder.HasOne(x => x.Card)
                .WithMany(x => x.TrTappingTransactions)
                .HasForeignKey(fk => fk.CardUID)
                .HasConstraintName("FK_TrTappingTransaction_MsCard")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
