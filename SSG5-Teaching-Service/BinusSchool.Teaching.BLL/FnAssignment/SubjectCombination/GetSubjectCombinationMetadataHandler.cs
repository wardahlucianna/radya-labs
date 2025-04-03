using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnAssignment.SubjectCombination.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.SubjectCombination
{
    public class GetSubjectCombinationMetadataHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public GetSubjectCombinationMetadataHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<IdCollection, SubjectCombinationMetadataValidator>();
            var metaDataSubjectCombination = await _dbContext.Entity<MsSubjectCombination>()
                .Where(x=> body.Ids.Contains(x.Id))
                .Select(x=>new GetListSubjectCombinationTimetableResult
                {
                    Id = x.Id,
                    AcadYear = new CodeView
                    {
                        Id = x.Subject.Grade.Level.IdAcademicYear,
                        Code = x.Subject.Grade.Level.AcademicYear.Code,
                        Description = x.Subject.Grade.Level.AcademicYear.Description,
                        IdMapping = x.GradePathwayClassroom.GradePathway.Grade.Level.AcademicYear.Id
                    },
                    Level = new CodeView
                    {
                        Id = x.Subject.Grade.IdLevel,
                        Code = x.Subject.Grade.Level.Code,
                        Description = x.Subject.Grade.Level.Description
                    },
                    Grade = new CodeView
                    {
                        Id = x.Subject.IdGrade,
                        Code = x.Subject.Grade.Code,
                        Description = x.Subject.Grade.Description
                    },
                    Class = new CodeView
                    {
                        Id = x.GradePathwayClassroom.Id,
                        Code = x.GradePathwayClassroom.Classroom.Code,
                        Description = x.GradePathwayClassroom.Classroom.Description
                    },
                    Subject = new SubjectVm
                    {
                        Id = x.Subject.Id,
                        SubjectId = x.Subject.SubjectID,
                        SubjectName = x.Subject.Code,
                        Description = x.Subject.Description,
                        MaxSession = x.Subject.MaxSession
                    },
                    Department = new CodeView
                    {
                        Id = x.Subject.Department.Id,
                        Code = x.Subject.Department.Code,
                        Description = x.Subject.Department.Description,
                    },
                    Streaming = x.Subject.SubjectPathways.Select(sp => new CodeView
                    {
                        Id = sp.GradePathwayDetail.Pathway.Id,//string.Join(",",x.Subject.SubjectPathways.Select(x=>x.GradePathwayDetail.Pathway.Id)),
                        Code = sp.GradePathwayDetail.Pathway.Code,//string.Join(" & ", x.Subject.SubjectPathways.Select(p => p.GradePathwayDetail.Pathway.Code)),
                        Description = sp.GradePathwayDetail.Pathway.Description,//string.Join(" & ", x.Subject.SubjectPathways.Select(p => p.GradePathwayDetail.Pathway.Description))
                    }).ToList(),
                    TotalSession = x.Subject.MaxSession
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(metaDataSubjectCombination as object);
        }
    }
}
