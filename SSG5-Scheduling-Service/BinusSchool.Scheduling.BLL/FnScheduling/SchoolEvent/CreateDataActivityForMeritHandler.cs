using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator;
using BinusSchool.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{

    public class CreateDataActivityForMeritHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IStringLocalizer _localizer;
        private readonly IConfiguration _configuration;
        private readonly IApiService<IMeritDemeritTeacher> _meritDemeritTeacherService;

        public CreateDataActivityForMeritHandler(ISchedulingDbContext dbContext,
         IStringLocalizer localizer,
         IConfiguration configuration,
         IApiService<IMeritDemeritTeacher> meritDemeritTeacherService)
        {
            _dbContext = dbContext;
            _localizer = localizer;
            _configuration = configuration;
            _meritDemeritTeacherService = meritDemeritTeacherService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CreateActivityDataToMeritRequest, CreateDataActivityForMeritValidator>();
            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();

            // send merit to stundet
            try
            {
                _ = _meritDemeritTeacherService
                    .SetConfigurationFrom(apiConfig)
                    .Execute.AddMeritStudent(new AddMeritStudentRequest
                    {
                        IdAcademicYear = body.IdAcademicYear,
                        MeritStudents = body.MeritStudents.Select(x => new MeritStudents{
                            IdHomeroomStudent = x.IdHomeroomStudent,
                            Note = x.Note,
                            IdLevel = x.IdLevel,
                            IdGrade = x.IdGrade,
                            Semeter = x.Semeter,
                            IdMeritDemeritMapping = x.IdMeritDemeritMapping,
                            NameMeritDemeritMapping = x.NameMeritDemeritMapping,
                            Point = x.Point
                        }).ToList()
                    });
            }
            catch (Exception ex)
            {
                throw;
            }
            
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
