using System.Linq;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Entities
{
    /// <summary>
    /// Entity base with Id that has no Primary Key
    /// </summary>
    public abstract class UniquelessEntity : ActiveEntity, IUniqueId
    {
        public string Id { get; set; }
    }

    public class UniquelessEntityConfiguration<T> : ActiveEntityConfiguration<T> where T : UniquelessEntity, IEntity
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            // // customize table name when implement type IKindDb
            // // that means this table is reference table from different database
            // if (typeof(IKindDb).IsAssignableFrom(typeof(T)))
            // {
            //     var kinds = typeof(T).GetInterfaces().Where(x => typeof(IKindDb).IsAssignableFrom(x) && x != typeof(IKindDb));
            //     if (kinds.Count() > 2)
            //         throw new System.Exception("Can only implement one kind database.");
                
            //     var tableName = string.Format("{0}[BSS_{1}_DB]", typeof(T).Name, kinds.First().Name[1..^2].ToUpper());
            //     builder.ToTable(tableName);
            // }

            // set table name
            // var tableName = typeof(T).Name switch
            // {
            //     string val when val.StartsWith("Hs") => val.Remove(1, 1), // HsEntityName => HEntityName
            //     // string val when val.StartsWith("Tr") => val.Insert(0, "H"), // TrEntityName => HTrEntityName
            //     _ => typeof(T).Name
            // };
            // builder.ToTable(tableName);

            // set column name with format Id{EntityName}
            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(T).Name.Remove(0, 2))
                .HasMaxLength(36);
            
            base.Configure(builder);
        }
    }
}