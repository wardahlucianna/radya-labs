using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Student;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.User.FnBlocking.Blocking
{
    public class GetListStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private IDictionary<string, object> _Data;
        private IUserDbContext _dbContext;
        private IFeatureManagerSnapshot _featureManager;

        public GetListStudentBlockingHandler(IUserDbContext userDbContext, [FromServices] IFeatureManagerSnapshot featureManager)
        {
            _dbContext = userDbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            IReadOnlyList<IItemValueVm> items = default;

            var param = Request.ValidateParams<GetListBlockingRequest>();

            if (!string.IsNullOrEmpty(param.IdStudent))
            {
                if (param.IdStudent[0] == 'P')
                {
                    param.IdStudent = param.IdStudent[1..];
                }
            }

            var isFeatureActive = await _featureManager.IsEnabledAsync(FeatureFlags.StudentBlocking);

            if (isFeatureActive)
            {
                var Message = await _dbContext.Entity<MsBlockingMessage>()
               .Where(e => e.IdSchool == param.IdSchool)
               .Select(e => e.Content)
               .FirstOrDefaultAsync(CancellationToken);
                if (Message != null)
                {
                    var Content = Handlebars.Compile(Message);

                    var GetListStudentBlocking = await (from StudentBlocking in _dbContext.Entity<MsStudentBlocking>()
                                                        join Student in _dbContext.Entity<MsStudent>() on StudentBlocking.IdStudent equals Student.Id
                                                        join BlockingType in _dbContext.Entity<MsBlockingType>() on StudentBlocking.IdBlockingType equals BlockingType.Id
                                                        join BlockingCategory in _dbContext.Entity<MsBlockingCategory>() on StudentBlocking.IdBlockingCategory equals BlockingCategory.Id
                                                        join BlockingMessage in _dbContext.Entity<MsBlockingMessage>() on StudentBlocking.IdBlockingCategory equals BlockingMessage.IdCategory
                                                        join Feature in _dbContext.Entity<MsFeature>() on BlockingType.IdFeature equals Feature.Id into JoinedFeature
                                                        from Feature in JoinedFeature.DefaultIfEmpty()
                                                        join BlockingTypeSubFeature in _dbContext.Entity<MsBlockingTypeSubFeature>() on BlockingType.Id equals BlockingTypeSubFeature.IdBlockingType into JoinedSubFeature
                                                        from BlockingTypeSubFeature in JoinedSubFeature.DefaultIfEmpty()
                                                        join SubFeature in _dbContext.Entity<MsFeature>() on BlockingTypeSubFeature.IdSubFeature equals SubFeature.Id into _JoinedSubFeature
                                                        from SubFeature in _JoinedSubFeature.DefaultIfEmpty()
                                                        where StudentBlocking.IsBlocked == true && StudentBlocking.IdStudent == param.IdStudent && BlockingType.Category == "FEATURE"
                                                        select new
                                                        {
                                                            Id = StudentBlocking.Id,
                                                            BlockingTypeCategory = BlockingType.Category,
                                                            BlockingCategoryName = BlockingCategory.Name,
                                                            IdFeature = BlockingType.IdFeature,
                                                            ActionFeature = Feature.Action,
                                                            ControllerFeature = Feature.Controller,
                                                            IdSubFeature = BlockingTypeSubFeature.IdSubFeature,
                                                            ActionSubFeature = SubFeature.Action,
                                                            ControllerSubFeature = SubFeature.Controller,
                                                            BlockingMessage = BlockingMessage.Content
                                                        })
                                           .ToListAsync(CancellationToken);

                    var listDataBLocking = new List<GetListBlockingResult>();

                    var getListStudentBlockingFeature = GetListStudentBlocking.Where(x => x.IdSubFeature == null).ToList();
                    foreach (var BLocking in getListStudentBlockingFeature.Select(x => new { idFeature = x.IdFeature }).Distinct().ToList())
                    {
                        List<string> GetCategoryName = default;
                        GetCategoryName = getListStudentBlockingFeature
                                    .Where(e => e.IdFeature == BLocking.idFeature)
                                    .Select(e => e.BlockingCategoryName)
                                    .ToList();

                        var GeneratedContent = "";
                        if (GetCategoryName != null)
                        {
                            _Data = new Dictionary<string, object>
                            {
                                { "categoryBlocking", string.Join(", ", GetCategoryName) },
                            };
                            GeneratedContent = Content(_Data);
                            foreach (var dataBLocking in GetListStudentBlocking.Where(x => x.IdFeature == BLocking.idFeature))
                            {
                                listDataBLocking.Add(new GetListBlockingResult
                                {
                                    Id = dataBLocking.Id,
                                    IdFeature = dataBLocking.IdFeature,
                                    ActionFeature = dataBLocking.ActionFeature,
                                    ControllerFeature = dataBLocking.ControllerFeature,
                                    IdSubFeature = dataBLocking.IdSubFeature,
                                    ActionSubFeature = dataBLocking.ActionSubFeature,
                                    ControllerSubFeature = dataBLocking.ControllerSubFeature,
                                    BLockingMessage = dataBLocking.BlockingMessage,
                                });
                            }
                        }
                    }


                    var getListStudentBlockingSubFeature = GetListStudentBlocking.Where(x => x.IdSubFeature != null).ToList();
                    foreach (var BLocking in getListStudentBlockingSubFeature.Select(x => new { idSubFeature = x.IdSubFeature }).Distinct().ToList())
                    {
                        List<string> GetCategoryName = default;
                        GetCategoryName = getListStudentBlockingSubFeature
                                    .Where(e => e.IdSubFeature == BLocking.idSubFeature)
                                    .Select(e => e.BlockingCategoryName)
                                    .ToList();

                        var GeneratedContent = "";
                        if (GetCategoryName != null)
                        {
                            _Data = new Dictionary<string, object>
                            {
                                { "categoryBlocking", string.Join(", ", GetCategoryName) },
                            };
                            GeneratedContent = Content(_Data);
                            foreach (var dataBLocking in GetListStudentBlocking.Where(x => x.IdSubFeature == BLocking.idSubFeature))
                            {
                                listDataBLocking.Add(new GetListBlockingResult
                                {
                                    Id = dataBLocking.Id,
                                    IdFeature = dataBLocking.IdFeature,
                                    ActionFeature = dataBLocking.ActionFeature,
                                    ControllerFeature = dataBLocking.ControllerFeature,
                                    IdSubFeature = dataBLocking.IdSubFeature,
                                    ActionSubFeature = dataBLocking.ActionSubFeature,
                                    ControllerSubFeature = dataBLocking.ControllerSubFeature,
                                    BLockingMessage = dataBLocking.BlockingMessage,
                                });
                            }
                        }
                    }


                    items = listDataBLocking.Select(x => new GetListBlockingResult
                    {
                        Id = x.Id,
                        IdFeature = x.IdFeature,
                        ActionFeature = x.ActionFeature,
                        ControllerFeature = x.ControllerFeature,
                        IdSubFeature = x.IdSubFeature,
                        ActionSubFeature = x.ActionSubFeature,
                        ControllerSubFeature = x.ControllerSubFeature,
                        BLockingMessage = x.BLockingMessage,
                    }).ToList();
                }                
            }            

            return Request.CreateApiResult2(items as object);
        }
    }
}
