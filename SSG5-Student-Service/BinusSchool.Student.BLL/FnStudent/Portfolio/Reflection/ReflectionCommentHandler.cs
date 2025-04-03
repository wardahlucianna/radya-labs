using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.Portfolio.Reflection.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection
{
    public class ReflectionCommentHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public ReflectionCommentHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetReflectionStudentComment = await _dbContext.Entity<TrReflectionStudentComment>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetReflectionStudentComment.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrReflectionStudentComment>().UpdateRange(GetReflectionStudentComment);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var items = await _dbContext
                        .Entity<TrReflectionStudentComment>()
                        .Where(e => e.Id == id)
                        .Select(e => new GetDetailReflectionCommentResult
                        {
                            Id = e.Id,
                            Comment = e.Comment,
                            IdUser = e.IdUserComment,
                            IdReflection = e.IdReflectionStudent
                        })
                        .SingleOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);

        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddReflectionCommentRequest, AddReflectionCommentValidator>();

            var NewReflectionStudentComment = new TrReflectionStudentComment
            {
                Id = Guid.NewGuid().ToString(),
                IdUserComment = body.IdUser,
                Comment = body.Comment,
                IdReflectionStudent = body.IdReflection
            };

            _dbContext.Entity<TrReflectionStudentComment>().Add(NewReflectionStudentComment);
            await _dbContext.SaveChangesAsync(CancellationToken);

            await ProcessEmailAndNotifScenario3(body, NewReflectionStudentComment.Id);

            await ProcessEmailAndNotifScenario4(body, NewReflectionStudentComment.Id);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateReflectionCommentRequest, UpdateReflectionCommentValidator>();

            var GetReflectionStudentComment = await _dbContext
                                        .Entity<TrReflectionStudentComment>()
                                        .Where(e => e.Id == body.Id)
                                        .SingleOrDefaultAsync(CancellationToken);

            GetReflectionStudentComment.Comment = body.comment;

            _dbContext.Entity<TrReflectionStudentComment>().Update(GetReflectionStudentComment);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private async Task ProcessEmailAndNotifScenario3(AddReflectionCommentRequest param, string IdReflectionStudentComment)
        {
            var User = await _dbContext.Entity<MsUser>()
            .Select(x => new {
                x.Id,
                x.DisplayName
            }
            )
            .ToListAsync(CancellationToken);

            var Staff = await _dbContext.Entity<MsStaff>()
                    .Select(x => new {
                        x.IdBinusian,
                    }
                    )
                    .ToListAsync(CancellationToken);

            var dataReceipment = await _dbContext.Entity<TrReflectionStudentComment>()
                      .Include(x => x.ReflectionStudent).ThenInclude(x => x.Student)
                      .Include(x => x.User)
                      .Where(x => x.IdReflectionStudent == param.IdReflection && x.IdUserComment != param.IdUser)
                      .Select(x => new EmailCommentPortofolioResult
                      {
                          IdReceive = x.IdUserComment,
                          ReceiveName = x.User.DisplayName
                      })
                      .ToListAsync();

            var dataComment = await _dbContext.Entity<TrReflectionStudentComment>()
                              .Include(x => x.ReflectionStudent).ThenInclude(x => x.Student)
                              .Include(x => x.User)
                              .Where(x => x.Id == IdReflectionStudentComment)
                              .Select(x => new EmailCommentPortofolioResult
                              {
                                  //IdReceive = x.IdUserComment,
                                  //ReceiveName = x.User.DisplayName,
                                  IdAcademicYear = x.ReflectionStudent.IdAcademicYear,
                                  IdStudent = x.ReflectionStudent.IdStudent,
                                  Semester = x.ReflectionStudent.Semester,
                                  UserNameComment = x.User.DisplayName,
                                  IdUserCreate = x.ReflectionStudent.UserIn,
                                  //UserNameCreate =  User.Where(y=> y.Id == x.CourseworkAnecdotalStudent.UserIn).Select(y=>y.DisplayName).FirstOrDefault(),
                                  TabMenu = "Reflection",
                                  TabMenuIdx = "Reflection",
                                  Date = x.DateIn.Value,
                                  SchoolName = x.ReflectionStudent.Student.IdSchool,
                                  Comment = x.Comment
                              })
                              .FirstOrDefaultAsync();

            foreach (var item in dataReceipment)
            {
                item.IdAcademicYear = dataComment.IdAcademicYear;
                item.IdStudent = dataComment.IdStudent;
                item.Semester = dataComment.Semester;
                item.UserNameComment = dataComment.UserNameComment;
                item.IdUserCreate = dataComment.IdUserCreate;
                item.TabMenu = dataComment.TabMenu;
                item.TabMenuIdx = dataComment.TabMenuIdx;
                item.Date = dataComment.Date;
                item.SchoolName = dataComment.SchoolName;
                item.Comment = dataComment.Comment;
            }


            if (dataReceipment.Count == 0)
            {
                return;
            }

            foreach (var item in dataReceipment)
            {
                item.UserNameCreate = User.Where(y => y.Id == item.IdUserCreate).Select(y => y.DisplayName).FirstOrDefault();
            }

            var ListRecipients = dataReceipment.Where(x => x.IdReceive != dataReceipment.First().IdUserCreate).Select(x => new CodeWithIdVm
            {
                Id = x.IdReceive,
                Code = x.ReceiveName
            }).ToList();

            if (ListRecipients.Any(x => x.Code.Contains("Parent")))
            {
                var GetData = ListRecipients.Where(x => x.Code.Contains("Parent")).ToList();

                foreach (var item in GetData)
                {
                    var getParent = item.Id.Split("P");

                    if (getParent.Length > 1)
                    {
                        var Parent = await _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Parent)
                            .Where(x => x.IdStudent == getParent[1])
                            .Select(y => new DataParentCourseworkAnecdotal
                            {
                                EmailParent = y.Parent != null ? y.Parent.PersonalEmailAddress : "",
                                ParentName = y.Parent != null ? !string.IsNullOrEmpty(y.Parent.FirstName) ? y.Parent.FirstName : y.Parent.LastName : "",
                            })
                            .ToListAsync(CancellationToken);

                        foreach (var resultParent in dataReceipment)
                        {
                            if (resultParent.IdReceive == item.Id)
                            {
                                resultParent.DataParents = Parent;
                                resultParent.ForWho = "Parent";
                            }
                        }
                    }
                }
            }

            foreach (var result in dataReceipment)
            {
                if (Staff.Any(x => x.IdBinusian == result.IdReceive))
                {
                    result.ForWho = "Teacher";
                }
                else
                {
                    if (string.IsNullOrEmpty(result.ForWho))
                    {
                        result.ForWho = "Student";
                    }
                }
            }


            if (dataReceipment != null)
            {
                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();

                if (KeyValues.ContainsKey("GetDataCourseworkAnecdotalCommentEmail"))
                {
                    KeyValues.Remove("GetDataCourseworkAnecdotalCommentEmail");
                }

                paramTemplateNotification.Add("GetDataCourseworkAnecdotalCommentEmail", dataReceipment);

                foreach (var item in ListRecipients)
                {
                    IEnumerable<string> IdRecipients = new string[] { item.Id };

                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "PAR3")
                        {
                            IdRecipients = ListRecipients.Select(x => x.Id).ToList(),
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                }
            }
        }

        private async Task ProcessEmailAndNotifScenario4(AddReflectionCommentRequest param, string IdReflectionStudentComment)
        {
            var User = await _dbContext.Entity<MsUser>()
            .Select(x => new {
                x.Id,
                x.DisplayName
            }
            )
            .ToListAsync(CancellationToken);

            var Staff = await _dbContext.Entity<MsStaff>()
                .Select(x => new {
                    x.IdBinusian,
                }
                )
                .ToListAsync(CancellationToken);

            var dataReceipment = await _dbContext.Entity<TrReflectionStudentComment>()
                              .Include(x => x.ReflectionStudent).ThenInclude(x => x.Student)
                              .Include(x => x.User)
                              .Where(x => x.Id == IdReflectionStudentComment)
                              .Select(x => new EmailCommentPortofolioResult
                              {
                                  //IdReceive = x.IdUserComment,
                                  //ReceiveName = x.User.DisplayName,
                                  IdAcademicYear = x.ReflectionStudent.IdAcademicYear,
                                  IdStudent = x.ReflectionStudent.IdStudent,
                                  Semester = x.ReflectionStudent.Semester,
                                  UserNameComment = x.User.DisplayName,
                                  IdUserCreate = x.ReflectionStudent.UserIn,
                                  //UserNameCreate =  User.Where(y=> y.Id == x.CourseworkAnecdotalStudent.UserIn).Select(y=>y.DisplayName).FirstOrDefault(),
                                  TabMenu = "Reflection",
                                  TabMenuIdx = "Reflection",
                                  Date = x.DateIn.Value,
                                  SchoolName = x.ReflectionStudent.Student.IdSchool,
                                  Comment = x.Comment
                              })
                              .ToListAsync();

            if (dataReceipment == null)
            {
                return;
            }

            foreach (var item in dataReceipment)
            {
                item.UserNameCreate = User.Where(y => y.Id == item.IdUserCreate).Select(y => y.DisplayName).FirstOrDefault();
            }


            var ListRecipients = dataReceipment.Select(x => new CodeWithIdVm
            {
                Id = x.IdUserCreate,
                Code = x.UserNameCreate
            }).ToList();

            foreach (var result in dataReceipment)
            {
                if (Staff.Any(x => x.IdBinusian == result.IdUserCreate))
                {
                    result.ForWho = "Teacher";
                }
                else
                {
                    if (string.IsNullOrEmpty(result.ForWho))
                    {
                        result.ForWho = "Student";
                    }
                }
            }

            if (dataReceipment != null)
            {
                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();

                if (KeyValues.ContainsKey("GetDataCourseworkAnecdotalCommentEmail"))
                {
                    KeyValues.Remove("GetDataCourseworkAnecdotalCommentEmail");
                }

                paramTemplateNotification.Add("GetDataCourseworkAnecdotalCommentEmail", dataReceipment);

                foreach (var item in ListRecipients)
                {
                    IEnumerable<string> IdRecipients = new string[] { item.Id };

                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "PAR4")
                        {
                            IdRecipients = ListRecipients.Select(x => x.Id).ToList(),
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                }
            }
        }
    }
}
