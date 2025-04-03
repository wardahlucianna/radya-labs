using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Extensions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.School.FnSchool.PublishSurvey.Validator;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;

namespace BinusSchool.School.FnSchool.PublishSurvey
{
    public class AddMappingStudentLearningSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IPublishSurvey _servicePublishService;

        public AddMappingStudentLearningSurveyHandler(ISchoolDbContext dbContext, IPublishSurvey servicePublishService)
        {
            _dbContext = dbContext;
            _servicePublishService = servicePublishService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMappingStudentLearningSurveyRequest, AddMappingStudentLearningSurveyValidator>();

            var listPublishSurveyMapping = await _dbContext.Entity<TrPublishSurveyMapping>()
                                        .Where(e => e.IdPublishSurvey == body.IdPublishSurvey && e.HomeroomStudent.IdHomeroom==body.IdHomeroom)
                                        .ToListAsync(CancellationToken);

            foreach(var item in listPublishSurveyMapping) 
            {
                var getMappingStudentLearningSurveysBody = body.MappingStudentLearningSurveys
                           .Where(e => e.IdLesson == item.IdLesson && e.IdHomeroomStudent == item.IdHomeroomStudent && e.BinusianId == item.IdBinusian)
                           .ToList();

                var exsisPublishMapping = getMappingStudentLearningSurveysBody.Any();

                if (exsisPublishMapping)
                {
                    if(!item.IsMapping)
                        item.IsMapping = true;
                }
                else
                {
                    if (item.IsMapping)
                        item.IsMapping = false;
                }

                _dbContext.Entity<TrPublishSurveyMapping>().Update(item);
            }

            foreach (var item in body.MappingStudentLearningSurveys)
            {
                var getMappingStudentLearningSurveys = listPublishSurveyMapping
                           .Where(e => e.IdLesson == item.IdLesson && e.IdHomeroomStudent == item.IdHomeroomStudent && e.IdBinusian == item.BinusianId)
                           .ToList();

                var exsisPublishMappingBody = getMappingStudentLearningSurveys.Any();

                if (!exsisPublishMappingBody)
                {
                    TrPublishSurveyMapping newPublishSurveyMapping = new TrPublishSurveyMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBinusian = item.BinusianId,
                        IdHomeroomStudent = item.IdHomeroomStudent,
                        IdLesson = item.IdLesson,
                        IdPublishSurvey = body.IdPublishSurvey,
                        IsMapping = true,
                    };

                    _dbContext.Entity<TrPublishSurveyMapping>().Add(newPublishSurveyMapping);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
