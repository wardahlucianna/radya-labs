using System;
using BinusSchool.Common.Utils;

namespace BinusSchool.Common.Abstractions
{
    public interface IMachineDateTime
    {
        DateTime ServerTime { get; }
    }

    public class MachineDateTime : IMachineDateTime
    {
        public DateTime ServerTime => DateTimeUtil.ServerTime;
    }
}
