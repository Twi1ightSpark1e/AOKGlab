using System;
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
using Newtonsoft.Json;

namespace Client
{
    public struct MapUnit
    {
        public int x, y;
        public byte value;
    }

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
                LastInstance.addressTextBox.Enabled = LastInstance.connectButton.Enabled = !connected;
                LastInstance.changeModelButton.Enabled = LastInstance.cullFaceModeButton.Enabled = connected;
                if (value)
                {
                    Application.Idle += LastInstance.mainForm_onIdle;
                    if (stopwatch == null)
                        stopwatch = Stopwatch.StartNew();
                    else stopwatch.Restart();
                }
                else
                {
                    Application.Idle -= LastInstance.mainForm_onIdle;
                    stopwatch?.Stop();
                }
                LastInstance.glControl1.Invalidate();
            }
        }
        
        private static List<GraphicObject> graphicObjects;
        private static Model[] models;
        private static Camera camera;
        private static Stopwatch stopwatch;
        private static object locker = new object();
        private static string lastSentCoordinates;
        private static bool isCullingFaces;

        private static int lastTick;
        private static string lastFrameRate;
        private static int frameRate;

        public static string CalculateFrameRate()
        {
            if (Environment.TickCount - lastTick >= 1000)
            {
                string outputMode = Model.OutputMode == OutputMode.Lines ? "Вывод линий" : "Вывод треугольников";
                string cullingMode = isCullingFaces ? "Отсечение граней" : "Нет отсечения граней";
                lastFrameRate = $"Лабораторная работа №5, клиент; FPS: {frameRate.ToString()}; {outputMode}; {cullingMode}";
                frameRate = 0;
                lastTick = Environment.TickCount;
            }
            frameRate++;
            return lastFrameRate;
        }

        public MainForm()
        {
            LastInstance = this;
            InitializeComponent();

            glControl1 = new GLControl(new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(32), 24, 0, 8));
            glControl1.BackColor = System.Drawing.Color.Black;
            glControl1.Location = new System.Drawing.Point(12, 12);
            glControl1.Name = "glControl1";
            glControl1.Size = new System.Drawing.Size(590, 474);
            glControl1.TabIndex = 1;
            glControl1.VSync = false;
            glControl1.Load += new EventHandler(glControl1_Load);
            glControl1.Paint += new PaintEventHandler(glControl1_Paint);
            glControl1.Resize += new EventHandler(glControl1_Resize);
            glControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(glControl1);

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

        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float aspect = glControl1.Width / glControl1.Height;
            if (aspect == 0)
                aspect = glControl1.Height / glControl1.Width;
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(0.785398f, aspect, 0.01f, 5000.0f); //0,785398 это 45 градусов в радианах
            GL.LoadMatrix(ref perspective);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            //Установим фоновый цвет
            //GL.ClearColor(new OpenTK.Graphics.Color4(105, 29, 142, 0)); //сиреневый цвет
            GL.ClearColor(new OpenTK.Graphics.Color4(0, 0, 0, 0));
            //Не будем ничего рисовать, пока не подключимся к серверу
            if (Connected)
            {
                //Для начала очистим буфер
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                //Настройки для просмотра
                //Настройка позиции "глаз"
                camera.SetCamera();
                //Отображаем все элементы
                foreach (GraphicObject graphicObject in graphicObjects)
                    graphicObject.Show();
                //Поменяем местами буферы
                glControl1.SwapBuffers();
            }
        }

        private void CoordinatesReceived(string message)
        {
            lock (locker)
            {
                message = message.Remove(0, 6);
                string[] coordinates = message.Split(';');
                camera.RadianX = float.Parse(coordinates[0]);
                camera.RadianY = float.Parse(coordinates[1]);
                camera.Radius = float.Parse(coordinates[2]);
                if (!Connected)
                    Invoke((MethodInvoker)delegate
                    {
                        Connected = true;
                    });
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            addressTextBox.Enabled = connectButton.Enabled = false;
            ClientMode.Start(addressTextBox.Text);
            ClientMode.Client.OnReceive += (message) =>
            {
                string[] messages = message.Split('&');
                foreach (string msg in messages)
                {
                    if (msg.StartsWith("arr"))
                    {
                        graphicObjects = new List<GraphicObject>();
                        MapUnit[] units = JsonConvert.DeserializeObject<MapUnit[]>(msg.Remove(0, 3));
                        int rowsCount = units.Max((unit) => unit.x) + 1;
                        int columnsCount = units.Max((unit) => unit.y) + 1;
                        Invoke((MethodInvoker)delegate
                        {
                            models[2] = Model.CreateFlat(rowsCount, columnsCount);
                        });
                        graphicObjects.Add(new GraphicObject(models[2], new Vector3(0, 0, 0), 0));
                        foreach (MapUnit unit in units)
                        {
                            switch (unit.value)
                            {
                                case 1:
                                    graphicObjects.Add(new GraphicObject(models[0], new Vector3((unit.x - rowsCount / 2) * 2, 0, (unit.y - columnsCount / 2) * 2), 0));
                                    break;
                                case 2:
                                    graphicObjects.Add(new GraphicObject(models[1], new Vector3((unit.x - rowsCount / 2) * 2, 0, (unit.y - columnsCount / 2) * 2), 0));
                                    break;
                            }
                        }
                        ClientMode.Client.SendMessage("ackarr");
                    }
                    else if (msg.StartsWith("close"))
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            Connected = false;
                            GL.ClearColor(new OpenTK.Graphics.Color4(0, 0, 0, 0));
                            graphicObjects.Clear();
                        });
                    }
                    else if (msg.StartsWith("coords"))
                        CoordinatesReceived(msg);
                }
            };
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
            }
            glControl1.Invalidate();
            Text = CalculateFrameRate();

            //log.Add($"UP: {state.IsKeyDown(Key.Up).ToString():6} DOWN: {state.IsKeyDown(Key.Down).ToString():6} LEFT: {state.IsKeyDown(Key.Left).ToString():6} RIGHT: {state.IsKeyDown(Key.Right).ToString():6} PLUS: {(state.IsKeyDown(Key.Plus) || state.IsKeyDown(Key.KeypadPlus)).ToString():6} MINUS: {(state.IsKeyDown(Key.Minus) || state.IsKeyDown(Key.KeypadMinus)).ToString():6} X: {camera.Eye.X} Y: {camera.Eye.Y} Z: {camera.Eye.Z}");
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            ChangeCullingFaces(true);
            Model.OutputMode = OutputMode.Triangles;

            models = new Model[]
            {
                Model.CreateCube(),
                Model.CreateParallelepiped(),
                null
            };  
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StreamWriter sw = new StreamWriter("log.txt");
            foreach (string line in log)
                sw.WriteLine(line);
            sw.Flush();
            sw.Close();
        }

        private void changeModelButton_Click(object sender, EventArgs e)
        {
            int temp = (int)Model.OutputMode;
            Model.OutputMode = (OutputMode)(++temp % Enum.GetNames(typeof(OutputMode)).Length);

            string outputMode = Model.OutputMode == OutputMode.Lines ? "Вывод линий" : "Вывод треугольников";
            string cullingMode = isCullingFaces ? "Отсечение граней" : "Нет отсечения граней";
            Text = $"Лабораторная работа №5, клиент; FPS: {frameRate.ToString()}; {outputMode}; {cullingMode}";
        }

        private void cullFaceModeButton_Click(object sender, EventArgs e)
        {
            ChangeCullingFaces(!isCullingFaces);
        }

        private void ChangeCullingFaces(bool value)
        {
            if (value)
            {
                GL.Enable(EnableCap.CullFace);
                GL.FrontFace(FrontFaceDirection.Ccw);
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }
            isCullingFaces = value;

            string outputMode = Model.OutputMode == OutputMode.Lines ? "Вывод линий" : "Вывод треугольников";
            string cullingMode = isCullingFaces ? "Отсечение граней" : "Нет отсечения граней";
            Text = $"Лабораторная работа №5, клиент; FPS: {frameRate.ToString()}; {outputMode}; {cullingMode}";
        }
    }
}
