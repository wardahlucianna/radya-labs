using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;

namespace BinusSchool.Student.FnStudent.CreateEmailForStudent
{
    public class CheckEmailAvailabilityHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;
        public CheckEmailAvailabilityHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<CheckEmailAvailabilityRequest>();

            #region Check Email Exist or not
            bool available = true;

            var query = _dbContext.Entity<MsStudent>().Where(x => x.BinusianEmailAddress == param.Email).FirstOrDefault();

            if (query != null)
            {
                available = false;
            }
            //Check Azure
            else
            {
                #region Check Azure
                //string UrlCheckEmail = "http://10.200.207.65:8009/api/ActiveDirectory/ValidateEmail";
                //string keyValue = "Bearer 23bd1f2b31c064c4bb71abcba0a59bd4c5bc550bd7ac40e56e982c21aa5c5757";

                //WebRequest req = WebRequest.Create(UrlCheckEmail);
                //req.Headers.Add("Authorization", keyValue);
                //req.ContentType = "application/json";
                //req.Method = "POST";

                //string json = "{\"username\":\"" + param.Email + "\"}";
                //using (var streamwriter = new StreamWriter(req.GetRequestStream()))
                //{
                //    streamwriter.Write(json);
                //}

                //WebResponse wr = req.GetResponse();
                //Stream receiveStream = wr.GetResponseStream();

                //DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Result));
                //Result objResponseBaseonEmail = (Result)serializer.ReadObject(receiveStream);
                //available = objResponseBaseonEmail.Status == "1" ? false : true;

                #endregion
            }
            #endregion

            List<GetStudentEmailRecomendationResult> retVal = new List<GetStudentEmailRecomendationResult>();

            #region isi data
            retVal.Add(new GetStudentEmailRecomendationResult
            {
                EmailRecomendation = param.Email,
                Available = available
            });
            #endregion

            return Request.CreateApiResult2(retVal as object);
        }

        [DataContract]
        private class Result
        {
            [DataMember(Name = "Status")]
            public string Status { get; set; }

            [DataMember(Name = "msg")]
            public string Message { get; set; }
        }
    }
}
