using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Xml;
using ModelLayer;
using DataLayer;
using System.Threading;

namespace EmailClient
{
    public partial class frmLogin : Form
    {
        TcpClient _client;
        NetworkStream _stream;
        StreamReader _reader;
        StreamWriter _writer;
        User RegUser=new User();
        public frmLogin()
        {
            InitializeComponent();
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUserName.Text = "";
            txtPassword.Text = "";

            
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            //validate if the user name and password is provided
            if (txtUserName.Text.Trim() == "")
            {
                MessageBox.Show("User Name is Required!", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                txtUserName.Focus();
            }
            else if (txtPassword.Text.Trim() == "")
            {
                MessageBox.Show("Password is Required!", "Email Client", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                txtPassword.Focus();
            }
            else
            {
                //validat user against the email server db and get new emails.
                Receive();
                if (RegUser.EmailAddress != null)
                {
                    //valid user direct to InBox
                    InBox frmInBox = new InBox(RegUser);
                    frmInBox.Show();
                    this.Hide();


                }
                else
                {
                    //user does't exist in the db
                    txtUserName.Text = "";
                    txtPassword.Text = "";
                }
            }
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

        private void Receive()
        {
            
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
            catch (Exception e)
            {
                MessageBox.Show("Email Server connection failed!", "Email client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                Application.Exit();
            }
            //get emails from pop3 server and save in local db
            string strMessage = String.Empty;
            while (true)
            {
                try
                {
                    strMessage = Read();
                    //MessageBox.Show(strMessage);
                    if (strMessage.Length > 0)
                    {
                        if (strMessage.ToUpper().StartsWith("220"))
                        {
                            // Login Process
                            #region MyRegion
                            Write("USER " + txtUserName.Text.Trim());
                            strMessage = Read();
                            //MessageBox.Show(strMessage);
                            if (strMessage.ToUpper().StartsWith("+OK"))
                            {
                                Write("PASS " + txtPassword.Text.Trim());
                                strMessage = Read();
                                if (strMessage.ToUpper().StartsWith("550"))
                                {
                                    //login failed
                                    MessageBox.Show("User Name or/and Password invalid.", "EmailClient", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Write("QUIT");
                                    strMessage = Read();
                                    _client.Close();
                                    _stream.Close();
                                    break;
                                }
                                else
                                {
                                    //login is successfull
                                    RegUser.UserName = txtUserName.Text.Trim();
                                    RegUser.Password = txtPassword.Text.Trim();
                                    RegUser.EmailAddress= strMessage.Substring(4).Replace("\r\n","");
                                                                        
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
                                       bool message1= DbAccess.InsertEmail(xmlDoc);
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
                catch (Exception e)
                {
                    MessageBox.Show("Below Error Occured! "+ Environment.NewLine+e.Message, "Email client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    Application.Exit();
                }
            }
            }

              
        

    }
}
