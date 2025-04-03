using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Storage;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class UnivInformationManagementPortalProcessApprovalHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;

        public UnivInformationManagementPortalProcessApprovalHandler(IStudentDbContext studentDbContext, IMachineDateTime dateTime,IServiceProvider provider,
            IConfiguration configuration)

        {
            _dbContext = studentDbContext;
            _dateTime = dateTime;
            _provider = provider;
            _configuration = configuration;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<AddUnivInformationManagementPortalApprovalRequest, AddUnivInformationManagementPortalApprovalValidator>();

            if (body.IsApproval)
            {
                var GetUnivInformationManagementPortal = await _dbContext.Entity<MsUniversityPortalApproval>()
                                                       .Include(e => e.UniversityPortal)
                                                       .Where(e => e.IdUniversityPortal == body.IdUnivInformationManagementPortal && e.IdSchool == body.IdSchool).SingleOrDefaultAsync(CancellationToken);

                if (GetUnivInformationManagementPortal == null)
                {
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["University Information Management Portal"], "Id", body.IdUnivInformationManagementPortal));
                }

                var existsData = _dbContext.Entity<MsUniversityPortal>()
                .Any(x => x.Name == GetUnivInformationManagementPortal.UniversityPortal.Name && x.IdSchool == body.IdSchool);

                if (existsData)
                {
                    throw new BadRequestException($"University name { GetUnivInformationManagementPortal.UniversityPortal.Name} already exists.");
                }

                var existsData1 = _dbContext.Entity<MsUniversityPortal>()
                .Any(x => x.Website == GetUnivInformationManagementPortal.UniversityPortal.Website && x.IdSchool == body.IdSchool);

                if (existsData1)
                {
                    throw new BadRequestException($"Website { GetUnivInformationManagementPortal.UniversityPortal.Website} already exists.");
                }

                // set approval
                GetUnivInformationManagementPortal.ApprovalIdUser = body.IdUserApproval;
                GetUnivInformationManagementPortal.StatusApproval = "Approved";
                GetUnivInformationManagementPortal.ApprovalDate = _dateTime.ServerTime;
                _dbContext.Entity<MsUniversityPortalApproval>().Update(GetUnivInformationManagementPortal);


                // cloning data 

                //load data source
                var GetUnivInformationManagementPortalSource = await _dbContext.Entity<MsUniversityPortal>().Where(e => e.Id == body.IdUnivInformationManagementPortal).SingleOrDefaultAsync(CancellationToken);
                var GetUnivInformationManagementPortalSourceSheet = await _dbContext.Entity<MsUniversityPortalFactSheet>().Where(e => e.IdUniversityPortal == body.IdUnivInformationManagementPortal).ToListAsync(CancellationToken);
                var GetUnivInformationManagementPortalSourceLogo = await _dbContext.Entity<MsUniversityPortalLogo>().Where(e => e.IdUniversityPortal == body.IdUnivInformationManagementPortal).ToListAsync(CancellationToken);

                if (GetUnivInformationManagementPortalSource is null)
                {
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["University Information Management Portal"], "Id", body.IdUnivInformationManagementPortal));
                }


                var idUnivInformationManagementPortal = Guid.NewGuid().ToString();

                var newUnivInformationManagementPortal = new MsUniversityPortal
                {
                    Id = idUnivInformationManagementPortal,
                    IdSchool = body.IdSchool,
                    IdSchoolFrom = GetUnivInformationManagementPortal.UniversityPortal.IdSchoolFrom,
                    Name = GetUnivInformationManagementPortalSource.Name,
                    Description = GetUnivInformationManagementPortalSource.Description,
                    Website = GetUnivInformationManagementPortalSource.Website,
                    Email = GetUnivInformationManagementPortalSource.Email,
                    ContactPerson = GetUnivInformationManagementPortalSource.ContactPerson,
                    IsLogoAsSquareImage = GetUnivInformationManagementPortalSource.IsLogoAsSquareImage,
                    IsShareOtherSchool = false
                };

                var containerBlobName = "university-information";
                var connectionString = _configuration.GetConnectionString("Student:AccountStorage");
                var storageManager = new StorageManager(connectionString, _provider.GetService<ILogger<StorageManager>>());

                if (GetUnivInformationManagementPortalSourceSheet != null)
                {

                    var blobContainer = await storageManager.GetOrCreateBlobContainer(containerBlobName, PublicAccessType.None, CancellationToken);


                    foreach (var ItemFactSheet in GetUnivInformationManagementPortalSourceSheet)
                    {
                        var filename = Path.GetFileNameWithoutExtension(ItemFactSheet.OriginalName);
                        var fileDestination = filename + "_" + DateTime.Now.ToString("yyyyHHmmss") + "." + ItemFactSheet.FileType;
                        var _newBlob = await storageManager.CopyBlob(blobContainer, ItemFactSheet.FileName, fileDestination);

                        var newUnivInformationManagementPortalFactSheet = new MsUniversityPortalFactSheet
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUniversityPortal = idUnivInformationManagementPortal,
                            OriginalName = ItemFactSheet.OriginalName,
                            Url = _newBlob.destination.Uri.OriginalString,
                            FileName = fileDestination,
                            FileType = ItemFactSheet.FileType,
                            FileSize = ItemFactSheet.FileSize,
                        };
                        _dbContext.Entity<MsUniversityPortalFactSheet>().Add(newUnivInformationManagementPortalFactSheet);

                    }
                }

                if (GetUnivInformationManagementPortalSourceLogo != null)
                {
                    var blobContainer = await storageManager.GetOrCreateBlobContainer(containerBlobName, PublicAccessType.None, CancellationToken);

                    foreach (var ItemLogo in GetUnivInformationManagementPortalSourceLogo)
                    {
                        var filename = Path.GetFileNameWithoutExtension(ItemLogo.OriginalName);
                        var fileDestination = filename + "_" + DateTime.Now.ToString("yyyyHHmmss") + "." + ItemLogo.FileType;
                        var _newBlob = await storageManager.CopyBlob(blobContainer, ItemLogo.FileName, fileDestination);

                        var newUnivInformationManagementPortalLogo = new MsUniversityPortalLogo
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUniversityPortal = idUnivInformationManagementPortal,
                            OriginalName = ItemLogo.OriginalName,
                            Url = _newBlob.destination.Uri.OriginalString,
                            FileName = fileDestination,
                            FileType = ItemLogo.FileType,
                            FileSize = ItemLogo.FileSize,
                        };
                        _dbContext.Entity<MsUniversityPortalLogo>().Add(newUnivInformationManagementPortalLogo);

                    }
                }
                _dbContext.Entity<MsUniversityPortal>().Add(newUnivInformationManagementPortal);

                await _dbContext.SaveChangesAsync(CancellationToken);

            }
            else
            {

                var GetUnivInformationManagementPortalApproval = await _dbContext.Entity<MsUniversityPortalApproval>().Where(e => e.IdUniversityPortal == body.IdUnivInformationManagementPortal && e.IdSchool == body.IdSchool).SingleOrDefaultAsync(CancellationToken);

                // set approval

                GetUnivInformationManagementPortalApproval.ApprovalIdUser = body.IdUserApproval;
                GetUnivInformationManagementPortalApproval.StatusApproval = "Decline";
                GetUnivInformationManagementPortalApproval.ApprovalDate = _dateTime.ServerTime;

                _dbContext.Entity<MsUniversityPortalApproval>().Update(GetUnivInformationManagementPortalApproval);


                await _dbContext.SaveChangesAsync(CancellationToken);

            }

            return Request.CreateApiResult2();

        }

    }
}
