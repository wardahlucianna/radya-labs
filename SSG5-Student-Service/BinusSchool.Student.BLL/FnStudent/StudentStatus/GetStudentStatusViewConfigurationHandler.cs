using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class GetStudentStatusViewConfigurationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly ICurrentAcademicYear _currentAcademicYearApi;

        public GetStudentStatusViewConfigurationHandler(
            IStudentDbContext dbContext,
            ICurrentAcademicYear currentAcademicYearApi)
        {
            _dbContext = dbContext;
            _currentAcademicYearApi = currentAcademicYearApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentStatusViewConfigurationRequest>(
                            nameof(GetStudentStatusViewConfigurationRequest.IdSchool),
                            nameof(GetStudentStatusViewConfigurationRequest.IdAcademicYear)
                            );


            var getCurrentAYApi = await _currentAcademicYearApi.GetActiveAcademicYear(new GetActiveAcademicYearRequest
            {
                SchoolID = param.IdSchool
            });

            var getCurrentAY = getCurrentAYApi.Payload;

            var anyTrStudentStatus = _dbContext.Entity<TrStudentStatus>()
                                        .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                                        .Any();

            var result = new GetStudentStatusViewConfigurationResult
            {
                IsEditable = getCurrentAY.AcademicYearId.ToUpper().Trim() == param.IdAcademicYear.ToUpper().Trim() ? true : false,
                EnableGenerateStudentStatus = getCurrentAY.AcademicYearId.ToUpper().Trim() == param.IdAcademicYear.ToUpper().Trim() ? true : false,
                EnableCreateStudentRecord = (getCurrentAY.AcademicYearId.ToUpper().Trim() == param.IdAcademicYear.ToUpper().Trim() && anyTrStudentStatus) ? true : false
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
