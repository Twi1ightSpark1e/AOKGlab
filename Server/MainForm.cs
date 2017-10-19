using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public struct MapUnit
    {
        public int x, z, value;

        public void SetContent(int value)
        {
            this.value = value;
        }

        public void SetCoordinates((int x, int z) coordinates)
        {
            x = coordinates.x;
            z = coordinates.z;
        }
    }

    public struct CameraCoordinates
    {
        public float radianX, radianY, radius;
    }

    enum MoveDirection
    {
        None = 0,
        Left = -2,
        Up = -1,
        Down = 1,
        Right = 2
    }

    public partial class MainForm : Form
    {
        private bool listening;
        private const string StartServer = "Запуск сервера";
        private const string StopServer = "Остановка сервера";
        public bool Listening
        {
            get
            {
                return listening;
            }
            set
            {
                listening = value;
                portTextBox.Enabled = defaultMapButton.Enabled = !value;
                startButton.Text = startButton.Text == StartServer ? StopServer : StartServer;
            }
        }

        private List<(TcpClient client, MapUnit mapUnit)> players = new List<(TcpClient client, MapUnit mapUnit)>();
        private static List<MapUnit> mapUnits = new List<MapUnit>();
        private static CameraCoordinates cameraCoordinates = new CameraCoordinates();

        public MainForm()
        {
            InitializeComponent();

            string mapFileName;
            if (File.Exists("map_latest.txt"))
                mapFileName = "map_latest.txt";
            else if (File.Exists("map_default.txt"))
                mapFileName = "map_default.txt";
            else
            {
                MessageBox.Show("Файл карты проходимости не найден");
                Application.Exit();
                return;
            }
            ReadFromMap(mapFileName);
        }

        private void ReadFromMap(string mapFileName)
        {
            mapUnits.Clear();
            try
            {
                StreamReader sr = new StreamReader(mapFileName);
                int lines = 0;
                Random rnd = new Random();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    for (int i = 0; i < line.Length; i++)
                    {
                        var square = new Square();
                        square.Location = new Point(i * 20, lines * 20);
                        square.Tag = $"{i};{lines}";
                        square.Height = square.Width = 20;
                        square.Visible = true;
                        square.SquareContent = (SquareContent)Enum.Parse(typeof(SquareContent), line[i].ToString());
                        square.Click += square_Click;
                        square.DoubleClick += square_Click;
                        squaresPanel.Controls.Add(square);
                        mapUnits.Add(new MapUnit()
                        {
                            x = i,
                            z = lines,
                            value = byte.Parse(line[i].ToString())
                        });
                    }
                    lines++;
                }
                sr.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка при работе с файлом проходимости");
                Application.Exit();
            }
        }

        private void square_Click(object sender, EventArgs e)
        {
            foreach (Square square in squaresPanel.Controls)
            {
                if (((Square)sender).Tag == square.Tag)
                {
                    int temp = (int)square.SquareContent;
                    temp = ++temp % (Enum.GetNames(typeof(SquareContent)).Length-1);
                    square.SquareContent = (SquareContent)temp;

                    for (int i = 0; i < mapUnits.Count; i++)
                    {
                        if (square.Tag.ToString() == $"{mapUnits[i].x};{mapUnits[i].z}")
                        {
                            var tempUnit = mapUnits[i];
                            tempUnit.value = (byte)temp;
                            mapUnits.RemoveAt(i);
                            mapUnits.Insert(i, tempUnit);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            foreach (var player in players)
            {
                foreach (Square square in squaresPanel.Controls)
                {
                    if (square.Tag.ToString() == $"{player.mapUnit.x};{player.mapUnit.z}")
                        square.SquareContent = SquareContent.Empty;
                }
            }
            players.Clear();

            ServerMode.Stop();
            startButton.Click -= stopButton_Click;
            startButton.Click += startButton_Click;
            ServerMode.Server.OnConnected -= ServerMode_OnConnected;
            ServerMode.Server.OnDisconnect -= ServerMode_OnDisconnect;
            ServerMode.Server.OnReceive -= ServerMode_OnReceive;
            Listening = false;
            startButton.Enabled = squaresPanel.Enabled = defaultMapButton.Enabled = true;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            portTextBox.Enabled = squaresPanel.Enabled = defaultMapButton.Enabled = false;
            startButton.Click -= startButton_Click;
            startButton.Click += stopButton_Click;
            ServerMode.Server.OnConnected += ServerMode_OnConnected;
            ServerMode.Server.OnDisconnect += ServerMode_OnDisconnect;
            ServerMode.Server.OnReceive += ServerMode_OnReceive;
            cameraCoordinates.radianX = cameraCoordinates.radianY = 1;
            cameraCoordinates.radius = 70;
            Listening = ServerMode.Start(this);
        }

        private void ServerMode_OnDisconnect(TcpClient client)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].client == client)
                {
                    ServerMode.Server.SendAllExcept(client, $"delplayer({players[i].mapUnit.x};{players[i].mapUnit.z})");
                    foreach (Square square in squaresPanel.Controls)
                    {
                        if (square.Tag.ToString() == $"{players[i].mapUnit.x};{players[i].mapUnit.z}")
                        {
                            square.SquareContent = SquareContent.Empty;
                            break;
                        }
                    }
                    players.RemoveAt(i);
                    break;
                }
            }
        }

        private void ServerMode_OnReceive(TcpClient client, string message)
        {
            string[] messages = message.Split('&');
            foreach (string msg in messages)
            {
                if (msg.StartsWith("move"))
                {
                    MoveDirection direction = (MoveDirection)Enum.Parse(typeof(MoveDirection), msg.Remove(0, 4));
                    int i = 0;
                    while (i < players.Count && !players.ElementAt(i).client.Equals(client))
                        i++;
                    if (i == players.Count)
                        throw new Exception("Не найден игровой кубик");
                    List<MapUnit> modifiedUnits = new List<MapUnit>();
                    if (MoveSquare(direction, players.ElementAt(i).mapUnit, ref modifiedUnits, out (int targetX, int targetZ) targets))
                    {
                        foreach (MapUnit unit in modifiedUnits)
                        {
                            for (int j = 0; j < mapUnits.Count; j++)
                            {
                                if (mapUnits[j].x == unit.x && mapUnits[j].z == unit.z)
                                {
                                    mapUnits.RemoveAt(j);
                                    mapUnits.Insert(j, unit);
                                    break;
                                }
                            }
                            foreach (Square square in squaresPanel.Controls)
                            {
                                if (square.Tag.ToString() == $"{unit.x};{unit.z}")
                                {
                                    square.SquareContent = (SquareContent)unit.value;
                                    break;
                                }
                            }
                        }
                        ServerMode.Server.SendAll(msg.Insert(4, $"({players.ElementAt(i).mapUnit.x};{players.ElementAt(i).mapUnit.z})"));
                        players.RemoveAt(i);
                        players.Insert(i, (client, new MapUnit() { x = targets.targetX, z = targets.targetZ, value = 0 }));
                        foreach (Square square in squaresPanel.Controls)
                        {
                            if (square.Tag.ToString() == $"{targets.targetX};{targets.targetZ}")
                            {
                                square.SquareContent = SquareContent.Player;
                                break;
                            }
                        }
                    }
                    else ServerMode.Server.SendTo(client, "errmove");
                }
                else if (msg.StartsWith("ack"))
                {
                    if (msg.Remove(0, 3) == "players")
                        ServerMode.Server.SendTo(client, $"field{JsonConvert.SerializeObject(mapUnits)}");
                }
                else ServerMode.Server.SendAll(msg);
            }
        }

        private bool MoveSquare(MoveDirection direction, MapUnit playerUnit, ref List<MapUnit> modifiedUnits, out (int targetX, int targetZ) targetCoordinates)
        {
            int targetX = playerUnit.x, targetZ = playerUnit.z;
            switch (direction)
            {
                case MoveDirection.Up:
                case MoveDirection.Down:
                    targetZ += Math.Sign((sbyte)direction);
                    break;
                case MoveDirection.Left:
                case MoveDirection.Right:
                    targetX += Math.Sign((sbyte)direction);
                    break;
            }
            targetCoordinates = (targetX, targetZ);
            MapUnit nextUnit = new MapUnit
            {
                value = -2
            };
            foreach (MapUnit unit in mapUnits)
            {
                if (unit.x == targetX && unit.z == targetZ)
                {
                    nextUnit = unit;
                    break;
                }
            }
            if (nextUnit.value == -2)
                throw new Exception("Не найден следующий за игровым кубик");
            bool isPlayerCube = false;
            foreach (var player in players)
            {
                if (player.mapUnit.x == targetX && player.mapUnit.z == targetZ)
                {
                    return false;
                }
                if (playerUnit.x == player.mapUnit.x && playerUnit.z == player.mapUnit.z)
                    isPlayerCube = true;
            }
            if (nextUnit.value == (int)SquareContent.Empty)
            {
                nextUnit.SetContent(playerUnit.value);
                modifiedUnits.Add(nextUnit);
                playerUnit.SetContent((int)SquareContent.Empty);
                modifiedUnits.Add(playerUnit);
                return true;
            }
            else if (nextUnit.value == (int)SquareContent.LightBarrier && /*playerUnit.value == (int)SquareContent.Player*/isPlayerCube)
            {
                if (MoveSquare(direction, nextUnit, ref modifiedUnits, out _))
                {
                    nextUnit.SetContent(playerUnit.value);
                    modifiedUnits.Add(nextUnit);
                    playerUnit.SetContent((int)SquareContent.Empty);
                    modifiedUnits.Add(playerUnit);
                    return true;
                }
                return false;
            }
            return false;
        }

        private void ServerMode_OnConnected(TcpClient client)
        {
            //Выбираем координаты для нового игрока
            //var emptyFields = mapUnits.Where((unit) => unit.value == (int)SquareContent.Empty).ToList();
            var emptyFields = new List<MapUnit>();
            for (int i = 0; i < mapUnits.Count; i++)
                if (mapUnits[i].value == (int)SquareContent.Empty)
                    emptyFields.Add(mapUnits[i]);

            Random rnd = new Random();
            bool playersNearby;
            int tries = 0;
            MapUnit selectedUnit;
            do
            {
                //selectedUnit = emptyFields[rnd.Next(emptyFields.Count)];
                selectedUnit = emptyFields.ElementAt(rnd.Next(emptyFields.Count));
                var xRange = Enumerable.Range(selectedUnit.x - 1, selectedUnit.x + 1);
                var zRange = Enumerable.Range(selectedUnit.z - 1, selectedUnit.z + 1);
                playersNearby = false;
                foreach (var player in players)
                {
                    if ((xRange.Where((x) => x == player.mapUnit.x).Count() > 0) &&
                        (xRange.Where((z) => z == player.mapUnit.z).Count() > 0))
                    {
                        playersNearby = true;
                        tries++;
                        continue;
                    }
                }
            }
            while (playersNearby && (tries < 10));
            if (tries == 10)
            {
                ServerMode.Server.SendTo(client, $"errplace"); //невозможно выбрать место для нового игрока
                client.Close();
            }
            else
            {
                string playersString = players.Aggregate(string.Empty, (accum, player) => accum + $"({player.mapUnit.x};{player.mapUnit.z})");
                ServerMode.Server.SendTo(client, $"players({selectedUnit.x};{selectedUnit.z}){playersString}");
                ServerMode.Server.SendAllExcept(client, $"newplayer({selectedUnit.x};{selectedUnit.z})");

                players.Add((client, selectedUnit));
                foreach (Square square in squaresPanel.Controls)
                {
                    if (square.Tag.ToString() == $"{selectedUnit.x};{selectedUnit.z}")
                    {
                        square.SquareContent = SquareContent.Player;
                        break;
                    }
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Listening)
                ServerMode.Stop();
            StreamWriter sw = new StreamWriter("map_latest.txt", false);
            int latestX = 0;
            foreach (MapUnit unit in mapUnits)
            {
                if (latestX != unit.z)
                {
                    sw.WriteLine();
                    latestX = unit.z;
                }
                sw.Write(unit.value);
            }
            sw.Flush();
            sw.Close();
        }

        private void defaultMapButton_Click(object sender, EventArgs e)
        {
            File.Delete("map_latest.txt");
            if (!File.Exists("map_default.txt"))
            {
                MessageBox.Show("Файл с картой проходимости по умолчанию не найден");
                Application.Exit();
                return;
            }
            squaresPanel.Controls.Clear();
            ReadFromMap("map_default.txt");
        }
    }
}
