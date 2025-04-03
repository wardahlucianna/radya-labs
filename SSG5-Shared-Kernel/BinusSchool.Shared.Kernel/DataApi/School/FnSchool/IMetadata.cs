using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IMetadata : IFnSchool
    {
        [Get("/school/metadata")]
        Task<ApiErrorResult<GetMetadataResult>> GetMetadata(GetMetadataRequest body);
    }
}