using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Document.FnDocument.Document
{
    public class UpdateDocumentRequest
    {
        public UpdateDocumentRequest()
        {
            HistoryChangeFieldNote = new List<HistoryChangeFieldNote>();
        }
        public string Id { get; set; }
        public string IdFormBuilderTemplate { get; set; }
        public string JsonDocumentValue { get; set; }
        public string JsonFormElement { get; set; }
        public string IdFormDocumentRefence { get; set; }
        public bool IsSaveAsDraft { get; set; }
        public IEnumerable<HistoryChangeFieldNote> HistoryChangeFieldNote { get; set; }
    }

    public class HistoryChangeFieldNote
    {
        public string FieldName { get; set; }
        public string Note { get; set; }
    }
}
