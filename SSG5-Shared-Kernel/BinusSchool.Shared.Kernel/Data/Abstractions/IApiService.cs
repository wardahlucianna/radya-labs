using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Abstractions
{
    [Obsolete(
        "Retrieve Refit interface service directly rater than wrap with IApiService<T>. " +
        "For Functions app, need to explicitly set useLegacyHttpClient to False & register api domains will be consumed. " +
        "See FnUserStartup class for example.")]
    public interface IApiService<T> where T : class
    {
        T Execute { get; }
        void With(string host = null, params KeyValuePair<string, string>[] headers);
    }
}