using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Persistence.StudentDb.Abstractions;

namespace BinusSchool.Student.FnStudent.StudentSafeReport
{
    public class StudentSafeReportHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbStudentContext;
        private string _smtpMail;
        // private readonly string _emailErrorTo = "samuel.fernandez@binus.edu";
        // private readonly string _emailErrorCC = "samuel.fernandez@binus.edu;simprug-isdevelopment@binus.edu;yginting@binus.edu;vincentia.octaviana@binus.edu";
        private readonly string _emailErrorTo = "itdevschool@binus.edu";
        private readonly string _emailErrorCC = "itdevschool@binus.edu";
        public StudentSafeReportHandler(IStudentDbContext dbStudentContext)
        {
            _dbStudentContext = dbStudentContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new System.NotImplementedException();
        }
    }
}
