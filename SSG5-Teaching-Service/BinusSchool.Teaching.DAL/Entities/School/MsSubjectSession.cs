using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsSubjectSession : AuditEntity, ITeachingEntity
    {
        public string IdSubject { get; set; }
        public int Content { get; set; }
        public int Length { get; set; }

        public virtual MsSubject Subject { get; set; }
    }

    internal class MsSubjectSessionConfiguration : AuditEntityConfiguration<MsSubjectSession>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectSession> builder)
        {
            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectSessions)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsSubjectSession_MsSubject")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
