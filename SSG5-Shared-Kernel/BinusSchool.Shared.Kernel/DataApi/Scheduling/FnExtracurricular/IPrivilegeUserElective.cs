using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IPrivilegeUserElective : IFnExtracurricular
    {
        [Get("/privilege-elective/GetPrivilegeUserElective")]
        Task<ApiErrorResult<List<GetPrivilegeUserElectiveResult>>> GetPrivilegeUserElective(GetPrivilegeUserElectiveRequest query);

        [Get("/privilege-elective/GetPrivilegeShowButtonUserElective")]
        Task<ApiErrorResult<GetPrivilegeShowButtonUserElectiveResult>> GetPrivilegeShowButtonUserElective(GetPrivilegeShowButtonUserElectiveRequest query);
    }
}
