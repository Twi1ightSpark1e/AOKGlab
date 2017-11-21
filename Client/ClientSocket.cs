using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
	class ClientSocket
	{
        // информация о сервере и состояние подключения
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
            Host = host;
            Port = port;
			if ((Host != "") && (Port != 0))
			{
                // разрешаем DNS или парсим адрес в IPAddress
				IPAddress parseAddress;
				var list = IPAddress.TryParse(Host, out parseAddress) ? new[] {parseAddress} : Dns.GetHostEntry(Host).AddressList;
                // по каждому адресу из списка разрешения пытаемся подключиться
				foreach (IPAddress address in list)
				{
					client = new TcpClient();
                    client.ReceiveBufferSize = client.SendBufferSize = 65536;
					var connectionTask = client.ConnectAsync(address, Port);
					connectionTask.Wait(3000);
					if (connectionTask.IsCompleted)
						break;
				}
                // если не удалось подключиться - вылетаем
				if (!client.Connected)
					throw new Exception("Не удалось установить подключение к серверу");
                clientStream = client.GetStream();
                OnConnected?.Invoke();
                // начало чтения из сетевого потока
				message = new byte[client.ReceiveBufferSize];
				clientStream.BeginRead(message, 0, message.Length, Listen, message);
			}
			else throw new Exception("Адрес сервера или порт не установлены");
		}

		public void Disconnect()
		{
			client.Close();
            OnDisconnect?.Invoke(null);
        }

		private void Listen(IAsyncResult res)
		{
			int read; // количество байт, считанных из сетевого потока
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
            clientStream.BeginRead(message, 0, message.Length, Listen, message); // сразу начинаем следующее считывание, чтобы более оперативно получать обновления
            // преобразовываем массив байт в строку и поднимаем событие
			message = res.AsyncState as byte[];
			string messageString = Encoding.Default.GetString(message);
			messageString = messageString.Substring(0, read);
			OnReceive?.Invoke(messageString);
		}

		public void SendMessage(string message)
		{
			if (!client.Connected)
				throw new Exception("Нет подключения к серверу");
			byte[] messageBytes = Encoding.Default.GetBytes(message);
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
