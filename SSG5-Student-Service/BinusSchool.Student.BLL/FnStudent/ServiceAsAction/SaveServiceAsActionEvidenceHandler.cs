using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.ServiceAsAction.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class SaveServiceAsActionEvidenceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveServiceAsActionEvidenceHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var param = await Request.ValidateBody<SaveServiceAsActionEvidenceRequest, SaveServiceAsActionEvidenceValidator>();

                var dataExperience = await _dbContext.Entity<TrServiceAsActionForm>()
                    .Include(x => x.ServiceAsActionHeader)
                        .ThenInclude(x => x.Student)
                    .Where(x => x.Id == param.IdServiceAsActionForm)
                    .FirstOrDefaultAsync(CancellationToken);

                if (dataExperience == null) throw new Exception("Experience Not Found");

                if (String.IsNullOrEmpty(param.IdServiceAsActionEvidence))
                {
                    var newData = new TrServiceAsActionEvidence
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdServiceAsActionForm = param.IdServiceAsActionForm,
                        EvidenceType = param.EvidenceType
                    };
                    _dbContext.Entity<TrServiceAsActionEvidence>().Add(newData);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    var loMappings = new List<TrServiceAsActionMapping>();

                    foreach (var id in param.IdLoMappings)
                    {
                        var loMapping = new TrServiceAsActionMapping
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdServiceAsActionEvidence = newData.Id,
                            IdMappingLearningOutcome = id
                        };

                        loMappings.Add(loMapping);
                    }

                    _dbContext.Entity<TrServiceAsActionMapping>().AddRange(loMappings);

                    if(param.FIGM != null)
                    {
                        if (param.FIGM.Count > 0)
                        {
                            var listEvidence = new List<TrServiceAsActionUpload>();

                            foreach (var figm in param.FIGM)
                            {
                                var evidence = new TrServiceAsActionUpload
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdServiceAsActionEvidence = newData.Id,
                                    EvidenceFIGM = figm
                                };

                                listEvidence.Add(evidence);
                                _dbContext.Entity<TrServiceAsActionUpload>().AddRange(listEvidence);
                            }
                        }
                    }

                    if(!String.IsNullOrEmpty(param.Text))
                    {
                        var evidence  = new TrServiceAsActionUpload
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdServiceAsActionEvidence = newData.Id,
                            EvidenceText = param.Text
                        };

                        _dbContext.Entity<TrServiceAsActionUpload>().AddRange(evidence);

                    }

                    if (!String.IsNullOrEmpty(param.Url))
                    {
                        var evidence = new TrServiceAsActionUpload
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdServiceAsActionEvidence = newData.Id,
                            EvidenceURL = param.Url
                        };

                        _dbContext.Entity<TrServiceAsActionUpload>().AddRange(evidence);

                    }
                }
                else
                {
                    var checkEvidence = await _dbContext.Entity<TrServiceAsActionEvidence>()
                        .Where(x => x.Id == param.IdServiceAsActionEvidence)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (checkEvidence == null) throw new Exception("Evidence Not Found");

                    checkEvidence.EvidenceType = param.EvidenceType;
                    _dbContext.Entity<TrServiceAsActionEvidence>().Update(checkEvidence);

                    var loMappings = await _dbContext.Entity<TrServiceAsActionMapping>()
                        .Where(x => x.IdServiceAsActionEvidence == param.IdServiceAsActionEvidence)
                        .ToListAsync(CancellationToken);

                    var deletedMappings = loMappings.Where(x => !param.IdLoMappings.Any(y => y == x.IdMappingLearningOutcome)).ToList();
                    var newMappings = param.IdLoMappings.Where(x => !loMappings.Any(y => y.IdMappingLearningOutcome == x)).ToList();

                    if (deletedMappings.Count > 0)
                    {
                        foreach (var item in deletedMappings)
                        {
                            item.IsActive = false;
                            _dbContext.Entity<TrServiceAsActionMapping>().Update(item);
                        }
                    }

                    if (newMappings.Count > 0)
                    {
                        var listNewMappings = new List<TrServiceAsActionMapping>();

                        foreach (var id in newMappings)
                        {
                            var loMapping = new TrServiceAsActionMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdServiceAsActionEvidence = param.IdServiceAsActionEvidence,
                                IdMappingLearningOutcome = id
                            };

                            listNewMappings.Add(loMapping);
                        }

                        _dbContext.Entity<TrServiceAsActionMapping>().AddRange(listNewMappings);
                    }

                    var getUploads = await _dbContext.Entity<TrServiceAsActionUpload>()
                        .Where(x => x.IdServiceAsActionEvidence == param.IdServiceAsActionEvidence)
                        .ToListAsync(CancellationToken);

                    if (getUploads.Count > 0)
                    {
                        foreach(var item in getUploads  )
                        {
                            item.IsActive = false;
                            _dbContext.Entity<TrServiceAsActionUpload>().Update(item);
                        }
                    }

                    if(param.FIGM != null)
                    {
                        if (param.FIGM.Count > 0)
                        {
                            var listEvidence = new List<TrServiceAsActionUpload>();

                            foreach (var figm in param.FIGM)
                            {
                                var evidence = new TrServiceAsActionUpload
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdServiceAsActionEvidence = param.IdServiceAsActionEvidence,
                                    EvidenceFIGM = figm
                                };

                                listEvidence.Add(evidence);
                                _dbContext.Entity<TrServiceAsActionUpload>().AddRange(listEvidence);
                            }
                        }
                    }
                    else
                    {
                        var evidence = new TrServiceAsActionUpload
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdServiceAsActionEvidence = param.IdServiceAsActionEvidence,
                            EvidenceText = param?.Text ?? null,
                            EvidenceURL = param?.Url ?? null
                        };

                        _dbContext.Entity<TrServiceAsActionUpload>().Add(evidence);
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                if (String.IsNullOrEmpty(param.IdServiceAsActionEvidence))
                {
                    List<string> Recipients = new List<string>();

                    var getAdvisors = await _dbContext.Entity<MsHomeroomStudent>()
                   .Include(x => x.Homeroom)
                       .ThenInclude(x => x.HomeroomTeachers)
                   .Where(x => x.IdStudent == dataExperience.ServiceAsActionHeader.IdStudent && x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == dataExperience.ServiceAsActionHeader.IdAcademicYear)
                   .SelectMany(x => x.Homeroom.HomeroomTeachers.Select(y => y.IdBinusian))
                   .Distinct()
                   .ToListAsync(CancellationToken);

                    if (!string.IsNullOrEmpty(dataExperience.IdSupervisor))
                    {
                        Recipients.Add(dataExperience.IdSupervisor);
                    }
                    Recipients.AddRange(getAdvisors);

                    var student = dataExperience.ServiceAsActionHeader.Student;

                    var studentName = NameUtil.GenerateFullName(student.FirstName, student.MiddleName, student.LastName);
                    var activityName = dataExperience.ExpName;

                    var Notification = SAS2Notification(KeyValues, studentName, Recipients, activityName, AuthInfo);

                }

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

        public static string SAS2Notification(IDictionary<string, object> KeyValues, string user, List<string> recipients, string activityName, AuthenticationInfo AuthInfo)
        {
            if (KeyValues.ContainsKey("user"))
            {
                KeyValues.Remove("user");
            }

            if (KeyValues.ContainsKey("activityName"))
            {
                KeyValues.Remove("activityName");
            }

            KeyValues.Add("user", user);
            KeyValues.Add("activityName", activityName);


            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SAASA2")
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
