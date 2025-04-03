namespace BinusSchool.Data.Model.Util.FnNotification.SendGrid
{
    public class GetSendGridTemplateRequest
    {
        public int PageSize { get; set; }
        public string PageToken { get; set; }
        public string Search { get; set; }
    }
}
