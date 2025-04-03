using System.Collections.Generic;

namespace BinusSchool.Data.Configurations
{
    public class BinusSchoolApiConfiguration2
    {
        public BinusSchoolApimConfiguration Apim { get; set; }
        public IDictionary<string, IDictionary<string, BinusSchoolFunctionConfiguration>> Function { get; set; }
    }

    public class BinusSchoolApimConfiguration : ApiConfigurationWithName
    {
        public IDictionary<string, string> Header { get; set; }
    }

    public class BinusSchoolFunctionConfiguration : BinusSchoolApimConfiguration
    {
        public string Prefix { get; set; }
    }
}