﻿using System;
using System.Windows.Forms;

namespace UniverGraphics
{
	class ClientMode
	{
		private static ClientSocket client;
		public static void Start(string host)
		{
			client = new ClientSocket(host, 4115);
			client.OnReceive += (message) =>
			{
				if (message.StartsWith("color"))
				{
					int colorId = Convert.ToInt32(message.Remove(0, 5));
                    MainForm.Index = colorId;
				}
			};
            client.OnConnected += () =>
            {
                MainForm.Connected = true;
            };
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

		public static void SendColor(int colorId)
		{
			client.SendMessage($"color{colorId}");
		}
	}
}