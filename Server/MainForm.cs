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
                startButton.Enabled = mapUnits.Where((unit) => unit.value == (int)SquareContent.Player).Count() == 1;
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
                    temp = ++temp % Enum.GetNames(typeof(SquareContent)).Length;
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
                    startButton.Enabled = mapUnits.Where((unit) => unit.value == (int)SquareContent.Player).Count() == 1;
                    break;
                }
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            ServerMode.Stop();
            startButton.Click -= stopButton_Click;
            startButton.Click += startButton_Click;
            ServerMode.Server.OnConnected -= ServerMode_OnConnected;
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
            ServerMode.Server.OnReceive += ServerMode_OnReceive;
            cameraCoordinates.radianX = cameraCoordinates.radianY = 1;
            cameraCoordinates.radius = 70;
            Listening = ServerMode.Start(this);
        }

        private void ServerMode_OnReceive(TcpClient client, string message)
        {
            string[] messages = message.Split('&');
            foreach (string msg in messages)
            {
                if (msg.StartsWith("move"))
                {
                    MoveDirection direction = (MoveDirection)Enum.Parse(typeof(MoveDirection), msg.Remove(0, 4));
                    MapUnit player = new MapUnit()
                    {
                        value = -1
                    };
                    int i = 0;
                    for (i = 0; i < mapUnits.Count; i++)
                    {
                        if (mapUnits[i].value == (int)SquareContent.Player)
                        {
                            player = mapUnits[i];
                            break;
                        }
                    }
                    if (player.value == -1)
                        throw new Exception("Не найден игровой кубик");
                    List<MapUnit> modifiedUnits = new List<MapUnit>();
                    MoveSquare(direction, player, ref modifiedUnits);
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
                    ServerMode.Server.SendAll(msg);
                }
                else ServerMode.Server.SendAll(msg);
            }
        }

        private void MoveSquare(MoveDirection direction, MapUnit playerUnit, ref List<MapUnit> modifiedUnits)
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
            MapUnit nextUnit = new MapUnit
            {
                value = -1
            };
            foreach (MapUnit unit in mapUnits)
            {
                if (unit.x == targetX && unit.z == targetZ)
                {
                    nextUnit = unit;
                    break;
                }
            }
            if (nextUnit.value == -1)
                throw new Exception("Не найден следующий за игровым кубик");
            if (nextUnit.value == (int)SquareContent.Empty)
            {
                nextUnit.SetContent(playerUnit.value);
                modifiedUnits.Add(nextUnit);
                playerUnit.SetContent((int)SquareContent.Empty);
                modifiedUnits.Add(playerUnit);
            }
            else if (nextUnit.value == (int)SquareContent.Barrier && playerUnit.value == (int)SquareContent.Player)
            {
                MoveSquare(direction, nextUnit, ref modifiedUnits);
                nextUnit.SetContent(playerUnit.value);
                modifiedUnits.Add(nextUnit);
                playerUnit.SetContent((int)SquareContent.Empty);
                modifiedUnits.Add(playerUnit);
            }
        }

        private void ServerMode_OnConnected(TcpClient client)
        {
            ServerMode.Server.SendTo(client, $"arr{JsonConvert.SerializeObject(mapUnits)}");
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
