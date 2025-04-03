using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Configurations
{
    public class ApiConfiguration
    {
        public string Host { get; set; }
        public int Timeout { get; set; }
        public IDictionary<string, string> Secret { get; set; }
    }

    public class ApiConfigurationWithName : ApiConfiguration
    {
        public string Name { get; set; }
    }
}