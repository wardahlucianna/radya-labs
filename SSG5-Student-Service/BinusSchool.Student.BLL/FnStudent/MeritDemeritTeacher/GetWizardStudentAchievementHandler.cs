using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetWizardStudentAchievementHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetWizardStudentAchievementHandler(IStudentDbContext EntryMeritDemetitDbContext, IMachineDateTime dateTime)
        {
            _dbContext = EntryMeritDemetitDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetWizardStudentAchievementRequest>();

            var GetSemeterAy = _dbContext.Entity<MsPeriod>()
                       .Include(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                       .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                       .Select(e => new { Semester = e.Semester, IdAcademicYear = e.Grade.MsLevel.IdAcademicYear })
                       .Distinct().SingleOrDefault();

            if (GetSemeterAy == null)
                throw new BadRequestException("Academic and semester are't found");


            var predicate = PredicateBuilder.Create<TrEntryMeritStudent>(x => x.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == GetSemeterAy.IdAcademicYear 
                && x.HomeroomStudent.Student.IdBinusian==param.IdUser
                && x.HomeroomStudent.Homeroom.Semester== GetSemeterAy.Semester);

            var GetEntryMeritStudent = await _dbContext.Entity<TrEntryMeritStudent>()
                .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.MsGradePathwayClassroom).ThenInclude(e=>e.GradePathway).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel)
                .Include(w=>w.MeritDemeritMapping)
                        .Where(predicate)
                        .OrderBy(e=>e.DateMerit)
                        .Take(3)
                        .ToListAsync(CancellationToken);

            var getIdHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                       .Where(e => e.Homeroom.Grade.MsLevel.IdAcademicYear==GetSemeterAy.IdAcademicYear && e.Homeroom.Semester== GetSemeterAy.Semester)
                                       .Select(e => e.Id)
                                       .Distinct().FirstOrDefaultAsync(CancellationToken);

            var Items = new GetWizardStudentAchievementResult
            {
                IdAcademicYear = GetSemeterAy.IdAcademicYear,
                IdHomeroomStudent = getIdHomeroomStudent,
                Semester = GetSemeterAy.Semester.ToString(),
                WizardStudentAchievement = GetEntryMeritStudent
                                    .Select(e => new GetWizardStudentAchievement
                                    {
                                        Date = e.DateMerit,
                                        MeritDemerit = e.MeritDemeritMapping.DisciplineName,

                                    })
                                    .ToList()
            };

            return Request.CreateApiResult2(Items as object);
        }
    }
}
