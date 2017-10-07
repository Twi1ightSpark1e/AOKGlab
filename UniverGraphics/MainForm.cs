﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;

namespace UniverGraphics
{
    public partial class MainForm : Form
    {
        public static MainForm LastInstance { get; private set; }

        private List<string> log = new List<string>();

        private static bool connected;
        public static bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
                LastInstance.glControl1.Refresh();
                LastInstance.serverButton.Enabled = LastInstance.addressTextBox.Enabled = LastInstance.connectButton.Enabled = !connected;
                LastInstance.nextButton.Enabled = LastInstance.autoChangeColorButton.Enabled = connected;
                if (value)
                {
                    Application.Idle += LastInstance.mainForm_onIdle;
                    if (stopwatch == null)
                        stopwatch = Stopwatch.StartNew();
                    else stopwatch.Restart();
                }
                else Application.Idle -= LastInstance.mainForm_onIdle;
            }
        }
        private static bool listening;
        public static bool Listening
        {
            get
            {
                return listening;
            }
            set
            {
                listening = value;
                LastInstance.glControl1.Refresh();
                LastInstance.serverButton.Enabled = LastInstance.addressTextBox.Enabled = LastInstance.connectButton.Enabled = !listening;
                LastInstance.nextButton.Enabled = LastInstance.autoChangeColorButton.Enabled = listening;
                if (value)
                {
                    ServerMode.Server.OnConnected += (client) =>
                    {
                        Thread.Sleep(500);
                        ServerMode.Server.SendTo(client, $"coords{lastSentCoordinates}");
                    };
                    Application.Idle += LastInstance.mainForm_onIdle;
                    if (stopwatch == null)
                        stopwatch = Stopwatch.StartNew();
                    else stopwatch.Restart();
                }
                else Application.Idle -= LastInstance.mainForm_onIdle;
            }

        }

        private static int colorIndex;
        public static int ColorIndex
        {
            get
            {
                return colorIndex;
            }
            set
            {
                colorIndex = value;
                int color = colorIndex;
                foreach (LittleHome house in houses)
                    house.Color = colors[color++ % colors.Count];
                LastInstance.Invoke((MethodInvoker)delegate
                {
                    LastInstance.glControl1.Refresh();
                });
            }
        }
        private static List<(byte red, byte green, byte blue)> colors = new List<(byte red, byte green, byte blue)>()
        {
            (  0,   0,   0),
            (255, 255, 255),
            (  0, 255,   0),
            (255,   0,   0),
            (  0,   0, 255)
        };
        private static LittleHome[] houses = new LittleHome[4];
        private static Camera camera;
        private static Stopwatch stopwatch;
        private static object locker = new object();
        private static string lastSentCoordinates;

        public MainForm()
        {
            LastInstance = this;
            InitializeComponent();
            
            camera = new Camera()
            {
                Radius = 70f,
                RadianX = 1f,
                RadianY = 1f,
                Eye    = new Vector3((float)(Math.Cos(0) * 25f * Math.Cos(0)), 
                                     (float)(Math.Sin(0) * 25f), 
                                     (float)(Math.Cos(0) * 25f * Math.Sin(0))),
                Target = new Vector3( 0, 0, 0),
                Up     = new Vector3( 0, 1, 0)
            };
        }

        private void NextColor()
        {
            ColorIndex = (ColorIndex + 1) % colors.Count;
            if (Connected)
                ClientMode.SendColor(ColorIndex);
            else if (Listening)
                ServerMode.SendColorAll(ColorIndex);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            NextColor();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(0.785398f, glControl1.Width / glControl1.Height, 0.01f, 5000.0f); //0,785398 это 45 градусов в радианах
            GL.LoadMatrix(ref perspective);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            //Установим фоновый цвет
            //GL.ClearColor(new OpenTK.Graphics.Color4(105, 29, 142, 0)); //сиреневый цвет
            GL.ClearColor(new OpenTK.Graphics.Color4(0, 0, 0, 0));
            //Не будем ничего рисовать, пока не подключимся к серверу
            if (Connected || Listening)
            {
                //Для начала очистим буфер
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                //Настройки для просмотра
                //Настройка позиции "глаз"
                camera.SetCamera();
                foreach (LittleHome house in houses)
                    house.Show();
                //Поменяем местами буферы
                glControl1.SwapBuffers();
            }
        }

        private void CoordinatesReceived(string message)
        {
            lock (locker)
            {
                if (message.StartsWith("coords"))
                {
                    message = message.Remove(0, 6);
                    string[] coordinates = message.Split(';');
                    camera.RadianX = float.Parse(coordinates[0]);
                    camera.RadianY = float.Parse(coordinates[1]);
                    camera.Radius = float.Parse(coordinates[2]);
                    if (!Connected && !Listening)
                        Invoke((MethodInvoker)delegate
                        {
                            Connected = true;
                        });
                }
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            serverButton.Enabled = addressTextBox.Enabled = connectButton.Enabled = false;
            ClientMode.Start(addressTextBox.Text);
            ClientMode.Client.OnReceive += (message) =>
            {
                CoordinatesReceived(message);
            };
            Text = "Лабораторная работа №2 (клиент)";
        }

        private void serverButton_Click(object sender, EventArgs e)
        {
            serverButton.Enabled = addressTextBox.Enabled = connectButton.Enabled = false;
            ServerMode.Start(this);
            ServerMode.Server.OnReceive += (client, message) => 
            {
                CoordinatesReceived(message);
            };
            Text = "Лабораторная работа №2 (сервер)";
        }

        private void autoChangeColorButton_Click(object sender, EventArgs e)
        {
            changeColorTimer.Enabled = !changeColorTimer.Enabled;
            if (autoChangeColorButton.Tag.Equals("0"))
                autoChangeColorButton.Text = "Остановить";
            else autoChangeColorButton.Text = "Автосмена цветов";
            autoChangeColorButton.Tag = ((int.Parse((string)autoChangeColorButton.Tag) + 1) % 2).ToString();
        }

        private void changeColorTimer_Tick(object sender, EventArgs e)
        {
            NextColor();
        }

        private void mainForm_onIdle(object sender, EventArgs e)
        {
            float millisecondsElapsed = (float)stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            var state = Keyboard.GetState();
            Vector3 eye = camera.Eye;
            Directions dirs = Directions.None;
            dirs |= state.IsKeyDown(Key.Up) ? Directions.Up : Directions.None;
            dirs |= state.IsKeyDown(Key.Down) ? Directions.Down : Directions.None;
            dirs |= state.IsKeyDown(Key.Left) ? Directions.Left : Directions.None;
            dirs |= state.IsKeyDown(Key.Right) ? Directions.Right : Directions.None;
            dirs |= (state.IsKeyDown(Key.Plus) || state.IsKeyDown(Key.KeypadPlus)) ? Directions.Forward : Directions.None;
            dirs |= (state.IsKeyDown(Key.Minus) || state.IsKeyDown(Key.KeypadMinus)) ? Directions.Backward : Directions.None;
            label2.Text = "UP: " + state.IsKeyDown(Key.Up).ToString();
            label3.Text = "DOWN: " + state.IsKeyDown(Key.Down).ToString();
            label4.Text = "LEFT: " + state.IsKeyDown(Key.Left).ToString();
            label5.Text = "RIGHT: " + state.IsKeyDown(Key.Right).ToString();
            label6.Text = "PLUS: " + (state.IsKeyDown(Key.Plus) || state.IsKeyDown(Key.KeypadPlus)).ToString();
            label7.Text = "MINUS: " + (state.IsKeyDown(Key.Minus) || state.IsKeyDown(Key.KeypadMinus)).ToString();
            label8.Text = "X: " + eye.X.ToString();
            label9.Text = "Y: " + eye.Y.ToString();
            label10.Text = "Z: " + eye.Z.ToString();
            camera.CurrentDirection = dirs;
            camera.Simulate(millisecondsElapsed);
            if (camera.ChangedCoordinates != string.Empty && lastSentCoordinates != camera.ChangedCoordinates)
            {
                if (Connected)
                {
                    ClientMode.SendCoordinates(camera.ChangedCoordinates);
                    lastSentCoordinates = camera.ChangedCoordinates;
                }
                else if (Listening)
                {
                    ServerMode.SendCoordinatesAll(camera.ChangedCoordinates);
                    lastSentCoordinates = camera.ChangedCoordinates;
                }
            }
            glControl1.Invalidate();
            
            //log.Add($"UP: {state.IsKeyDown(Key.Up).ToString():6} DOWN: {state.IsKeyDown(Key.Down).ToString():6} LEFT: {state.IsKeyDown(Key.Left).ToString():6} RIGHT: {state.IsKeyDown(Key.Right).ToString():6} PLUS: {(state.IsKeyDown(Key.Plus) || state.IsKeyDown(Key.KeypadPlus)).ToString():6} MINUS: {(state.IsKeyDown(Key.Minus) || state.IsKeyDown(Key.KeypadMinus)).ToString():6} X: {camera.Eye.X} Y: {camera.Eye.Y} Z: {camera.Eye.Z}");
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.CullFace(CullFaceMode.Back);

            List<LittleHome> houses = new List<LittleHome>();
            try
            {
                StreamReader sr = new StreamReader("map.txt");
                int lines = 0;
                Random rnd = new Random();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    for (int i = 0; i < line.Length; i++)
                    {
                        ShapeMode mode = (ShapeMode)Enum.Parse(typeof(ShapeMode), line[i].ToString());
                        houses.Add(new LittleHome(0, (0, 0, 0), new Vector3((i - line.Length / 2) * 2, 0, (lines - line.Length / 2)*2), mode));
                    }
                    lines++;
                }
                sr.Close();
                MainForm.houses = houses.ToArray();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Файл карты проходимости не найден");
                Application.Exit();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StreamWriter sw = new StreamWriter("log.txt");
            foreach (string line in log)
                sw.WriteLine(line);
            sw.Flush();
            sw.Close();
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
        }
    }
}
