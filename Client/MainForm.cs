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

        private static List<Material> materials;
        private static List<Model> models;
        private Model playerModel;
        private int fieldRowsCount, fieldColumnsCount;
        private Light light;
        private static Camera camera;
        private static Stopwatch stopwatch;
        private static bool isCullingFaces;
        private bool waitUntilMoveReceive;

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

        private List<(int x, int z)> players = new List<(int x, int z)>();
        private List<GraphicObject> playerObjects = new List<GraphicObject>();
        private static MoveDirection movePositiveX() => MoveDirection.Right;
        private static MoveDirection movePositiveZ() => MoveDirection.Down;
        private static MoveDirection moveNegativeX() => MoveDirection.Left;
        private static MoveDirection moveNegativeZ() => MoveDirection.Up;
        private delegate MoveDirection MoveKeyHandler();
        private Dictionary<char, MoveKeyHandler> keyHandlers = new Dictionary<char, MoveKeyHandler>();
        private List<MoveKeyHandler> moves = new List<MoveKeyHandler>()
        {
            movePositiveZ,
            movePositiveX,
            moveNegativeZ,
            moveNegativeX
        };
        private List<char> keys = new List<char>()
        {
            'W', 'A', 'S', 'D'
        };

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
            camera.OnChangedDirections += (state) =>
            {
                byte key = 0;
                for (int i = (5 - state) % 4; i < ((5 - state) % 4 + 4); i++)
                {
                    keyHandlers[keys[key++]] = moves[i%4];
                }
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
                light.Apply();
                //Отображаем все элементы
                for (int i = 0; i < graphicObjects.Count; i++)
                    graphicObjects[i].Show();
                //Поменяем местами буферы
                glControl1.SwapBuffers();
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            addressTextBox.Enabled = connectButton.Enabled = false;
            players.Clear();
            ClientMode.Start(addressTextBox.Text);
            ClientMode.Client.OnReceive += (message) =>
            {
                string[] messages = message.Split('&');
                foreach (string msg in messages)
                {
                    if (msg.StartsWith("players"))
                    {
                        var coordinates = msg.Remove(0, 7).Split('(', ')').ToList().Where((str) => str != string.Empty);
                        foreach (string coordinate in coordinates)
                        {
                            string[] xz = coordinate.Split(';');
                            players.Add((int.Parse(xz[0]), int.Parse(xz[1])));
                        }
                        ClientMode.Client.SendMessage("ackplayers");
                    }
                    else if (msg.StartsWith("newplayer"))
                    {
                        var coordinates = msg.Remove(0, 10).Split('(', ')').ToList().Where((str) => str != string.Empty);
                        string[] xz = coordinates.ElementAt(0).Split(';');
                        players.Add((int.Parse(xz[0]), int.Parse(xz[1])));
                        graphicObjects.Add(new GraphicObject(playerModel, materials[2], (int.Parse(xz[0]), int.Parse(xz[1])), (fieldRowsCount, fieldColumnsCount), 0));
                    }
                    else if (msg.StartsWith("delplayer"))
                    {
                        var coordinates = msg.Remove(0, 10).Split('(', ')').ToList().Where((str) => str != string.Empty);
                        string[] xz = coordinates.ElementAt(0).Split(';');
                        foreach (var player in players)
                        {
                            if (int.Parse(xz[0]) == player.x && int.Parse(xz[1]) == player.z)
                            {
                                (int x, int z) coords = (int.Parse(xz[0]), int.Parse(xz[1]));
                                for (int i = 0; i < players.Count; i++)
                                    if (players[i].Equals(coords))
                                    {
                                        players.RemoveAt(i);
                                        break;
                                    }
                                for (int i = 0; i < graphicObjects.Count; i++)
                                    if (graphicObjects[i].Position.Equals(coords))
                                    {
                                        graphicObjects.RemoveAt(i);
                                        break;
                                    }
                                break;
                            }
                        }
                    }
                    else if (msg.StartsWith("field"))
                    {
                        graphicObjects = new List<GraphicObject>();
                        MapUnit[] units = JsonConvert.DeserializeObject<MapUnit[]>(msg.Remove(0, 5));
                        fieldColumnsCount = units.Max((unit) => unit.x) + 1;
                        fieldRowsCount = units.Max((unit) => unit.z) + 1;
                        Invoke((MethodInvoker)delegate
                        {
                            models.Add(Model.CreateFlat(fieldRowsCount, fieldColumnsCount));
                        });
                        //Получаем модели объектов
                        Model lightBarrierModel = null, heavyBarrierModel = null, wallModel = null, flatModel = null;
                        foreach (Model model in models)
                        {
                            switch (model.Shape)
                            {
                                case ShapeMode.LightBarrier:
                                    lightBarrierModel = model;
                                    break;
                                case ShapeMode.HeavyBarrier:
                                    heavyBarrierModel = model;
                                    break;
                                case ShapeMode.Player:
                                    playerModel = model;
                                    break;
                                case ShapeMode.Wall:
                                    wallModel = model;
                                    break;
                                case ShapeMode.Empty:
                                    flatModel = model;
                                    break;
                            }
                        }
                        if (lightBarrierModel == null || heavyBarrierModel == null || playerModel == null || wallModel == null || flatModel == null)
                            throw new Exception("Одна из ключевых моделей не определена");

                        graphicObjects.Add(new GraphicObject(flatModel, materials[4], (0, 0), (0, 0), 0)); //плоская модель
                        foreach (MapUnit unit in units)
                        {
                            switch ((ShapeMode)unit.value)
                            {
                                case ShapeMode.LightBarrier:
                                    graphicObjects.Add(new GraphicObject(lightBarrierModel, materials[0], (unit.x, unit.z), (fieldRowsCount, fieldColumnsCount), 0));
                                    break;
                                case ShapeMode.HeavyBarrier:
                                    graphicObjects.Add(new GraphicObject(heavyBarrierModel, materials[1], (unit.x, unit.z), (fieldRowsCount, fieldColumnsCount), 0));
                                    break;
                                case ShapeMode.Wall:
                                    graphicObjects.Add(new GraphicObject(wallModel, materials[3], (unit.x, unit.z), (fieldRowsCount, fieldColumnsCount), 0));
                                    break;
                            }
                        }
                        GraphicObject playerObject = new GraphicObject(playerModel, materials[2], players[0], (fieldRowsCount, fieldColumnsCount), 0);
                        playerObject.OnSimulationFinished += () =>
                        {
                            waitUntilMoveReceive = false;
                        };
                        graphicObjects.Insert(0, playerObject);
                        playerObjects.Add(playerObject);
                        for (int i = 1; i < players.Count; i++)
                        {
                            var enemyObject = new GraphicObject(playerModel, materials[2], players.ElementAt(i), (fieldRowsCount, fieldColumnsCount), 0);
                            graphicObjects.Insert(0, enemyObject);
                            playerObjects.Add(enemyObject);
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
                        List<string> splitted = msg.Split('(', ')').ToList();
                        splitted.RemoveAt(0); //нулевой элемент - координаты передвигающегося кубика; первый элемент - его направление движения
                        string[] xz = splitted[0].Split(';');
                        MoveDirection direction = (MoveDirection)Enum.Parse(typeof(MoveDirection), splitted[1]);
                        foreach (GraphicObject graphicObject in graphicObjects)
                        {
                            if (graphicObject.Position.Equals((int.Parse(xz[0]), int.Parse(xz[1]))))
                            {
                                graphicObject.Move(direction);
                                //waitUntilMoveReceive = false;
                                break;
                            }                            
                        }
                    }
                    else if (msg.StartsWith("err"))
                    {
                        if (msg.Remove(0, 3) == "move")
                            waitUntilMoveReceive = false;
                        else if (msg.Remove(0, 3) == "place")
                        {
                            MessageBox.Show("Сервер не может обработать нового игрока");
                            Connected = false;
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
                label8.Text = "radianX: " + camera.RadianX;
                label9.Text = "radianY: "+ camera.RadianY;
                label10.Text = "radius: " + camera.Radius;
                camera.CurrentDirection = dirs;
                camera.Simulate(millisecondsElapsed / 1000);
                int move = 0;
                if (state.IsKeyDown(Key.W))
                    move = (int)(keyHandlers['W']());
                if (state.IsKeyDown(Key.A))
                    move = (int)(keyHandlers['A']());
                if (state.IsKeyDown(Key.S))
                    move = (int)(keyHandlers['S']());
                if (state.IsKeyDown(Key.D))
                    move = (int)(keyHandlers['D']());
                if (move != 0 && !waitUntilMoveReceive)
                {
                    MoveDirection moveDirection = (MoveDirection)move;
                    waitUntilMoveReceive = true;
                    ClientMode.Client.SendMessage($"move{moveDirection}");
                    Debug.WriteLine("Move sent");
                }
            }
            for (int i = 0; i < playerObjects.Count; i++)
                if (playerObjects[i].IsMoving)
                    playerObjects[i].Simulate(millisecondsElapsed / 1000);
            glControl1.Invalidate();
            Text = CalculateFrameRate();
            label10.Text = waitUntilMoveReceive.ToString();

            //log.Add($"UP: {state.IsKeyDown(Key.Up).ToString():6} DOWN: {state.IsKeyDown(Key.Down).ToString():6} LEFT: {state.IsKeyDown(Key.Left).ToString():6} RIGHT: {state.IsKeyDown(Key.Right).ToString():6} PLUS: {(state.IsKeyDown(Key.Plus) || state.IsKeyDown(Key.KeypadPlus)).ToString():6} MINUS: {(state.IsKeyDown(Key.Minus) || state.IsKeyDown(Key.KeypadMinus)).ToString():6} X: {camera.Eye.X} Y: {camera.Eye.Y} Z: {camera.Eye.Z}");
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            ChangeCullingFaces(true);
            Model.OutputMode = OutputMode.Triangles;

            models = new List<Model>()
            {
                Model.CreateLightBarrier(),
                Model.CreateHeavyBarrier(),
                Model.CreatePlayer(),
                Model.CreateWall()
            };

            materials = new List<Material>()
            {
                Material.CreateLightBarrier(),
                Material.CreateHeavyBarrier(),
                Material.CreatePlayer(),
                Material.CreateWall(),
                Material.CreateFlat()
            };

            light = new Light()
            {
                Position = new Vector3(10, 10, 10),
                Ambient = new Vector3(1, 1, 1),
                Diffuse = new Vector3(.7f, .7f, .7f),
                Specular = new Vector3(.5f, .5f, .5f),
                LightName = LightName.Light0
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
