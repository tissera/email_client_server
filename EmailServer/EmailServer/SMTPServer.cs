using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using ModelLayer;
using DataLayer;

namespace EmailServer
{
    class SMTPServer
    {        
        TcpClient _client;
        NetworkStream _stream;
        StreamReader _reader;
        StreamWriter _writer;

        string user;
        string pass;
        public SMTPServer(TcpClient client)
        {
            _client = client;
            _client.ReceiveTimeout = 15000;
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream);
            _writer.NewLine = "\r\n";
            _writer.AutoFlush = true;
        }
          
        private void Write(String strMessage)
        {
            NetworkStream clientStream = _client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(strMessage + "\r\n");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }
        private String Read()
        {
            byte[] messageBytes = new byte[8192];
            int bytesRead = 0;
            NetworkStream clientStream = _client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();

            bytesRead = clientStream.Read(messageBytes, 0, 8192);
            string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
            return strMessage;
        }

        public void Run()
        {
            Template emailTemplate = new Template();
            List<Receiver> lstEmailReciver = new List<Receiver>();
            List<Receiver> lstEmailReciver1 = new List<Receiver>();
            Write("220 localhost -- Email server");
            string strMessage = String.Empty;
            while (true)
            {
                try
                {
                    strMessage = Read();
                }
                catch (Exception e)
                {
                    //A socket error has occured
                    Console.Error.WriteLine(e.Message);
                    break;
                }
                if (strMessage.Length > 0)
                {
                    #region POP3
                    if (strMessage.ToUpper().StartsWith("USER"))
                    {
                        //Remove carriage returns [\r\n] from the receiving string and get the user name.
                        var str = strMessage.Replace("\n", "").Replace("\r", "");
                        string [] arrUser = str.Split(' ');
                        user = arrUser[1];
                        Write("+OK");
                    }
                    if (strMessage.ToUpper().StartsWith("PASS"))
                    {
                        //Remove carriage returns [\r\n] from the receiving string nd get eh password,
                        var str = strMessage.Replace("\n", "").Replace("\r", "");
                        string[] arrPass = str.Split(' ');
                        pass = arrPass[1];
                        //validate user with db record
                        User u = DbAccess.UserValidation(user, pass);
                        if (u.EmailAddress != null)
                        {
                            Write("+OK,"+u.EmailAddress);
                        }
                        else
                        {
                            Write("550 UserName and/or Password incorrect.");
                        }
                    }
                    if (strMessage.ToUpper().StartsWith("RETR"))
                    {
                        //Retrieves all the new message for the user
                        List<Mail> lstNewEmails= DbAccess.SelectNewEmails();
                        if (lstNewEmails.Count() > 0)
                        {
                            StringBuilder objstring = new StringBuilder();
                            objstring.Append("<root>");
                            foreach (Mail newEmail in lstNewEmails)
                            {
                                objstring.Append("<mail>");

                                objstring.Append("<MailID>");
                                objstring.Append(newEmail.MailID);
                                objstring.Append("</MailID>");
                                objstring.Append("<Date>");
                                objstring.Append(newEmail.Date);
                                objstring.Append("</Date>");
                                objstring.Append("<From>");
                                objstring.Append(newEmail.From);
                                objstring.Append("</From>");
                                objstring.Append("<Subject>");
                                objstring.Append(newEmail.Subject);
                                objstring.Append("</Subject>");
                                objstring.Append("<To>");
                                objstring.Append(newEmail.To);
                                objstring.Append("</To>");
                                objstring.Append("<CC>");
                                objstring.Append(newEmail.CC);
                                objstring.Append("</CC>");
                                objstring.Append("<BCC>");
                                objstring.Append(newEmail.BCC);
                                objstring.Append("</BCC>");
                                objstring.Append("<Body>");
                                objstring.Append(newEmail.Body);
                                objstring.Append("</Body>");

                                objstring.Append("</mail>");
                            }
                            objstring.Append("</root>");
                            Write(objstring.ToString());
                            
                        }
                        else
                        {
                            Write(".");
                        }                       
                    }
                    #endregion
                    if (strMessage.StartsWith("QUIT"))
                    {
                        Write("221 OK");
                        _client.Close();
                        break;//exit while
                    }
                    #region SMTP
                    if (strMessage.StartsWith("EHLO"))
                    {
                        Write("250 OK");
                    }
                    if (strMessage.StartsWith("RCPT TO"))
                    {
                        string[] rcpt = strMessage.Split('<');
                        string[] to = rcpt[1].Split('>');

                        Receiver emailReceiver = new Receiver();
                        emailReceiver.Type = Receivertype.To.ToString();
                        emailReceiver.EmailAddress = to[0];
                        lstEmailReciver1.Add(emailReceiver);
                        Write("250 OK");
                    }
                    if (strMessage.StartsWith("MAIL FROM"))
                    {
                        string[] from = strMessage.Split('<');
                        string[] fromEmail = from[1].Split('>');
                        emailTemplate.SenderEmailAddrss = fromEmail[0];
                        Write("250 OK");
                    }
                    if (strMessage.StartsWith("DATA"))
                    {
                        Write("354 Start input, end data with <CRLF>.<CRLF>");
                        Thread.Sleep(1000);
                        strMessage = Read();
                        string[] fullBody = strMessage.Split('\n');

                        foreach (string s in fullBody)
                        {
                            string[] f = s.Split('\r');

                            if (f[0].StartsWith("From:"))
                            {
                                string[] from = f[0].ToString().Split(':');
                                emailTemplate.SenderEmailAddrss = from[1].Trim();
                            }
                            if (f[0].StartsWith("To:"))
                            {
                                string[] lstTo = f[0].ToString().Split(':');
                                string[] To = lstTo[1].Split(',');
                                foreach (string email in To)
                                {
                                    Receiver emailReceiver = new Receiver();
                                    emailReceiver.Type = Receivertype.To.ToString();
                                    emailReceiver.EmailAddress = email.Trim();
                                    lstEmailReciver.Add(emailReceiver);
                                }
                            }
                            if (f[0].StartsWith("Cc:"))
                            {
                                string[] lstCc = f[0].ToString().Split(':');
                                string[] Cc = lstCc[1].Split(',');
                                foreach (string email in Cc)
                                {
                                    Receiver emailReceiver = new Receiver();
                                    emailReceiver.Type = Receivertype.CC.ToString();
                                    emailReceiver.EmailAddress = email.Trim();
                                    lstEmailReciver.Add(emailReceiver);
                                }
                            }
                            if (f[0].StartsWith("Date:"))
                            {
                                string[] lstDate = f[0].ToString().Split(':');
                                string mailDate = f[0].Substring(6);
                                emailTemplate.CreatedDate = Convert.ToDateTime(mailDate);
                            }
                            if (f[0].StartsWith("Subject:"))
                            {
                                string[] lstSubject = f[0].ToString().Split(':');
                                emailTemplate.Subject = lstSubject[1];
                            }
                        }
                        //index of the blank line
                        int indexOfBlank = strMessage.IndexOf("\r\n\r\n");
                        string bodyString = strMessage.Substring(indexOfBlank + 4);
                        //remove body ending character (\r\n\r\n.\r\n)
                        string bodyText = bodyString.Replace("\r\n\r\n.\r\n", "");
                        
                        emailTemplate.Body = bodyText.Replace("=0A","\r\n").Trim();
                        //to idendify Bcc sending email addresses
                        var Bcc = from msg in lstEmailReciver1
                                  where !lstEmailReciver.Any(x => x.EmailAddress.Trim() == msg.EmailAddress.Trim())
                                  select msg;
                        foreach (var B in Bcc)
                        {
                            Receiver emailReceiver = new Receiver();
                            emailReceiver.Type = Receivertype.BCC.ToString();
                            emailReceiver.EmailAddress = B.EmailAddress;
                            lstEmailReciver.Add(emailReceiver);
                        }
                        //remove duplicate email address
                        List<Receiver> lstresultEmailReciver = new List<Receiver>();
                        for (int i = 0; i < lstEmailReciver.Count; i++)
                        {
                            // Assume not duplicate.
                            bool duplicate = false;
                            for (int z = 0; z < i; z++)
                            {
                                if (lstEmailReciver[z].EmailAddress == lstEmailReciver[i].EmailAddress)
                                {
                                    // This is a duplicate.
                                    duplicate = true;
                                    break;
                                }
                            }
                            // If not duplicate, add to result.
                            if (!duplicate)
                            {
                                lstresultEmailReciver.Add(lstEmailReciver[i]);
                            }
                        }///////////
                       
                        emailTemplate.lstReceiver = lstresultEmailReciver;
                        Write("250 OK");
                        //write to db
                        try
                        {
                            bool success = DbAccess.InsertEmail(emailTemplate);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine(e.Message);
                        }
                    } 
                    #endregion
                }

            }
        }
    }
}
