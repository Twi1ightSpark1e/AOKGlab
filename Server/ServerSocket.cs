using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
	class ServerSocket
	{
		public int Port { get; set; }
		private TcpListener server;
		private bool started;
		private List<TcpClient> clients = new List<TcpClient>();
		private List<Thread> listeners = new List<Thread>();
		private int clientsCount, listenersCount, nicksCount;
		private Thread accepter;
        private Form owner;

		internal delegate void OnConnectedDelegate(TcpClient client);
		internal delegate void OnDisconnectDelegate(TcpClient client);
		internal delegate void OnMessageReceiveDelegate(TcpClient client, string message);

		public event OnConnectedDelegate OnConnected;
		public event OnDisconnectDelegate OnDisconnect;
		public event OnMessageReceiveDelegate OnReceive;

		public ServerSocket()
		{
			Port = 0;
		}

		public ServerSocket(int port)
		{
			Port = port;
		}

		public string Start(Form owner)
		{
            this.owner = owner;
			server = new TcpListener(IPAddress.Any, Port);
			clients.Clear();
			listeners.Clear();

			try
			{
				server.Start();
				started = true;
				Console.WriteLine("Сервер запущен");

				accepter = new Thread(Accepter);
				accepter.Start();

				clientsCount = listenersCount = nicksCount = 0;
			}
			catch (SocketException e)
			{
				return e.Message;
			}
            return null;
		}

		private async void Accepter()
		{
			while (started)
			{
				TcpClient newClient;
				try
				{
					newClient = await server.AcceptTcpClientAsync();
				}
				catch (Exception)
				{
					continue;
				}
				clients.Add(newClient);
				clientsCount++;
				nicksCount++;
				listeners.Add(new Thread(ClientListener));
				listenersCount++;
				listeners[listeners.Count - 1].Start(clients.Count - 1);
                if (owner.InvokeRequired)
                    owner.Invoke((MethodInvoker) delegate
                    {
                        OnConnected?.Invoke(newClient);
                    });
			}
		}

		private void ClientListener(object num)
		{
			bool connectionClosed = false;
			TcpClient client = clients[(int)num];
			NetworkStream clientStream = client.GetStream();
			AutoResetEvent res = new AutoResetEvent(false);
			byte[] message = new byte[client.ReceiveBufferSize];
			while (started)
			{
				res.Reset();
				int read = 0;
				IAsyncResult result = null;
				try
				{
					result = clientStream.BeginRead(message, 0, client.ReceiveBufferSize, asyncResult =>
					{
						read = 0;
						try
						{
							read = clientStream.EndRead(asyncResult);
						}
						catch (IOException)
						{
							connectionClosed = true;
							res.Set();
							return;
						}
						if (read == 0)
						{
							connectionClosed = true;
							res.Set();
							return;
						}
						message = asyncResult.AsyncState as byte[];
						res.Set();
					}, message);
				}
				catch (IOException)
				{
					if (result != null)
						clientStream.EndRead(result);
					connectionClosed = true;
				}
				WaitHandle.WaitAll(new WaitHandle[] { res });
				if (!started)
					break;
				if (connectionClosed)
				{
					NullConnection((int)num);
					connectionClosed = false;
                    if (owner.InvokeRequired)
                        owner.Invoke((MethodInvoker)delegate
                        {
                            OnDisconnect?.Invoke(client);
                        });
					break;
				}
				if (message[0] != 0)
				{
					string messageString = Encoding.ASCII.GetString(message).Substring(0, read);
                    if (owner.InvokeRequired)
                        owner.Invoke((MethodInvoker)delegate
                        {
                            OnReceive?.Invoke(client, messageString);
                        });
				}
			}
		}

		private void NullConnection(int num)
		{
			if ((listeners.Count > num) && (listeners[num] != null))
			{
				listeners[num] = null;
				listenersCount--;
			}
			if ((clients.Count > num) && (clients[num] != null))
			{
				clients[num] = null;
				nicksCount--;
			}

			CheckForNullCounts();
		}

		private void CheckForNullCounts()
		{
			if ((clientsCount == 0) && (listenersCount == 0) && (nicksCount == 0))
			{
				clients.Clear();
				listeners.Clear();
			}
		}

		public void Stop()
		{
			foreach (var client in clients)
				if (client != null)
					if ((client.Connected) && (client.GetStream().CanWrite))
						SendTo(client, "close");
			started = false;
			server.Stop();
			Console.WriteLine("Сервер остановлен");
		}

		public void SendTo(TcpClient client, string value)
		{
			byte[] messageBytes = Encoding.Default.GetBytes(value);
			if (client != null)
				if ((client.Connected) && (client.GetStream().CanWrite))
					client.GetStream().Write(messageBytes, 0, messageBytes.Length);
		}

		public void SendAll(string message)
		{
			byte[] messageBytes = Encoding.Default.GetBytes(message);
			foreach (TcpClient client in clients)
			{
				if (client != null)
					if ((client.Connected) && (client.GetStream().CanWrite))
						client.GetStream().Write(messageBytes, 0, messageBytes.Length);
			}
		}

		public void SendAllExcept(TcpClient tcpClient, string message)
		{
			byte[] messageBytes = Encoding.Default.GetBytes(message);
			foreach (TcpClient client in clients)
			{
				if (client != null)
					if ((client.Connected) && (client.GetStream().CanWrite) && (!tcpClient.Equals(client)))
						client.GetStream().Write(messageBytes, 0, messageBytes.Length);
			}
		}
	}
}
