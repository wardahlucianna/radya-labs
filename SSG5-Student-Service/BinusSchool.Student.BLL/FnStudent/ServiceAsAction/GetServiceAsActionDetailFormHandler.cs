using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NPOI.OpenXmlFormats;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class GetServiceAsActionDetailFormHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public GetServiceAsActionDetailFormHandler
        (
            IStudentDbContext studentDbContext,
            IConfiguration configuration
        )
        {
            _dbContext = studentDbContext;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetServiceAsActionDetailFormRequest>(
                    nameof(GetServiceAsActionDetailFormRequest.IdUser),
                    nameof(GetServiceAsActionDetailFormRequest.IdServiceAsActionForm),
                    nameof(GetServiceAsActionDetailFormRequest.IsIncludeComment)
                );

            var result = new GetServiceAsActionDetailFormResult();

            var getData = await _dbContext.Entity<TrServiceAsActionForm>()
                    .Include(x => x.UserApprove)
                    .Include(x => x.ServiceAsActionHeader)
                        .ThenInclude(y => y.Student)
                    .Include(x => x.ServiceAsActionHeader)
                        .ThenInclude(x => x.AcademicYear)
                    .Include(x => x.ServiceAsActionMappingSdgs)
                        .ThenInclude(x => x.LTServiceAsActionSdgs)
                    .Include(x => x.ServiceAsActionStatus)
                    .Include(x => x.Supervisor)
                    .Include(x => x.ServiceAsActionLocation)
                    .Include(x => x.ServiceAsActionMappingTypes)
                        .ThenInclude(y => y.ServiceAsActionType)
                    .Include(x => x.ServiceAsActionEvidences)
                    .Include(x => x.LOMappings)
                        .ThenInclude(y => y.MappingLearningOutcome)
                            .ThenInclude(y => y.LearningOutcome)
                    .Include(x => x.ServiceAsActionEvidences)
                        .ThenInclude(y => y.LOMappings)
                            .ThenInclude(y => y.MappingLearningOutcome)
                            .ThenInclude(y => y.LearningOutcome)
                .Where(x => x.Id == param.IdServiceAsActionForm)
                .FirstOrDefaultAsync(CancellationToken);

            var getUserRole = await _dbContext.Entity<MsUserRole>()
                .Include(x => x.Role)
                .Where(x => x.IdUser == param.IdUser)
                .ToListAsync(CancellationToken);

            var studentHeader = getData.ServiceAsActionHeader;

            var getGrade = await _dbContext.Entity<MsHomeroomStudent>()
                .Where(x => studentHeader.IdStudent == x.IdStudent && x.Homeroom.Grade.MsLevel.IdAcademicYear == studentHeader.IdAcademicYear)
                .Select(x => new
                {
                    Grade = new CodeWithIdVm
                    {
                        Id = x.Homeroom.Grade.Id,
                        Code = x.Homeroom.Grade.Code,
                        Description = x.Homeroom.Grade.Description
                    },
                    Classroom = new ItemValueVm
                    {
                        Id = x.Homeroom.MsGradePathwayClassroom.Classroom.Id,
                        Description = x.Homeroom.MsGradePathwayClassroom.Classroom.Description
                    }
                })
                .FirstOrDefaultAsync(CancellationToken);

            var getPeriod = await _dbContext.Entity<MsPeriod>()
                .Where(x => x.IdGrade == getGrade.Grade.Id)
                .GroupBy(x => x.Semester)
                .Select(x => new
                {
                    Semester = x.Key,
                    StartDate = x.Min(y => y.StartDate),
                    EndDate = x.Max(y => y.EndDate)
                })
                .ToListAsync();

            var getSemester2Period = getPeriod.Where(x => x.Semester == 2).FirstOrDefault();

            var getDateNow = DateTime.Now;

            bool isOnSemester2 = getSemester2Period.StartDate <= getDateNow;

            bool IsExperienceApproved = getData.ServiceAsActionStatus.StatusDesc == "Approved" ? true : false;  
            bool IsNotComplete = getData.ServiceAsActionStatus.StatusDesc == "Complete" ? false : true;
            bool isStudent = getUserRole.Select(y => y.Role.Code).ToList().Contains("Student") ? true : false;
            bool isAdvisor = param.IsAdvisor != null ? (bool) param.IsAdvisor : false;


            result.StatusDetail = new GetServiceAsActionDetailFormResult_StatusDetail
            {
                Status = new ItemValueVm
                {
                    Id = getData.ServiceAsActionStatus.Id,
                    Description = getData.ServiceAsActionStatus.StatusDesc
                },
                Revision = getData.RevisionNote == null ? null : new GetServiceAsActionDetailFormResult_StatusDetail_Revision
                {
                    Approver = new GetServiceAsActionDetailFormResult_StatusDetail_Revision_Approver
                    {
                        Id = getData.UserApprove == null ? null : getData.UserApprove.Id,
                        Description = getData.UserApprove == null ? null : getData.UserApprove.DisplayName
                    },
                    Date = getData.ApprovedDate,
                    Note = getData.RevisionNote
                },
                CanChangeStatus = isAdvisor 
                                    && (isOnSemester2 ? getData.DateIn < getSemester2Period.StartDate && !IsNotComplete ? false : true : true) // Check if the form is submitted on semester 1 and Accessed on Semester 2 and it's completed 
            };

            if (getData == null) return Request.CreateApiResult2(null as object);

            var getMappingTypes = getData.ServiceAsActionMappingTypes.ToList();
            var getMappedSdgs = getData.ServiceAsActionMappingSdgs.ToList();    
            var getMappedLO = getData.LOMappings.ToList();
            var evidences = getData.ServiceAsActionEvidences.ToList();

            var studentData = new ItemValueVm
            {
                Id = getData.ServiceAsActionHeader.Student.Id,
                Description = NameUtil.GenerateFullName(getData.ServiceAsActionHeader.Student.FirstName, getData.ServiceAsActionHeader.Student.MiddleName, getData.ServiceAsActionHeader.Student.LastName)
            };

            var gradeData = getGrade.Grade;
            var getClassroom = getGrade.Classroom;

            var experienceDetail = new GetServiceAsActionDetailFormResult_ExperienceDetail
            {
                AcademicYear = new ItemValueVm
                {
                    Id = getData.ServiceAsActionHeader.IdAcademicYear,
                    Description = getData.ServiceAsActionHeader.AcademicYear.Description
                },
                ExperienceName = getData.ExpName,
                StartDate = getData.StartDate,
                EndDate = getData.EndDate,
                ExperienceLocation = new ItemValueVm
                {
                    Id = getData.ServiceAsActionLocation.Id,
                    Description = getData.ServiceAsActionLocation.SALocDes
                },
                ExperienceType = getMappingTypes.Count() == 0 ? null : getMappingTypes.Select(x => new ItemValueVm
                {
                    Id = x.IdServiceAsActionType,
                    Description = x.ServiceAsActionType.TypeDesc
                }).ToList(),
                ExperienceSdgs = getMappedSdgs.Count() == 0 ? null : getMappedSdgs.Select(x => new ItemValueVm
                {
                    Id = x.IdServiceAsActionSdgs,
                    Description = x.LTServiceAsActionSdgs.SdgsDesc
                }).ToList(),
                CanAddEvidence = IsExperienceApproved && IsNotComplete && isStudent,
                
            };

            var supervisorData = new GetServiceAsActionDetailFormResult_SupervisorData
            {
                Supervisor = getData.Supervisor != null ? new ItemValueVm
                {
                    Id = getData.Supervisor.Id,
                    Description = getData.Supervisor.DisplayName
                } : new ItemValueVm
                {
                    Id = null,
                    Description = getData.SupervisorName ?? null
                }
                ,
                SupervisorContact = getData.SupervisorContact ?? null,
                SupervisorEmail = getData.SupervisorEmail ?? null,
                SupervisorTitle = getData.SupervisorTitle ?? null
            };

            var organizationDetail = new GetServiceAsActionDetailFormResult_OrganizationDetail
            {
                ActivityDescription = getData.ActivityDesc,
                ContributionTMC = getData.ContributionTMC,
                Organization = getData.OrganizationName
            };

            var loMapping = getMappedLO.Count() == 0 ? null : getMappedLO.Select(x => new ItemValueVm
            {
                Id = x.MappingLearningOutcome.Id,
                Description = x.MappingLearningOutcome.LearningOutcome.LearningOutcomeName,
            }).ToList();

            result.Student = studentData;
            result.Grade = gradeData;
            result.Classroom = getClassroom;
            result.ExperienceDetail = experienceDetail;
            result.SupervisorData = supervisorData;
            result.OrganizationDetail = organizationDetail;
            result.LearningOutcomes = loMapping;

            if (evidences.Count == 0)
            {
                result.Evidences = null;

                return Request.CreateApiResult2(result as object);
            }
            else
            {
                var getTrUploads = await _dbContext.Entity<TrServiceAsActionUpload>()
                    .Where(x => evidences.Select(y => y.Id).ToList().Any(y => y == x.IdServiceAsActionEvidence))
                    .ToListAsync(CancellationToken);

                var comments = await _dbContext.Entity<TrServiceAsActionComment>()
                    .Include(x => x.Comentator)
                    .Where(x => evidences.Select(y => y.Id).ToList().Any(y => y == x.IdServiceAsActionEvidence))
                    .ToListAsync(CancellationToken);

                var evidencesList = new List<GetServiceAsActionDetailFormResult_Evidence>();

                foreach (var evidence in evidences.OrderByDescending(x => x.DateIn).ToList())
                {
                    var uploadData = getTrUploads.Where(x => x.IdServiceAsActionEvidence == evidence.Id).ToList();

                    var mappedLo = evidence.LOMappings.ToList();

                    var commentData = comments.Where(x => x.IdServiceAsActionEvidence == evidence.Id).ToList();

                    var evidenceData = new GetServiceAsActionDetailFormResult_Evidence();

                    if (evidence.EvidenceType == "Text" || evidence.EvidenceType == "Link")
                    {
                        evidenceData = new GetServiceAsActionDetailFormResult_Evidence
                        {
                            IdServiceAsActionEvidence = evidence.Id,
                            Datein = evidence.DateIn,
                            CanAddComment = IsExperienceApproved && IsNotComplete,
                            CanDeleteEvidence = IsExperienceApproved && IsNotComplete && (isStudent || isAdvisor),
                            CanEditEvidence = IsExperienceApproved && IsNotComplete && (isStudent || isAdvisor),
                            EvidenceType = evidence.EvidenceType,
                            EvidenceText = evidence.Uploads.FirstOrDefault().EvidenceText ?? null, // karena kalau dia text / link, dia cuma punya 1 upload
                            EvidenceURL  = evidence.Uploads.FirstOrDefault().EvidenceURL ?? null,
                            LearningOutcomes = mappedLo.Count() == 0 ? null : mappedLo.Select(x => new ItemValueVm
                            {
                                Id = x.MappingLearningOutcome.Id,
                                Description = x.MappingLearningOutcome.LearningOutcome.LearningOutcomeName
                            }).ToList(),
                            Urls = null,
                            Comments = param.IsIncludeComment == false ? null :
                            commentData.Count() == 0 ? null :
                            commentData.Select(x => new GetServiceAsActionDetailFormResult_Evidence_Comment
                            {
                                IdServiceAsActionComment = x.Id,
                                Comment = x.CommentDesc,
                                Commentator = new ItemValueVm
                                {
                                    Id = x.Comentator.Id,
                                    Description = x.Comentator.DisplayName
                                },
                                CommentDate = x.DateUp == null ? x.DateIn : x.DateUp,
                                CanDeleteComment = IsExperienceApproved && IsNotComplete && (x.IdCommentator == param.IdUser),
                                CanEditComment = IsExperienceApproved && IsNotComplete && (x.IdCommentator == param.IdUser)
                            }).ToList()
                        };
                    }
                    else
                    {

                        var getUploads = evidence.Uploads.ToList();

                        var uploads = new List<GetServiceAsActionDetailFormResult_Evidence_Uploads>();

                        var StorageSetting = _configuration.GetConnectionString("Student:AccountStorage");
                        var serviceClient = new BlobServiceClient(StorageSetting);
                        var containerClient = serviceClient.GetBlobContainerClient("saa-evidences");

                        foreach (var upload in getUploads)
                        {
                            string fileName = Path.GetFileName(Uri.UnescapeDataString(upload.EvidenceFIGM));
                            var blobClient = containerClient.GetBlobClient(fileName);
                            try
                            {
                                var properties = await blobClient.GetPropertiesAsync();
                                var fileSize = properties.Value.ContentLength;
                                string fileType = Path.GetExtension(fileName).TrimStart('.').ToUpper();

                                var dataUpload = new GetServiceAsActionDetailFormResult_Evidence_Uploads
                                {
                                    EvidenceFIGM = upload.EvidenceFIGM,
                                    FileName = fileName,
                                    FileSize = fileSize,
                                    FileType = fileType
                                };

                                uploads.Add(dataUpload);
                            }
                            catch (Exception ex)
                            {
                                throw new BadRequestException("Error: Blob not found. Verify the blob name and container name.");
                            }
                        }

                        evidenceData = new GetServiceAsActionDetailFormResult_Evidence
                        {
                            IdServiceAsActionEvidence = evidence.Id,
                            Datein = evidence.DateIn,
                            CanAddComment = IsExperienceApproved && IsNotComplete,
                            CanDeleteEvidence = IsExperienceApproved && IsNotComplete && (isStudent || isAdvisor),
                            CanEditEvidence = IsExperienceApproved && IsNotComplete && (isStudent || isAdvisor),
                            EvidenceType = evidence.EvidenceType,
                            EvidenceText = evidence.Uploads.FirstOrDefault().EvidenceText ?? null, // karena kalau dia text / link, dia cuma punya 1 upload
                            EvidenceURL = evidence.Uploads.FirstOrDefault().EvidenceURL ?? null,
                            LearningOutcomes = mappedLo.Count() == 0 ? null : mappedLo.Select(x => new ItemValueVm
                            {
                                Id = x.MappingLearningOutcome.Id,
                                Description = x.MappingLearningOutcome.LearningOutcome.LearningOutcomeName
                            }).ToList(),
                            Urls = uploads,
                            Comments = param.IsIncludeComment == false ? null :
                            commentData.Count() == 0 ? null :
                            commentData.Select(x => new GetServiceAsActionDetailFormResult_Evidence_Comment
                            {
                                IdServiceAsActionComment = x.Id,
                                Comment = x.CommentDesc,
                                Commentator = new ItemValueVm
                                {
                                    Id = x.Comentator.Id,
                                    Description = x.Comentator.DisplayName
                                },
                                CommentDate = x.DateUp == null ? x.DateIn : x.DateUp,
                                CanDeleteComment = IsExperienceApproved && IsNotComplete && (x.IdCommentator == param.IdUser),
                                CanEditComment = IsExperienceApproved && IsNotComplete && (x.IdCommentator == param.IdUser)
                            })
                            .OrderByDescending(x => x.CommentDate).ToList()
                        };
                    }
                    evidencesList.Add(evidenceData);
                }

                result.Evidences = evidencesList;

                return Request.CreateApiResult2(result as object);
            }
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            var getConnectionString = _configuration.GetConnectionString("Student:AccountStorage");
            try
            {
                var storageAccount = CloudStorageAccount.Parse(getConnectionString);
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Scoring:AccountStorage"]);
                return storageAccount;
            }

        }
    }
}
