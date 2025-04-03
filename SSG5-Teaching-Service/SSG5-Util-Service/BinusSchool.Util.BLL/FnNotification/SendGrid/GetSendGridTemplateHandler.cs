using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using Newtonsoft.Json;
using SendGrid;
using NamingStrategy = BinusSchool.Common.Utils.NamingStrategy;

namespace BinusSchool.Util.FnNotification.SendGrid
{
    public class GetSendGridTemplateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISendGridClient _sendGridClient;

        public GetSendGridTemplateHandler(ISendGridClient sendGridClient)
        {
            _sendGridClient = sendGridClient;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetSendGridTemplateRequest>();
            if (param.PageSize <= 0)
                param.PageSize = 10;
            
            const string format = @"{
                'generations': 'dynamic',
                'page_size': {0},
                'page_token': '{1}',
                'query': '(template_name LIKE {2})'
            }";
            var queryParams = format
                .Replace("{0}", param.PageSize.ToString())
                .Replace("{1}", param.PageToken)
                .Replace("{2}", "\"" + param.Search + "\"");
            
            var response = await _sendGridClient.RequestAsync(
                BaseClient.Method.GET,
                urlPath: "/templates",
                queryParams: queryParams,
                cancellationToken: CancellationToken);
            var templates = await response.Body.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetSendGridTemplateResult>(templates, SerializerSetting.GetJsonSerializer(true, NamingStrategy.SnakeCase));

            return Request.CreateApiResult2(result.Result as object,
                new Dictionary<string, object>
                {
                    { "totalItem", result._Metadata.Count },
                    { "prevPageToken", GetParamFromUrl(result._Metadata.Prev, "page_token" ) },
                    { "nextPageToken", GetParamFromUrl(result._Metadata.Next, "page_token" ) }
                });
        }

        private string GetParamFromUrl(string url, string paramName)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            
            var uri = new Uri(url);
            var parsed = uri.ParseQueryString();
            
            return parsed[paramName];
        }
    }
}
