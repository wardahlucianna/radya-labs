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
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.User.FnUser;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Data.Configurations;
using BinusSchool.Persistence.StudentDb.Entities.User;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{

    public class UpdateStatusExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStringLocalizer _localizer;
        private readonly IConfiguration _configuration;
        private readonly IUser _userService;

        public UpdateStatusExperienceHandler(IStudentDbContext dbContext,
         IStringLocalizer localizer,
         IConfiguration configuration,
         IUser userService)
        {
            _dbContext = dbContext;
            _localizer = localizer;
            _configuration = configuration;
            _userService = userService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateStatusExperienceRequest, UpdateExperienceStatusValidator>();

            var apiConfig = _configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>();


            var GerUserCAS = await _dbContext.Entity<TrCasAdvisorStudent>()
                              .Include(e => e.CasAdvisor).ThenInclude(e => e.UserCAS)
                              .Where(e => e.HomeroomStudent.IdStudent == body.IdStudent && e.CasAdvisor.IdAcademicYear == body.IdAcademicYear && e.CasAdvisor.IdUserCAS == body.IdUser)
                              .Select(e => e.CasAdvisor)
                              .FirstOrDefaultAsync(CancellationToken);

            if (GerUserCAS == null)
                throw new BadRequestException("You are not CAS advisor this student");


            var GetExperience = await _dbContext.Entity<TrExperience>()
                                .Include(x => x.AcademicYear)
                                .Where(e => e.Id == body.IdExperience)
                                .FirstOrDefaultAsync(CancellationToken);

            GetExperience.Status = body.ExperienceStatus;

            var getUserEmail = await _dbContext.Entity<MsUser>()
                             .Where(e => e.Email == GetExperience.SupervisorEmail)
                             .Select(e => e.Id)
                             .FirstOrDefaultAsync(CancellationToken);

            var userPassword = GenerateRandomPassword(6);

            if (body.ExperienceStatus == ExperienceStatus.NeedRevision)
            {
                var newExperienceStatusChangeHs = new TrExperienceStatusChangeHs
                {
                    Id = Guid.NewGuid().ToString(),
                    IdExperience = body.IdExperience,
                    IdUserApproval = AuthInfo.UserId,
                    ExperienceStatusChangeDate = DateTime.Now,
                    Note = body.Note
                };

                _dbContext.Entity<TrExperienceStatusChangeHs>().Add(newExperienceStatusChangeHs);
            }
            else if (body.ExperienceStatus == ExperienceStatus.Approved)
            {

                var idRoleBySchool = GetRoleGuestUser(GetExperience.AcademicYear.IdSchool);

                var idSchoolUser = GetExperience.AcademicYear.IdSchool;

                var emailUser =  GetExperience.SupervisorEmail;

                if (getUserEmail == null)
                {
                    try
                    {
                        var idUser = Guid.NewGuid().ToString();
                        var idUserRole = Guid.NewGuid().ToString();
                        // create user supervisor
                        _ = await _userService.AddUserSupervisorForExperience(new AddUserSupervisorForExperienceRequest
                            {
                                IdUser = idUser,
                                IdUserRole = idUserRole,
                                Password = userPassword,
                                IdSchool = idSchoolUser,
                                IsActiveDirectory = false,
                                Email = emailUser,
                                IdRole = idRoleBySchool
                            });

                        var newUser = new MsUser
                        {
                            Id = idUser,
                            Username = emailUser,
                            DisplayName = emailUser,
                            Email = emailUser,
                            IsActiveDirectory = false,
                            Status = true
                        };
                        _dbContext.Entity<MsUser>().Add(newUser);

                        var newUserRole = new MsUserRole
                        {
                            Id = idUserRole,
                            IdUser = idUser,
                            IdRole = idRoleBySchool,
                            IsDefault = true,
                            Username = emailUser
                        };
                        _dbContext.Entity<MsUserRole>().Add(newUserRole);

                        GetExperience.IdUserSupervisor = idUser;

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                else
                {
                    if(GetExperience.ExperienceLocation != ExperienceLocation.InSchool)
                    {
                        GetExperience.IdUserSupervisor = getUserEmail;
                    }
                }
            }

            _dbContext.Entity<TrExperience>().Update(GetExperience);

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Notification
            var GetExperienceEmail = await _dbContext.Entity<TrExperience>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Student)
                                .Where(e => e.Id == body.IdExperience)
                                .FirstOrDefaultAsync(CancellationToken);

            var EmailUpdateStatusExperience = new EmailUpdateStatusExperienceResult
            {
                Id = GetExperienceEmail.Id,
                CasAdvisorName = GerUserCAS.UserCAS.DisplayName,
                ExperienceName = GetExperienceEmail.ExperienceName,
                Status = GetExperienceEmail.Status.GetDescription(),
                StartDate = GetExperienceEmail.StartDate.ToString("dd MMM yyyy"),
                EndDate = GetExperienceEmail.EndDate.ToString("dd MMM yyyy"),
                Location = GetExperienceEmail.ExperienceLocation.GetDescription(),
                IdStudent = GetExperienceEmail.HomeroomStudent.IdStudent,
            };

            if (KeyValues.ContainsKey("GetExperienceEmail"))
            {
                KeyValues.Remove("GetExperienceEmail");
            }

            KeyValues.Add("GetExperienceEmail", EmailUpdateStatusExperience);
            var Notification = CAS7Notification(KeyValues, AuthInfo);

            if (body.ExperienceStatus == ExperienceStatus.Approved)
            {
                var EmailSupervisor = new EmailSupervisorResult
                {
                    Id = GetExperienceEmail.Id,
                    Username = GetExperienceEmail.SupervisorEmail,
                    Password = userPassword,
                    IsLabelPassword = getUserEmail == null?true:false,
                    IdUserSupervisor = GetExperienceEmail.IdUserSupervisor,
                    StudentName = GetExperienceEmail.HomeroomStudent.Student.FirstName
                                +(GetExperienceEmail.HomeroomStudent.Student.MiddleName==null?"":" "+ GetExperienceEmail.HomeroomStudent.Student.MiddleName)
                                +(GetExperienceEmail.HomeroomStudent.Student.LastName==null?"":" "+ GetExperienceEmail.HomeroomStudent.Student.LastName),
                    SupervisorName = GetExperienceEmail.SupervisorName
                };

                if (KeyValues.ContainsKey("GetEmailSupervisor"))
                {
                    KeyValues.Remove("GetEmailSupervisor");
                }

                KeyValues.Add("GetEmailSupervisor", EmailSupervisor);
                var NotificationSupervisor = CAS14Notification(KeyValues, AuthInfo);
            }
            #endregion

            return Request.CreateApiResult2();
        }

        public static string CAS7Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailUpdateStatusExperienceResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CAS7")
                {
                    IdRecipients = new List<string>
                    {
                        GetExperienceEmail.IdStudent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string CAS14Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetEmailSupervisor").Value;
            var GetEmailSupervisor = JsonConvert.DeserializeObject<EmailSupervisorResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CAS14")
                {
                    IdRecipients = new List<string>
                    {
                        GetEmailSupervisor.IdUserSupervisor,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public string GetRoleGuestUser(string idSchool)
        {
            if (idSchool == "1")
            {
                return "GUSIMPRUG";
            }
            else if (idSchool == "2")
            {
                return "GUSERPONG";
            }
            else if (idSchool == "3")
            {
                return "GUBEKASI";
            }
            else
            {
                return "GUSEMARANG";
            }
        }

        public static string GenerateRandomPassword(int size)
        {
            var random = new Random();
            var letters = "abcdefghijklmnopqrstuvwxyz";
            var numerics = "0123456789";
            var specials = "!@#$%^&*_?><";
            var reserved = new Dictionary<int, char>();

            while (reserved.Count < 3)
            {
                int key = random.Next(1, size);
                switch (reserved.Count)
                {
                    case 0:
                        if (!reserved.ContainsKey(key))
                            reserved.Add(key, letters.ToUpper()[random.Next(0, letters.Length - 1)]);
                        break;
                    case 1:
                        if (!reserved.ContainsKey(key))
                            reserved.Add(key, numerics[random.Next(0, numerics.Length - 1)]);
                        break;
                    case 2:
                        if (!reserved.ContainsKey(key))
                            reserved.Add(key, specials[random.Next(0, specials.Length - 1)]);
                        break;
                    default:
                        break;
                }
            }

            var builder = new StringBuilder();
            for (int i = 1; i <= size; i++)
            {
                if (reserved.ContainsKey(i))
                    builder.Append(reserved[i]);
                else
                    builder.Append(letters[random.Next(0, letters.Length - 1)]);
            }
            return builder.ToString();
        }
    }
}
