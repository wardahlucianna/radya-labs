using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.StudentAchivementToPdf;
using Refit;
namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface IStudentAchievementToPdf : IFnConverter
    {
        [Get("/student-achievement-to-pdf")]
        Task<StudentAchivementToPdfResult> ConvertStudentAchievementToPdf([Query] StudentAchievementToPdfRequest param);
    }
}
