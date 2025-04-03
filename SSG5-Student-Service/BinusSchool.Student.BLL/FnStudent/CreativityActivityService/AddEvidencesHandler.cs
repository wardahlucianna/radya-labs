using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using System.Net;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{

    public class AddEvidencesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public AddEvidencesHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddEvidencesRequest, AddEvidencesValidator>();

            var isExperience = await _dbContext.Entity<TrExperience>()
                       .Where(x => x.Id == body.IdExperience)
                       .FirstOrDefaultAsync(CancellationToken);

            if (isExperience == null)
               throw new BadRequestException($"{body.IdExperience} not found");

            var newIdEvidences = Guid.NewGuid().ToString();

            var newEvidences = new TrEvidences
            {
               Id = newIdEvidences,
               IdExperience = body.IdExperience,
               EvidencesType = body.EvidencesType,
               EvidencesValue = body.EvidencesText,
               Url = body.EvidencesLink
            };

            _dbContext.Entity<TrEvidences>().Add(newEvidences);

            var ListEvidencesAttachment = new List<TrEvidencesAttachment>();

            if(body.EvidencesType == EvidencesType.Image || body.EvidencesType == EvidencesType.File)
            {
                if (body.Attachments != null || body.Attachments.Count > 0)
                {
                    foreach (var attach in body.Attachments)
                    {
                        if(!string.IsNullOrWhiteSpace(attach.Url))
                        {
                                var newEvidencesAttachment = new TrEvidencesAttachment
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdEvidences = newIdEvidences,
                                    Url = attach.Url,
                                    File = attach.Filename,
                                    Size = attach.Filesize,
                                    FileType = attach.Filetype
                                };
                                ListEvidencesAttachment.Add(newEvidencesAttachment);
                        }
                    }
                    _dbContext.Entity<TrEvidencesAttachment>().AddRange(ListEvidencesAttachment);
                }
            }
            
            var ListEvidenceLearning = new List<TrEvidenceLearning>();

            if (body.IdLearningOutcomes.Count > 0)
            {
               foreach (var lo in body.IdLearningOutcomes)
               {
                   var newIdEvidenceLearning = Guid.NewGuid().ToString();
                   var newEvidenceLearning = new TrEvidenceLearning
                   {
                       Id = newIdEvidenceLearning,
                       IdEvidences = newIdEvidences,
                       IdLearningOutcome = lo
                   };
                   ListEvidenceLearning.Add(newEvidenceLearning);

               }
               _dbContext.Entity<TrEvidenceLearning>().AddRange(ListEvidenceLearning);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
