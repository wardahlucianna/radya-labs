using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Domain.NoEntities.Logging
{
    public class LogTrigger<T> : UniqueNoEntity
        where T : IEventTrigger
    {
        public bool IsSuccessExecute { get; set; }
        public string Trigger { get; set; }
        public T Property { get; set; }
    }
}