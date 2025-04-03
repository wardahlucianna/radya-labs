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
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.MapStudentPathway.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.MapStudentPathway
{
    public class CopyToNextAcademicYearHandler : FunctionsHttpCrudHandler
    {

        private readonly IStudentDbContext _dbContext;
        private readonly IGrade _grade;
        private readonly IPathway _pathway;
        private readonly IHomeroom _homeroom;
        private readonly IMachineDateTime _dateTime;

        public CopyToNextAcademicYearHandler(IStudentDbContext dbContext,
            IGrade grade,
            IPathway pathway,
            IHomeroom homeroom,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _grade = grade;
            _pathway = pathway;
            _homeroom = homeroom;
            _dateTime = dateTime;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();

        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {

            var param = Request.ValidateParams<GetMapStudentPathwayRequest>(nameof(GetMapStudentPathwayRequest.IdGrade));
            var columns = new[] { "grade", "description", "gender", "isActive", "streaming", "nextStreaming" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "grade.description" },
                { columns[1], "student.id" },
                { columns[2], "student.gender" },
                { columns[3], "isActive" }
            };

            var academicYearCurrent = await _dbContext.Entity<MsGrade>()
               .Include(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
              .Where(e => e.Id == param.IdGrade)
              .Select(e => new
              {
                  e.MsLevel.MsAcademicYear.IdSchool,
                  e.MsLevel.IdAcademicYear,
                  e.MsLevel.MsAcademicYear.Code
              })
              .FirstOrDefaultAsync(CancellationToken);

            if (academicYearCurrent == null)
                throw new BadRequestException("Current academic year is not found");

            var yearCurrent = Int32.Parse(academicYearCurrent.Code);
            var nextYear = yearCurrent + 1;

            var idAcademicYearNext = await _dbContext.Entity<MsAcademicYear>()
               .Where(e => e.Code == nextYear.ToString() && e.IdSchool == academicYearCurrent.IdSchool)
               .Select(e => e.Id)
               .FirstOrDefaultAsync(CancellationToken);
            //idAcademicYearNext = "SEMARANG2022";
            if (idAcademicYearNext == null)
                throw new BadRequestException("Next academic year is not found");

            #region get student pathway
            var predicate = PredicateBuilder.Create<MsStudentGrade>(x => x.IdGrade == param.IdGrade);
            if (param.ExceptIds != null && param.ExceptIds.Any())
                predicate = predicate.And(x => !param.ExceptIds.Contains(x.IdStudent));

            predicate = predicate.And(x => x.IsActive == true);

            var query = await _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Student)
                    .ThenInclude(x => x.Religion)
                .Include(x => x.StudentGradePathways)
                .Include(x => x.Grade)
                .SearchByIds(param)
                .Where(predicate)
                .IgnoreQueryFilters()
                .OrderByDynamic(param, aliasColumns)
                .Select(x => new
                {
                    x.Id,
                    x.IdStudent,
                    x.Student.FirstName,
                    x.Student.MiddleName,
                    x.Student.LastName,
                    x.Student.Gender,
                    x.StudentGradePathways,
                    x.Student.Religion.ReligionName,
                    x.IsActive,
                    x.Student.IdStudentStatus
                })
                .ToListAsync(CancellationToken);

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
                .Where(x => (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                    || (x.StartDate < _dateTime.ServerTime.Date
                        ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                        : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

            if (checkStudentStatus != null)
            {
                query = query.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();
            }

            // get grade
            var gradeResult = await _grade.GetGradeDetail(param.IdGrade);
            if (!gradeResult.IsSuccess)
                throw new BadRequestException(gradeResult.Message);

            // get pathways master
            var idPathways = query.SelectMany(x => x.StudentGradePathways.Select(y => y.IdPathway)).Distinct().ToList();
            idPathways.AddRange(query.SelectMany(x => x.StudentGradePathways.Select(y => y.IdPathwayNextAcademicYear)).Distinct().ToList());
            var pathwaysResult = await _pathway.GetPathways(new GetPathwayRequest
            {
                Ids = idPathways,
                IdSchool = new[] { gradeResult.Payload.School.Id },
                Return = CollectionType.Lov,
                GetAll = true
            });

            // count student per pathway
            var groups = query
                .SelectMany(x => x.StudentGradePathways)
                .Where(x => x.IsActive)
                .GroupBy(x => x.IdPathway)
                .Select(x => KeyValuePair.Create(
                    pathwaysResult.Payload?.FirstOrDefault(y => y.Id == x.Key)?.Description ?? x.Key,
                    x.Count()
                ));

            query = query
                    .ToList();

            // get last grades
            var idStudents = query.Select(x => x.IdStudent);
            var lastGrades = await _dbContext.Entity<MsStudentGrade>()
                .Where(x => idStudents.Contains(x.IdStudent) && x.IdGrade != param.IdGrade)
                .OrderByDescending(x => x.DateIn).ThenByDescending(x => x.DateUp)
                .ToListAsync(CancellationToken);

            var homerooms = Enumerable.Empty<GetHomeroomByStudentResult>();
            if (param.IncludeLastHomeroom.HasValue && param.IncludeLastHomeroom.Value)
            {

                var homeroomsParam = new GetHomeroomByStudentRequest
                {
                    Ids = idStudents,
                    IdGrades = lastGrades.Select(x => x.IdGrade).Distinct()
                };
                var homeroomsResult = await _homeroom.GetHomeroomByStudents(homeroomsParam);
                homerooms = homeroomsResult.Payload;
            }

            var listMapStudentPathways = query
                .Select(x => new GetMapStudentPathwayResult
                {
                    Id = x.IdStudent,
                    Code = gradeResult.Payload?.Description,
                    Description = NameUtil.GenerateFullNameWithId(x.IdStudent, x.FirstName, x.MiddleName, x.LastName),
                    IdStudent = x.IdStudent,
                    StudentName = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                    Gender = x.Gender,
                    Religion = x.ReligionName, // TODO: get religion from student profile
                        Pathway = !x.StudentGradePathways.Any()
                                                ? null
                                                : fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathway),
                    PathwayNextAcademicYear = !x.StudentGradePathways.Any()
                                                ? null
                                                : x.StudentGradePathways.FirstOrDefault(x => x.IsActive).IdPathwayNextAcademicYear == null
                                                ? fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathway)
                                                : fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathwayNextAcademicYear),
                    IdStudentGrade = x.Id,
                    LastPathway = fillLastPathway(x.Id),
                    LastHomeroom = fillLastHomeroom(x.IdStudent),
                    IsActive = true//x.IdStudentStatus == 1 ? true : false
                    })
                .If(!string.IsNullOrWhiteSpace(param.Search), x => x.Where(y
                    => y.Description.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                    || y.Code.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                    || y.Gender.ToString().Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                    || y.IsActive.ToString().Contains(param.Search, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            ItemValueVm fillPathway(string idPathway)
            {
                if (idPathway is null)
                    return null;

                var pathway = pathwaysResult.Payload?.FirstOrDefault(x => x.Id == idPathway);
                return new ItemValueVm(idPathway, pathway?.Description);
            }

            ItemValueVm fillLastPathway(string idStudentGrade)
            {
                var lastPathway = query
                    .First(x => x.Id == idStudentGrade)
                    .StudentGradePathways
                    .OrderByDescending(x => x.DateIn).ThenByDescending(x => x.DateUp)
                    .FirstOrDefault();

                if (lastPathway is null)
                    return null;

                var pathway = pathwaysResult.Payload?.FirstOrDefault(x => x.Id == lastPathway.IdPathway);
                return new ItemValueVm(lastPathway.IdPathway, pathway?.Description);
            }

            string fillLastHomeroom(string idStudent)
            {
                if (!param.IncludeLastHomeroom.HasValue || !param.IncludeLastHomeroom.Value)
                    return null;

                var homeroom = homerooms?.FirstOrDefault(x => x.IdStudents.Contains(idStudent));
                return homeroom is null
                    ? null
                    : $"{homeroom.Grade.Code}{homeroom.Classroom.Code} - {homeroom.Grade.Code} {homeroom.Classroom.Description}";
            }
            #endregion

            var listIdStudentCurrentAy = listMapStudentPathways.Select(e=>e.IdStudent).ToList();

            var listIdStudentNextAy = await _dbContext.Entity<MsStudentGrade>()
                                      .Where(e => listIdStudentCurrentAy.Contains(e.IdStudent)
                                      && e.Grade.MsLevel.IdAcademicYear == idAcademicYearNext
                                      )
                                      .Select(e => new
                                      {
                                          e.IdStudent,
                                          e.Id
                                      })
                                      .Distinct().ToListAsync(CancellationToken);

            var listMapStudentPathwaysNextAy = listMapStudentPathways
                                                .Select(x => new GetCopyMapStudentPathwayResult
                                                {
                                                    Id = x.Id,
                                                    Code = x.Code,
                                                    Description = x.Description,
                                                    IdStudent = x.IdStudent,
                                                    StudentName = x.StudentName,
                                                    Gender = x.Gender,
                                                    Religion = x.Religion,
                                                    Pathway = x.Pathway,
                                                    PathwayNextAcademicYear = x.PathwayNextAcademicYear,
                                                    LastPathway = x.LastPathway,
                                                    LastHomeroom = x.LastHomeroom,
                                                    IsActive = x.IsActive,
                                                    IsShowWarning = listIdStudentNextAy.Where(e=>e.IdStudent==x.IdStudent).Any()?false:true,
                                                    IdStudentGradeNextAy = listIdStudentNextAy.Where(e => e.IdStudent == x.IdStudent).Any() 
                                                                            ? listIdStudentNextAy.FirstOrDefault(e => e.IdStudent == x.IdStudent).Id
                                                                            : null,
                                                    IdStudentGradeCurrentAy = x.IdStudentGrade
                                                }).OrderBy(e => e.StudentName).ThenBy(e => e.Pathway == null ? 0 : 1).ToList();

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = listMapStudentPathwaysNextAy
                        .ToList();
            }
            else
            {
                items = listMapStudentPathwaysNextAy
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<CopyMapStudentPathwayRequest, CopyMapStudentPathwayValidation>();

            var listIdStudentGradeNextAy = body.MapStudentPathway.Select(e => e.IdStudentGradeNextAy).ToList();
            var listIdStudentGradeCurrentAy = body.MapStudentPathway.Select(e => e.IdStudentGradeCurrentAy).ToList();

             var listStudentGradeNext = await _dbContext.Entity<MsStudentGrade>()
                                   .Where(e => listIdStudentGradeNextAy.Contains(e.Id))
                                   .ToListAsync(CancellationToken);

            if (!listStudentGradeNext.Any())
                throw new BadRequestException("Student grade pathway is not found");


            var listStudentGradePathwayCurrentAy = await _dbContext.Entity<MsStudentGradePathway>()
                                  .Include(e => e.StudentGrade)
                                   .Where(e => listIdStudentGradeCurrentAy.Contains(e.IdStudentGrade))
                                   .ToListAsync(CancellationToken);

            var listStudentGradePathwayNext = await _dbContext.Entity<MsStudentGradePathway>()
                                  .Include(e => e.StudentGrade)
                                   .Where(e => listIdStudentGradeNextAy.Contains(e.IdStudentGrade))
                                   .ToListAsync(CancellationToken);

            foreach (var item in body.MapStudentPathway)
            {
                var listStudentGradePathwayNextById = listStudentGradePathwayNext.Where(e => e.IdStudentGrade == item.IdStudentGradeNextAy).FirstOrDefault();
                var listStudentGradePathwayCurrentById = listStudentGradePathwayCurrentAy.Where(e => e.IdStudentGrade == item.IdStudentGradeCurrentAy).FirstOrDefault();

                if (listStudentGradeNext == null || listStudentGradePathwayCurrentById==null)
                    continue;

                if (listStudentGradePathwayNextById == null)
                {
                    var newStdGradePathway = new MsStudentGradePathway
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdStudentGrade = item.IdStudentGradeNextAy,
                        IdPathway = listStudentGradePathwayCurrentById.IdPathwayNextAcademicYear,
                    };
                    _dbContext.Entity<MsStudentGradePathway>().Add(newStdGradePathway);
                }
                else
                {
                    listStudentGradePathwayNextById.IdPathway = item.IdPathwayNextAcademicYear;
                    _dbContext.Entity<MsStudentGradePathway>().Update(listStudentGradePathwayNextById);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
