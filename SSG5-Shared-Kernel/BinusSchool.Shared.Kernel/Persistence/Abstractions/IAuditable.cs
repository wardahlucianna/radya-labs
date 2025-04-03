using System;

namespace BinusSchool.Persistence.Abstractions
{
    public interface IAuditable : IActiveState
    {
        string UserIn { get; set; }
        DateTime? DateIn { get; set; }
        string UserUp { get; set; }
        DateTime? DateUp { get; set; }
    }
}
