using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentExitReason : AuditEntity, IStudentEntity
    {
        public string IdMsStudentExitReason { get; set; }
        public string IdStudentExit { get; set; }
        public virtual MsStudentExitReason StudentExitReason { get; set; }
        public virtual TrStudentExit StudentExit { get; set; }
    }

    internal class TrStudentExitReasonConfiguration : AuditEntityConfiguration<TrStudentExitReason>
    {
        public override void Configure(EntityTypeBuilder<TrStudentExitReason> builder)
        {
            builder.HasOne(x => x.StudentExit)
              .WithMany(y => y.TrStudentExitReasons)
              .HasForeignKey(fk => fk.IdStudentExit)
              .HasConstraintName("FK_TrStudentExitReason_TrStudentExit")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.StudentExitReason)
              .WithMany(y => y.TrStudentExitReasons)
              .HasForeignKey(fk => fk.IdMsStudentExitReason)
              .HasConstraintName("FK_TrStudentExitReason_MsStudentExitReasons")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
