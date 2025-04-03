using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Common.Functions
{
    public class CurrentFunctions : ICurrentFunctions
    {
        public string Domain { get; }
        public string Functions { get; }
        public Type DbContext { get; }
        public Assembly Assembly { get; }

        private readonly Lazy<string[]> _nameSpaces;

        public CurrentFunctions(Type functionsType, Type appDbContext)
        {
            _nameSpaces = new Lazy<string[]>(() => functionsType.Namespace.Split('.').ToArray());
            Domain = _nameSpaces.Value[1];
            Functions = _nameSpaces.Value[^1];
            Assembly = functionsType.Assembly;
            DbContext = appDbContext;
        }
    }
}
