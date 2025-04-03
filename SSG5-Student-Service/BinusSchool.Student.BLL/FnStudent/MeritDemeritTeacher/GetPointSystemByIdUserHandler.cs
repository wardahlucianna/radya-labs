using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetPointSystemByIdUserHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetPointSystemByIdUserHandler(IStudentDbContext EntryMeritDemetitDbContext, IMachineDateTime dateTime)
        {
            _dbContext = EntryMeritDemetitDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPointSystemByIdUserRequest>();

            var GetSemeterAy = _dbContext.Entity<MsPeriod>()
                      .Include(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                      .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                      .Select(e => new { Semester = e.Semester, IdAcademicYear = e.Grade.MsLevel.IdAcademicYear })
                      .Distinct().FirstOrDefault();

            if (GetSemeterAy == null)
                throw new BadRequestException("Academic and semester are't found");

            var GetPointSystem = true;
            GetAyLevelGrade GetAyLevelGrade = default;
            if (RoleConstant.Student == param.Role || RoleConstant.Parent == param.Role)
            {
                GetAyLevelGrade = await _dbContext.Entity<MsHomeroomStudent>()
                      .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                      .Where(e => e.IdStudent == param.IdUser && e.Homeroom.Grade.MsLevel.IdAcademicYear == GetSemeterAy.IdAcademicYear && e.Homeroom.Semester == GetSemeterAy.Semester)
                      .Select(e => new GetAyLevelGrade
                      {
                          IdAcademicYear = e.Homeroom.Grade.MsLevel.IdAcademicYear,
                          IdLevel = e.Homeroom.Grade.IdLevel,
                          IdGrade = e.Homeroom.IdGrade,
                      })
                      .FirstOrDefaultAsync(CancellationToken);

                GetPointSystem = GetAyLevelGrade != null
                      && _dbContext.Entity<MsMeritDemeritComponentSetting>()
                     .Include(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                     .Where(e => e.IdGrade == GetAyLevelGrade.IdGrade && e.Grade.IdLevel == GetAyLevelGrade.IdLevel && e.Grade.MsLevel.IdAcademicYear == GetAyLevelGrade.IdAcademicYear)
                     .Select(e => e.IsUsePointSystem)
                     .Distinct().SingleOrDefault();
            }

            //if (RoleConstant.Teacher == param.Role)
            //{
            //    GetAyLevelGrade = await _dbContext.Entity<MsHomeroomTeacher>()
            //          .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
            //          .Where(e => e.IdBinusian == param.IdUser && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == GetSemeterAy.IdAcademicYear && e.Homeroom.Semester == GetSemeterAy.Semester)
            //          .Select(e => new GetAyLevelGrade
            //          {
            //              IdAcademicYear = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear,
            //              IdLevel = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.IdLevel,
            //              IdGrade = e.Homeroom.MsGradePathwayClassroom.GradePathway.IdGrade,
            //          })
            //          .FirstOrDefaultAsync(CancellationToken);
            //}

            return Request.CreateApiResult2(GetPointSystem as object);
        }
    }
}
