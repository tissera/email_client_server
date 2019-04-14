using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace EmailServer
{
    class Program
    {
        
        private static Mutex mutex = null;
        static void Main(string[] args)
        {
            const string appName = "EmailServer";
            bool createdNew;
            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                return;
            }
            
                TcpListener _SMPTlistener = new TcpListener(IPAddress.Loopback, 25);
                _SMPTlistener.Start();
                while (true)
                {
                    SMTPServer handler = new EmailServer.SMTPServer(_SMPTlistener.AcceptTcpClient());
                    Thread thread = new Thread(new ThreadStart(handler.Run));
                    thread.Start();
                }
            
         }
        
    }
}
