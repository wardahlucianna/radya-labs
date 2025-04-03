using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.ProjectInformation.Helper;
using BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow.Validator;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow
{
    public class SaveSPCHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public SaveSPCHandler(ISchoolDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveSPCRequest, SaveSPCValidator>();

            try
            {
                if (string.IsNullOrEmpty(param.IdSchoolProjectCoordinator))
                {
                    var existedData = await _dbContext.Entity<MsSchoolProjectCoordinator>()
                        .Where(x => x.IdBinusian == param.IdBinusian)
                        .FirstOrDefaultAsync(CancellationToken);

                    if(existedData != null || param.PhotoUrl == null)
                    {
                        throw new Exception("User Existed");
                    }

                    var newPhoto = new SPCSaveImageHelper(_configuration);
                    var newUrl = await newPhoto.MoveFilesAndDeleteFromSourceContainerAsync(param.PhotoUrl + "." + param.FileType, "school-project-coordinator-temp", param.IdBinusian, param.FileType);

                    var newData = new MsSchoolProjectCoordinator
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBinusian = param.IdBinusian,
                        IdSchool = param.IdSchool,
                        Remarks = param.Remarks,
                        PhotoLink = string.Join(",", newUrl)
                    };

                    _dbContext.Entity<MsSchoolProjectCoordinator>().Add(newData);

                }
                else
                {
                    var editedData = await _dbContext.Entity<MsSchoolProjectCoordinator>()
                            .Where(x => x.Id == param.IdSchoolProjectCoordinator)
                            .FirstOrDefaultAsync(CancellationToken);

                    var existedData = await _dbContext.Entity<MsSchoolProjectCoordinator>()
                            .Where(x => x.IdBinusian == param.IdBinusian && x.Id != editedData.Id)
                            .FirstOrDefaultAsync(CancellationToken);

                    if(existedData != null)
                    {
                        throw new Exception("User Existed");
                    }

                    if (param.FileType != null)
                    {
                        var newPhoto = new SPCSaveImageHelper(_configuration);
                        await newPhoto.RemoveFileIfExists(editedData.IdBinusian, editedData.PhotoLink);
                        var newUrl = await newPhoto.MoveFilesAndDeleteFromSourceContainerAsync(param.PhotoUrl + "." + param.FileType, "school-project-coordinator-temp", param.IdBinusian, param.FileType);
                        editedData.PhotoLink = string.Join(",", newUrl);
                    }
                    else if (!editedData.PhotoLink.Contains(param.IdBinusian) && param.FileType == null)
                    {
                        string[] paths = editedData.PhotoLink.Split('.');
                        string fileType = paths[paths.Length - 1];

                        var newPhoto = new SPCSaveImageHelper(_configuration);
                        var newUrl = await newPhoto.MoveFilesAndDeleteFromSourceContainerAsync(editedData.IdBinusian + "." + fileType, "school-project-coordinator", param.IdBinusian, fileType);
                        editedData.PhotoLink = string.Join(",", newUrl);
                    }
                    editedData.IdSchool = param.IdSchool;
                    editedData.IdBinusian = param.IdBinusian;
                    editedData.Remarks = param.Remarks;

                    _dbContext.Entity<MsSchoolProjectCoordinator>().Update(editedData);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            catch (Exception ex)
            {
                var SPCSaveImageHelper = new SPCSaveImageHelper(_configuration);
                await SPCSaveImageHelper.RemoveFileWithFileName(string.Format("{0}{1}", param.IdBinusian, "." + param.FileType));

                throw new Exception(ex.Message);
            }

            return Request.CreateApiResult2();
        }
    }
}
