using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsAnswerSetOption : AuditEntity, ISchoolEntity
    {
        public string IdAnswerSet { get; set; }
        public string AnswerSetOptionName { get; set; }
        public int Order { get; set; }
        public virtual MsAnswerSet AnswerSet { get; set; }
    }

    internal class MsAnswerSetOptionConfiguration : AuditEntityConfiguration<MsAnswerSetOption>
    {
        public override void Configure(EntityTypeBuilder<MsAnswerSetOption> builder)
        {
            builder.Property(x => x.AnswerSetOptionName)
                .IsRequired();

            builder.HasOne(x => x.AnswerSet)
              .WithMany(x => x.AnswerSetOptions)
              .HasForeignKey(fk => fk.IdAnswerSet)
              .HasConstraintName("FK_MsAnswerSetOption_MsAnswerSet")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
