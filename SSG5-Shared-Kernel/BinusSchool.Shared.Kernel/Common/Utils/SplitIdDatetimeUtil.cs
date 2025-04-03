using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Common.Utils
{
    public class SplitIdDatetimeUtil
    {
        public class SplitIdDatetimeUtilResponse
        {
            public string Id { get; set; }
            public string ErrorMessage { get; set; }
            public bool IsValid { get; set; }
        }
        public static SplitIdDatetimeUtilResponse Split(string stringInputText)
        {
            var result = new SplitIdDatetimeUtilResponse();
            result.IsValid = true;

            if (!stringInputText.Contains("#"))
            {
                result.ErrorMessage = "Invalid format 1";
                result.IsValid = false;
                return result;
            }

            var split = stringInputText.Split('#');
            var dateNow = split[1];
            result.Id = split[0];
            
            if (dateNow != DateTimeUtil.ServerTime.ToString("ddMMyyyy"))
            {
                result.ErrorMessage = "Invalid format 2";
                result.IsValid = false;
            }

            return result;
        }
    }
}
