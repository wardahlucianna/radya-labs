using System;
using System.Reflection;

namespace BinusSchool.Data
{
    public class RefitUrlParameterFormatter : Refit.DefaultUrlParameterFormatter
    {
        public override string Format(object parameterValue, ICustomAttributeProvider attributeProvider, Type type)
        {
            if(parameterValue != null && typeof(DateTime?).IsAssignableFrom(parameterValue.GetType()))
                return ((DateTime)parameterValue).ToString("s"); 

            return base.Format(parameterValue, attributeProvider, type);
        }
    }
}
