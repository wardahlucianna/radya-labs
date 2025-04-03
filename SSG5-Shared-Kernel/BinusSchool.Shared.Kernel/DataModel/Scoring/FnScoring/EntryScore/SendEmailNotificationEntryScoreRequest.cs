using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class SendEmailNotificationEntryScoreRequest
    {
        public List<SendEmailNotificationEntryScoreRequest_Score> Scores { get; set; }
    }

    public class SendEmailNotificationEntryScoreRequest_Score
    {
        public string IdScore { get; set; }
        public string UserIn { get; set; }
        public string DateIn { get; set; }
        public string IdSubComponentCounter { get; set; }
        public string IdStudent { get; set; }
        public decimal MaxRawScore { get; set; }
        public decimal RawScore { get; set; }
        public decimal Score { get; set; }
        public string TextScore { get; set; }
    }
}
