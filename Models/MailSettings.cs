namespace OpenSourceHome.Models
{
    public class MailSettings
    {
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string LocalDomain { get; set; }
        public string MailServerAddress { get; set; }
        public string MailServerPort { get; set; }
        public string UserPassword { get; set; }
    }
}