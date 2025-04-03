using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class DetailEvidencesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DetailEvidencesHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailEvidencesRequest>();

            var dataEvidences = await _dbContext.Entity<TrEvidences>()
                                .Include(x => x.TrEvidencesAttachments)
                                .Include(x => x.TrEvidenceLearnings).ThenInclude(x => x.LearningOutcome)
                                .Where(x => x.Id == param.IdEvidences)
                                .FirstOrDefaultAsync(CancellationToken);

            if(dataEvidences == null)
                throw new BadRequestException($"{param.IdEvidences} not found");

            var result = new DetailEvidencesResult
            {
                Id = dataEvidences.Id,
                IdExperience = dataEvidences.IdExperience,
                DateIn = dataEvidences.DateIn,
                EvidencesType = dataEvidences.EvidencesType,
                EvidencesText = dataEvidences.EvidencesValue,
                EvidencesLink = dataEvidences.Url,
                Attachments = dataEvidences.TrEvidencesAttachments != null ? dataEvidences.TrEvidencesAttachments.Select(y => new DetailListEvidencesAttachment
                {
                    Url = y.Url,
                    Filename = y.File,
                    Filetype = y.FileType,
                    Filesize = y.Size
                }).ToList() : null,
                LearningOutcomes = dataEvidences.TrEvidenceLearnings != null ? dataEvidences.TrEvidenceLearnings.Select(y => new CodeWithIdVm
                {
                    Id = y.IdLearningOutcome,
                    Code = y.IdLearningOutcome,
                    Description = y.LearningOutcome.LearningOutcomeName
                }).ToList() : null
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
