using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   public class Receiver
    {
        public int ReceiverID { get; set; }
        public int TemplateID { get; set; }
        public string Type { get; set; }
        public string EmailAddress { get; set; }
        public DateTime SendDate { get; set; }
    }
    public enum Receivertype { CC, BCC, To };
}
