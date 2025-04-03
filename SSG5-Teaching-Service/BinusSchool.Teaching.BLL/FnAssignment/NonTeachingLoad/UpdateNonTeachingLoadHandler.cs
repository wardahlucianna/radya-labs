using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnAssignment.NonTeachingLoad.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad
{
    public class UpdateNonTeachingLoadHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IStringLocalizer _localizer;

        public UpdateNonTeachingLoadHandler(ITeachingDbContext dbContext, IStringLocalizer localizer)
        {
            _dbContext = dbContext;
            _localizer = localizer;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateNonTeachLoadRequest, UpdateNonTeachLoadValidator>();

            var existNonTeachingLoad = await _dbContext.Entity<MsNonTeachingLoad>().FindAsync(new[] { body.Id }, CancellationToken);
            if (existNonTeachingLoad is null)
                throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["NonTeachingLoad"], "Id", body.Id));

            var position = await _dbContext.Entity<MsTeacherPosition>().FirstOrDefaultAsync(x => x.Id == body.IdPosition, CancellationToken);
            if (position is null)
                throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["TeacherPosition"], "Id", body.IdPosition));

            var sameNonTeachingLoad = await _dbContext.Entity<MsNonTeachingLoad>()
                .Where(x => x.IdAcademicYear == body.IdAcadyear && x.IdTeacherPosition == body.IdPosition && x.Category == body.Category && x.Id != body.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (sameNonTeachingLoad != null)
                throw new BadRequestException(string.Format(_localizer["ExAlreadyUseInsert"], position.Description));

            #region Previous non teaching load by AY validation
            if (body.Category == Common.Model.Enums.AcademicType.Academic)
            {
                string previousAY = body.IdAcadyear.GetPreviousAcademicYear(4);

                var previousNonTeachingLoad = await _dbContext.Entity<MsNonTeachingLoad>()
                    .Where(x => x.IdAcademicYear == previousAY && x.Category == body.Category && x.IdTeacherPosition == body.IdPosition)
                    .Select(x => x.Parameter)
                    .FirstOrDefaultAsync(CancellationToken);

                if (previousNonTeachingLoad != null)
                {
                    List<CreateHierarchyResult> previousHierarchies = JsonConvert.DeserializeObject<List<CreateHierarchyResult>>(previousNonTeachingLoad, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    if (existNonTeachingLoad.Category == Common.Model.Enums.AcademicType.Academic)
                    {
                        if (previousHierarchies.Count != body.Hierarchies.Count)
                            throw new BadRequestException("Hierarchy must be same from last year");

                        foreach (var requestHierarchy in body.Hierarchies)
                        {
                            if (!previousHierarchies.Any(x => x.Class.ToLower() == requestHierarchy.ToLower()))
                                throw new BadRequestException("Hierarchy must be same from last year");
                        }
                    }
                }
                else
                {
                    var newHierarchies = new List<CreateHierarchyResult>();
                    foreach (var hierarchy in body.Hierarchies)
                    {
                        var newHierarchy = CreateHierarchy(hierarchy);
                        newHierarchies.Add(newHierarchy);
                    }

                    if (newHierarchies.Any())
                    {
                        string jsonHierarchy = JsonConvert.SerializeObject(newHierarchies, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                        existNonTeachingLoad.Parameter = jsonHierarchy;
                    }
                }                
            }
            #endregion

            existNonTeachingLoad.IdAcademicYear = body.IdAcadyear;
            existNonTeachingLoad.IdTeacherPosition = body.IdPosition;
            existNonTeachingLoad.Category = body.Category;
            existNonTeachingLoad.Load = body.Load;

            _dbContext.Entity<MsNonTeachingLoad>().Update(existNonTeachingLoad);

            // update loads on transaction non teaching load
            var trNonTeachingLoads = await _dbContext.Entity<TrNonTeachingLoad>()
                .Where(x => x.IdMsNonTeachingLoad == existNonTeachingLoad.Id)
                .ToListAsync(CancellationToken);

            trNonTeachingLoads.ForEach(tr => tr.Load = body.Load);
            _dbContext.Entity<TrNonTeachingLoad>().UpdateRange(trNonTeachingLoads);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private CreateHierarchyResult CreateHierarchy(string hierarchy)
        {
            return new CreateHierarchyResult
            {
                Url = $"/lov/{hierarchy.ToLower()}",
                PlaceHolder = $"Search {hierarchy}",
                Class = hierarchy.ToLower(),
                Label = hierarchy,
            };
        }
    }
}
