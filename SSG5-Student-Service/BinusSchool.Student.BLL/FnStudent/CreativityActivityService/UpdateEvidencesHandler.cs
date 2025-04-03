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

    public class UpdateEvidencesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public UpdateEvidencesHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateEvidencesRequest, UpdateEvidencesValidator>();

            var dataEvidences = await _dbContext.Entity<TrEvidences>()
                       .Where(x => x.Id == body.Id)
                       .FirstOrDefaultAsync(CancellationToken);

            if (dataEvidences == null)
               throw new BadRequestException($"{body.Id} not found");

            var isExperience = await _dbContext.Entity<TrExperience>()
                       .Where(x => x.Id == body.IdExperience)
                       .FirstOrDefaultAsync(CancellationToken);

            if (isExperience == null)
               throw new BadRequestException($"{body.IdExperience} not found");

               dataEvidences.EvidencesType = body.EvidencesType;
               dataEvidences.EvidencesValue = body.EvidencesText;
               dataEvidences.Url = body.EvidencesLink;

            _dbContext.Entity<TrEvidences>().Update(dataEvidences);

            var dataEvidencesAttachment = await _dbContext.Entity<TrEvidencesAttachment>()
                                        .Where(x => x.IdEvidences == dataEvidences.Id)
                                        .ToListAsync(CancellationToken);
            foreach(var dataOldAttachment in dataEvidencesAttachment)
            {
                dataOldAttachment.IsActive = false;
                _dbContext.Entity<TrEvidencesAttachment>().UpdateRange(dataOldAttachment);
            }

            var dataEvidencesLearning = await _dbContext.Entity<TrEvidenceLearning>()
                                        .Where(x => x.IdEvidences == dataEvidences.Id)
                                        .ToListAsync(CancellationToken);
            foreach(var dataOldLearning in dataEvidencesLearning)
            {
                dataOldLearning.IsActive = false;
                _dbContext.Entity<TrEvidenceLearning>().UpdateRange(dataOldLearning);
            }

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
                                    IdEvidences = dataEvidences.Id,
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
                       IdEvidences = dataEvidences.Id,
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
