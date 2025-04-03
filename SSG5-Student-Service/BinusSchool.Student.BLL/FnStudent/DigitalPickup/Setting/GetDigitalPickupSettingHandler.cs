using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using BinusSchool.Student.FnStudent.DigitalPickup.Validator;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Setting
{
    public class GetDigitalPickupSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDigitalPickupSettingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetDigitalPickupSettingRequest, GetDigitalPickupSettingValidator>();

            //List<string> filterGradeCode = new List<string>(new string[] { "ECY1", "ECY2", "ECY3", "1", "2" });

            var getListSetting = await _dbContext.Entity<MsGrade>()
                .Include(x => x.MsLevel)
                .Where(x => x.MsLevel.IdAcademicYear == param.IdAcademicYear)
                .Where(x => (param.IdLevel == null || param.IdLevel.Count == 0 || param.IdLevel.Contains(x.IdLevel)))
                //.Where(x => filterGradeCode.Contains(x.Code))
                .GroupJoin(_dbContext.Entity<MsDigitalPickupSetting>(),
                    grade => grade.Id,
                    setting => setting.IdGrade,
                    (grade, setting) => new { grade, setting })
                .SelectMany(x => x.setting.DefaultIfEmpty(),
                    (grade, setting) => new { grade.grade, setting })
                //.GroupJoin(_dbContext.Entity<MsDigitalPickupQrCode>(),
                //    gradeSetting => gradeSetting.setting.IdGrade,
                //    qr => qr.IdGrade,
                //    (gradeSetting, qr) => new { gradeSetting, qr })
                //.SelectMany(x => x.qr.DefaultIfEmpty(),
                //    (gradeSetting, qr) => new { gradeSetting.gradeSetting, qr })
                .Select(x => new GetDigitalPickupSettingResult
                {
                    Grade = new ItemValueVm
                    {
                        Id = x.grade.Id,
                        Description = x.grade.Description
                    },
                    Level = new ItemValueVm
                    {
                        Id = x.grade.IdLevel,
                        Description = x.grade.MsLevel.Description
                    },
                    ScanStartTime = x.setting.StartTime,
                    ScanEndTime = x.setting.EndTime,
                    CanDelete = x.setting != null
                })
                .ToListAsync(CancellationToken);

            if (param.IdStatus == "1")
            {
                getListSetting = getListSetting.Where(x => x.ScanStartTime != null && x.ScanEndTime != null).ToList();
            }
            else if (param.IdStatus == "2")
            {
                getListSetting = getListSetting.Where(x => x.ScanStartTime == null && x.ScanEndTime == null).ToList();
            }

            return Request.CreateApiResult2(getListSetting as object);
        }
    }
}
