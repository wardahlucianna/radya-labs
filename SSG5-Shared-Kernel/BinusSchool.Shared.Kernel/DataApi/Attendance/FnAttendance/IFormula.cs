using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Formula;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IFormula : IFnAttendance
    {
        [Get("/formula/detail/{idLevel}")]
        Task<ApiErrorResult<FormulaResult>> GetFormulaDetail(string idLevel);

        [Post("/formula")]
        Task<ApiErrorResult> SetFormula([Body] SetFormulaRequest body);
    }
}
