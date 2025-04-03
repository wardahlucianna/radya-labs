using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.Abstractions
{
    public interface ISeedHandler
    {
        Task PlantSeed<TContext>(TContext dbContext) where TContext : DbContext;
    }
}