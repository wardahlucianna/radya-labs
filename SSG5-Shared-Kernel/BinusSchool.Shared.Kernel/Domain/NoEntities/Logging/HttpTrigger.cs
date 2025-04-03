using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Domain.NoEntities.Logging
{
    public class HttpTrigger : IEventTrigger
    {
        public string UserAgent { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string Payload { get; set; }
    }
}