using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;
namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardStudentDetailMeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public WizardStudentDetailMeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<WizardDetailStudentRequest>();

            string[] _columns = { "Date", "DeciplineName", "Point", "Note", "Teacher"};

            var DataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e=>e.Student)
                                .Include(e=>e.Homeroom).ThenInclude(e=>e.MsGradePathwayClassroom).ThenInclude(e=>e.GradePathway).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel)
                                .Where(e => e.IdStudent == param.IdStudent && e.Semester==param.Semester && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear==param.IdAcademicYear)
                                .Select(e => new
                                {
                                    Student = string.IsNullOrEmpty(e.Student.FirstName) != null ? $"{e.Student.Id} - {e.Student.FirstName.Trim()} {e.Student.LastName.Trim()}" : $"{e.Student.Id} - {e.Student.LastName.Trim()}",
                                    Homeroom = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Code + " - "
                                                + e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code
                                                + e.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            if (DataStudent == null)
                throw new BadRequestException(string.Format("Homeroom student is not found"));

            var PointStudent = await _dbContext.Entity<TrStudentPoint>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway)
                                    .ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                .Where(e => e.HomeroomStudent.IdStudent == param.IdStudent 
                                    && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                    && e.HomeroomStudent.Homeroom.Semester== param.Semester)
                                .Select(e => new
                                {
                                    TotalDemerit = e.DemeritPoint,
                                    TotalMerit = e.MeritPoint,
                                    LevelOfInfraction = e.LevelOfInteraction.NameLevelOfInteraction,
                                    Sanction = e.SanctionMapping.SanctionName,
                                    IdAY = e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            var query = (from EntryMeritStudent in _dbContext.Entity<TrEntryMeritStudent>()
                         join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                         join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntryMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join user in _dbContext.Entity<MsUser>() on EntryMeritStudent.MeritUserCreate equals user.Id into JoinedUser
                         from user in JoinedUser.DefaultIfEmpty()
                         where HomeroomStudent.IdStudent == param.IdStudent
                         && EntryMeritStudent.Status == "Approved"
                         && Level.IdAcademicYear == param.IdAcademicYear 
                         && Homeroom.Semester == param.Semester
                         select new
                         {
                             Date = EntryMeritStudent.DateMerit,
                             DeciplineName = MeritDemeritMapping.DisciplineName,
                             Point = EntryMeritStudent.Point.ToString(),
                             Note = EntryMeritStudent.Note,
                             Teacher = user.DisplayName,
                         });

            //ordering
            switch (param.OrderBy)
            {
                case "Date":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Date)
                        : query.OrderBy(x => x.Date);
                    break;
                case "DeciplineName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.DeciplineName)
                        : query.OrderBy(x => x.DeciplineName);
                    break;
                case "Point":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Point)
                        : query.OrderBy(x => x.Point);
                    break;
                case "Note":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Note)
                        : query.OrderBy(x => x.Note);
                    break;
            };

            WizardStudentDetailMeritResult itemsMerit = new WizardStudentDetailMeritResult();
            List<WizardStudentDetailMerit> items = new List<WizardStudentDetailMerit>();
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                   .Select(e => new WizardStudentDetailMerit
                   {
                       Date = e.Date,
                       DeciplineName = e.DeciplineName,
                       Point = e.Point,
                       Note = e.Note,
                       Teacher = e.Teacher,
                   })
                   .ToList();

                itemsMerit.Merit = items;
                itemsMerit.Student = DataStudent.Student;
                itemsMerit.Homeroom = DataStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent == null?0:PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent == null ? 0 : PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent == null ? null : PointStudent.LevelOfInfraction;
                itemsMerit.Sanction = PointStudent == null ? null : PointStudent.Sanction;
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                   .Select(e => new WizardStudentDetailMerit
                   {
                       Date = e.Date,
                       DeciplineName = e.DeciplineName,
                       Point = e.Point,
                       Note = e.Note,
                       Teacher = e.Teacher,
                   })
                   .ToList();


                itemsMerit.Merit = items;
                itemsMerit.Student = DataStudent.Student;
                itemsMerit.Homeroom = DataStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent == null ? 0 : PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent == null ? 0 : PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent == null ? null : PointStudent.LevelOfInfraction;
                itemsMerit.Sanction = PointStudent == null ? null : PointStudent.Sanction;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Date).CountAsync(CancellationToken);

            return Request.CreateApiResult2(itemsMerit as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
