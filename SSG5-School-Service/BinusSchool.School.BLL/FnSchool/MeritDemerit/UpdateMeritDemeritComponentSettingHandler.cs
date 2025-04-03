
using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
//using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class UpdateMeritDemeritComponentSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public UpdateMeritDemeritComponentSettingHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateMeritDemeritComponentSettingRequest, UpdateMeritDemeritComponentSettingValidator>();

            var GetSchool = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == body.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            if (GetSchool == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.IdSchool));

            var BodyMeritDemeritCompSett = body.MeritDemeritComponentSetting;
            var GetMeritDemeritCompSett = _dbContext.Entity<MsMeritDemeritComponentSetting>()
                .Include(e=>e.Grade).ThenInclude(e=>e.Level).ThenInclude(e=>e.AcademicYear)
                .Where(e=>e.Grade.Level.AcademicYear.IdSchool == body.IdSchool)
                .ToList();

            foreach (var itemMeritDemeritCompSett in BodyMeritDemeritCompSett)
            {
                var GetMeritDemeritCompSettById = GetMeritDemeritCompSett.SingleOrDefault(e => e.IdGrade == itemMeritDemeritCompSett.IdGrade && e.Grade.Level.IdAcademicYear == itemMeritDemeritCompSett.IdAcademicYear);

                if (GetMeritDemeritCompSettById != null)
                {
                    GetMeritDemeritCompSettById.IdGrade = itemMeritDemeritCompSett.IdGrade;
                    GetMeritDemeritCompSettById.IsUseDemeritSystem = itemMeritDemeritCompSett.IsDemeritSystem;
                    GetMeritDemeritCompSettById.IsUsePointSystem = itemMeritDemeritCompSett.IsPointSystem;

                    _dbContext.Entity<MsMeritDemeritComponentSetting>().Update(GetMeritDemeritCompSettById);
                }
                else
                {
                    _dbContext.Entity<MsMeritDemeritComponentSetting>().Add(new MsMeritDemeritComponentSetting
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdGrade = itemMeritDemeritCompSett.IdGrade,
                        IsUsePointSystem = itemMeritDemeritCompSett.IsPointSystem,
                        IsUseDemeritSystem = itemMeritDemeritCompSett.IsPointSystem,
                    });
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
