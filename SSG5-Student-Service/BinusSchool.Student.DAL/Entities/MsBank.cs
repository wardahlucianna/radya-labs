using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsBank : AuditEntity, IStudentEntity
    {        
        public string BankName { get; set; }
        public string BranchOfficeName { get; set; }
        public string SwiftCode { get; set; }
        public string BICode { get; set; }
        public virtual ICollection<MsBankAccountInformation> BankAccountInformation { get; set; }
    }

    internal class MsBankConfiguration : AuditEntityConfiguration<MsBank>
    {
        public override void Configure(EntityTypeBuilder<MsBank> builder)
        {        
            builder.Property(x => x.BankName)             
                .HasMaxLength(100);

            builder.Property(x => x.BranchOfficeName)             
                .HasMaxLength(200);    

             base.Configure(builder); 

        }
    }

}
