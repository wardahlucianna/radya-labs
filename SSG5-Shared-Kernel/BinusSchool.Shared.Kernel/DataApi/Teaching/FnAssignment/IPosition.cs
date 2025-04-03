using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Position;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface IPosition : IFnAssignment
    {
        [Get("/assignment/position")]
        Task<ApiErrorResult<IEnumerable<PositionGetResult>>> GetPositions(PositionGetRequest query);
    }
}
