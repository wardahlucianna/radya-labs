using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class HMsFormDocChangeNote : AuditEntity, IDocumentEntity
    {
        public string IdsFormDocChange { get; set; }
        public string FieldName { get; set; }
        public string Note { get; set; }

        public virtual HMsFormDocChange FormDocChange { get; set; }
    }

    internal class HsFormDocChangeNoteConfiguration : AuditEntityConfiguration<HMsFormDocChangeNote>
    {
        public override void Configure(EntityTypeBuilder<HMsFormDocChangeNote> builder)
        {
            builder.Property(x => x.FieldName)
                .IsRequired();
            
            builder.Property(x => x.Note)
                .IsRequired();

            builder.HasOne(x => x.FormDocChange)
                .WithMany(x => x.DocChangeNotes)
                .HasForeignKey(fk => fk.IdsFormDocChange)
                .HasConstraintName("FK_HsDocChangeNote_HsFormDocChange")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
