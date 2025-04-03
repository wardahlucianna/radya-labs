using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetEntryMeritDemeritDisiplineHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetEntryMeritDemeritDisiplineHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEntryMeritDemeritDisiplineRequest>();
            var predicate = PredicateBuilder.Create<MsMeritDemeritMapping>
            (
                x => x.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear &&
                x.Grade.IdLevel == param.Idlevel &&
                x.IdGrade == param.IdGrade &&
                x.Category == (MeritDemeritCategory)param.Category
            );

            if (!string.IsNullOrEmpty(param.IdLevelInfraction))
                predicate = predicate.And(x => x.IdLevelOfInteraction == param.IdLevelInfraction);

            var item =  await _dbContext.Entity<MsMeritDemeritMapping>()
                .Where(predicate)
               .Select(x => new GetEntryMeritDemeritDisiplineResult
               {
                   Id = x.Id,
                   Description = x.DisciplineName,
                   Point = x.Point.ToString(),
                   IdLevelOfInteraction = x.IdLevelOfInteraction,
               }).ToListAsync(CancellationToken);


            return Request.CreateApiResult2(item as object);
        }


    }
}
