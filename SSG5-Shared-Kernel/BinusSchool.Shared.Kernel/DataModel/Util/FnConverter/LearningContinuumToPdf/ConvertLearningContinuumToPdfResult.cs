using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Util.FnConverter.LearningContinuumToPdf
{
    public class ConvertLearningContinuumToPdfResult
    {

    }

    public class ExportLearningContinuumToPdfResult_Data
    {
        public ItemValueVm NextStudent { get; set; }
        public ItemValueVm PrevStudent { get; set; }
        public DateTime? LastSavedDate { get; set; }
        public string LastSavedBy { get; set; }
        public List<string> IdLearningContinuumList { get; set; }
    }
}
