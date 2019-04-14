using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer;
using System.Configuration;
using System.Data.SqlClient;
using System.Xml;

namespace DataLayer
{
    public class DbAccess
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["EmailServerConString"].ConnectionString;
        
        public static void DBAccess()
        {

        }
        //add new user
        public static bool InsertUser(User newUser)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO tblUserDetails VALUES(@UserName,@Password, @EmailAddress,@isActive)", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", newUser.UserName);
                        cmd.Parameters.AddWithValue("@Password", newUser.Password);
                        cmd.Parameters.AddWithValue("@EmailAddress", newUser.EmailAddress);
                        cmd.Parameters.AddWithValue("@isActive", "1");

                        int rows = cmd.ExecuteNonQuery();
                        //number of record got inserted
                        if (rows > 0)
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
        //check if the provided username and password is correct
        public static User UserValidation(string userName, string password)
        {
            User temp = new User();
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT [EmailAddress] FROM [dbo].[tblUserDetails] WHERE [UserName]=@userName AND [Password]=@password", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@userName", userName.Trim());
                        cmd.Parameters.AddWithValue("@password", password.Trim());
                        SqlDataReader Reader = cmd.ExecuteReader();

                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {
                                temp.UserName = userName.Trim();
                                temp.Password = password.Trim();
                                temp.EmailAddress = Reader["EmailAddress"].ToString();   
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

            return temp;
        }
        //insert new emails to table
        public static bool InsertEmail(Template email)
        {            
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_InsertEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SenderEmailAddress", email.SenderEmailAddrss);
                        cmd.Parameters.AddWithValue("@CreatedDate", email.CreatedDate);
                        cmd.Parameters.AddWithValue("@Subject", email.Subject);
                        cmd.Parameters.AddWithValue("@Body", email.Body);
                                                
                        string to = "";
                        string cc = "";
                        string bcc = "";
                        ////
                        foreach (Receiver r in email.lstReceiver)
                        {
                            //////
                            if (r.Type == Receivertype.To.ToString())
                            {
                                to = to + ',' + r.EmailAddress;
                            }
                            if (r.Type == Receivertype.CC.ToString())
                            {
                                cc=cc+','+r.EmailAddress;
                            }
                            if (r.Type == Receivertype.BCC.ToString())
                            {
                                bcc=bcc+','+r.EmailAddress;
                            }
                           
                        }
                        //////////
                        if (to.Length > 0)
                        {
                            to = to.Substring(1);
                        }
                        if (cc.Length > 0)
                        {
                            cc = cc.Substring(1);
                        }
                        if (bcc.Length > 0)
                        {
                            bcc = bcc.Substring(1);
                        }
                        
                        cmd.Parameters.AddWithValue("@ToEmailAddress", to);
                        cmd.Parameters.AddWithValue("@CcEmailAddress", cc);
                        cmd.Parameters.AddWithValue("@BccEmailAddress", bcc);

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
        //select new email to send Client 
        public static List<Mail> SelectNewEmails()
        {
            List<Mail> lstMail = new List<Mail>();
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                using (conn)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Sp_SelectEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        
                        SqlDataReader Reader = cmd.ExecuteReader();
                        
                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {
                                Mail mail = new Mail();
                                mail.MailID = Convert.ToInt32(Reader["TemplateID"]);
                                mail.Date = Convert.ToDateTime(Reader["CreatedDate"]);
                                mail.From = Reader["SenderEmailAddress"].ToString();
                                
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
            // return lstTemplate;
            return lstMail;
        }
    }
}
