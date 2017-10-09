using System;
using System.Windows.Forms;

namespace Client
{
	class ClientMode
	{
		private static ClientSocket client;
        public static ClientSocket Client => client;

		public static void Start(string host)
		{
			client = new ClientSocket(host, 4115);
            client.OnDisconnect += (reason) =>
            {
                MainForm.Connected = false;
            };
			try
			{
				client.Connect();
			}
			catch (AggregateException e)
			{
                MessageBox.Show($"Произошла ошибка: {e.Flatten()}");
			}
		}

        public static void SendCoordinates(string value)
        {
            client.SendMessage($"coords{value}");
        }
	}
}
