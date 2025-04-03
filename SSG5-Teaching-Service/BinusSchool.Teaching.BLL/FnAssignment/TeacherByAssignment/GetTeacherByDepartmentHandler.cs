using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherByAssignment;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Teaching.FnAssignment.TeacherByAssignment
{
    public class GetTeacherByDepartmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private readonly IStringLocalizer _localizer;

        public GetTeacherByDepartmentHandler(ITeachingDbContext teachingDbContext, IApiService<ISchool> schoolApi, IStringLocalizer localizer)
        {
            _teachingDbContext = teachingDbContext;
            _localizer = localizer;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            // var result = new List<GetTimeTableByUserResult>();
            //var param = Request.ValidateParams<GetTimeTableByUserRequest>(new string[] { nameof(GetTimeTableByUserRequest.IdSchoolUser), nameof(GetTimeTableByUserRequest.IdSchoolAcademicYear) });
            var param = await Request.GetBody<GetTeacherByDepartmentRequest>();

            // var data = await _teachingDbContext.Entity<TrTimeTablePrefHeader>()
            // .Include(p => p.Childs)
            // .Include(p => p.TimetablePrefDetails)
            // .ThenInclude(p => p.TeachingLoads)
            // .Where(p => p.IsActive && p.IdParent == null)
            // .ToListAsync(CancellationToken);

            var data2 = await _teachingDbContext.Entity<TrTeachingLoad>()
            .Include(x => x.TimetablePrefDetail)
            .Where(x => x.TimetablePrefDetail != null)
            .ToListAsync(CancellationToken);

            //result =
            //(
            //    from _dataTeachingLoad in data2
            //    join _dataSubjectCombination in reqSubjectCombination.Payload on _dataTeachingLoad.TimetablePrefDetail.IdTimetablePrefHeader equals _dataSubjectCombination.Id
            //    where
            //        _dataSubjectCombination.AcadYear.Id == param.AcademicYearId
            //    select new GetTimeTableByUserResult
            //    {
            //        Id = _dataTeachingLoad.Id,
            //        Class = _dataSubjectCombination.Class.Description,
            //        Department = _dataSubjectCombination.Department.Description,
            //        Grade = _dataSubjectCombination.Grade.Description,
            //        Subject = _dataSubjectCombination.Subject.SubjectName,
            //        TotalSession = _dataTeachingLoad.Load
            //    }
            //).ToList();

            var idSubjectCombs = data2.Select(x => x.TimetablePrefDetail.IdTimetablePrefHeader).Distinct();
            var subjectCombs = await _teachingDbContext.Entity<MsSubjectCombination>()
                .Where(x
                    => idSubjectCombs.Contains(x.Id)
                    && x.Subject.Grade.Level.IdAcademicYear == param.AcademicYearId)
                .Select(x => new { x.Id, Department = new ItemValueVm(x.Subject.IdDepartment, x.Subject.Department.Description) })
                .ToListAsync(CancellationToken);

            var retVal =
            (
                from _dataTeachingLoad in data2
                join _dataSubjectCombination in subjectCombs on _dataTeachingLoad.TimetablePrefDetail.IdTimetablePrefHeader equals _dataSubjectCombination.Id
                select new GetTeacherByDepartmentResult
                {
                    BinusianId = _dataTeachingLoad.IdUser,
                    Department = _dataSubjectCombination.Department.Description
                }
                ).Where(x => x.Department == param.Department).ToList();


            return Request.CreateApiResult2(retVal as object, null);
        }

    }
}
