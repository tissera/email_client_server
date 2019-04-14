using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModelLayer;
using DataLayer;
using System.Threading;
using System.Timers;
using System.Net.Sockets;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Configuration;

namespace EmailClient
{
    public partial class InBox : Form
    {
        //read interval time from config
       static double interval = Convert.ToDouble(ConfigurationManager.AppSettings["interval"]);
            
        static TcpClient _client;
        static NetworkStream _stream;
        static StreamReader _reader;
        static StreamWriter _writer;
        static System.Timers.Timer _timer;
        private static System.ComponentModel.BackgroundWorker _worker;

        List<Mail> lstEmails = new List<Mail>();
        string userName;
      static  User u;
        public InBox(User LogedUser)
        {
            InitializeComponent();
            u = LogedUser;
            this.WindowState = FormWindowState.Maximized;
            lblEmailAddress.Text = LogedUser.EmailAddress;
            userName = LogedUser.UserName;
            //start timer
            Start();
        }
        static void Start()
        {
            //_timer = new System.Timers.Timer(900000);//15 mins
            if (interval > 300000)
            {
                _timer = new System.Timers.Timer(interval);
            }
            else
            {
                _timer = new System.Timers.Timer(300000);//5 mins
            }

                _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true;
            
        }
        static void _timer_Elapsed(object sender,ElapsedEventArgs e)
        {            
            _worker = new System.ComponentModel.BackgroundWorker();
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            _worker.WorkerSupportsCancellation =true;
            if (!_worker.IsBusy)
            {
                _worker.RunWorkerAsync();
            }
        }
        private static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //loged in user
            User RegUser = u;
            /////////////
            
            try
            {
                _client = new TcpClient("localhost", 25);
                _client.ReceiveTimeout = 15000;
                _stream = _client.GetStream();
                _reader = new StreamReader(_stream);
                _writer = new StreamWriter(_stream);
                _writer.NewLine = "\r\n";
                _writer.AutoFlush = true;
            }
            catch (Exception e1)
            {
            }
            //get emails from pop3 server and save in local db
            string strMessage = String.Empty;
            while (true)
            {
                try
                {
                    strMessage = Read();
            
                    if (strMessage.Length > 0)
                    {
                        if (strMessage.ToUpper().StartsWith("220"))
                        {
                            // Login Process
                            #region MyRegion
                            Write("USER " + RegUser.UserName.Trim());
                            strMessage = Read();
                           
                            if (strMessage.ToUpper().StartsWith("+OK"))
                            {
                                Write("PASS " + RegUser.Password.Trim());
                                strMessage = Read();
                                if (strMessage.ToUpper().StartsWith("550"))
                                {
                                    //login failed
                                    Write("QUIT");
                                    strMessage = Read();
                                    _client.Close();
                                    _stream.Close();
                                    break;
                                }
                                else
                                {
                                    //login is successfull
                                   
                                    Write("RETR");
                                    strMessage = Read();
                                    if (strMessage.ToUpper().StartsWith("."))
                                    {
                                        //No new emails
                                        // Send QUIT command to close session from POP server
                                        Write("QUIT");
                                    }
                                    else
                                    {
                                        //convert to xml
                                        XmlDocument xmlDoc = new XmlDocument();
                                        xmlDoc.LoadXml(strMessage.ToString());

                                        // Send QUIT command to close session from POP server
                                        Write("QUIT");
                                        //insert to db
                                        bool message1 = DbAccess.InsertEmail(xmlDoc);
                                        
                                    }
                                }
                            }
                            #endregion
                        }
                        if (strMessage.ToUpper().StartsWith("221"))
                        {
                            _client.Close();
                            _stream.Close();

                            break;
                        }
                    }
                }
                catch (Exception e2)
                {
                    
                }
            }
            ////////////////
            Thread.Sleep(5000);
            
        }

