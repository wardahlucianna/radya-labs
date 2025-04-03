using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.MapStudentGrade.Validator;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BinusSchool.Student.FnStudent.MapStudentGrade
{
    public class MapStudentGradeHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IPathway _pathway;
        private readonly IGrade _grade;

        public MapStudentGradeHandler(IStudentDbContext dbContext, IPathway pathway, IGrade grade)
        {
            _dbContext = dbContext;
            _pathway = pathway;
            _grade = grade;
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
            var param = Request.ValidateParams<GetMapStudentGradeRequest>(new string[] { });
            var columns = new[] { "academicYear", "level", "grade" };

            var predicate = PredicateBuilder.Create<MsGrade>(x => x.MsLevel.IdAcademicYear == param.AcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                EF.Functions.Like(x.Description, param.SearchPattern())
                || EF.Functions.Like(x.MsLevel.Description, param.SearchPattern())
                );

            var query = _dbContext.Entity<MsGrade>()
                .Include(x => x.MsLevel)
                    .ThenInclude(x => x.MsAcademicYear)
                .Where(predicate)
                .AsQueryable();

            query = param.OrderBy switch
            {
                "acadyear" => param.OrderType == Common.Model.Enums.OrderType.Asc
                    ? query.OrderBy(x => x.MsLevel.MsAcademicYear.Code)
                    : query.OrderByDescending(x => x.MsLevel.MsAcademicYear.Code),
                "level" => param.OrderType == Common.Model.Enums.OrderType.Asc
                    ? query.OrderBy(x => x.MsLevel.OrderNumber).ThenBy(x => x.OrderNumber)
                    : query.OrderByDescending(x => x.MsLevel.OrderNumber).ThenBy(x => x.OrderNumber),
                "grade" => param.OrderType == Common.Model.Enums.OrderType.Asc
                    ? query.OrderBy(x => x.OrderNumber)
                    : query.OrderByDescending(x => x.OrderNumber),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == Common.Model.Enums.CollectionType.Lov)
            {
                items = await query.Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.Description
                }).Distinct().ToListAsync(CancellationToken);
            }
            else
            {
                items = await query.SetPagination(param)
                    .Select(x => new GetMapStudentGradeResult
                    {
                        AcademicYear = new ItemValueVm
                        {
                            Id = x.MsLevel.IdAcademicYear,
                            Description = x.MsLevel.MsAcademicYear.Description,
                        },
                        Grade = new ItemValueVm
                        {
                            Id = x.Id,
                            Description = x.Description
                        },
                        Level = new ItemValueVm
                        {
                            Id = x.IdLevel,
                            Description = x.MsLevel.Description
                        },
                    })
                    .ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<CreateMapStudentGradeRequest, CreateMapStudentGradeValidator>();

            var grade = await _dbContext.Entity<MsGrade>()
                .AnyAsync(x => x.Id == body.IdGrade);

            if (!grade)
                throw new NotFoundException("Grade not found");

            if (body.Ids.Any() && body.RemoveIds.Any())
            {
                if (body.Ids.Intersect(body.RemoveIds).Any())
                    throw new BadRequestException("Can't remove and add same students");
            }

            var existStudentGrades = await _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Student)
                .Include(x => x.StudentGradePathways)
                .Where(x => body.Ids.Contains(x.IdStudent) && x.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                .ToListAsync(CancellationToken);

            var newStudentGrades = new List<string>();

            foreach (var newStudent in body.Ids)
            {
                if (!existStudentGrades.Where(x => x.IdStudent == newStudent).Any() || existStudentGrades.Where(x => x.IdStudent == newStudent && x.IdGrade != body.IdGrade).Any())
                    newStudentGrades.Add(newStudent);
            }

            if (newStudentGrades.Any())
            {
                var existStudent = await _dbContext.Entity<MsStudent>().Where(x => newStudentGrades.Contains(x.Id)).ToListAsync(CancellationToken);
                if (existStudent.Count() != newStudentGrades.Count())
                    throw new BadRequestException("one or more student id not found");

                foreach (var newStudent in newStudentGrades)
                {
                    var newStudentGrade = new MsStudentGrade
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdStudent = newStudent,
                        IdGrade = body.IdGrade
                    };
                    await _dbContext.Entity<MsStudentGrade>().AddAsync(newStudentGrade, CancellationToken);

                    var deletedStudentGrade = existStudentGrades.FirstOrDefault(x => x.IdStudent == newStudent);

                    if (deletedStudentGrade != null)
                    {
                        deletedStudentGrade.IsActive = false;
                        _dbContext.Entity<MsStudentGrade>().Update(deletedStudentGrade);

                        if (deletedStudentGrade.StudentGradePathways.Any())
                        {
                            var newStudentGradePathway = new MsStudentGradePathway
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdStudentGrade = newStudentGrade.Id,
                                IdPathway = deletedStudentGrade.StudentGradePathways.FirstOrDefault().IdPathway,
                                IdPathwayNextAcademicYear = deletedStudentGrade.StudentGradePathways.FirstOrDefault().IdPathwayNextAcademicYear
                            };

                            await _dbContext.Entity<MsStudentGradePathway>().AddAsync(newStudentGradePathway, CancellationToken);
                        }
                    }
                }
            }

            var failedRemoveStudents = new List<string>();
            if (body.RemoveIds.Count() > 0)
            {
                var deleteStudentGrades = await _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.StudentGradePathways)
                    .Include(x => x.Student)
                    .Where(x => body.RemoveIds.Contains(x.IdStudent) && x.IdGrade == body.IdGrade)
                    .ToListAsync(CancellationToken);

                if (deleteStudentGrades.Count() == 0)
                    throw new NotFoundException("Student grade not found");

                var firstStudentGrades = await _dbContext.Entity<MsStudentGrade>()
                    .Where(x => body.RemoveIds.Contains(x.IdStudent) && x.IdGrade != body.IdGrade && x.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                    .OrderBy(x => x.DateIn)
                    .IgnoreQueryFilters()
                    .ToListAsync(CancellationToken);

                // get grade
                var gradeResult = await _grade.GetGradeDetail(body.IdGrade);
                if (!gradeResult.IsSuccess)
                    throw new BadRequestException(gradeResult.Message);

                var idPathways = deleteStudentGrades.SelectMany(x => x.StudentGradePathways.Select(y => y.IdPathway)).Distinct().ToList();
                idPathways.AddRange(deleteStudentGrades.SelectMany(x => x.StudentGradePathways.Select(y => y.IdPathwayNextAcademicYear)).Distinct().ToList());
                var pathwaysResult = await _pathway.GetPathways(new GetPathwayRequest
                {
                    Ids = idPathways,
                    IdSchool = new[] { gradeResult.Payload.School.Id },
                    Return = CollectionType.Lov,
                    GetAll = true
                });

                var noPathway = pathwaysResult.Payload.Where(x => x.Description.ToLower() == "no pathway").Select(x => x.Id).FirstOrDefault();

                foreach (var removeIdStudent in body.RemoveIds)
                {
                    var studentGrade = deleteStudentGrades.FirstOrDefault(x => x.IdStudent == removeIdStudent && x.IdGrade == body.IdGrade);
                    if (studentGrade != null)
                    {
                        var currentStudentGradePathway = studentGrade.StudentGradePathways.FirstOrDefault();
                        if (currentStudentGradePathway != null)
                        {
                            if (noPathway == null || currentStudentGradePathway.IdPathway != noPathway)
                            {
                                failedRemoveStudents.Add(NameUtil.GenerateFullNameWithId(studentGrade.IdStudent, studentGrade.Student.FirstName, studentGrade.Student.MiddleName, studentGrade.Student.LastName));
                                continue;
                            }
                        }
                        studentGrade.IsActive = false;
                        _dbContext.Entity<MsStudentGrade>().Update(studentGrade);

                        // Reactivate first assigned grade
                        var firstStudentGrade = firstStudentGrades.Where(x => x.IdStudent == removeIdStudent)
                            .OrderBy(x => x.DateIn)
                            .FirstOrDefault();

                        if (firstStudentGrade != null)
                        {
                            firstStudentGrade.IsActive = true;
                            _dbContext.Entity<MsStudentGrade>().Update(firstStudentGrade);
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            if (failedRemoveStudents.Any())
                throw new BadRequestException($"Failed to remove, the selected student has mapped the student pathway : \n {string.Join(", ", failedRemoveStudents)}");

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<CopyNextAYMapStudentGradeRequest, CopyNextAYMapStudentGradeValidator>();

            var sourceStudentQuery = _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.MsLevel)
                        .ThenInclude(x => x.MsAcademicYear)
                .Include(x => x.Student)
                .Include(x => x.StudentGradePathways)
                .Where(x => x.IdGrade == body.IdGrade).AsQueryable();

            if (body.ExcludeStudentIds.Any())
                sourceStudentQuery = sourceStudentQuery.Where(x => !body.ExcludeStudentIds.Any(y => y == x.IdStudent));

            var sourceStudents = await sourceStudentQuery.ToListAsync(CancellationToken);

            var gradeSourceQuery = await _dbContext.Entity<MsGrade>().Where(x => x.Id == body.IdGrade).FirstOrDefaultAsync(CancellationToken);
            
            if (gradeSourceQuery == null)
                throw new NotFoundException("Grade source not found");

            var gradeTargetQuery = await _dbContext.Entity<MsGrade>()
                .Include(x => x.MsLevel)
                    .ThenInclude(x => x.MsAcademicYear)
                .Where(x => x.MsLevel.IdAcademicYear == body.IdAcademicYearTarget
                && x.OrderNumber == gradeSourceQuery.OrderNumber + 1)
                .FirstOrDefaultAsync(CancellationToken);

            if (gradeTargetQuery == null)
                throw new NotFoundException("Grade target not found");

            var listStudentGradeExist = await _dbContext.Entity<MsStudentGrade>()
                                    .Where(x => x.IdGrade == gradeTargetQuery.Id && x.Grade.MsLevel.IdAcademicYear == body.IdAcademicYearTarget)
                                    .Select(x=> x.IdStudent)
                                    .ToListAsync();

            sourceStudents = sourceStudents.Where(x=> !listStudentGradeExist.Contains(x.IdStudent)).ToList();

            List<MsStudentGrade> newStudentGrades = new List<MsStudentGrade>();
            List<MsStudentGradePathway> newStudentGradePathways = new List<MsStudentGradePathway>();
            foreach (var sourceStudent in sourceStudents)
            {
                var newStudentGrade = new MsStudentGrade
                {
                    Id = Guid.NewGuid().ToString(),
                    IdStudent = sourceStudent.IdStudent,
                    IdGrade = gradeTargetQuery.Id
                };

                newStudentGrades.Add(newStudentGrade);

                if (sourceStudent.StudentGradePathways.Any())
                {
                    var newStudentGradePathway = new MsStudentGradePathway
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdStudentGrade = newStudentGrade.Id,
                        IdPathway = sourceStudent.StudentGradePathways.FirstOrDefault().IdPathwayNextAcademicYear,
                        IdPathwayNextAcademicYear = sourceStudent.StudentGradePathways.FirstOrDefault().IdPathwayNextAcademicYear
                    };

                    newStudentGradePathways.Add(newStudentGradePathway);
                }
            }

            if (newStudentGrades.Any())
                await _dbContext.Entity<MsStudentGrade>().AddRangeAsync(newStudentGrades, CancellationToken);
            if (newStudentGradePathways.Any())
                await _dbContext.Entity<MsStudentGradePathway>().AddRangeAsync(newStudentGradePathways, CancellationToken);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
