using System;
using System.Reflection;

namespace BinusSchool.Common.Functions.Abstractions
{
    public interface ICurrentFunctions
    {
        string Domain { get; }
        Type DbContext { get; }
        Assembly Assembly { get; }
    }
}
