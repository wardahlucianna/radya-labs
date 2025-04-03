using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.SchedulingDb.Abstractions
{
    public interface ISchedulingDbContext : IAppDbContext<ISchedulingEntity> { }
}
