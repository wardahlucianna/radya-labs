using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MapStudentPathway.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MapStudentPathway
{
    public class MapStudentPathwayHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IGrade _grade;
        private readonly IPathway _pathway;
        private readonly IHomeroom _homeroom;
        private readonly IMachineDateTime _dateTime;

        public MapStudentPathwayHandler(IStudentDbContext dbContext,
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
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMapStudentPathwayRequest>(nameof(GetMapStudentPathwayRequest.IdGrade));
            var columns = new[] { "grade", "description", "gender", "isActive","streaming","nextStreaming" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "grade.description" },
                { columns[1], "student.id" },
                { columns[2], "student.gender" },
                { columns[3], "isActive" }
            };

            var predicate = PredicateBuilder.Create<MsStudentGrade>(x => x.IdGrade == param.IdGrade);
            // if (!string.IsNullOrWhiteSpace(param.Search))
            //     predicate = predicate.And(x
            //         => EF.Functions.Like(x.Student.FirstName, param.SearchPattern())
            //         || EF.Functions.Like(x.Student.MiddleName, param.SearchPattern())
            //         || EF.Functions.Like(x.Student.LastName, param.SearchPattern())
            //         || EF.Functions.Like(x.Student.Gender.ToString(), param.SearchPattern())
            //         || EF.Functions.Like(x.IsActive.ToString(), param.SearchPattern()));
            if (param.ExceptIds != null && param.ExceptIds.Any())
                predicate = predicate.And(x => !param.ExceptIds.Contains(x.IdStudent));

            // query existing student that mapped to pathway

            predicate = predicate.And(x => x.IsActive == true);

            var query = await _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Student)
                    .ThenInclude(x=>x.Religion)
                .Include(x => x.StudentGradePathways)
                .Include(x => x.Grade)
                .SearchByIds(param)
                .Where(predicate)
                .IgnoreQueryFilters()
                .OrderByDynamic(param, aliasColumns)
                .Select(x=>new
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

            var listStudent = query.Select(e => e.IdStudent).ToList();


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

            //FillConfiguration();
            //_pathwayService.SetConfigurationFrom(ApiConfiguration);
            //_gradeService.SetConfigurationFrom(ApiConfiguration);

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

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName)
                    })
                    .If(!string.IsNullOrWhiteSpace(param.Search), x => x.Where(y => y.Description.Contains(param.Search, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            else
            {
                var data = query
                    .Select(e => new
                    {
                        e.Id,
                        e.IdStudent,
                        studentName = NameUtil.GenerateFullName(e.FirstName,e.MiddleName,e.LastName),
                        e.Gender,
                        e.ReligionName,
                        e.StudentGradePathways,
                    })
                    // .Skip(param.CalculateOffset()).Take(param.Size)
                    .ToList();

                // get last grades
                var idStudents = data.Select(x => x.IdStudent);
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

                items = data
                    .Select(x => new GetMapStudentPathwayResult
                    {
                        Id = x.IdStudent,
                        Code = gradeResult.Payload?.Description,
                        Description = $"{x.IdStudent} - {x.studentName}",
                        IdStudent = x.IdStudent,
                        StudentName = x.studentName,
                        Gender = x.Gender,
                        Religion = x.ReligionName, // TODO: get religion from student profile
                        LastPathway = !x.StudentGradePathways.Any()
                                        ? null
                                        : fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathway),
                        PathwayNextAcademicYear = !x.StudentGradePathways.Any()
                                                    ? null
                                                    : x.StudentGradePathways.FirstOrDefault(x => x.IsActive).IdPathwayNextAcademicYear == null
                                                    ? fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathway)
                                                    : fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathwayNextAcademicYear),
                        Pathway = !x.StudentGradePathways.Any()
                                                    ? null
                                                    : x.StudentGradePathways.FirstOrDefault(x => x.IsActive).IdPathwayNextAcademicYear == null
                                                    ? fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathway)
                                                    : fillPathway(x.StudentGradePathways.FirstOrDefault(x => x.IsActive)?.IdPathwayNextAcademicYear),
                        LastHomeroom = fillLastHomeroom(x.IdStudent),
                        IsActive = true//x.IdStudentStatus == 1 ? true : false
                    })
                    .OrderBy(e => e.StudentName).ThenBy(e => e.Pathway == null ? 0 : 1)
                    .If(!string.IsNullOrWhiteSpace(param.Search), x => x.Where(y 
                        => y.Description.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                        || y.Code.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                        || y.Gender.ToString().Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                        || y.IsActive.ToString().Contains(param.Search, StringComparison.OrdinalIgnoreCase)))
                    .SetPagination(param)
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
            }

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(query.Count).AddProperty(KeyValuePair.Create("mappeds", groups as object)).AddColumnProperty(columns));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMapStudentPathwayRequest, UpdateMapStudentPathwayValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var existStudents = await _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.StudentGradePathways)
                .Where(x => x.IdGrade == body.IdGrade)
                .ToListAsync(CancellationToken);

            foreach (var map in body.Mappeds)
            {
                var existStudent = existStudents.Find(x => x.IdStudent == map.IdStudent);
                if (existStudent != null)
                {
                    // check if already have pathway
                    if (existStudent.StudentGradePathways.Any())
                    {
                        // if pathway different, then update
                        if (existStudent.StudentGradePathways.First().IdPathway != map.IdPathway || existStudent.StudentGradePathways.First().IdPathwayNextAcademicYear != map.IdPathwayNextAcademicYear)
                        {
                            var existStdGradePathway = existStudent.StudentGradePathways.First();
                            existStdGradePathway.IdPathway = map.IdPathway;
                            existStdGradePathway.IdPathwayNextAcademicYear = map.IdPathwayNextAcademicYear;

                            _dbContext.Entity<MsStudentGradePathway>().Update(existStdGradePathway);
                        }
                    }
                    // else create new
                    else
                    {
                        var newStdGradePathway = new MsStudentGradePathway
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudentGrade = existStudent.Id,
                            IdPathway = map.IdPathway,
                            IdPathwayNextAcademicYear = map.IdPathwayNextAcademicYear,
                        };
                        _dbContext.Entity<MsStudentGradePathway>().Add(newStdGradePathway);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
