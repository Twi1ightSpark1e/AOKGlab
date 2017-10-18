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
        public int x, z;
        public byte value;
    }

    public partial class MainForm : Form
    {
        public static MainForm LastInstance { get; private set; }

        private List<string> log = new List<string>();
        private bool isActive;
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
        internal static List<GraphicObject> Scene => graphicObjects;
        private static List<Model> models;
        private static Camera camera;
        private static Stopwatch stopwatch;
        private static object locker = new object();
        private static bool isCullingFaces;
        private bool waitUntilMoveReceive;

        private static int lastTick;
        private static string lastFrameRate;
        private static int frameRate;

        private GraphicObject playerObject;

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
                RadianX = 1.57f,
                RadianY = 1.4f,
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
                if (Connected)
                    Invoke((MethodInvoker)delegate
                    {
                        glControl1.Invalidate();
                    });
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
                        int columnsCount = units.Max((unit) => unit.x) + 1;
                        int rowsCount = units.Max((unit) => unit.z) + 1;
                        Invoke((MethodInvoker)delegate
                        {
                            models.Add(Model.CreateFlat(rowsCount, columnsCount));
                        });
                        //Получаем модели объектов
                        Model cubeModel = null, playerModel = null, parallelepipedModel = null, flatModel = null;
                        foreach (Model model in models)
                        {
                            switch (model.Shape)
                            {
                                case ShapeMode.Cube:
                                    cubeModel = model;
                                    break;
                                case ShapeMode.Player:
                                    playerModel = model;
                                    break;
                                case ShapeMode.Parallelepiped:
                                    parallelepipedModel = model;
                                    break;
                                case ShapeMode.Flat:
                                    flatModel = model;
                                    break;
                            }
                        }
                        if (cubeModel == null || cubeModel == null || cubeModel == null || flatModel == null)
                            throw new Exception("Одна из ключевых моделей не определена");

                        graphicObjects.Add(new GraphicObject(flatModel, (0, 0), (0, 0), 0)); //плоская модель
                        foreach (MapUnit unit in units)
                        {
                            switch ((ShapeMode)unit.value)
                            {
                                case ShapeMode.Cube:
                                    graphicObjects.Add(new GraphicObject(cubeModel, (unit.x, unit.z), (rowsCount, columnsCount), 0));
                                    break;
                                case ShapeMode.Player:
                                    playerObject = new GraphicObject(playerModel, (unit.x, unit.z), (rowsCount, columnsCount), 0); //кубик игрока
                                    graphicObjects.Add(playerObject);
                                    break;
                                case ShapeMode.Parallelepiped:
                                    graphicObjects.Add(new GraphicObject(parallelepipedModel, (unit.x, unit.z), (rowsCount, columnsCount), 0));
                                    break;
                            }
                        }
                        if (!Connected)
                            Invoke((MethodInvoker)delegate
                            {
                                Connected = true;
                            });
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
                    else if (msg.StartsWith("move"))
                    {
                        MoveDirection direction = (MoveDirection)Enum.Parse(typeof(MoveDirection), msg.Remove(0, 4));
                        if (playerObject.CanMove(direction))
                        {
                            playerObject.Move(direction);
                            waitUntilMoveReceive = false;
                        }
                    }
                }
            };
        }

        private void mainForm_onIdle(object sender, EventArgs e)
        {
            float millisecondsElapsed = (float)stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            var state = Keyboard.GetState();
            Vector3 eye = camera.Eye;
            if (isActive)
            {
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
                label8.Text = "playerX: " + playerObject.Position.x.ToString();
                label9.Text = "playerY: 0"/* + eye.Y.ToString()*/;
                label10.Text = "playerZ: " + playerObject.Position.z.ToString();
                camera.CurrentDirection = dirs;
                camera.Simulate(millisecondsElapsed / 1000);
                int position = camera.ViewPosition();
                int move = 0;
                switch (position)
                {
                    case 1:
                        if (state.IsKeyDown(Key.W))
                            move = (int)(MoveDirection.Down);
                        if (state.IsKeyDown(Key.A))
                            move = (int)(MoveDirection.Right);
                        if (state.IsKeyDown(Key.S))
                            move = (int)(MoveDirection.Up);
                        if (state.IsKeyDown(Key.D))
                            move = (int)(MoveDirection.Left);
                        break;
                    case 2:
                        if (state.IsKeyDown(Key.W))
                            move = (int)(MoveDirection.Left);
                        if (state.IsKeyDown(Key.A))
                            move = (int)(MoveDirection.Down);
                        if (state.IsKeyDown(Key.S))
                            move = (int)(MoveDirection.Right);
                        if (state.IsKeyDown(Key.D))
                            move = (int)(MoveDirection.Up);
                        break;
                    case 3:
                        if (state.IsKeyDown(Key.W))
                            move = (int)(MoveDirection.Up);
                        if (state.IsKeyDown(Key.A))
                            move = (int)(MoveDirection.Left);
                        if (state.IsKeyDown(Key.S))
                            move = (int)(MoveDirection.Down);
                        if (state.IsKeyDown(Key.D))
                            move = (int)(MoveDirection.Right);
                        break;
                    case 4:
                        if (state.IsKeyDown(Key.W))
                            move = (int)(MoveDirection.Right);
                        if (state.IsKeyDown(Key.A))
                            move = (int)(MoveDirection.Up);
                        if (state.IsKeyDown(Key.S))
                            move = (int)(MoveDirection.Left);
                        if (state.IsKeyDown(Key.D))
                            move = (int)(MoveDirection.Down);
                        break;

                }
                if (move != 0 && !waitUntilMoveReceive)
                {
                    MoveDirection moveDirection = (MoveDirection)move;
                    if (playerObject.CanMove(moveDirection))
                    {
                        ClientMode.Client.SendMessage($"move{moveDirection}");
                        waitUntilMoveReceive = true;
                    }
                }
            }
            if (playerObject.IsMoving)
                playerObject.Simulate(millisecondsElapsed / 1000);
            glControl1.Invalidate();
            Text = CalculateFrameRate();

            //log.Add($"UP: {state.IsKeyDown(Key.Up).ToString():6} DOWN: {state.IsKeyDown(Key.Down).ToString():6} LEFT: {state.IsKeyDown(Key.Left).ToString():6} RIGHT: {state.IsKeyDown(Key.Right).ToString():6} PLUS: {(state.IsKeyDown(Key.Plus) || state.IsKeyDown(Key.KeypadPlus)).ToString():6} MINUS: {(state.IsKeyDown(Key.Minus) || state.IsKeyDown(Key.KeypadMinus)).ToString():6} X: {camera.Eye.X} Y: {camera.Eye.Y} Z: {camera.Eye.Z}");
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            ChangeCullingFaces(true);
            Model.OutputMode = OutputMode.Triangles;

            models = new List<Model>()
            {
                Model.CreateCube(),
                Model.CreatePlayer(),
                Model.CreateParallelepiped()
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

        private void MainForm_Activated(object sender, EventArgs e)
        {
            isActive = true;
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            isActive = false;
        }
    }
}
