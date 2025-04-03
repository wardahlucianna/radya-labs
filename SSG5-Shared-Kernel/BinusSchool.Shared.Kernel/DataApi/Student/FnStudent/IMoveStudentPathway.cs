using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MoveStudentPathway;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMoveStudentPathway : IFnStudent
    {
        [Post("/student/move-pathway")]
        Task<ApiErrorResult> AddMoveStudentPathway([Body] AddMoveStudentPathwayRequest body);
    }
}