using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Setting
{
    public class GetCurrentAYHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetCurrentAYHandler(IStudentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCurrentAYRequest>(nameof(GetCurrentAYRequest.IdSchool));

            var getPeriod = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.MsLevel)
                    .ThenInclude(x => x.MsAcademicYear)
                .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                .OrderByDescending(x => x.Grade.MsLevel.MsAcademicYear.Code).ThenByDescending(x => x.Semester)
                .ToListAsync(CancellationToken);

            var res = getPeriod.Where(x => x.StartDate <= _dateTime.ServerTime && x.EndDate >= _dateTime.ServerTime)
                .Select(x => new GetCurrentAYResult
                {
                    AcademicYear = new ItemValueVm
                    {
                        Id = x.Grade.MsLevel.IdAcademicYear,
                        Description = x.Grade.MsLevel.MsAcademicYear.Description,
                    },
                    Semester = Convert.ToString(x.Semester)
                })
                .FirstOrDefault();

            if(res == null)
            {
                res = getPeriod
                    .Select(x => new GetCurrentAYResult
                    {
                        AcademicYear = new ItemValueVm
                        {
                            Id = x.Grade.MsLevel.IdAcademicYear,
                            Description = x.Grade.MsLevel.MsAcademicYear.Description,
                        },
                        Semester = Convert.ToString(x.Semester)
                    })
                    .FirstOrDefault();
            }

            return Request.CreateApiResult2(res as object);

        }
    }
}
