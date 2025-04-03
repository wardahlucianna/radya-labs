using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.LearningContinuumToPdf;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface ILearningContinuumToPdf : IFnConverter
    {
        [Post("/learning-continuum-to-pdf")]
        Task<HttpResponseMessage> ConvertLearningContinuumToPdf([Body] ConvertLearningContinuumToPdfRequest body);
    }
}
