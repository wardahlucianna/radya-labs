using System;
using System.Collections.Generic;

namespace BinusSchool.Domain.Abstractions
{
    /// <summary>
    /// Marker interface to mark a class as a seed
    /// </summary>
    /// <typeparam name="T">class which implement IEntity</typeparam>
    public interface ISeed<T> where T : IEntity
    {
        IEnumerable<T> GetSeeds();
    }
}