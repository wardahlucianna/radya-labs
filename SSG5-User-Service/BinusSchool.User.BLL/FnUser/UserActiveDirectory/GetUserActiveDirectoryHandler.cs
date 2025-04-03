using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.UserActiveDirectory;
using Microsoft.Graph;

namespace BinusSchool.User.FnUser.UserActiveDirectory
{
    public class GetUserActiveDirectoryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "id", "displayName", "userPrincipalName", "mail", "companyName" };
        private static readonly string[] _columnsFilter = _columns.Except(new[] { _columns[0], _columns[4] }).ToArray();
        private static readonly string _columnsStr = string.Join(',', _columns);
        
        private readonly GraphServiceClient _graphServiceClient;

        public GetUserActiveDirectoryHandler(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetUserActiveDirectoryRequest>();
            
            var graphQuery = _graphServiceClient.Users
                .Request()
                // .Skip(param.CalculateOffset()) // not supported when get users
                // .Top(param.Size) // move to down
                .Select(_columnsStr);
            
            // if (!string.IsNullOrEmpty(param.OrderBy)) // supported when not doing search
            //     graphQuery = graphQuery.OrderBy($"{param.OrderBy} {param.OrderType}");
            if (!param.GetAll ?? true)
                graphQuery = graphQuery.Top(param.Size);
            if (!string.IsNullOrEmpty(param.NextPageToken))
                graphQuery.QueryOptions.Add(new QueryOption("$skiptoken", param.NextPageToken));
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                var queryFilter = string.Join(" or ", _columnsFilter.Select(x => $"startsWith({x},'{param.Search}')"));
                graphQuery = graphQuery.Filter(queryFilter);
            }
            
            var users = await graphQuery.GetAsync(CancellationToken);
            var userDtos = users.Select(x => new GetUserActiveDirectoryResult
            {
                Id = x.Id,
                Name = x.DisplayName,
                UserPrincipalName = x.UserPrincipalName,
                Email = x.Mail,
                CompanyName = x.CompanyName
            });
            var props = param
                .CreatePaginationProperty(users.Count)
                .AddProperty("nextPageToken".WithValue(users.NextPageRequest?.QueryOptions.First(x => x.Name == "$skiptoken").Value));

            return Request.CreateApiResult2(userDtos as object, props);
        }
    }
}
