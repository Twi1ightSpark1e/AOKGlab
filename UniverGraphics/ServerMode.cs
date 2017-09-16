using System;
using System.Windows.Forms;

namespace UniverGraphics
{
	class ServerMode
	{
		private static ServerSocket server = new ServerSocket(4115);

		public static void Start(Form owner)
		{
			server.OnReceive += (client, message) =>
			{
				if (message.StartsWith("color"))
				{
                    MainForm.Index = Convert.ToInt32(message.Remove(0, 5));
					server.SendAllExcept(client, message);
				}
			};
			server.OnConnected += (client) =>
			{
				server.SendTo(client, $"color{MainForm.Index}");
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
            server.SendAll($"color{MainForm.Index}");
        }
	}
}