        private static void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
                        
        }

        private static void Write(String strMessage)
        {
            NetworkStream clientStream = _client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(strMessage + "\r\n");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }
        private static String Read()
        {
            byte[] messageBytes = new byte[8192];
            int bytesRead = 0;
            NetworkStream clientStream = _client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();

            bytesRead = clientStream.Read(messageBytes, 0, 8192);
            string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
            return strMessage;
        }

         //Bind Emails received
        private void BindListView(string type)
        {
            cbCategory.SelectedItem = null;
            txtSearchText.Text = "";

            txtFrom.Visible = true;
            txtTo.Visible = true;
            txtCc.Visible = true;
            txtSubject.Visible = true;
            rtbBody.Visible = true;

            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;

            txtFrom.Text = "";
            txtTo.Text = "";
            txtCc.Text = "";
            txtSubject.Text = "";
            rtbBody.Text = "";
            lblMailId.Text = "";

            btnReply.Enabled = false;
            btnForward.Enabled = false;
            btnDelete.Enabled = false;
            if (type == "inbox")
            {
                btnDelete.Visible = false;
                lstEmails = DbAccess.SelectNewEmails(u.EmailAddress);
            }
            else if (type == "sent")
            {
                lstEmails = DbAccess.SelectAllSentEmails(u.EmailAddress, true, false);
            }
            else if (type == "draft")
            {
                lstEmails = DbAccess.SelectAllSentEmails(u.EmailAddress, false, false);
            }
            else if (type == "deleted")
            {                
                lstEmails = DbAccess.SelectAllSentEmails(u.EmailAddress,false,true);
                if (lstEmails.Count() > 0)
                {
                    btnDelete.Enabled = true;
                }
            }      
            //gridlist view will have mails order by date created.
            gridListView.DataSource = lstEmails.OrderByDescending(x=>x.Date).ToList();
            
            this.gridListView.Columns["MailID"].Visible = false;
            this.gridListView.Columns["IsSent"].Visible = false;
            this.gridListView.Columns["To"].Visible = false;
            this.gridListView.Columns["CC"].Visible = false;
            this.gridListView.Columns["BCC"].Visible = false;
            this.gridListView.Columns["Body"].Width = 650;
        }
        private void InBox_Load(object sender, EventArgs e)
        {
            btnDeleted.Font = new Font(btnDeleted.Font.Name, btnDeleted.Font.Size, FontStyle.Regular);
            BindListView("inbox");
        }
        //Bind grid list view selected row details to controls
        private void gridListView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.gridListView.Rows[e.RowIndex];
                txtFrom.Text = row.Cells["From"].Value.ToString();
                if (row.Cells["To"].Value != null)
                {
                    txtTo.Text = row.Cells["To"].Value.ToString();
                }
                if (row.Cells["Cc"].Value!=null)
                {
                    txtCc.Text = row.Cells["Cc"].Value.ToString();
                }
                if (row.Cells["Subject"].Value!=null)
                {
                    txtSubject.Text = row.Cells["Subject"].Value.ToString();
                }
                if (row.Cells["Body"].Value != null)
                {
                    rtbBody.Text = row.Cells["Body"].Value.ToString();
                }
                lblMailId.Text = row.Cells["MailID"].Value.ToString();
                btnReply.Enabled = true;
                btnForward.Enabled = true;
                btnDelete.Enabled = true;
            }
        }
        //button inbox click event
        private void btnInbox_Click(object sender, EventArgs e)
        {
            btnDeleted.Font = new Font(btnDeleted.Font.Name, btnDeleted.Font.Size, FontStyle.Regular);
            BindListView("inbox");
            btnDelete.Text = "Delete";
            btnReply.Text = "Reply";
            btnReply.Image = Properties.Resources.reply;
            btnReply.Visible = true;
            btnForward.Visible = true;
            btnDelete.Visible = false;
        }

        private void btnReply_Click(object sender, EventArgs e)
        {
            Mail m = new Mail();
            // m.MailID = Convert.ToInt32(lblMailId.Text);
            m.MailID = 0;
            m.From = txtFrom.Text;
            
            if (btnReply.Text == "Edit")
            {
                m.MailID = Convert.ToInt32(lblMailId.Text);
                m.Subject = txtSubject.Text;
                m.Body = rtbBody.Text;
                m.From = "";
            }
            else
            {
                m.To = txtTo.Text;
                m.CC = txtCc.Text;
                m.Subject = "Re:"+txtSubject.Text;
                StringBuilder s = new StringBuilder();
                s.Append("\r\n");
                s.Append("\r\n");
                s.Append("\r\n");
                s.Append("--------------------------------------------------\r\n");
                s.Append("From: "+txtFrom.Text+"\r\n");
                s.Append("To: " + txtTo.Text + "\r\n");
                s.Append("Cc: " + txtCc.Text + "\r\n");
                s.Append("Subject: " + txtSubject.Text + "\r\n");
                s.Append(rtbBody.Text);
                m.Body = s.ToString();
            }
           DraftMail formDraft = new DraftMail(m,u);
           formDraft.ShowDialog();
        }

        private void btnDraft_Click(object sender, EventArgs e)
        {
            btnDeleted.Font = new Font(btnDeleted.Font.Name, btnDeleted.Font.Size, FontStyle.Regular);
            BindListView("draft");
            btnDelete.Text = "Delete";
            btnReply.Text = "Edit";
            btnReply.Image = Properties.Resources.edit;
            btnReply.Visible = true;
            btnForward.Visible = false;
            btnDelete.Visible = true;
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            btnDeleted.Font = new Font(btnDeleted.Font.Name, btnDeleted.Font.Size, FontStyle.Regular);
            CreateMail formCreatMail = new CreateMail(u);
            formCreatMail.ShowDialog();
        }

        private void btnSent_Click(object sender, EventArgs e)
        {
            btnDeleted.Font = new Font(btnDeleted.Font.Name, btnDeleted.Font.Size, FontStyle.Regular);
            BindListView("sent");
            btnDelete.Text = "Delete";
            btnReply.Text = "Reply";
            btnReply.Image = Properties.Resources.reply;
            btnReply.Visible = true;
            btnForward.Visible = true;
            btnDelete.Visible = true;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            
            string keyWord = txtSearchText.Text.Trim().ToUpper();
            if (cbCategory.SelectedItem!=null && keyWord != "")
            {
                string colName = cbCategory.SelectedItem.ToString();
                if (colName == "From")
                {
                    var lstEmailsFilted = lstEmails.Where(x => x.From.Trim().ToUpper().StartsWith(keyWord)).OrderByDescending(x=> x.Date).ToList();
                    gridListView.DataSource = lstEmailsFilted;
                }
                else if (colName == "Subject")
                {
                    var lstEmailsFilted = lstEmails.Where(x => x.Subject.Trim().ToUpper().StartsWith(keyWord)).OrderByDescending(x=>x.Date).ToList();
                    gridListView.DataSource = lstEmailsFilted;
                }
                else
                {
                    gridListView.DataSource = lstEmails;
                    MessageBox.Show("Please select a valid column to search", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else
            {
                if (cbCategory.SelectedItem == null)
                {
                    gridListView.DataSource = lstEmails;
                    MessageBox.Show("Please select a valid column to search", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    cbCategory.Focus();
                }
                else if(keyWord == "")
                {
                    gridListView.DataSource = lstEmails;
                    MessageBox.Show("Please select a valid keyword to search", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    txtSearchText.Focus();
                }
            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            Mail m = new Mail();
            //  m.MailID = Convert.ToInt32(lblMailId.Text);
            m.MailID = 0;
            m.From = "";
            m.To = "";
            m.CC = "";
            //if (btnReply.Text == "Edit")
            //{
            //    m.Subject = txtSubject.Text;
            //    m.Body = rtbBody.Text;
            //}
            //else
            //{
                m.Subject = "Fwd:" + txtSubject.Text;
                StringBuilder s = new StringBuilder();
                s.Append("\r\n");
                s.Append("\r\n");
                s.Append("\r\n");
                s.Append("-----------------Forwarded message-----------------\r\n");
                s.Append("From: " + txtFrom.Text + "\r\n");
                s.Append("To: " + txtTo.Text + "\r\n");
                s.Append("Cc: " + txtCc.Text + "\r\n");
                s.Append("Subject: " + txtSubject.Text + "\r\n");
                s.Append(rtbBody.Text);
                m.Body = s.ToString();
            //}
            DraftMail formDraft = new DraftMail(m, u);
            formDraft.ShowDialog();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string message = "";
            if (btnDelete.Text == "Delete All")//permanant delete
            {
                message = DbAccess.DeleteEmail(0, u.EmailAddress);
                if (message == "Success")
                {
                    btnDelete.Enabled = false;
                    MessageBox.Show("All Emails Deleted.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Information);


                    btnDeleted.PerformClick();
                }
                else
                {
                    MessageBox.Show("Unable to Delete the Email.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                //delete marked
                message = DbAccess.DeleteEmail(Convert.ToInt32(lblMailId.Text.Trim()), u.EmailAddress);
                if (message == "Success")
                {
                    txtFrom.Text = "";
                    txtTo.Text = "";
                    txtCc.Text = "";
                    txtSubject.Text = "";
                    rtbBody.Text = "";
                    lblMailId.Text = "";

                    btnReply.Enabled = false;
                    btnForward.Enabled = false;
                    btnDelete.Enabled = false;
                    MessageBox.Show("The Selected Email Deleted.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Information);


                    btnDeleted.PerformClick();
                }
                else
                {
                    MessageBox.Show("Unable to Delete the Email.", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            
            
        }

        private void btnDeleted_Click(object sender, EventArgs e)
        {
            btnDeleted.Font = new Font(btnDeleted.Font.Name, btnDeleted.Font.Size, FontStyle.Bold);
            btnDelete.Text = "Delete All";
            BindListView("deleted");
            btnReply.Visible = false;
            btnForward.Visible = false;
            
            txtFrom.Visible = false;
            txtTo.Visible = false;
            txtCc.Visible = false;
            txtSubject.Visible = false;
            rtbBody.Visible = false;
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;

        }

        private void InBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }
    }
}
