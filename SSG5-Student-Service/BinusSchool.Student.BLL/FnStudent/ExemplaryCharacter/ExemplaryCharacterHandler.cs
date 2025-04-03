using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class ExemplaryCharacterHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;
        private IDbContextTransaction _transaction;

        public ExemplaryCharacterHandler(IStudentDbContext dbContext, IMachineDateTime dateTime, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _configuration = configuration;
        }

        private static readonly string[] _imageExtensions = {
            ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", "HEIC" //etc
        };

        private static readonly string[] _videoExtensions = {
            ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", ".AVI", ".MP4", ".DIVX", ".WMV", "HEVC", "MOV" //etc
        };

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var UserLogin = AuthInfo.UserId;
            //var UserLogin = "TEACHER1SP";

            var getExemplaryCharacter = await _dbContext.Entity<TrExemplary>()
                .Include(x => x.ExemplaryLikes)
                .Include(x => x.ExemplaryStudents)
                    .ThenInclude(x => x.Student)
                .Include(x => x.ExemplaryStudents)
                    .ThenInclude(x => x.Homeroom)
                    .ThenInclude(x => x.Grade)
                .Include(x => x.ExemplaryStudents)
                    .ThenInclude(x => x.Homeroom)
                    .ThenInclude(x => x.MsGradePathwayClassroom)
                    .ThenInclude(x => x.Classroom)
                .Include(x => x.ExemplaryAttachments)
                .Include(x => x.TrExemplaryValues)
                    .ThenInclude(x => x.LtExemplaryValue)
                .Include(x => x.LtExemplaryCategory)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(CancellationToken);

            if (getExemplaryCharacter is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExemplaryCharacter"], "Id", id));

            string SASToken = GetSasUri(5);

            var ReturnResult = new GetDetailExemplaryCharacterResult
            {
                IdExemplary = getExemplaryCharacter.Id,
                DatePosted = getExemplaryCharacter.PostedDate,
                ExemplaryDate = getExemplaryCharacter.ExemplaryDate,
                Category = new ItemValueVm
                { 
                    Id = getExemplaryCharacter.IdExemplaryCategory,
                    Description = getExemplaryCharacter.LtExemplaryCategory.LongDesc
                },
                Description = getExemplaryCharacter.Description,
                CountLikes = getExemplaryCharacter.ExemplaryLikes == null ? 0 : getExemplaryCharacter.ExemplaryLikes.Count(),
                IsYouLiked = getExemplaryCharacter.ExemplaryLikes == null ? false : getExemplaryCharacter.ExemplaryLikes.Where(b => b.UserIn == UserLogin).Count() > 0,
                Updatedby = (getExemplaryCharacter.UserUp != null ? (_dbContext.Entity<MsUser>().Where(a => a.Id == getExemplaryCharacter.UserUp).FirstOrDefault()?.DisplayName ?? getExemplaryCharacter.UserUp) : null),
                UpdatedDateView = getExemplaryCharacter.DateUp != null ? ((DateTime)getExemplaryCharacter.DateUp).ToString("dd MMM yyyy HH:mm") : null,
                Postedby = (getExemplaryCharacter.UserIn != null ? (_dbContext.Entity<MsUser>().Where(a => a.Id == getExemplaryCharacter.UserIn).FirstOrDefault()?.DisplayName ?? getExemplaryCharacter.UserIn) : null),
                PostedDate = getExemplaryCharacter.PostedDate,
                PostedDateView = getExemplaryCharacter.PostedDate.ToString("dd MMM yyyy HH:mm"),
                StudentList = getExemplaryCharacter.ExemplaryStudents.Count() > 0 ?
                            getExemplaryCharacter.ExemplaryStudents.Select(y => new GetDetailExemplaryCharacterResult_Student
                            {
                                IdExemplaryStudent = y.Id,
                                Student = new NameValueVm
                                {
                                    Id = y.IdStudent,
                                    Name = NameUtil.GenerateFullName(y.Student.FirstName, y.Student.MiddleName, y.Student.LastName)
                                },
                                Homeroom = new ItemValueVm
                                {
                                    Id = y.IdHomeroom,
                                    Description = y.Homeroom.Grade.Description + " " + y.Homeroom.MsGradePathwayClassroom.Classroom.Code
                                },
                            }).ToList()
                            : null,
                ValueList = getExemplaryCharacter.TrExemplaryValues.Count() > 0 ?
                            getExemplaryCharacter.TrExemplaryValues.Select(y => new GetDetailExemplaryCharacterResult_Value
                            {
                                IdExemplaryValue = y.Id,
                                Value = new ItemValueVm
                                {
                                    Id = y.IdLtExemplaryValue,
                                    Description = y.LtExemplaryValue.ShortDesc
                                },
                            }).ToList()
                            : null,
                AttachmentList = getExemplaryCharacter.ExemplaryAttachments.Count() > 0 ?
                            getExemplaryCharacter.ExemplaryAttachments.Select(y => new GetDetailExemplaryCharacterResult_Attachment
                            {
                                IdExemplaryAttachment = y.Id,
                                Url = (y.Url + SASToken).Replace(" ", "%20"),
                                OriginalFileName = y.OriginalFileName,
                                FileName = y.FileName,
                                FileSize = y.FileSize,
                                FileType = y.FileType,
                                FileExtension = y.FileExtension,
                                UrlWithSASToken = (y.Url + SASToken).Replace(" ", "%20")
                            }).ToList()
                            : null
            };

            return Request.CreateApiResult2(ReturnResult as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var paramList = await Request.ValidateBody<List<SaveExemplaryCharacterRequest>, SaveExemplaryCharacterValidator>();
            var deleteExemplaryAttachment = new List<string>();
            var containerName = "exemplary-character";

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                foreach (var param in paramList)
                {
                    if (param.Student.Count() == 0)
                        throw new BadRequestException($"Error! Please Choose at least 1 student");

                    if (string.IsNullOrEmpty(param.IdExemplary))
                    {
                        var getAcademicYear = _dbContext.Entity<MsAcademicYear>()
                                            .Where(x => x.Id == param.IdAcademicYear)
                                            .FirstOrDefault();

                        if (getAcademicYear == null)
                            throw new BadRequestException($"Error! No Academic Year found for Academic Year with IdAcademicYear: {param.IdAcademicYear}");

                        var newTrExemplary = new TrExemplary()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            PostedDate = _dateTime.ServerTime,
                            ExemplaryDate = param.ExemplaryDate,
                            IdExemplaryCategory = param.IdExemplaryCategory,
                            Description = param.Description
                        };

                        _dbContext.Entity<TrExemplary>().Add(newTrExemplary);

                        List<TrExemplaryValue> exemplaryValues = new List<TrExemplaryValue>();
                        foreach (var paramValue in param.ValueList)
                        {
                            var newTrExemplaryValue = new TrExemplaryValue()
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdLtExemplaryValue = paramValue.IdLtExemplaryValue,
                                IdExemplary = newTrExemplary.Id
                            };
                            exemplaryValues.Add(newTrExemplaryValue);
                        }
                        _dbContext.Entity<TrExemplaryValue>().AddRange(exemplaryValues);

                        List<TrExemplaryStudent> exemplaryStudents = new List<TrExemplaryStudent>();
                        foreach (var paramStudent in param.Student)
                        {
                            var newTrExemplaryStudent = new TrExemplaryStudent()
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExemplary = newTrExemplary.Id,
                                IdStudent = paramStudent.IdStudent,
                                IdHomeroom = paramStudent.IdHomeroom
                            };
                            exemplaryStudents.Add(newTrExemplaryStudent);
                        }
                        _dbContext.Entity<TrExemplaryStudent>().AddRange(exemplaryStudents);

                        List<TrExemplaryAttachment> exemplaryAttachments = new List<TrExemplaryAttachment>();

                        foreach (var paramAttachment in param.Attachment)
                        {
                            string fileType = _videoExtensions.Contains(paramAttachment.FileExtension, StringComparer.OrdinalIgnoreCase) == true
                               ? "video"
                               : (_imageExtensions.Contains(paramAttachment.FileExtension, StringComparer.OrdinalIgnoreCase) == true ? "image" : "unknown");

                            var newTrExemplaryAttachment = new TrExemplaryAttachment()
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExemplary = newTrExemplary.Id,
                                OriginalFileName = paramAttachment.OriginalFileName,
                                FileName = paramAttachment.FileName,
                                FileSize = paramAttachment.FileSize,
                                FileExtension = paramAttachment.FileExtension,
                                FileType = fileType,
                                Url = paramAttachment.Url
                            };
                            exemplaryAttachments.Add(newTrExemplaryAttachment);
                        }
                        _dbContext.Entity<TrExemplaryAttachment>().AddRange(exemplaryAttachments);
                    }
                    else
                    {
                        var isExemplaryExist = _dbContext.Entity<TrExemplary>()
                                            .Where(x => x.Id == param.IdExemplary)
                                            .FirstOrDefault();

                        if (isExemplaryExist == null)
                            throw new BadRequestException($"Error! No getExemplary found for getExemplary with IdExemplary: {param.IdExemplary}");

                        isExemplaryExist.IdAcademicYear = param.IdAcademicYear;
                        isExemplaryExist.PostedDate = _dateTime.ServerTime;
                        isExemplaryExist.ExemplaryDate = param.ExemplaryDate;
                        isExemplaryExist.Title = param.IdExemplaryCategory;
                        isExemplaryExist.Description = param.Description;

                        _dbContext.Entity<TrExemplary>().Update(isExemplaryExist);

                        #region Update Exemplary Value
                        var exemplaryValues = await _dbContext.Entity<TrExemplaryValue>()
                            .Where(x => x.IdExemplary == param.IdExemplary)
                            .ToListAsync(CancellationToken);

                        var getExemplaryValueToDelete = exemplaryValues
                           .Where(ex => param.ValueList.All(ex2 => ex2.IdLtExemplaryValue != ex.IdLtExemplaryValue))
                           .ToList();

                        var getExemplaryValueToAdd = param.ValueList
                            .Where(ex => exemplaryValues.All(ex2 => ex2.IdLtExemplaryValue != ex.IdLtExemplaryValue))
                            .Select(x => new { IdLtExemplaryValue = x.IdLtExemplaryValue }).ToList();

                        // Delete ExemplaryCategory
                        _dbContext.Entity<TrExemplaryValue>().RemoveRange(getExemplaryValueToDelete);

                        // Add ExemplaryCategory
                        List<TrExemplaryValue> ListExemplaryValueAdd = new List<TrExemplaryValue>();
                        foreach (var exemplaryValueAdd in getExemplaryValueToAdd)
                        {
                            var paramExemplaryValueAdd = new TrExemplaryValue
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdLtExemplaryValue = exemplaryValueAdd.IdLtExemplaryValue,
                                IdExemplary = param.IdExemplary
                            };
                            ListExemplaryValueAdd.Add(paramExemplaryValueAdd);
                        }
                        _dbContext.Entity<TrExemplaryValue>().AddRange(ListExemplaryValueAdd);
                        #endregion

                        #region Update Exemplary Student
                        var exemplaryStudents = await _dbContext.Entity<TrExemplaryStudent>()
                            .Where(x => x.IdExemplary == param.IdExemplary)
                            .ToListAsync(CancellationToken);

                        var getExemplaryStudentToDelete = exemplaryStudents
                           .Where(ex => param.Student.All(ex2 => ex2.IdStudent != ex.IdStudent))
                           .ToList();

                        var getExemplaryStudentToAdd = param.Student
                            .Where(ex => exemplaryStudents.All(ex2 => ex2.IdStudent != ex.IdStudent))
                            .Select(x => new { IdStudent = x.IdStudent, IdHomeroom = x.IdHomeroom }).ToList();

                        // Delete Exemplary Student
                        _dbContext.Entity<TrExemplaryStudent>().RemoveRange(getExemplaryStudentToDelete);

                        // Add Exemplary Student
                        List<TrExemplaryStudent> ListExemplaryStudentAdd = new List<TrExemplaryStudent>();
                        foreach (var exemplaryStudentAdd in getExemplaryStudentToAdd)
                        {
                            var paramExemplaryStudentyAdd = new TrExemplaryStudent
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdHomeroom = exemplaryStudentAdd.IdHomeroom,
                                IdStudent = exemplaryStudentAdd.IdStudent,
                                IdExemplary = param.IdExemplary
                            };
                            ListExemplaryStudentAdd.Add(paramExemplaryStudentyAdd);
                        }
                        _dbContext.Entity<TrExemplaryStudent>().AddRange(ListExemplaryStudentAdd);
                        #endregion

                        #region Update Exemplary Attachment
                        var exemplaryAttachments = await _dbContext.Entity<TrExemplaryAttachment>()
                            .Where(x => x.IdExemplary == param.IdExemplary)
                            .ToListAsync(CancellationToken);

                        var getExemplaryAttachmentToDelete = exemplaryAttachments
                           .Where(ex => param.Attachment.All(ex2 => ex2.IdExemplaryAttachment != ex.Id))
                           .ToList();

                        var getExemplaryAttachmentToAdd = param.Attachment
                            .Where(ex => exemplaryAttachments.All(ex2 => ex2.Id != ex.IdExemplaryAttachment))
                            //.Select(x => new { IdStudent = x.IdStudent, IdHomeroom = x.IdHomeroom })
                            .ToList();

                        // Delete Exemplary Student
                        deleteExemplaryAttachment = getExemplaryAttachmentToDelete.Select(x => x.FileName).ToList();
                        _dbContext.Entity<TrExemplaryAttachment>().RemoveRange(getExemplaryAttachmentToDelete);

                        // Add Exemplary Student
                        List<TrExemplaryAttachment> ListExemplaryAttachmentAdd = new List<TrExemplaryAttachment>();


                        foreach (var exemplaryAttachmentAdd in getExemplaryAttachmentToAdd)
                        {
                            string fileType = _videoExtensions.Contains(exemplaryAttachmentAdd.FileExtension, StringComparer.OrdinalIgnoreCase) == true
                                ? "video"
                                : (_imageExtensions.Contains(exemplaryAttachmentAdd.FileExtension, StringComparer.OrdinalIgnoreCase) == true ? "image" : "unknown");

                            var paramExemplaryAttachmentAdd = new TrExemplaryAttachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExemplary = param.IdExemplary,
                                Url = exemplaryAttachmentAdd.Url,
                                OriginalFileName = exemplaryAttachmentAdd.OriginalFileName,
                                FileName = exemplaryAttachmentAdd.FileName,
                                FileSize = exemplaryAttachmentAdd.FileSize,
                                FileType = fileType,
                                FileExtension = exemplaryAttachmentAdd.FileExtension
                            };
                            ListExemplaryAttachmentAdd.Add(paramExemplaryAttachmentAdd);
                        }
                        _dbContext.Entity<TrExemplaryAttachment>().AddRange(ListExemplaryAttachmentAdd);
                        #endregion
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                foreach (var fileName in deleteExemplaryAttachment)
                {
                    await RemoveFileIfExists(fileName, containerName);
                }

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Student:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Student:AccountStorage"]);
                return storageAccount;
            }

        }

        public string GetSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("exemplary-character");

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad hoc SAS and others on the stored access policy.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            // Return the URI string for the container, including the SAS token.
            return sasContainerToken;

            //Return blob SAS Token
            //return sasContainerToken;
        }

        public Task RemoveFileIfExists(string fileName, string containerName)
        {
            if (string.IsNullOrEmpty(fileName))
                return Task.CompletedTask;

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return blockBlob.DeleteIfExistsAsync();
        }

    }
}
