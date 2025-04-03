using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetSameDataClassDiaryTypeSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetSameDataClassDiaryTypeSettingHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new GetSameDataClassDiaryTypeSettingResult();

            var body = await Request.ValidateBody<CopySettingClassDiaryTypeSettingRequest, CopySettingClassDiaryTypeSettingValidator>();

            var dataCurrentAY = _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Include(x => x.Academicyear)
                .Where(x => x.IdAcademicyear == body.IdAcademicYearCopyFrom && body.IdClassDiaryTypeSettings.Contains(x.Id))
                .Select(x => x.TypeName).ToList();

            var forcastAY = _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Include(x => x.Academicyear)
                .Where(x => x.IdAcademicyear == body.IdAcademicYearCopyTo)
                .Select(x => x.TypeName).ToList();

            var data = dataCurrentAY.Where(x => forcastAY.Any(y => y == x)).ToList();

            result.IsAllowToAY = data.Count == 0 ? true : false;

            return Request.CreateApiResult2(result as object);
        }
    }
}
