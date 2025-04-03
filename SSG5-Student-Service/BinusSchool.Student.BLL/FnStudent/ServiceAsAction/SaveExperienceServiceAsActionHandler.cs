using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.DAL.Entities;
using BinusSchool.Student.FnStudent.ServiceAsAction.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Cms;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class SaveExperienceServiceAsActionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IMessage _messageService;

        public SaveExperienceServiceAsActionHandler
        (
            IStudentDbContext dbContext,
            IMessage messageService
        )
        {
            _dbContext = dbContext;
            _messageService = messageService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveExperienceServiceAsActionRequest, SaveExperienceServiceAsActionValidator>();

            if (param.ExperienceDetail.StartDate > param.ExperienceDetail.EndDate) throw new BadRequestException("End Date must be greater than Start Date");

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDefaultFormStatus = await _dbContext.Entity<MsServiceAsActionStatus>()
                    .Where(x => x.StatusDesc == "To Be Determined")
                    .FirstOrDefaultAsync(CancellationToken);

                if (param.IdServiceAsActionForm == null)
                {
                    var getServiceAsActionHeader = await _dbContext.Entity<TrServiceAsActionHeader>()
                        .Where(x => x.IdStudent == param.IdStudent && x.IdAcademicYear == param.ExperienceDetail.IdAcademicYear)
                        .FirstOrDefaultAsync();

                    var serviceAsActionTypeList = param.ExperienceDetail.IdServiceAsActionTypes.ToList();
                    var serviceAsActionSdgsList = param.ExperienceDetail.IdServiceAsActionSdgs.ToList();
                    var loMappingList = param.IdLearningOutcomes.ToList();

                    if (getServiceAsActionHeader == null)
                    {
                        var getDefaultStatus = await _dbContext.Entity<MsServiceAsActionStatus>()
                            .Where(x => x.StatusDesc == "On Track")
                            .FirstOrDefaultAsync(CancellationToken);

                        if (getDefaultStatus == null) throw new BadRequestException("Default Status not found in Database");

                        var newHeader = new TrServiceAsActionHeader
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.ExperienceDetail.IdAcademicYear,
                            IdStudent = param.IdStudent,
                            IdStatusOverall = getDefaultStatus.Id
                        };

                        _dbContext.Entity<TrServiceAsActionHeader>().Add(newHeader);
                        await _dbContext.SaveChangesAsync(CancellationToken);
                    }

                    var getHeader = await _dbContext.Entity<TrServiceAsActionHeader>()
                        .Where(x => x.IdStudent == param.IdStudent && x.IdAcademicYear == param.ExperienceDetail.IdAcademicYear)
                        .FirstOrDefaultAsync();

                    var supervisorData = await MakeSupervisorData(_dbContext, param.SupervisorData);

                    if (getDefaultFormStatus == null) throw new BadRequestException("Default Experience Status not found in Database");

                    var newExperience = new TrServiceAsActionForm
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdServiceAsActionHeader = getHeader.Id,
                        ExpName = param.ExperienceDetail.ExperienceName,
                        IdServiceAsActionLocDesc = param.ExperienceDetail.IdServiceAsActionLocation,
                        StartDate = param.ExperienceDetail.StartDate,
                        EndDate = param.ExperienceDetail.EndDate,
                        IdSupervisor = supervisorData?.IdSupervisor ?? null,
                        SupervisorName = supervisorData?.SupervisorName ?? null,
                        SupervisorEmail = supervisorData?.SupervisorEmail ?? null,
                        SupervisorTitle = supervisorData?.SupervisorTitle ?? null,
                        SupervisorContact = supervisorData?.SupervisorContact ?? null,
                        OrganizationName = param.OrganizationDetail.Organization,
                        ContributionTMC = param.OrganizationDetail.ContributionTMC,
                        ActivityDesc = param.OrganizationDetail.ActivityDescription,
                        IdServiceAsActionStatus = getDefaultFormStatus.Id,
                    };

                    _dbContext.Entity<TrServiceAsActionForm>().Add(newExperience);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    var newTypeList = new List<TrServiceAsActionMappingType>();

                    foreach (var item in serviceAsActionTypeList)
                    {
                        var newMapping = new TrServiceAsActionMappingType
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdServiceAsActionForm = newExperience.Id,
                            IdServiceAsActionType = item
                        };

                        newTypeList.Add(newMapping);
                    }
                    _dbContext.Entity<TrServiceAsActionMappingType>().AddRange(newTypeList);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    var newSdgsList = new List<TrServiceAsActionMappingSdgs>();

                    foreach (var item in serviceAsActionSdgsList)
                    {
                        var newMapping = new TrServiceAsActionMappingSdgs
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdServiceAsActionForm = newExperience.Id,
                            IdServiceAsActionSdgs = item
                        };

                        newSdgsList.Add(newMapping);
                    }

                    _dbContext.Entity<TrServiceAsActionMappingSdgs>().AddRange(newSdgsList);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    var newLOList = new List<TrServiceAsActionMappingForm>();

                    foreach (var item in loMappingList)
                    {
                        var newMapping = new TrServiceAsActionMappingForm
                        {
                            IdSAMappingForm = Guid.NewGuid().ToString(),
                            IdServiceAsActionForm = newExperience.Id,
                            IdMappingLearningOutcome = item
                        };

                        newLOList.Add(newMapping);
                    }

                    _dbContext.Entity<TrServiceAsActionMappingForm>().AddRange(newLOList);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    //get Recepients for message

                    List<string> Recipients = new List<string>();

                    var student = await _dbContext.Entity<MsStudent>()
                        .Where(x => x.Id == param.IdStudent)
                        .FirstOrDefaultAsync(CancellationToken);

                    var studentName = NameUtil.GenerateFullName(student.FirstName, student.MiddleName, student.LastName);

                    var getAdvisors = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.HomeroomTeachers)
                        .Where(x => x.IdStudent == param.IdStudent && x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == param.ExperienceDetail.IdAcademicYear)
                        .SelectMany(x => x.Homeroom.HomeroomTeachers.Select(y => y.IdBinusian))
                        .Distinct()
                        .ToListAsync(CancellationToken);

                    Recipients.AddRange(getAdvisors);

                    if (!String.IsNullOrEmpty(supervisorData.IdSupervisor))
                    {
                        Recipients.Add(supervisorData.IdSupervisor);
                    }

                    // Send Message
                    var createMessage = await _messageService
                    .AddMessage(new AddMessageRequest
                    {
                        Type = UserMessageType.Announcement,
                        IdSender = param.IdStudent,
                        Recepients = Recipients,
                        Subject = $"You are assigned as Role (Supervisor/Advisor) of {studentName} for {newExperience.ExpName}",
                        Content = $"{studentName} has been successfully create a new activity '{newExperience.ExpName}'",
                        Attachments = new List<MessageAttachment>(), // attachments can't be null
                        IsSetSenderAsSchool = true,
                        IsDraft = false,
                        IsAllowReply = false,
                        IsMarkAsPinned = false,
                        GroupMembers = new List<string>(),
                        MessageFor = new List<MessageFor>(),
                    });
                }
                else
                {
                    var checkDataExist = await _dbContext.Entity<TrServiceAsActionForm>()
                        .Include(x => x.ServiceAsActionStatus)
                        .Where(x => x.Id == param.IdServiceAsActionForm)
                        .FirstOrDefaultAsync(CancellationToken);

                    if(checkDataExist == null) throw new BadRequestException("Data not found in Database");

                    #region Handling Mapping Type

                    var getMappedTypes = await _dbContext.Entity<TrServiceAsActionMappingType>()
                        .Where(x => x.IdServiceAsActionForm == param.IdServiceAsActionForm)
                        .ToListAsync(CancellationToken);

                    var newTypeList = param.ExperienceDetail.IdServiceAsActionTypes.ToList();
                    var deleteTypeList = getMappedTypes.Where(x => !newTypeList.Contains(x.IdServiceAsActionType)).ToList();
                    var addTypeList = newTypeList.Where(x => !getMappedTypes.Select(y => y.IdServiceAsActionType).Contains(x)).ToList();

                    if(addTypeList.Count > 0)
                    {
                        var newTypeMappingList = new List<TrServiceAsActionMappingType>();

                        foreach (var item in addTypeList)
                        {
                            var newMapping = new TrServiceAsActionMappingType
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdServiceAsActionForm = param.IdServiceAsActionForm,
                                IdServiceAsActionType = item
                            };

                            newTypeMappingList.Add(newMapping);
                        }

                        _dbContext.Entity<TrServiceAsActionMappingType>().AddRange(newTypeMappingList);
                    }
                    if(deleteTypeList.Count > 0)
                    {
                        _dbContext.Entity<TrServiceAsActionMappingType>().RemoveRange(deleteTypeList);
                    }
                    #endregion

                    #region Handling Mapping Sdgs

                    var getMappedSdgs = await _dbContext.Entity<TrServiceAsActionMappingSdgs>()
                        .Where(x => x.IdServiceAsActionForm == param.IdServiceAsActionForm)
                        .ToListAsync(CancellationToken);

                    var newSdgsList = param.ExperienceDetail.IdServiceAsActionSdgs.ToList();
                    var deleteSdgsList = getMappedSdgs.Where(x => !newSdgsList.Contains(x.IdServiceAsActionSdgs)).ToList();
                    var addSdgsList = newSdgsList.Where(x => !getMappedSdgs.Select(y => y.IdServiceAsActionSdgs).Contains(x)).ToList();

                    if (addSdgsList.Count > 0)
                    {
                        var newSdgsMappingList = new List<TrServiceAsActionMappingSdgs>();

                        foreach (var item in addSdgsList)
                        {
                            var newMapping = new TrServiceAsActionMappingSdgs
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdServiceAsActionForm = param.IdServiceAsActionForm,
                                IdServiceAsActionSdgs = item
                            };

                            newSdgsMappingList.Add(newMapping);
                        }

                        _dbContext.Entity<TrServiceAsActionMappingSdgs>().AddRange(newSdgsMappingList);
                    }
                    if(deleteSdgsList.Count > 0)
                    {
                        _dbContext.Entity<TrServiceAsActionMappingSdgs>().RemoveRange(deleteSdgsList);
                    }   

                    #endregion

                    #region Handling Mapping Learning Outcome

                    var getMappedLO = await _dbContext.Entity<TrServiceAsActionMappingForm>()
                        .Where(x => x.IdServiceAsActionForm == param.IdServiceAsActionForm)
                        .ToListAsync(CancellationToken);

                    var newLOList = param.IdLearningOutcomes.ToList();
                    var deleteLOList = getMappedLO.Where(x => !newLOList.Contains(x.IdMappingLearningOutcome)).ToList();
                    var addLOList = newLOList.Where(x => !getMappedLO.Select(y => y.IdMappingLearningOutcome).Contains(x)).ToList();

                    if (addLOList.Count > 0)
                    {
                        var newLOMappingList = new List<TrServiceAsActionMappingForm>();

                        foreach (var item in addLOList)
                        {
                            var newMapping = new TrServiceAsActionMappingForm
                            {
                                IdSAMappingForm = Guid.NewGuid().ToString(),
                                IdServiceAsActionForm = param.IdServiceAsActionForm,
                                IdMappingLearningOutcome = item
                            };

                            newLOMappingList.Add(newMapping);
                        }

                        _dbContext.Entity<TrServiceAsActionMappingForm>().AddRange(newLOMappingList);
                    }
                    if (deleteLOList.Count > 0)
                    {
                        _dbContext.Entity<TrServiceAsActionMappingForm>().RemoveRange(deleteLOList);
                    }

                    #endregion

                    var supervisorData = await MakeSupervisorData(_dbContext, param.SupervisorData);

                    checkDataExist.ExpName = param.ExperienceDetail.ExperienceName;
                    checkDataExist.IdServiceAsActionLocDesc = param.ExperienceDetail.IdServiceAsActionLocation;
                    checkDataExist.StartDate = param.ExperienceDetail.StartDate;
                    checkDataExist.EndDate = param.ExperienceDetail.EndDate;
                    checkDataExist.IdSupervisor = supervisorData?.IdSupervisor ?? null;
                    checkDataExist.SupervisorName = supervisorData?.SupervisorName ?? null;
                    checkDataExist.SupervisorEmail = supervisorData?.SupervisorEmail ?? null;
                    checkDataExist.SupervisorTitle = supervisorData?.SupervisorTitle ?? null;
                    checkDataExist.SupervisorContact = supervisorData?.SupervisorContact ?? null;
                    checkDataExist.OrganizationName = param.OrganizationDetail.Organization;
                    checkDataExist.ContributionTMC = param.OrganizationDetail.ContributionTMC;
                    checkDataExist.ActivityDesc = param.OrganizationDetail.ActivityDescription;
                    checkDataExist.IdServiceAsActionStatus = checkDataExist.ServiceAsActionStatus.StatusDesc == "Need Revision" ? getDefaultFormStatus.Id : checkDataExist.IdServiceAsActionStatus;

                    _dbContext.Entity<TrServiceAsActionForm>().Update(checkDataExist);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                if (!string.IsNullOrEmpty(param.IdServiceAsActionForm))
                {
                    List<string> Recipients = new List<string>();

                    #region GetRecipients for notif
                    var getUserRole = await _dbContext.Entity<MsUserRole>()
                        .Include(x => x.User)
                        .Include(x => x.Role)
                        .Where(x => x.IdUser == AuthInfo.UserId)
                        .ToListAsync(CancellationToken);

                    var getFormDetail = await _dbContext.Entity<TrServiceAsActionForm>()
                        .Include(x => x.ServiceAsActionHeader)
                            .ThenInclude(x => x.Student)
                        .Where(x => x.Id == param.IdServiceAsActionForm)
                        .FirstOrDefaultAsync(CancellationToken);

                    var getAdvisors = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.HomeroomTeachers)
                        .Where(x => x.IdStudent == getFormDetail.ServiceAsActionHeader.IdStudent && x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == getFormDetail.ServiceAsActionHeader.IdAcademicYear)
                        .SelectMany(x => x.Homeroom.HomeroomTeachers.Select(y => y.IdBinusian))
                        .Distinct()
                        .ToListAsync(CancellationToken);

                    if (getUserRole.FirstOrDefault().Role.Code == "Student")
                    {
                        var formDetail = getFormDetail;

                        Recipients.AddRange(getAdvisors);
                        if (!String.IsNullOrEmpty(formDetail.IdSupervisor))
                        {
                            Recipients.Add(formDetail.IdSupervisor);
                        }
                    }
                    else
                    {
                        var formDetail = getFormDetail;
                        var student = formDetail.ServiceAsActionHeader.IdStudent;

                        Recipients.Add(student);
                        if (!String.IsNullOrEmpty(formDetail.IdSupervisor))
                        {
                            Recipients.Add(formDetail.IdSupervisor);
                        }
                    }
                    #endregion

                    var getUser = await _dbContext.Entity<MsUser>()
                        .Where(x => x.Id == AuthInfo.UserId)
                        .FirstOrDefaultAsync(CancellationToken);

                    var getActivityName = getFormDetail.ExpName;

                    var Notification = SASNotification(KeyValues, getUser.DisplayName, Recipients, getActivityName, AuthInfo, param);

                }

                await _transaction.CommitAsync(CancellationToken);
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
            
            return Request.CreateApiResult2();

        }

        protected async Task<SupervisorItemVm> MakeSupervisorData(IStudentDbContext dbContext , SaveExperienceServiceAsActionRequest_SupervisorData supervisorData)
        {
            var getSupervisorExist = await dbContext.Entity<MsUser>()
                .Where(x => x.Email == supervisorData.SupervisorEmail.Trim())
                .FirstOrDefaultAsync(CancellationToken);

            var superVisorData = new SupervisorItemVm();

            if (getSupervisorExist == null)
            {
                superVisorData.IdSupervisor = null;
                superVisorData.SupervisorName = supervisorData.SupervisorName ?? null;
                superVisorData.SupervisorEmail = supervisorData.SupervisorEmail ?? null;
                superVisorData.SupervisorTitle = supervisorData.SupervisorTitle ?? null;
                superVisorData.SupervisorContact = supervisorData.SupervisorContact ?? null;
            }
            else
            {
                superVisorData.IdSupervisor = getSupervisorExist.Id;
                superVisorData.SupervisorName = getSupervisorExist.DisplayName;
                superVisorData.SupervisorEmail = getSupervisorExist.Email;
                superVisorData.SupervisorTitle = supervisorData.SupervisorTitle ?? null;
                superVisorData.SupervisorContact = supervisorData.SupervisorContact ?? null;
            }

            return superVisorData;
        }

        public static string SASNotification(IDictionary<string, object> KeyValues, string user, List<string> recipients, string activityName, AuthenticationInfo AuthInfo, SaveExperienceServiceAsActionRequest param)
        {
            if (KeyValues.ContainsKey("user"))
            {
                KeyValues.Remove("user");
            }

            if (KeyValues.ContainsKey("activityName"))
            {
                KeyValues.Remove("activityName");
            }

            if (KeyValues.ContainsKey("notifUrl"))
            {
                KeyValues.Remove("notifUrl");
            }

            KeyValues.Add("user", user);
            KeyValues.Add("activityName", activityName);
            KeyValues.Add("notifUrl", param.NotifUrl);


            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SAASA3")
                {
                    IdRecipients = recipients,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
