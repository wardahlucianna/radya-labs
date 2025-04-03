using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.SubjectCombination
{
    public class GetSubjectCombinationHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetSubjectCombinationRequest.IdGrade),
            nameof(GetSubjectCombinationRequest.IdAcadyear)
        });

        private readonly ITeachingDbContext _dbContext;

        public GetSubjectCombinationHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new GetSubjectCombinationResult();

            var param = Request.ValidateParams<GetSubjectCombinationRequest>(_requiredParams.Value);
            var predicate1 = PredicateBuilder.Create<MsGradePathwayClassroom>(x
                => x.GradePathway.Grade.Level.IdAcademicYear == param.IdAcadyear
                && x.GradePathway.IdGrade == param.IdGrade);
            var predicate2 = PredicateBuilder.Create<MsSubject>(x => x.SubjectPathways.Any(y
                => y.GradePathwayDetail.GradePathway.Grade.Level.IdAcademicYear == param.IdAcadyear
                && x.SubjectPathways.Any(z => z.GradePathwayDetail.GradePathway.IdGrade == param.IdGrade)));

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                predicate2 = predicate2.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            #region Columns

            var pathwayClassrooms = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(p => p.Classroom)
                .Include(p => p.GradePathway).ThenInclude(p => p.GradePathwayDetails).ThenInclude(p => p.Pathway)
                .Include(p => p.GradePathway).ThenInclude(p => p.Grade)
                .OrderBy(p => p.Classroom.Code)
                .Where(predicate1)
                .ToListAsync(CancellationToken);

            var classDataVms = pathwayClassrooms
                .Select(p => new ClassData
                {
                    Id = p.Id,
                    Code = p.Classroom.Code,
                    Description = p.Classroom.Description,
                    Pathways = p.GradePathway.GradePathwayDetails
                        .Select(x => new PathwayData()
                        {
                            Id = x.IdPathway,
                            Code = x.Pathway.Code,
                            Description = x.Pathway.Description,
                        })
                        .ToList()
                });
            result.ClassDataColumns = classDataVms.ToList();

            #endregion

            #region Row

            // need to ignore query filter to get subject combinations, including that already not active
            var subjects = await _dbContext.Entity<MsSubject>()
                .Include(p => p.Grade)
                .Include(p => p.SubjectPathways).ThenInclude(p => p.GradePathwayDetail)
                .Include(p => p.SubjectCombinations).ThenInclude(p => p.GradePathwayClassroom)
                .Where(predicate2)
                .IgnoreQueryFilters()
                .ToListAsync(CancellationToken);

            var subjectDataVms = subjects
                .Where(x => x.IsActive)
                .Select(p => new SubjectData
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description,
                    TotalSession = p.MaxSession,
                    Pathways = p.SubjectPathways.Distinct().Where(x => x.IsActive).Select(x => new PathwayData
                    {
                        Id = x.GradePathwayDetail.IdPathway,
                        Code = x.GradePathwayDetail.Pathway.Code,
                        Description = x.GradePathwayDetail.Pathway.Description,
                    }).ToList(),
                    SubjectCombinationData = p.SubjectCombinations.Where(x => x.IsActive).Select(x =>
                        new SubjectCombinationData
                        {
                            Id = x.Id,
                            IdClassGrade = x.IdGradePathwayClassroom,
                            IdSubject = x.IdSubject
                        }).ToList(),
                    Grade = new CodeWithIdVm
                    {
                        Id = p.Grade.Id,
                        Code = p.Grade.Code,
                        Description = p.Grade.Description,
                    }
                });
            result.SubjectDataRow = subjectDataVms.ToList();

            #endregion

            if (subjects.Count != 0)
            {
                var subjectCombs = subjects
                    .Where(x => x.SubjectCombinations.Count != 0)
                    .SelectMany(x => x.SubjectCombinations)
                    .OrderByDescending(x => x.DateIn).ThenByDescending(x => x.DateUp)
                    .ToArray();

                if (subjectCombs.Length != 0)
                {
                    // enable/disable edit subject combination
                    var idSubjectCombs = subjectCombs.Where(x => x.IsActive).Select(x => x.Id).Distinct();
                    // fetch timetable status with id subject combinations
                    var ttResults = await _dbContext.Entity<TrTimeTablePrefHeader>()
                        .Where(x => idSubjectCombs.Contains(x.Id))
                        .ToListAsync(CancellationToken);
                    // update field isAlreadyUse based on previous timetable status
                    result.SubjectDataRow
                        .ForEach(x => x.SubjectCombinationData
                            .ForEach(y => y.IsAlreadyUse = ttResults?.Find(z => z.Id == y.Id)?.Status ?? false));

                    // audit result
                    result.Audit = subjectCombs.First().GetRawAuditResult2();
                    var idUsers = new[] { result.Audit.UserIn?.Id, result.Audit.UserUp?.Id };
                    var users = await _dbContext.Entity<MsUser>()
                        .Where(x => idUsers.Contains(x.Id))
                        .Select(x => new NameValueVm(x.Id, x.DisplayName))
                        .ToListAsync(CancellationToken);

                    if (result.Audit.UserIn != null)
                        result.Audit.UserIn.Name = users.Find(x => x.Id == result.Audit.UserIn.Id)?.Name;
                    if (result.Audit.UserUp != null)
                        result.Audit.UserUp.Name = users.Find(x => x.Id == result.Audit.UserUp.Id)?.Name;
                }
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
