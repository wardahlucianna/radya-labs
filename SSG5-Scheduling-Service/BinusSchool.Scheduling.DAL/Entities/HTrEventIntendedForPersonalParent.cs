using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForPersonalParent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdParent { get; set; }
        public string IdEventIntendedFor { get; set; }
        public virtual MsParent Parent { get; set; }
        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
    }

    internal class HTrEventIntendedForPersonalParentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForPersonalParent>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForPersonalParent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForPersonalParent).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventIntendedFor)
            .WithMany(x => x.EventIntendedForPersonalParents)
            .HasForeignKey(fk => fk.IdEventIntendedFor)
            .HasConstraintName("FK_HTrEventIntendedForPersonalParent_HTrEventIntendedFor")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.Parent)
            .WithMany(x => x.HistoryEventIntendedForPersonalParents)
            .HasForeignKey(fk => fk.IdParent)
            .HasConstraintName("FK_HTrEventIntendedForPersonalParent_MsParent")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
