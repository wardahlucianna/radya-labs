using System;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.EmergencyAttendance.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance
{
    public class UnsubmitHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public UnsubmitHandler(
            IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UnsubmitRequest, UnsubmitValidator>();

            var data = await _dbContext.Entity<TrStudentSafeReport>()
                                       .SingleOrDefaultAsync(x => x.Id == body.IdEmergencyAttendance);
            if (data is null)
                throw new NotFoundException(null);

            data.IsActive = false;
            _dbContext.Entity<TrStudentSafeReport>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
