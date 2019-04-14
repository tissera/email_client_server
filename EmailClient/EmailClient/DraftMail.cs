using System;
using System.Windows.Forms;
using ModelLayer;
using DataLayer;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text;

namespace EmailClient
{
    public partial class DraftMail : Form
    {
        User RegUser = new User();
        public DraftMail(Mail m,User u)
        {
            InitializeComponent();
            RegUser = u;
            this.txtFrom.Text = u.EmailAddress;
            this.txtTo.Text = m.From;
            this.txtCc.Text = m.CC;
            this.txtSubject.Text = m.Subject;
            this.rtbBody.Text = m.Body;
            this.lblMailId.Text = m.MailID.ToString();
        }
       
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            
        }
        //validate email address
        //bool IsValidEmail(string email)
        //{
        //    try
        //    {
        //        var addr = new System.Net.Mail.MailAddress(email);
        //        return addr.Address == email;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        //remove special characers
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] >= '0' && str[i] <= '9')
                    || (str[i] >= 'A' && str[i] <= 'z'
                        || (str[i] == ' ' || str[i] == '@' || str[i] == ',' || str[i] == '\r' || str[i] == '\n' || str[i] == ':' || str[i] == '-' || str[i] == '.' || str[i] == '_')))
                {
                    sb.Append(str[i]);
                }
            }

            return sb.ToString();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            //validate email address
             if ((txtTo.Text.Trim() != "") || (txtCc.Text.Trim() != "") || (txtSubject.Text.Trim() != "") || (rtbBody.Text.Trim() != ""))
            {// front end validation

                string subject = "";
                string body = "";
                //remove special char from body and subject text
                if (txtSubject.Text.Trim() !="")
                {
                    subject = RemoveSpecialCharacters(txtSubject.Text.Trim());
                }
                if (rtbBody.Text.Trim()!="")
                {
                    body = RemoveSpecialCharacters(rtbBody.Text.Trim());
                }
                Mail m = new Mail();
                m.MailID = Convert.ToInt32(lblMailId.Text.ToString());
                m.From = RegUser.EmailAddress.Trim();
                m.IsSent = false;
                m.Subject = subject;
                m.Body = body;
                m.To = txtTo.Text.Trim();
                m.CC = txtCc.Text.Trim();
                m.BCC = "";
                string message = DbAccess.SaveDraftEmail(m);
                if (message == "Success")
                {
                    MessageBox.Show("Saved Successfully.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Unable to Save.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Blank Emails Will not Be Saved.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {   //input validation
            try {
                if ((txtTo.Text.Trim() != "") && (txtSubject.Text.Trim() != "") && (rtbBody.Text.Trim() != ""))
                {
                    string subject = "";
                    string body = "";
                    //remove special char from body and subject text
                    subject = RemoveSpecialCharacters(txtSubject.Text.Trim());
                    body = RemoveSpecialCharacters(rtbBody.Text.Trim());

                    MailMessage m = new MailMessage(txtFrom.Text, txtTo.Text, subject, body);
                    if (txtCc.Text.Trim() != "") 
                    {
                        string[] lstCC = txtCc.Text.Trim().Split(',');
                        foreach (string c in lstCC)
                        {
                            MailAddress cc = new MailAddress(c);
                            m.CC.Add(cc);
                        }
                    }
                    m.IsBodyHtml = false;
                    //Save to db
                    Mail mail = new Mail();
                    mail.MailID = Convert.ToInt32(lblMailId.Text.Trim());
                    mail.From = txtFrom.Text.Trim();
                    mail.To = txtTo.Text.Trim();
                    mail.CC = txtCc.Text.Trim();
                    mail.BCC = "";
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsSent = true;
                    string saveMessage = DbAccess.SaveDraftEmail(mail);
                    if (saveMessage == "Success")
                    {
                        //Send to SMTP server
                        string message = Send(m);
                        MessageBox.Show(message, "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Email Saving Failed.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (txtTo.Text.Trim() == "")
                    {
                        MessageBox.Show("To Email Address requird.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtTo.Focus();
                    }
                    else if (txtSubject.Text.Trim() == "")
                    {
                        MessageBox.Show("Email Subject requird.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtSubject.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Email Body text requird.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        rtbBody.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }

        //Send emails to Server
        private string Send(MailMessage m)
        {
            
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "localhost";
            try
            {
                client.Send(m);
                return "Email Send Successfully!";
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception caught in CreateMessageWithAttachment(): {0}",ex.ToString());
                return ex.Message;
            }
            finally { client.Dispose(); }
           
        }
        
        private void DraftMail_Load(object sender, EventArgs e)
        {

        }

        private void txtCc_TextChanged(object sender, EventArgs e)
        {
            txtCc.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtCc.AutoCompleteSource = AutoCompleteSource.CustomSource;

            AutoCompleteStringCollection emails = DbAccess.availableEmails();
            txtCc.AutoCompleteCustomSource = emails;
        }

        private void txtTo_TextChanged(object sender, EventArgs e)
        {
            txtTo.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtTo.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection emails = DbAccess.availableEmails();
            txtTo.AutoCompleteCustomSource = emails;
        }
    }
}
