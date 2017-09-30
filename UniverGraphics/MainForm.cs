using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Diagnostics;
using System.IO;

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

        public MainForm()
        {
            LastInstance = this;
            InitializeComponent();

            List<Vector3> multiplyList = new List<Vector3>()
            {
                new Vector3(1, 0, -1),
                new Vector3(1, 0, 1),
                new Vector3(-1, 0, 1),
                new Vector3(-1, 0, -1)
            };
            int angle = -45;
            for (int i = 0; i < houses.Length; i++)
            {
                houses[i] = new LittleHome(angle -= 90, colors[i % colors.Count], multiplyList[i] * 3);
                //angle -= 90;
            }
            camera = new Camera()
            {
                Eye    = new Vector3(12, 7, 0),
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
            GL.ClearColor(new OpenTK.Graphics.Color4(105, 29, 142, 0));
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
                    foreach (string coordinate in coordinates)
                    {
                        Vector3 temp;
                        switch (coordinate[0])
                        {
                            case 'x':
                                temp = camera.Eye;
                                temp.X = float.Parse(coordinate.Remove(0, 2));
                                camera.Eye = temp;
                                break;
                            case 'y':
                                temp = camera.Eye;
                                temp.Y = float.Parse(coordinate.Remove(0, 2));
                                camera.Eye = temp;
                                break;
                            case 'z':
                                temp = camera.Eye;
                                temp.Z = float.Parse(coordinate.Remove(0, 2));
                                camera.Eye = temp;
                                break;
                        }
                    }
                }
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            serverButton.Enabled = addressTextBox.Enabled = connectButton.Enabled = false;
            ClientMode.Start(addressTextBox.Text);
            ClientMode.Client.OnReceive += CoordinatesReceived;
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
            //Text = elapsedTicks.ToString();
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
            if (camera.ChangedCoordinates != string.Empty)
            {
                if (Connected)
                    ClientMode.SendCoordinates(camera.ChangedCoordinates);
                else if (Listening)
                    ServerMode.SendCoordinatesAll(camera.ChangedCoordinates);
            }
            glControl1.Invalidate();
            
            //log.Add($"UP: {state.IsKeyDown(Key.Up).ToString():6} DOWN: {state.IsKeyDown(Key.Down).ToString():6} LEFT: {state.IsKeyDown(Key.Left).ToString():6} RIGHT: {state.IsKeyDown(Key.Right).ToString():6} PLUS: {(state.IsKeyDown(Key.Plus) || state.IsKeyDown(Key.KeypadPlus)).ToString():6} MINUS: {(state.IsKeyDown(Key.Minus) || state.IsKeyDown(Key.KeypadMinus)).ToString():6} X: {camera.Eye.X} Y: {camera.Eye.Y} Z: {camera.Eye.Z}");
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
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
