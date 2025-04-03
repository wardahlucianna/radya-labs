using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class WizardStudentDetailDemeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public WizardStudentDetailDemeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<WizardDetailStudentRequest>();
            string[] _columns = { "Date", "DeciplineName", "Point", "Note", "Teacher" };

            var GetIdGrade = _dbContext.Entity<MsHomeroomStudent>()
                        .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                        .Where(e => e.IdStudent == param.IdStudent
                            && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                            && e.Homeroom.Semester == param.Semester
                            )
                        .Select(e => e.Homeroom.MsGradePathwayClassroom.GradePathway.IdGrade)
                        .Distinct().SingleOrDefault();

            var MeritDemeritComponentSetting = await _dbContext.Entity<MsMeritDemeritComponentSetting>()
                                                 .Where(e => e.IdGrade == GetIdGrade)
                                                 .SingleOrDefaultAsync(CancellationToken);

            var query = (from EntryDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>()
                         join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryDemeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                         join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntryDemeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join user in _dbContext.Entity<MsUser>() on EntryDemeritStudent.DemeritUserCreate equals user.Id into JoinedUser
                         from user in JoinedUser.DefaultIfEmpty()
                         where HomeroomStudent.IdStudent == param.IdStudent
                         && EntryDemeritStudent.Status == "Approved"
                         && Level.IdAcademicYear == param.IdAcademicYear && Homeroom.Semester == param.Semester
                         select new
                         {
                             Date = EntryDemeritStudent.DateDemerit,
                             DeciplineName = MeritDemeritMapping.DisciplineName,
                             Point = EntryDemeritStudent.Point.ToString(),
                             Note = EntryDemeritStudent.Note,
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

            WizardStudentDetailDemeritResult itemsDemerit = new WizardStudentDetailDemeritResult();
            List<WizardStudentDetailMerit> items = new List<WizardStudentDetailMerit>();
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);
                if (result != null)
                {
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

                    itemsDemerit.Demerit = items;
                    itemsDemerit.IsShowDemerit = MeritDemeritComponentSetting == null ? true : MeritDemeritComponentSetting.IsUseDemeritSystem;
                }
                else
                {
                    return Request.CreateApiResult2(itemsDemerit as object, param.CreatePaginationProperty(0).AddColumnProperty(_columns));
                }
               
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                if (result != null)
                {
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

                    itemsDemerit.Demerit = items;
                    itemsDemerit.IsShowDemerit = MeritDemeritComponentSetting == null ? true : MeritDemeritComponentSetting.IsUseDemeritSystem;
                }
                else
                {
                    return Request.CreateApiResult2(itemsDemerit as object, param.CreatePaginationProperty(0).AddColumnProperty(_columns));
                }
                
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Date).CountAsync(CancellationToken);

            return Request.CreateApiResult2(itemsDemerit as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
