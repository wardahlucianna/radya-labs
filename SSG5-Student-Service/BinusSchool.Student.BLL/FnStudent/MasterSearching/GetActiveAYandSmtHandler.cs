using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class GetActiveAYandSmtHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetActiveAYandSmtHandler(IStudentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetActiveAYandSmtRequest>(
                    nameof(GetActiveAYandSmtRequest.IdSchool));

            var getPeriod = _dbContext.Entity<MsPeriod>()
                    .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                    .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new GetActiveAYandSmtResult
                    {
                        AcademicYear = new ItemValueVm
                        {
                            Id = x.Grade.MsLevel.MsAcademicYear.Id,
                            Description = x.Grade.MsLevel.MsAcademicYear.Description
                        },
                        Semester = new ItemValueVm
                        {
                            Id = x.Semester.ToString(),
                            Description = x.Semester.ToString()
                        },
                    })
                    .FirstOrDefault();

            var retVal = new GetActiveAYandSmtResult();

            if (getPeriod != null)
            {
                retVal = getPeriod;
                //retVal.AcademicYear = new ItemValueVm
                //{
                //    Id = getPeriod.Grade.MsLevel.MsAcademicYear.Id,
                //    Description = getPeriod.Grade.MsLevel.MsAcademicYear.Description
                //};
                //retVal.Semester = new ItemValueVm
                //{
                //    Id = getPeriod.Semester.ToString(),
                //    Description = getPeriod.Semester.ToString()
                //};
            }
            else
            {
                retVal = _dbContext.Entity<MsPeriod>()
                    .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                    .Where(x => x.StartDate <= _dateTime.ServerTime)
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new GetActiveAYandSmtResult
                    {
                        AcademicYear = new ItemValueVm
                        {
                            Id = x.Grade.MsLevel.MsAcademicYear.Id,
                            Description = x.Grade.MsLevel.MsAcademicYear.Description
                        },
                        Semester = new ItemValueVm
                        {
                            Id = x.Semester.ToString(),
                            Description = x.Semester.ToString()
                        },
                    })
                    .FirstOrDefault();
            }

            return Request.CreateApiResult2(retVal as object);
        }
    }
}
