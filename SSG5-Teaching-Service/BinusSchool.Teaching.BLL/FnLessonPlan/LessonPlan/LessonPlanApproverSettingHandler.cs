using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanApproverSettingHandler : FunctionsHttpCrudHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public LessonPlanApproverSettingHandler(ITeachingDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetLessonPlanApproverSettingRequest>(nameof(GetLessonPlanApproverSettingRequest.IdSchool));

            var query = _dbContext.Entity<MsLessonPlanApproverSetting>()
                .Include(x => x.Role)
                .Include(x => x.TeacherPosition)
                .Include(x => x.Staff)
                .Where(x => x.Role.IdSchool == param.IdSchool)
                .AsQueryable();

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == Common.Model.Enums.CollectionType.Lov)
            {
                items = await query.Select(x => new GetLessonPlanApproverSettingResult
                {
                    Id = x.Id,
                    IdSchool = x.Role.IdSchool,
                    Role = new CodeWithIdVm
                    {
                        Id = x.IdRole,
                        Code = x.Role.Code,
                        Description = x.Role.Description
                    },
                    Position = !string.IsNullOrEmpty(x.IdBinusian) ? null :
                            new CodeWithIdVm
                            {
                                Id = x.IdTeacherPosition,
                                Code = x.TeacherPosition.Code,
                                Description = x.TeacherPosition.Description
                            },
                    User = !string.IsNullOrEmpty(x.IdBinusian) ? 
                            new CodeWithIdVm
                            {
                                Id = x.IdBinusian,
                                Description = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                            } : null
                }).AsNoTracking().ToListAsync(CancellationToken);
            }
            else
            {
                items = await query.SetPagination(param).Select(x => new GetLessonPlanApproverSettingResult
                {
                    Id = x.Id,
                    IdSchool = x.Role.IdSchool,
                    Role = new CodeWithIdVm
                    {
                        Id = x.IdRole,
                        Code = x.Role.Code,
                        Description = x.Role.Description
                    },
                    Position = !string.IsNullOrEmpty(x.IdBinusian) ? null :
                            new CodeWithIdVm
                            {
                                Id = x.IdTeacherPosition,
                                Code = x.TeacherPosition.Code,
                                Description = x.TeacherPosition.Description
                            },
                    User = !string.IsNullOrEmpty(x.IdBinusian) ?
                            new CodeWithIdVm
                            {
                                Id = x.IdBinusian,
                                Description = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                            } : null
                }).AsNoTracking().ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddLessonPlanApproverSettingRequest, AddLessonPlanApproverSettingValidator>();

            var lessonPlanApproverSettings = await _dbContext.Entity<MsLessonPlanApproverSetting>()
                .Include(x => x.Role)
                .Where(x => x.Role.IdSchool == body.IdSchool).ToListAsync(CancellationToken);

            if (body.ListLessonPlanApproverSettings.Count() == 0)
            {
                lessonPlanApproverSettings.ForEach(x => x.IsActive = false);
                _dbContext.Entity<MsLessonPlanApproverSetting>().UpdateRange(lessonPlanApproverSettings);
            }
            else
            {
                var deletedApproverSettings = lessonPlanApproverSettings.Where(x => !body.ListLessonPlanApproverSettings
                .Select(y => y.Id).Contains(x.Id)).ToList();

                if (deletedApproverSettings.Any())
                {
                    deletedApproverSettings.ForEach(x => x.IsActive = false);
                    _dbContext.Entity<MsLessonPlanApproverSetting>().UpdateRange(deletedApproverSettings);
                }

                foreach (var item in body.ListLessonPlanApproverSettings)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        var newLessonPlanApproverSetting = new MsLessonPlanApproverSetting
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdRole = item.IdRole,
                            IdTeacherPosition = item.IdTeacherPosition,
                            IdBinusian = item.IdStaff
                        };

                        await _dbContext.Entity<MsLessonPlanApproverSetting>().AddAsync(newLessonPlanApproverSetting, CancellationToken);

                        continue;
                    }
                    else
                    {
                        var selectedApproverSetting = lessonPlanApproverSettings.Where(x => x.Id == item.Id).FirstOrDefault() ?? throw new KeyNotFoundException("Lesson plan approver setting not found");
                        selectedApproverSetting.IdRole = item.IdRole;
                        selectedApproverSetting.IdTeacherPosition = item.IdTeacherPosition;
                        selectedApproverSetting.IdBinusian = item.IdStaff;

                        _dbContext.Entity<MsLessonPlanApproverSetting>().Update(selectedApproverSetting);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new System.NotImplementedException();
        }
    }
}
