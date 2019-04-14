using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{

    public class Mail
    {
        public int MailID { get; set; }
        public DateTime Date { get; set; }
        public string From { get; set; }
        public bool IsSent { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Body { get; set; }
    }
}
