using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSubject.Subject
{
    public class SubjectPathwayHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public SubjectPathwayHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetSubjectPathwayForAscTimetableRequest>();
            var getData = await _dbContext.Entity<MsSubject>()
                                         .Include(p => p.Grade)
                                         .Include(p => p.SubjectMappingSubjectLevels)
                                               .ThenInclude(p=> p.SubjectLevel)
                                         .Include(p => p.SubjectPathways)
                                         .Where(p => (param.SubjectCode.Any(x => x == p.Code) || param.SubjectCode.Any(x => x == p.SubjectID)) && 
                                                     p.SubjectType.IdSchool == param.IdSchool && 
                                                     param.GradeCode.Any(x=> x== p.Grade.Code))
                                         .Select(p=> new GetSubjectPathwayForAscTimetableResult 
                                         {
                                            IdSubject=p.Id,
                                            SubjectCode=p.SubjectID,
                                            IdGrade=p.IdGrade,
                                            GradeCode=p.Grade.Code,
                                            SubjectDescription=p.Description,
                                            SubjectLevel=p.SubjectMappingSubjectLevels.Count()>0?p.SubjectMappingSubjectLevels.Select(x=> new SubjectLevelVM 
                                            {
                                                IdSubjectLevel=x.IdSubjectLevel,
                                                SubjectlevelCode=x.SubjectLevel.Code,
                                                SubjectlevelDesc=x.SubjectLevel.Description,
                                                IsDefault=true,
                                            }).ToList():new List<SubjectLevelVM>(),
                                         })
                                         .ToListAsync();

            return Request.CreateApiResult2(getData as object);
        }
    }
}
