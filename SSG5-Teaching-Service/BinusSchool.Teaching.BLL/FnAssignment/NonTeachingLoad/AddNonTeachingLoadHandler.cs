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
    public class AddNonTeachingLoadHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IStringLocalizer _localizer;

        public AddNonTeachingLoadHandler(ITeachingDbContext dbContext, IStringLocalizer localizer)
        {
            _dbContext = dbContext;
            _localizer = localizer;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddNonTeachLoadRequest, AddNonTeachLoadValidator>();

            var position = await _dbContext.Entity<MsTeacherPosition>().FindAsync(new[] { body.IdPosition }, CancellationToken);
            if (position is null)
                throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["TeacherPosition"], "Id", body.IdPosition));

            var sameNonTeachingLoad = await _dbContext.Entity<MsNonTeachingLoad>()
                .Where(x => x.IdAcademicYear == body.IdAcadyear && x.IdTeacherPosition == body.IdPosition && x.Category == body.Category)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (sameNonTeachingLoad != null)
                throw new BadRequestException(string.Format(_localizer["ExAlreadyUseInsert"], position.Description));

            var nonTeachLoad = new MsNonTeachingLoad
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcadyear,
                IdTeacherPosition = body.IdPosition,
                Category = body.Category,
                Load = body.Load
            };

            if (body.Category == Common.Model.Enums.AcademicType.Academic)
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
                    nonTeachLoad.Parameter = jsonHierarchy;
                }
            }

            _dbContext.Entity<MsNonTeachingLoad>().Add(nonTeachLoad);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        public CreateHierarchyResult CreateHierarchy(string hierarchy)
        {
            string url = hierarchy;
            if (hierarchy.ToLower() == "streaming")
            {
                url = "gradepathwaydetail";
            }
            else if (hierarchy.ToLower() == "classroom")
            {
                url = "ClassroomByGradePathway";
            }
            else
            {
                url = hierarchy.ToLower();
            }

            return new CreateHierarchyResult
            {
                Url = $"/lov/{url}",
                PlaceHolder = $"Search {hierarchy}",
                Class = hierarchy.ToLower(),
                Label = hierarchy,
            };
        }
    }
}
