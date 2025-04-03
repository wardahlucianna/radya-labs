using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction.Status;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction.Type;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMasterServiceAsAction : IFnStudent
    {
        [Get("/student/master-service-as-action/get-list-service-as-action-status-temporary")]
        Task<ApiErrorResult<IEnumerable<GetListServiceAsActionStatusResult>>> GetListExperienceStatusTemporary(GetListServiceAsActionStatusRequest query);

        [Get("/student/master-service-as-action/get-list-service-as-action-location")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListExperienceLocation(GetListServiceAsActionLocationRequest query);

        [Get("/student/master-service-as-action/get-list-service-as-action-type")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListExperienceType(GetListExperienceTypeRequest query);

        [Get("/student/master-service-as-action/get-list-service-as-action-sdg")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListServiceAsActionSdg();
    }
}
