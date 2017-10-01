using System;
using System.Windows.Forms;

namespace UniverGraphics
{
	class ServerMode
	{
		private static ServerSocket server = new ServerSocket(4115);
        public static ServerSocket Server => server;

		public static void Start(Form owner)
		{
			server.OnReceive += (client, message) =>
			{
				if (message.StartsWith("color"))
				{
                    MainForm.ColorIndex = Convert.ToInt32(message.Remove(0, 5));
				}
                server.SendAllExcept(client, message);
            };
			server.OnConnected += (client) =>
			{
                server.SendTo(client, $"color{MainForm.ColorIndex}");
			};
            try
            {
                server.Start(owner);
                MainForm.Listening = true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Произошла ошибка: {e.Message}");
            }
		}

        public static void SendColorAll(int colorId)
        {
            server.SendAll($"color{colorId}");
        }

        public static void SendCoordinatesAll(string value)
        {
            server.SendAll($"coords{value}");
        }
	}
}
