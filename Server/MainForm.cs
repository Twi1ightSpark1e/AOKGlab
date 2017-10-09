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
        public int x, y;
        public byte value;
    }

    public struct CameraCoordinates
    {
        public float radianX, radianY, radius;
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
                            y = lines,
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
                    temp = ++temp % Enum.GetNames(typeof(SquareContent)).Length;
                    square.SquareContent = (SquareContent)temp;

                    for (int i = 0; i < mapUnits.Count; i++)
                    {
                        if (square.Tag.ToString() == $"{mapUnits[i].x};{mapUnits[i].y}")
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
            if (message.StartsWith("ack"))
            {
                message = message.Remove(0, 3);
                if (message == "arr")
                {
                    ServerMode.Server.SendTo(client, $"coords{cameraCoordinates.radianX};{cameraCoordinates.radianY};{cameraCoordinates.radius}");
                }
            }
            else if (message.StartsWith("coords"))
            {
                message = message.Remove(0, 6);
                string[] coordinates = message.Split(';');
                cameraCoordinates.radianX = float.Parse(coordinates[0]);
                cameraCoordinates.radianY = float.Parse(coordinates[1]);
                cameraCoordinates.radius = float.Parse(coordinates[2]);
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
                if (latestX != unit.y)
                {
                    sw.WriteLine();
                    latestX = unit.y;
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
