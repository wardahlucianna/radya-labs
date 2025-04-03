using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Constants;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using NPOI.SS.Formula.Functions;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using Microsoft.Azure.WebJobs;
using NPOI.POIFS.FileSystem;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using NPOI.Util;
using NPOI.HPSF;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class DeleteLessonPlanBlobHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly string _containerPath;
        private readonly string _documentStorageConnection;

        public DeleteLessonPlanBlobHandler(ITeachingDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;

            _containerPath = "lesson-plan-download";
#if DEBUG
            _documentStorageConnection = configuration["ConnectionStrings:Teaching:AccountStorageDocument"];
            //_documentStorageConnection = "UseDevelopmentStorage=true";
#else 
            _documentStorageConnection = configuration["ConnectionStrings:Teaching:AccountStorageDocument"];
#endif
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteLessonPlanBlobRequest, DeleteLessonPlanBlobValidator>();

            try
            {
                var storageAccount = CloudStorageAccount.Parse(_documentStorageConnection);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(_containerPath);

                var blob = blobContainer.GetBlockBlobReference(body.FileName);
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            return Request.CreateApiResult2();
        }
    }
}
