using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer;
using System.Configuration;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;

namespace DataLayer
{
    public class DbAccess
    {
       

        public static string connectionString_client = ConfigurationManager.ConnectionStrings["EmailClientConString"].ConnectionString;
        //get all the received emails for a given email address
        public static List<Mail> SelectNewEmails(string emailAddress)
        {
            List<Mail> lstMail = new List<Mail>();
            try
            {
                SqlConnection conn = new SqlConnection(connectionString_client);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_SelectEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EmailAddress", emailAddress);
                        SqlDataReader Reader = cmd.ExecuteReader();

                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {
                                Mail email = new Mail();
                                email.MailID = Convert.ToInt32(Reader["TemplateID"]);
                                email.Date = Convert.ToDateTime(Reader["CreatedDate"]);
                                email.From = Reader["SenderEmailAddress"].ToString();
                                email.Subject = Reader["Subject"].ToString();
                                email.To = Reader["To"].ToString();
                                email.CC = Reader["Cc"].ToString();
                                email.BCC = Reader["Bcc"].ToString();
                                email.Body = Reader["Body"].ToString();

                                lstMail.Add(email);
                            }
                         }
                        Reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Log exception
                //Display Error message

            }
            //return lstTemplate;
            return lstMail;
        }
        //get all sent emails, draft email or deleted emails
        public static List<Mail> SelectAllSentEmails(string SenderemailAddress,bool isSent, bool isDeleted)
        {
               List<Mail> lstMail = new List<Mail>();
            try
            {
                SqlConnection conn = new SqlConnection(connectionString_client);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_SelectAllSentEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SenderEmailAddress", SenderemailAddress);
                        cmd.Parameters.AddWithValue("@IsSent",isSent);
                        if (isDeleted!=false)
                        {
                            cmd.Parameters.AddWithValue("@IsDeleted", isDeleted);
                        }
                        SqlDataReader Reader = cmd.ExecuteReader();

                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {
                                Mail mail = new Mail ();
                                mail.MailID= Convert.ToInt32(Reader["EmailID"]);
                                mail.Date = Convert.ToDateTime(Reader["Date"]);
                                mail.From = Reader["SenderEmailAddress"].ToString();
                                mail.IsSent = Convert.ToBoolean(Reader["IsSent"]);
                                mail.Subject = Reader["Subject"].ToString();
                                mail.Body = Reader["Body"].ToString();
                                mail.To = Reader["ToEmailAddress"].ToString();
                                mail.CC = Reader["CcEmailAddress"].ToString();
                                mail.BCC = Reader["BccEmailAddress"].ToString();
                                lstMail.Add(mail);
                            }
                        }
                        Reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Log exception
                //Display Error message

            }
            //return lstTemplate;
            return lstMail;
        }
        //get draft emails for editing
        public static Mail SelectIdWiseSentEmails(string SenderemailAddress, int emailId)
        {
            Mail mail = new Mail(); 
            try
            {
                SqlConnection conn = new SqlConnection(connectionString_client);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_SelectIdWiseSentEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        //cmd.Parameters.AddWithValue("@SenderEmailAddress", SenderemailAddress);
                        cmd.Parameters.AddWithValue("@EmailID", emailId);
                        SqlDataReader Reader = cmd.ExecuteReader();

                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {                                
                                mail.MailID = Convert.ToInt32(Reader["EmailID"]);
                                mail.Date = Convert.ToDateTime(Reader["Date"]);
                                mail.From = Reader["SenderEmailAddress"].ToString();
                                mail.IsSent = Convert.ToBoolean(Reader["IsSent"]);
                                mail.Subject = Reader["Subject"].ToString();
                                mail.Body = Reader["Body"].ToString();
                                mail.To = Reader["ToEmailAddress"].ToString();
                                mail.CC = Reader["CcEmailAddress"].ToString();
                                mail.BCC = Reader["BccEmailAddress"].ToString();
                             
                            }
                        }
                        Reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Log exception
                //Display Error message

            }
            //return lstTemplate;
            return mail;
        }
        // save draft email to client db table
        public static string SaveDraftEmail(Mail email)
        {
            string Message = string.Empty;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString_client);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_InsertUpdateSentEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        if (email.MailID != 0)
                        {
                            cmd.Parameters.AddWithValue("@EmailID", email.MailID);
                        }
                        cmd.Parameters.AddWithValue("@SenderEmailAddress", email.From);
                        cmd.Parameters.AddWithValue("@IsSent",email.IsSent);
                        cmd.Parameters.AddWithValue("@Subject", email.Subject);
                        cmd.Parameters.AddWithValue("@Body", email.Body);
                        cmd.Parameters.AddWithValue("@ToEmailAddress", email.To);
                        cmd.Parameters.AddWithValue("@CcEmailAddress", email.CC);
                        cmd.Parameters.AddWithValue("@BccEmailAddress", email.BCC);
                                                
                        SqlParameter message = cmd.Parameters.Add("@Message", System.Data.SqlDbType.NVarChar, 100);
                        message.Direction = System.Data.ParameterDirection.Output;

                        cmd.ExecuteNonQuery();
                        Message = cmd.Parameters["@Message"].Value.ToString();

                        
                    }
                }
            }
            catch (SqlException ex)
            {
                //Log exception
                //Display Error message
                
            }
            return Message;
        }
        //insert new emails from mail servert to client db table
        public static bool InsertEmail(XmlDocument xmlDoc)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString_client);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_InsertEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        
                        cmd.Parameters.AddWithValue("@xmlReceiver", xmlDoc.InnerXml);
                        
                        SqlParameter message = cmd.Parameters.Add("@Message", System.Data.SqlDbType.NVarChar, 100);
                        message.Direction = System.Data.ParameterDirection.Output;

                        cmd.ExecuteNonQuery();
                        string Message = cmd.Parameters["@Message"].Value.ToString();

                        if (Message == "Success")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                //Log exception
                //Display Error message
                return false;
            }
        }
        //get available email addresses from db
        public static AutoCompleteStringCollection availableEmails()
        {
            AutoCompleteStringCollection lstEmail = new AutoCompleteStringCollection();
            try
            {
                SqlConnection conn = new SqlConnection(connectionString_client);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT[EmailAddress] FROM [dbo].[tblAlias]", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        SqlDataReader Reader = cmd.ExecuteReader();

                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {
                                string email = Reader["EmailAddress"].ToString();
                                lstEmail.Add(email);
                            }
                        }
                        Reader.Close();
                    }
                }
            }

            catch (SqlException ex)
            {
                //Log exception
                //Display Error message

            }
            return lstEmail;
        }
        //delete send and draft emails
        public static string DeleteEmail(int emailID, string senderEmailAddress)
        {
            string cmdText = "";
            if (emailID!=0)
            {
                cmdText= "update[dbo].[tblSentEmailDetails] set [IsDelete] = 1 where EmailID = '"+emailID+"'";
            }
            else
            {
                cmdText = "Delete from [dbo].[tblSentEmailDetails] where [IsDelete] = 1 AND [SenderEmailAddress] = '" + senderEmailAddress.Trim()+"'";
            }
            string Message = string.Empty;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString_client);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.ExecuteNonQuery();
                        Message = "Success";
                    }
                }
            }

            catch (SqlException ex)
            {
                //Log exception
                //Display Error message
                Message = "Failed";
            }
            return Message;
            
        }
    }
}
