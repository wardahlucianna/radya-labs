using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using Refit;

namespace BinusSchool.Data.Api.Workflow.FnWorkflow
{
    public interface IWorkflow : IFnWorkflow
    {
        [Get("/workflow")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetListWorkflow(CollectionSchoolRequest query);

        [Get("/workflow/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetDetailWorkflow(string id);
    }
}
