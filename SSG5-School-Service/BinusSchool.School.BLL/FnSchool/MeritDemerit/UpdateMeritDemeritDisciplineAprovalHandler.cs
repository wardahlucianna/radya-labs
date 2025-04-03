using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.School.FnSchool.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class UpdateMeritDemeritDisciplineAprovalHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public UpdateMeritDemeritDisciplineAprovalHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateMeritDemeritDisciplineAprovalRequest, UpdateMeritDemeritDisciplineAprovalValidator>();

            var GetSchool = await _dbContext.Entity<MsAcademicYear>()
                .Where(x => x.Id == body.IdAcademic)
                .FirstOrDefaultAsync(CancellationToken);

            if (GetSchool == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", body.IdAcademic));

            var BodyMeritDemeritApprovalSetting = body.MeritDemeritApprovalSetting;
            var GetMeritDemeritApprovalSetting = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
                .Include(e => e.Level).ThenInclude(e => e.AcademicYear)
                .Where(e => e.Level.AcademicYear.Id == body.IdAcademic)
                .ToListAsync(CancellationToken);

            var GetTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
               .Where(e => e.IdSchool == GetSchool.IdSchool)
               .ToListAsync(CancellationToken);

            foreach (var ItemMeritDemeritApprovalSetting in BodyMeritDemeritApprovalSetting)
            {
                var ItemMeritDemeritApprovalSettingByID = GetMeritDemeritApprovalSetting.SingleOrDefault(e => e.IdLevel == ItemMeritDemeritApprovalSetting.IdLevel && e.Level.IdAcademicYear == ItemMeritDemeritApprovalSetting.IdAcademicYear);

                var ExsisTeacherPosaitionApproval1 = GetTeacherPosition.Any(e => e.Id == ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval1);
                var ExsisTeacherPosaitionApproval2 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval2 == null?true:GetTeacherPosition.Any(e => e.Id == ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval2);
                var ExsisTeacherPosaitionApproval3 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval3 == null ? true : GetTeacherPosition.Any(e => e.Id == ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval3);

                if (!ExsisTeacherPosaitionApproval1)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["TeacherPosition"], "Id", ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval1));

                if (!ExsisTeacherPosaitionApproval2)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["TeacherPosition"], "Id", ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval2));

                if (!ExsisTeacherPosaitionApproval3)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["TeacherPosition"], "Id", ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval3));

                if (ItemMeritDemeritApprovalSettingByID != null)
                {
                    ItemMeritDemeritApprovalSettingByID.IdLevel = ItemMeritDemeritApprovalSetting.IdLevel;
                    ItemMeritDemeritApprovalSettingByID.Approval1 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval1;
                    ItemMeritDemeritApprovalSettingByID.Approval2 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval2 == null ? null : ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval2;
                    ItemMeritDemeritApprovalSettingByID.Approval3 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval3 == null ? null : ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval3;

                    _dbContext.Entity<MsMeritDemeritApprovalSetting>().Update(ItemMeritDemeritApprovalSettingByID);
                }
                else
                {
                    _dbContext.Entity<MsMeritDemeritApprovalSetting>().Add(new MsMeritDemeritApprovalSetting
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdLevel = ItemMeritDemeritApprovalSetting.IdLevel,
                        Approval1 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval1,
                        Approval2 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval2== null ? null: ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval2,
                        Approval3 = ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval3 == null ? null : ItemMeritDemeritApprovalSetting.IdTeacherPositionApproval3,
                    });
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }


    }
}
