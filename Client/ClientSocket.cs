﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UniverGraphics
{
	class ClientSocket
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public bool Connected => client.Connected;

		private TcpClient client;
		private NetworkStream clientStream;
		private byte[] message;
#region events
		internal delegate void OnConnectedDelegate();
		internal delegate void OnDisconnectDelegate(string reason);
		internal delegate void OnMessageReceiveDelegate(string message);

		public event OnConnectedDelegate OnConnected;
		public event OnDisconnectDelegate OnDisconnect;
		public event OnMessageReceiveDelegate OnReceive;
#endregion

		public ClientSocket()
		{
			Host = "";
			Port = 0;
		}

		public ClientSocket(string host, int port)
		{
			Host = host;
			Port = port;
		}

		public void Connect()
		{
			Connect(Host, Port);
		}

		public void Connect(string host, int port)
		{
			if ((Host != "") && (Port != 0))
			{
				IPAddress parseAddress;
				var list = IPAddress.TryParse(host, out parseAddress) ? new[] {parseAddress} : Dns.GetHostEntry(Host).AddressList;
				foreach (IPAddress address in list)
				{
					client = new TcpClient();
					var connectionTask = client.ConnectAsync(address, port);
					connectionTask.Wait(3000);
					if (connectionTask.IsCompleted)
						break;
				}
				if (!client.Connected)
					throw new Exception("Cannot establish connection to server");
				OnConnected?.Invoke();
				clientStream = client.GetStream();
				message = new byte[client.ReceiveBufferSize];
				clientStream.BeginRead(message, 0, message.Length, Listen, message);
			}
			else throw new Exception("Host or port is not specified");
		}

		public void Disconnect()
		{
			client.Close();
			CallDisconnect("");
		}

		private void Listen(IAsyncResult res)
		{
			int read;
			try
			{
				read = clientStream.EndRead(res);
			}
			catch
			{
				if (Connected == true)
					CallDisconnect("Сервер закрыл соединение");
				return;
			}
			if (read == 0)
			{
				if (Connected == true)
					CallDisconnect("Сервер закрыл соединение");
				return;
			}
            clientStream.BeginRead(message, 0, message.Length, Listen, message);
			message = res.AsyncState as byte[];
			string messageString = Encoding.Default.GetString(message);
			messageString = messageString.Substring(0, read);
			OnReceive?.Invoke(messageString);
		}

		public void SendMessage(string message)
		{
			if (!client.Connected)
				throw new Exception("No connection with server");
			byte[] messageBytes = Encoding.ASCII.GetBytes(message);
			clientStream.Write(messageBytes, 0, messageBytes.Length);
		}

		private void CallDisconnect(string reason)
		{
			if (client.Connected)
				client.Close();
			OnDisconnect?.Invoke(reason);
		}
	}
}