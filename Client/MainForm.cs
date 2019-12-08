using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Newtonsoft.Json;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Client.Materials;

namespace Client
{
    public partial class MainForm : Form
    {
        public static MainForm LastInstance { get; private set; }
        private ClientSocket clientSocket;
        private bool isActive;
        private bool connected;
        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
                addressTextBox.Enabled = !connected;
                changeModelButton.Enabled = cullFaceModeButton.Enabled = lightingButton.Enabled = filterLevelButton.Enabled = connected;
                connectButton.Click -= connectButton_Click;
                connectButton.Click -= disconnectButton_Click;
                if (value)
                {
                    connectButton.Enabled = true;
                    connectButton.Text = "Отключиться";
                    connectButton.Click -= connectButton_Click;
                    connectButton.Click += disconnectButton_Click;

                    Application.Idle += LastInstance.mainForm_onIdle;
                    if (stopwatch == null)
                        stopwatch = Stopwatch.StartNew();
                    else stopwatch.Restart();
                    if (fpsCountHelper == null)
                        fpsCountHelper = Stopwatch.StartNew();
                    else fpsCountHelper.Restart();

                    AudioManager.Play(SoundType.Ambient, new Vector2(0, 0));
                }
                else
                {
                    connectButton.Enabled = true;
                    connectButton.Text = "Подключиться";
                    connectButton.Click += connectButton_Click;
                    connectButton.Click -= disconnectButton_Click;

                    Application.Idle -= LastInstance.mainForm_onIdle;
                    stopwatch?.Stop();
                    fpsCountHelper?.Stop();

                    GL.ClearColor(new OpenTK.Graphics.Color4(0, 0, 0, 0));
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    LastInstance.glControl1.SwapBuffers();
                }
                LastInstance.glControl1.Invalidate();
            }
        }

        private static MapUnit[] units;
        private static List<GraphicObject> explosions = new List<GraphicObject>();
        private static List<GraphicObject> graphicObjects;
        internal static List<GraphicObject> Scene => graphicObjects;

        //private static List<IMaterial> materials;
        private IMaterial playerMaterial, bombMaterial, lightBarrierMaterial, heavyBarrierMaterial, wallMaterial, flatMaterial, explosionMaterial, particleMaterial;
        private Model playerModel, bombModel, lightBarrierModel, heavyBarrierModel, wallModel, flatModel, explosionModel;
        private int fieldRowsCount, fieldColumnsCount;
        private Light light;
        private static Camera camera;
        private static Stopwatch stopwatch, fpsCountHelper;
        private static bool isCullingFaces;
        private bool waitUntilResultReceive;
        private static float frameRate;

        private Sprite authorsLogo;
        private Sprite bombIcon;
        private Sprite barIcon;
        private bool bombHasBeenPlanted = false;
        private int bombProgress = 0;
        private const int bombBarsCount = 5;

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

            glControl1 = new GLControl(new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(32), 24, 0, 16));
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
            float aspect = (float)glControl1.Width / glControl1.Height;
            if (aspect == 0)
                aspect = (float)glControl1.Width / glControl1.Height;
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
                //Настройка позиции "глаз"
                camera.SetCamera();
                //Включаем источник света
                light.Apply();
                //Отображаем все элементы
                for (int i = 0; i < graphicObjects.Count; i++)
                    graphicObjects[i].Show();
                //Рисуем следы от взрывов
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(-1, -1);
                for (int i = 0; i < explosions.Count; i++)
                    explosions[i].Show();
                GL.Disable(EnableCap.PolygonOffsetFill);
                GL.Disable(EnableCap.Blend);
                //Выключаем  текстурирование перед выводом спрайтов
                Texture.Disable();
                //Рисуем иконку ВУЗа
                authorsLogo.Draw(glControl1.Width - authorsLogo.Width, glControl1.Height - authorsLogo.Height);
                //Иконка бомбы и её прогресс
                if (bombHasBeenPlanted)
                {
                    bombIcon.Draw(0, glControl1.Height - bombIcon.Height);
                    int xOffset = bombIcon.Width + 5;
                    for (int i = 0; i < bombProgress; i++)
                    {
                        barIcon.Draw(xOffset, glControl1.Height - barIcon.Height);
                        xOffset += barIcon.Width + 5;
                    }
                }
                //Поменяем местами буферы
                glControl1.SwapBuffers();
            }
        }

        private void CreateField()
        {
            graphicObjects.Add(new GraphicObject(flatModel, flatMaterial, (0, 0), (0, 0), 0)); //плоская модель
            foreach (MapUnit unit in units)
            {
                switch ((ShapeMode)unit.value)
                {
                    case ShapeMode.LightBarrier:
                        graphicObjects.Add(new GraphicObject(lightBarrierModel, lightBarrierMaterial, (unit.x, unit.z), (fieldRowsCount, fieldColumnsCount), 0));
                        break;
                    case ShapeMode.HeavyBarrier:
                        graphicObjects.Add(new GraphicObject(heavyBarrierModel, heavyBarrierMaterial, (unit.x, unit.z), (fieldRowsCount, fieldColumnsCount), 0));
                        break;
                    case ShapeMode.Wall:
                        graphicObjects.Add(new GraphicObject(wallModel, wallMaterial, (unit.x, unit.z), (fieldRowsCount, fieldColumnsCount), 0));
                        break;
                }
            }
        }

        private void CreatePlayers()
        {
            GraphicObject playerObject = new GraphicObject(playerModel, playerMaterial, players[0], (fieldRowsCount, fieldColumnsCount), 0);
            playerObject.OnSimulationFinished += () =>
            {
                waitUntilResultReceive = false;
            };
            graphicObjects.Insert(0, playerObject);
            playerObjects.Add(playerObject);
            for (int i = 1; i < players.Count; i++)
            {
                var enemyObject = new GraphicObject(playerModel, playerMaterial, players.ElementAt(i), (fieldRowsCount, fieldColumnsCount), 0);
                graphicObjects.Insert(0, enemyObject);
                playerObjects.Add(enemyObject);
            }
        }

        private void PlayersHandler(string message)
        {
            var coordinates = message.Remove(0, 7).Split('(', ')').ToList().Where((str) => str != string.Empty);
            foreach (string coordinate in coordinates)
            {
                string[] xz = coordinate.Split(';');
                players.Add((int.Parse(xz[0]), int.Parse(xz[1])));
            }
            try
            {
                clientSocket.SendMessage("ackplayers");
            }
            catch
            {
                Connected = false;
            }
        }

        private void NewPlayerHandler(string message)
        {
            var coordinates = message.Remove(0, 10).Split('(', ')').ToList().Where((str) => str != string.Empty);
            string[] xz = coordinates.ElementAt(0).Split(';');
            players.Add((int.Parse(xz[0]), int.Parse(xz[1])));
            var enemyObject = new GraphicObject(playerModel, playerMaterial, (int.Parse(xz[0]), int.Parse(xz[1])), (fieldRowsCount, fieldColumnsCount), 0);
            graphicObjects.Add(enemyObject);
            playerObjects.Add(enemyObject);
        }

        private void DeletePlayerHandler(string message)
        {
            var coordinates = message.Remove(0, 10).Split('(', ')').ToList().Where((str) => str != string.Empty);
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
                    for (int i = 0; i < playerObjects.Count; i++)
                        if (playerObjects[i].Position.Equals(coords))
                        {
                            playerObjects.RemoveAt(i);
                            break;
                        }
                    break;
                }
            }
        }

        private void FieldHandler(string message)
        {
            graphicObjects = new List<GraphicObject>();
            units = JsonConvert.DeserializeObject<MapUnit[]>(message.Remove(0, 5));
            fieldColumnsCount = units.Max((unit) => unit.x) + 1;
            fieldRowsCount = units.Max((unit) => unit.z) + 1;
            AudioManager.Initialize((fieldColumnsCount, fieldRowsCount));
            Invoke((MethodInvoker)delegate
            {
                flatModel = Model.CreateFlat(fieldRowsCount, fieldColumnsCount);
            });
            CreateField();
            CreatePlayers();
            if (!Connected)
                Invoke((MethodInvoker)delegate
                {
                    Connected = true;
                });
        }

        private void CloseHandler()
        {
            Invoke((MethodInvoker)delegate
            {
                Connected = false;
                graphicObjects.Clear();
                explosions.Clear();
            });
            AudioManager.UnloadAll();
        }

        private void MoveHandler(string message)
        {
            List<string> splitted = message.Split('(', ')').ToList();
            splitted.RemoveAt(0); //нулевой элемент - координаты передвигающегося кубика; первый элемент - его направление движения
            string[] xz = splitted[0].Split(';');
            MoveDirection direction = (MoveDirection)Enum.Parse(typeof(MoveDirection), splitted[1]);
            foreach (GraphicObject playerObject in playerObjects)
            {
                if (playerObject.Position.Equals((int.Parse(xz[0]), int.Parse(xz[1]))))
                {
                    playerObject.Move(direction);
                    break;
                }
            }
            AudioManager.Play(SoundType.Shift, new Vector2(int.Parse(xz[0]), int.Parse(xz[1])));
            Debug.WriteLine(JsonConvert.SerializeObject(playerObjects.Select((player) => player.Position)));
        }

        private void BombHandler(string message)
        {
            List<string> splitted = message.Split('(', ')').ToList();
            splitted.RemoveAt(0);
            string[] xz = splitted[0].Split(';');
            string[] radiusTime = splitted[1].Split(';');
            int bombTriggerRadius = int.Parse(radiusTime[0]);
            float bombTriggerTime = float.Parse(radiusTime[1]);
            GraphicObject bombObject = new GraphicObject(bombModel, bombMaterial, ((int.Parse(xz[0]), int.Parse(xz[1]))), (fieldColumnsCount, fieldRowsCount), 0);
            graphicObjects.Add(bombObject);
            Thread bombTriggerThread = new Thread((triggerTime) =>
            {
                bombHasBeenPlanted = true;
                bombProgress = bombBarsCount;
                while (bombProgress > 0)
                {
                    Thread.Sleep((int)((float)triggerTime * 1000 / bombBarsCount));
                    bombProgress--;
                }
                for (int i = 0; i < graphicObjects.Count; i++)
                {
                    if ((graphicObjects[i].Position.x >= bombObject.Position.x - bombTriggerRadius) &&
                        (graphicObjects[i].Position.x <= bombObject.Position.x + bombTriggerRadius) &&
                        (graphicObjects[i].Position.z >= bombObject.Position.z - bombTriggerRadius) &&
                        (graphicObjects[i].Position.z <= bombObject.Position.z + bombTriggerRadius) &&
                        (graphicObjects[i].CurrentModel.Shape == ShapeMode.LightBarrier))
                    {
                        graphicObjects.RemoveAt(i--);
                    }
                }
                graphicObjects.Remove(bombObject);
                explosions.Add(new GraphicObject(explosionModel, explosionMaterial, (int.Parse(xz[0]), int.Parse(xz[1])), (fieldColumnsCount, fieldRowsCount), 0));
                bombHasBeenPlanted = false;
                AudioManager.Play(SoundType.Explosion, new Vector2(int.Parse(xz[0]), int.Parse(xz[1])));
                ParticleSystem.Create(new Vector2(int.Parse(xz[0]), int.Parse(xz[1])), 2f);
            });
            bombTriggerThread.Start(bombTriggerTime);
            waitUntilResultReceive = false;
        }

        private void ErrorHandler(string message)
        {
            if ((message.Remove(0, 3) == "move") || ((message.Remove(0, 3) == "bomb")))
                waitUntilResultReceive = false;
            else if (message.Remove(0, 3) == "place")
            {
                MessageBox.Show("Сервер не может обработать нового игрока");
                Connected = false;
            }
        }

        private void DeadHandler()
        {
            AudioManager.Play(SoundType.Death, new Vector2(1, 0));
            Invoke((MethodInvoker)delegate
            {
                clientSocket.Disconnect();
                MessageBox.Show("Вы взорвались на бомбе!");
            });
            CloseHandler();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            addressTextBox.Enabled = connectButton.Enabled = false;
            players.Clear();
            playerObjects.Clear();
            explosions.Clear();
            clientSocket = new ClientSocket(addressTextBox.Text, 4115);
            clientSocket.OnDisconnect += (reason) =>
            {
                Connected = false;
            };
            clientSocket.OnReceive += (msg) =>
            {
                if (msg.StartsWith("players"))
                    PlayersHandler(msg);
                else if (msg.StartsWith("newplayer"))
                    NewPlayerHandler(msg);
                else if (msg.StartsWith("delplayer"))
                    DeletePlayerHandler(msg);
                else if (msg.StartsWith("field"))
                    FieldHandler(msg);
                else if (msg.StartsWith("close"))
                    CloseHandler();
                else if (msg.StartsWith("move"))
                    MoveHandler(msg);
                else if (msg.StartsWith("bomb"))
                    BombHandler(msg);
                else if (msg.StartsWith("err"))
                    ErrorHandler(msg);
                else if (msg.StartsWith("dead"))
                    DeadHandler();
            };
            try
            {
                clientSocket.Connect();
            }
            catch (AggregateException exception)
            {
                MessageBox.Show($"Произошла ошибка: {exception.Flatten()}");
                Connected = false;
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            clientSocket.Disconnect();
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
                camera.CurrentDirection = dirs;
                camera.Simulate(millisecondsElapsed / 1000);
                AudioManager.SetListener(camera.Eye, camera.RadianX, camera.RadianY);

                int move = 0;
                if (state.IsKeyDown(Key.W))
                    move = (int)(keyHandlers['W']());
                if (state.IsKeyDown(Key.A))
                    move = (int)(keyHandlers['A']());
                if (state.IsKeyDown(Key.S))
                    move = (int)(keyHandlers['S']());
                if (state.IsKeyDown(Key.D))
                    move = (int)(keyHandlers['D']());
                if (!waitUntilResultReceive)
                {
                    if (move != 0)
                    {
                        MoveDirection moveDirection = (MoveDirection)move;
                        waitUntilResultReceive = true;
                        try
                        { 
                            clientSocket.SendMessage($"move{moveDirection}");
                        }
                        catch
                        {
                            Connected = false;
                        }
                    }
                    else if (state.IsKeyDown(Key.Space))
                    {
                        waitUntilResultReceive = true;
                        try
                        {
                            clientSocket.SendMessage("bomb");
                        }
                        catch
                        {
                            Connected = false;
                        }
                    }
                }
            }
            for (int i = 0; i < playerObjects.Count; i++)
                if (playerObjects[i].IsMoving)
                    playerObjects[i].Simulate(millisecondsElapsed / 1000);
            if (fpsCountHelper.ElapsedMilliseconds >= 1000)
            {
                frameRate = 1 / (millisecondsElapsed / 1000);
                fpsCountHelper.Restart();
                SetText();
            }
            ParticleSystem.Simulate(millisecondsElapsed / 1000);
            glControl1.Invalidate();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            if (new SelectConfigurationForm().ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return;
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            ChangeCullingFaces(true);
            Model.OutputMode = OutputMode.Triangles;
                        
            // Модели
            lightBarrierModel = Model.CreateLightBarrier();
            heavyBarrierModel = Model.CreateHeavyBarrier();
            playerModel = Model.CreatePlayer();
            bombModel = Model.CreateBomb();
            wallModel = Model.CreateWall();
            explosionModel = Model.CreateExplosion();
            // Материалы
            lightBarrierMaterial = PhongTexturedMaterial.CreateLightBarrier();
            heavyBarrierMaterial = PhongTexturedMaterial.CreateHeavyBarrier();
            playerMaterial = PhongMaterial.CreatePlayer();
            wallMaterial = PhongTexturedMaterial.CreateWall();
            flatMaterial = PhongTexturedMaterial.CreateFlat();
            bombMaterial = PhongTexturedMaterial.CreateBomb();
            explosionMaterial = PhongTexturedMaterial.CreateExplosion();

            light = new Light()
            {
                Position = new Vector3(0, 6, -10),
                Ambient = new Vector3(1f, 1f, 1f),
                Diffuse = new Vector3(1f, 1f, 1f),
                Specular = new Vector3(1f, 1f, 1f),
                LightName = LightName.Light0
            };
            Light.LightMode = LightMode.All;

            authorsLogo = new Sprite("sprites/authors.png", true);
            bombIcon = new Sprite("sprites/bomb.ico", true);
            barIcon = new Sprite("sprites/bar.bmp", true);

            foreach (Control control in this.Controls)
            {
                control.PreviewKeyDown += new PreviewKeyDownEventHandler(control_PreviewKeyDown);
            }

            Connected = false;

            WavLoader.InitializeWaves();

            var particleTexture = Texture.GetTextureByName("textures/cloud.png");
            particleMaterial = new ColorTexturedMaterial()
            {
                Color = new Vector4(255, 255, 255, 255),
                Texture = particleTexture
            };
            ParticleSystem.Material = particleMaterial;
        }

        private void changeModelButton_Click(object sender, EventArgs e)
        {
            int temp = (int)Model.OutputMode;
            Model.OutputMode = (OutputMode)(++temp % Enum.GetNames(typeof(OutputMode)).Length);
            SetText();
        }

        private void filterLevelButton_Click(object sender, EventArgs e)
        {
            int filter = (int)Texture.FilterMode;
            Texture.FilterMode = (FilterMode)((++filter) % Enum.GetNames(typeof(FilterMode)).Length);
            SetText();
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
            SetText();
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            isActive = true;
        }

        private void lightingButton_Click(object sender, EventArgs e)
        {
            Light.LightMode = (LightMode)((int)++Light.LightMode % (Enum.GetNames(typeof(LightMode)).Length));
            SetText();
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            isActive = false;
        }

        private void SetText()
        {
            string outputMode = Model.OutputMode == OutputMode.Lines ? "Вывод линий" : "Вывод треугольников";
            string cullingMode = isCullingFaces ? "Отсечение граней" : "Нет отсечения граней";
            string lightMode = string.Empty;
            switch (Light.LightMode)
            {
                case LightMode.All:
                    lightMode = "Полное освещение";
                    break;
                case LightMode.OnlyAmbient:
                    lightMode = "Общее освещение";
                    break;
                case LightMode.OnlyDiffuse:
                    lightMode = "Диффузное освещение";
                    break;
                case LightMode.OnlySpecular:
                    lightMode = "Спекулярное освещение";
                    break;
            }
            Text = $"FPS: {frameRate.ToString("N2")}; {outputMode}; {cullingMode}; {lightMode}; {Texture.FilterMode.ToString()}";
        }

        void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                e.IsInputKey = true;
            }
        }
    }
}
