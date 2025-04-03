using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BinusSchool.School.FnSchool.ProjectInformation.Helper;

namespace BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow
{
    public class DeleteSPCHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public DeleteSPCHandler(ISchoolDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var param = await Request.ValidateBody<DeleteSPCRequest, DeleteSPCValidator>();

                var deletedData = await _dbContext.Entity<MsSchoolProjectCoordinator>()
                    .Where(x => x.Id == param.IdSchoolProjectCoordinator)
                    .FirstOrDefaultAsync(CancellationToken);

                if(deletedData == null)
                {
                    throw new Exception("No Data Or Data has been deleted");
                }

                deletedData.IsActive = false;

                _dbContext.Entity<MsSchoolProjectCoordinator>().Update(deletedData);

                var deleteFile = new SPCSaveImageHelper(_configuration);
                await deleteFile.RemoveFileIfExists(deletedData.IdBinusian, deletedData.PhotoLink);

                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Request.CreateApiResult2();
            
        }
    }
}
