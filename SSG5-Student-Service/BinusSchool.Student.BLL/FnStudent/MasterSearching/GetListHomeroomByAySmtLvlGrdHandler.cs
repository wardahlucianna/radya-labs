using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class GetListHomeroomByAySmtLvlGrdHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetListHomeroomByAySmtLvlGrdHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListHomeroomByAySmtLvlGrdRequest>(
                    nameof(GetListHomeroomByAySmtLvlGrdRequest.IdAcademicYear));

            var predicate = PredicateBuilder.True<MsHomeroom>();
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);
            if (param.Semester != 0)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);

            var getHomeroomList = await _dbContext.Entity<MsHomeroom>()
                                .Include(mH => mH.Grade).
                                    ThenInclude(mG => mG.MsLevel).
                                    ThenInclude(mLvl => mLvl.MsAcademicYear).
                                    ThenInclude(mAY => mAY.MsSchool)
                                .Where(predicate)
                                .Select(mH => new CodeWithIdVm
                                {
                                    Id = mH.Id,
                                    Code = mH.Grade.Description + " " + mH.MsGradePathwayClassroom.Classroom.Code,
                                    Description = mH.Grade.Description + " " + mH.MsGradePathwayClassroom.Classroom.Code
                                })
                                .ToListAsync(CancellationToken);

            List<GetListHomeroomByAySmtLvlGrdResult> SubjectList = new List<GetListHomeroomByAySmtLvlGrdResult>();
            if (getHomeroomList?.Count() > 1)
            {
                SubjectList.Add(new GetListHomeroomByAySmtLvlGrdResult()
                {
                    Id = "ALL",
                    Code = "All Homeroom",
                    Description = "All Homeroom"
                });
            }

            foreach (var item in getHomeroomList?.GroupBy(x => x.Id))
            {
                SubjectList.Add(new GetListHomeroomByAySmtLvlGrdResult()
                {
                    Id = item.Key,
                    Code = item.Select(x => x.Code).First(),
                    Description = item.Select(x => x.Description).First()
                });
            }

            return Request.CreateApiResult2(SubjectList?.OrderBy(x => x.Description) as object);
        }
    }
}
