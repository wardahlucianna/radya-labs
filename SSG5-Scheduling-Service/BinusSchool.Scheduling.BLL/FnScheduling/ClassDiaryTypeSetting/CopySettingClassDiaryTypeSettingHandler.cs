using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class CopySettingClassDiaryTypeSettingHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public CopySettingClassDiaryTypeSettingHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CopySettingClassDiaryTypeSettingRequest, CopySettingClassDiaryTypeSettingValidator>();

            List<CopySettingTypeSetting> ExsisTypeSettingCopy = new List<CopySettingTypeSetting>();

            var typeSettingCopyTo = await _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Where(x => x.IdAcademicyear == body.IdAcademicYearCopyTo)
                .ToListAsync(CancellationToken);

            var typeSetting = await _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Where(x => body.IdClassDiaryTypeSettings.Contains(x.Id))
                .Include(x => x.Academicyear)
                .ToListAsync(CancellationToken);

            var countSuccses = 0;

            foreach (var itemBodyTypeSetting in typeSetting)
            {
                var existsTypeSettingCopy = typeSettingCopyTo.Any(e => e.TypeName == itemBodyTypeSetting.TypeName);
                var typeSettingById = typeSetting.SingleOrDefault(e => e.Id == itemBodyTypeSetting.Id);

                if (!existsTypeSettingCopy)
                {
                    var newTypeSetting = new MsClassDiaryTypeSetting
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicyear = body.IdAcademicYearCopyTo,
                        TypeName = typeSettingById.TypeName,
                        OccurrencePerDay = typeSettingById.OccurrencePerDay,
                        MinimumStartDay = typeSettingById.MinimumStartDay,
                        AllowStudentEntryClassDiary = typeSettingById.AllowStudentEntryClassDiary
                    };

                    _dbContext.Entity<MsClassDiaryTypeSetting>().Add(newTypeSetting);

                    countSuccses++;
                }
                else
                {
                    ExsisTypeSettingCopy.Add(new CopySettingTypeSetting
                    {
                        IdAcademicYear = new CodeWithIdVm
                        {
                            Id = typeSettingById.Academicyear.Id,
                            Code = typeSettingById.IdAcademicyear,
                            Description = typeSettingById.Academicyear.Description
                        },
                        TypeName = typeSettingById.TypeName,
                        OccurencePerDay = typeSettingById.OccurrencePerDay,
                        MinimumStartDay = typeSettingById.MinimumStartDay,
                        AllowStudentEntryClassDiary = typeSettingById.AllowStudentEntryClassDiary
                    });
                }
            }

            CopySettingClassDiaryTypeSettingResult Items = new CopySettingClassDiaryTypeSettingResult
            {
                CountSucces = countSuccses.ToString(),
                CopySettingTypeSettings = ExsisTypeSettingCopy,
            };

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(Items as object);
        }

    }
}
