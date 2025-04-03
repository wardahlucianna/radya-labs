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

    public class DeleteEvidencesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DeleteEvidencesHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteEvidencesRequest,EvidencesDeleteValidator>();

            var dataEvidences = await _dbContext.Entity<TrEvidences>()
                              .Where(e => e.Id == body.IdEvidences)
                              .FirstOrDefaultAsync(CancellationToken);

            if(dataEvidences == null)
                throw new BadRequestException($"{body.IdEvidences} not found");

            dataEvidences.IsActive = false;

            _dbContext.Entity<TrEvidences>().Update(dataEvidences);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
