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
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public WizardTeacherHandler(IStudentDbContext EntryMeritDemetitDbContext, IMachineDateTime dateTime)
        {
            _dbContext = EntryMeritDemetitDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<WizardStudentHandlerRequest>();

            string[] _columns = { "IdStudent", "NameStudent", "Demerit", "LastUpdate" };

            var GetSemeterAy = _dbContext.Entity<MsPeriod>()
                      .Include(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                      .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                      .Select(e => new { Semester = e.Semester, IdAcademicYear = e.Grade.MsLevel.IdAcademicYear })
                      .FirstOrDefault();

            if (GetSemeterAy == null)
                throw new BadRequestException("Academic and semester are't found");

            var GetHomerrom = await _dbContext.Entity<MsHomeroomTeacher>()
                       .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                       .Where(e => e.IdBinusian == param.IdUser && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == GetSemeterAy.IdAcademicYear && e.Homeroom.Semester == GetSemeterAy.Semester)
                       .Select(e => e.IdHomeroom)
                       .ToListAsync(CancellationToken);

            var query = (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                         join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                         join User in _dbContext.Entity<MsUser>() on Student.Id equals User.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join StudentPoint in _dbContext.Entity<TrStudentPoint>() on HomeroomStudent.Id equals StudentPoint.IdHomeroomStudent into JoinedStudentPoint
                         from StudentPoint in JoinedStudentPoint.DefaultIfEmpty()
                         join UserUpdate in _dbContext.Entity<MsUser>() on StudentPoint.UserUp equals UserUpdate.Id into JoinedUserUpdate
                         from UserUpdate in JoinedUserUpdate.DefaultIfEmpty()
                         join UserInsert in _dbContext.Entity<MsUser>() on StudentPoint.UserIn equals UserInsert.Id into JoinedUserInsert
                         from UserInsert in JoinedUserInsert.DefaultIfEmpty()
                         join LevelOfInteraction in _dbContext.Entity<MsLevelOfInteraction>() on StudentPoint.IdLevelOfInteraction equals LevelOfInteraction.Id into JoinedLevelOfInteraction
                         from LevelOfInteraction in JoinedLevelOfInteraction.DefaultIfEmpty()
                         join SanctionMapping in _dbContext.Entity<MsSanctionMapping>() on StudentPoint.IdSanctionMapping equals SanctionMapping.Id into JoinedSanctionMapping
                         from SanctionMapping in JoinedSanctionMapping.DefaultIfEmpty()
                         where Level.IdAcademicYear == GetSemeterAy.IdAcademicYear 
                         && GetHomerrom.Contains(Homeroom.Id) 
                         && Homeroom.Semester== GetSemeterAy.Semester
                         //&& StudentPoint.DemeritPoint>0
                         select new
                         {
                             IdHomeroomStudent = HomeroomStudent.Id,
                             IdStudent = HomeroomStudent.IdStudent,
                             IdHomeroom = Homeroom.Id,
                             IdLevel = Level.Id,
                             IdGrade = Grade.Id,
                             IdAcademicYear = AcademicYear.Id,
                             NameStudent = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                             Demerit = StudentPoint.DemeritPoint,
                             LastUpdate = StudentPoint.DateUp == null ? StudentPoint.DateIn : StudentPoint.DateUp,
                             UserUpdate = StudentPoint.UserUp == null ? UserInsert.DisplayName : UserUpdate.DisplayName,
                         }).OrderByDescending(e=>e.LastUpdate);


            switch (param.OrderBy)
            {
                case "IdStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdStudent)
                        : query.OrderBy(x => x.IdStudent);
                    break;
                case "NameStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NameStudent)
                        : query.OrderBy(x => x.NameStudent);
                    break;
                case "Demerit":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Demerit)
                        : query.OrderBy(x => x.Demerit);
                    break;
                case "LastUpdate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LastUpdate)
                        : query.OrderBy(x => x.LastUpdate);
                    break;
            };

            List<WizardTeacherResult> items = new List<WizardTeacherResult>() ;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new WizardTeacherResult
                {
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    IdStudent = x.IdStudent,
                    NameStudent = x.NameStudent,
                    Demerit = x.Demerit,
                    LastUpdate = x.LastUpdate,
                    UserUpdate = x.UserUpdate,
                    IdHomeroom = x.IdHomeroom,
                    IdLevel = x.IdLevel,
                    IdGrade = x.IdGrade,
                    IdAcademicYear = x.IdAcademicYear,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new WizardTeacherResult
                {
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    IdStudent = x.IdStudent,
                    NameStudent = x.NameStudent,
                    Demerit = x.Demerit,
                    LastUpdate = x.LastUpdate,
                    UserUpdate = x.UserUpdate,
                    IdHomeroom = x.IdHomeroom,
                    IdLevel = x.IdLevel,
                    IdGrade = x.IdGrade,
                    IdAcademicYear = x.IdAcademicYear,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdStudent).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));

        }
    }
}
