using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSubject.Subject.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSubject.Subject
{
    public class CopySubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public CopySubjectHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CopySubjectRequest, CopySubjectValidator>();
            // check academic year from & to
            var reqAcadyears = new[] { body.IdAcadyearFrom, body.IdAcadyearTo };
            var acadyears = await _dbContext.Entity<MsAcademicYear>()
                .Where(x => reqAcadyears.Contains(x.Id))
                // .Select(x => x.Id)
                .ToListAsync(CancellationToken);
            // throw when any not exist academic year
            if (acadyears.Count != reqAcadyears.Length)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Acadyear"], "Id", string.Join(", ", reqAcadyears.Except(acadyears.Select(x => x.Id)))));

            // take existing subject that will copy
            var subjects = await _dbContext.Entity<MsSubject>()
                .Include(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                .Include(x => x.SubjectPathways).ThenInclude(x => x.GradePathwayDetail).ThenInclude(x => x.Pathway)
                .Include(x => x.SubjectMappingSubjectLevels)
                .Include(x => x.SubjectSessions)
                .Include(x => x.Department)
                .Where(x => x.Grade.Level.IdAcademicYear == body.IdAcadyearFrom || x.Grade.Level.IdAcademicYear == body.IdAcadyearTo)
                .ToListAsync(CancellationToken);
            // throw when existing subject empty

            var subjectsFrom = subjects.Where(x => x.Grade.Level.IdAcademicYear == body.IdAcadyearFrom).ToList();
            var subjectsTo = subjects.Where(x => x.Grade.Level.IdAcademicYear == body.IdAcadyearTo).ToList();
            if (subjectsFrom.Count == 0)
                throw new BadRequestException(string.Format(Localizer["ExNotHave"], $"{Localizer["Acadyear"]} {acadyears.Find(x => x.Id == body.IdAcadyearTo).Description}", Localizer["Subject"]));
            // make sure academic year to dont have any subject
            //var acadyearTo = subjectsFrom.Find(x => x.Grade.Level.IdAcademicYear == body.IdAcadyearTo);
            //if (acadyearTo != null)
            //    throw new BadRequestException(string.Format(Localizer["ExAlreadyHas"], Localizer["Acadyear"], acadyearTo.Grade.Level.AcademicYear.Description, Localizer["Subject"]));

            var gradeFrom = await _dbContext.Entity<MsGrade>()
                .Where(x => x.Level.IdAcademicYear == body.IdAcadyearFrom)
                .ToListAsync(CancellationToken);


            // take existing grade that will receive copy
            var gradesTo = await _dbContext.Entity<MsGrade>()
                .Where(x => x.Level.IdAcademicYear == body.IdAcadyearTo)
                .ToListAsync(CancellationToken);

            var gradeNotExist = gradeFrom.Where(x => gradesTo.Any(y => y.Code != x.Code)).Select(x => x.Description).ToList();

            // throw when grade empty
            // if (gradesTo.Count != gradeFrom.Count)
            //     //throw new BadRequestException(string.Format(Localizer["ExNotHave"], $"{Localizer["Acadyear"]} {acadyears.Find(x => x.Id == body.IdAcadyearTo).Description}", Localizer["Grade"]));
            //     throw new BadRequestException($"Grade {string.Join(",", gradeNotExist)} not exist in academic year {acadyears.Find(x => x.Id == body.IdAcadyearTo).Description}");


            var departmentFrom = await _dbContext.Entity<MsDepartment>()
               .Where(x => x.IdAcademicYear == body.IdAcadyearFrom)
               .ToListAsync(CancellationToken);


            // take existing grade that will receive copy
            var departmentTo = await _dbContext.Entity<MsDepartment>()
                .Where(x => x.IdAcademicYear == body.IdAcadyearTo)
                .ToListAsync(CancellationToken);

            var departmentNotExist = departmentFrom.Where(x => departmentTo.Any(y => y.Code != x.Code)).Select(x => x.Description).ToList();

            // if (departmentTo.Count != departmentFrom.Count)
            //     //throw new BadRequestException(string.Format(Localizer["ExNotHave"], $"{Localizer["Acadyear"]} {acadyears.Find(x => x.Id == body.IdAcadyearTo).Description}", Localizer["Grade"]));
            //     throw new BadRequestException($"Department {string.Join(",", departmentNotExist)} not exist in academic year {acadyears.Find(x => x.Id == body.IdAcadyearTo).Description}");
            // take existing pathway detail that will receive copy

            var pathwayDetailsFrom = await _dbContext.Entity<MsGradePathwayDetail>()
               .Include(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.Level)
               .Where(x => x.GradePathway.Grade.Level.IdAcademicYear == body.IdAcadyearFrom)
               .ToListAsync(CancellationToken);


            var pathwayDetailsTo = await _dbContext.Entity<MsGradePathwayDetail>()
                .Include(x => x.Pathway)
                .Include(x => x.GradePathway).ThenInclude(x => x.Grade)
                .Where(x => x.GradePathway.Grade.Level.IdAcademicYear == body.IdAcadyearTo)
                .ToListAsync(CancellationToken);

            // if (pathwayDetailsTo.Count < pathwayDetailsFrom.Count)
            //     throw new BadRequestException($"Acadyear {acadyears.Find(x => x.Id == body.IdAcadyearTo).Description} doesnt have same data with class package in Acadyear {acadyears.Find(x => x.Id == body.IdAcadyearFrom).Description}");

            foreach (var subject in subjectsFrom)
            {
                if (subjectsTo.Find(x => x.SubjectID == subject.SubjectID) != null)
                    continue;
                if (gradesTo.Where(x => x.Code == subject.Grade.Code).Select(x => x.Id).FirstOrDefault() == null)
                    continue;
                if (departmentTo.Where(x => x.Code == subject.Department.Code).Select(x => x.Id).FirstOrDefault() == null)
                    continue;
                var subjectTo = new MsSubject
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGrade = gradesTo.Where(x => x.Code == subject.Grade.Code).Select(x => x.Id).FirstOrDefault(),
                    IdDepartment = departmentTo.Where(x => x.Code == subject.Department.Code).Select(x => x.Id).FirstOrDefault(),
                    IdCurriculum = subject.IdCurriculum,
                    IdSubjectType = subject.IdSubjectType,
                    Code = subject.Code,
                    Description = subject.Description,
                    MaxSession = subject.MaxSession,
                    SubjectID = subject.SubjectID,
                    IdSubjectGroup = subject.IdSubjectGroup,
                    IsNeedLessonPlan = subject.IsNeedLessonPlan,
                };
                _dbContext.Entity<MsSubject>().Add(subjectTo);
                if (subject.SubjectSessions.Count != 0)
                {
                    foreach (var sessionFrom in subject.SubjectSessions)
                    {
                        var sessionTo = new MsSubjectSession
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSubject = subjectTo.Id,
                            Content = sessionFrom.Content,
                            Length = sessionFrom.Length
                        };
                        _dbContext.Entity<MsSubjectSession>().Add(sessionTo);
                    }
                }
                if (subject.SubjectMappingSubjectLevels.Count != 0)
                {
                    foreach (var subjectLevelFrom in subject.SubjectMappingSubjectLevels)
                    {
                        var subjectLevelTo = new MsSubjectMappingSubjectLevel
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSubject = subjectTo.Id,
                            IdSubjectLevel = subjectLevelFrom.IdSubjectLevel,
                            IsDefault = subjectLevelFrom.IsDefault,
                        };
                        _dbContext.Entity<MsSubjectMappingSubjectLevel>().Add(subjectLevelTo);
                    }
                }
                if (subject.SubjectPathways.Count != 0)
                {
                    foreach (var pathwayFrom in subject.SubjectPathways)
                    {
                        var pathwayDetailDest = pathwayDetailsTo.FirstOrDefault(x =>
                           x.Pathway.Code == pathwayFrom.GradePathwayDetail.Pathway.Code
                           && x.GradePathway.Grade.Code == pathwayFrom.GradePathwayDetail.GradePathway.Grade.Code);
                        if (pathwayDetailDest != null)
                        {
                            var pathwayTo = new MsSubjectPathway
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdSubject = subjectTo.Id,
                                IdGradePathwayDetail = pathwayDetailDest.Id
                            };
                            _dbContext.Entity<MsSubjectPathway>().Add(pathwayTo);
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
