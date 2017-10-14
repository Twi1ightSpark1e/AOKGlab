using System;
using System.Windows.Forms;

namespace Server
{
	class ServerMode
	{
        private static ServerSocket server = new ServerSocket(4115);
        public static ServerSocket Server => server;

		public static bool Start(Form owner)
		{
			//server.OnReceive += (client, message) =>
			//{
			//	server.SendAll(message);
   //         };
            string result = server.Start(owner);
            if (result != null)
            {
                MessageBox.Show(result);
                return false;
            }
            return true;
		}

        public static void Stop()
        {
            server.Stop();
        }

        public static void SendCoordinatesAll(string value)
        {
            server.SendAll($"coords{value}");
        }
	}
}
