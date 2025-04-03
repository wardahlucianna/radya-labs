using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.SubjectSession;
using Refit;

namespace BinusSchool.Data.Api.School.FnSubject
{
    public interface ISubjectSession : IFnSubject
    {
        [Get("/school/subject-session")]
        Task<ApiErrorResult<IEnumerable<GetSubjectSessionResult>>> GetSubjectSessions(GetSubjectSessionRequest query);
    }
}
