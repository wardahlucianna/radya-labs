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

    public class DeleteExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DeleteExperienceHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteExperienceRequest,DeleteExperienceValidator>();

            var dataExperience = await _dbContext.Entity<TrExperience>()
                              .Where(e => e.Id == body.IdExperience)
                              .FirstOrDefaultAsync(CancellationToken);

            if(dataExperience == null)
                throw new BadRequestException($"{body.IdExperience} not found");

            dataExperience.IsActive = false;

            _dbContext.Entity<TrExperience>().Update(dataExperience);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
