using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace UniverGraphics
{
    public partial class MainForm : Form
    {
        public static MainForm LastInstance { get; private set; }

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
                LastInstance.nextButton.Enabled = connected;
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
                LastInstance.nextButton.Enabled = listening;
            }

        }

        private static int index;
        public static int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                LastInstance.glControl1.Refresh();
            }
        }
        private static List<(sbyte red, sbyte green, sbyte blue)> colors = new List<(sbyte red, sbyte green, sbyte blue)>()
        {
            (127,  127, 127),
            (-65,  -56,  76),
            (109, -100, -92),
            (127,   -1, -89),
            (111,  100,  48),
            (-16,   18,  62),
            ( 67,   67,  67),
        };

        public MainForm()
        {
            LastInstance = this;
            InitializeComponent();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            Index = (Index + 1) % colors.Count;
            if (Connected)
                ClientMode.SendColor(Index);
            else if (Listening)
                ServerMode.SendColorAll(Index);
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.Perspective(45.0f, glControl1.Width / glControl1.Height, 0.01f, 5000.0f);
            GL.LoadMatrix(ref perspective);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            //Установим фоновый цвет
            GL.ClearColor(0.22f, 0.88f, 0.11f, 0.5f);
            //Не будем ничего рисовать, пока не подключимся к серверу
            if (Connected || Listening)
            { 
                //Для начала очистим буфер
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                //Настройки для просмотра
                //Настройка позиции "глаз"
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                Matrix4 modelview = Matrix4.LookAt(3, 3, 5, 0, 0, 0, 0, 1, 0);
                GL.LoadMatrix(ref modelview);
                //Установим цвет фигуры
                (sbyte red, sbyte green, sbyte blue) color = colors[Index];
                GL.Color3(color.red, color.green, color.blue);
                //Нарисуем фигуру
                PaintCube();
                //Поменяем местами буферы
                glControl1.SwapBuffers();
            }
        }

        private void PaintCube()
        {
            //ребро 1
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(1, 1, -1);
            GL.Vertex3(1, -1, -1);
            GL.Vertex3(-1, -1, -1);
            GL.Vertex3(-1, 1, -1);
            GL.End();
            //ребро 2
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, -1, -1);
            GL.Vertex3(1, -1, -1);
            GL.Vertex3(1, -1, 1);
            GL.Vertex3(-1, -1, 1);
            GL.End();
            //ребро 3
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, 1, -1);
            GL.Vertex3(-1, -1, -1);
            GL.Vertex3(-1, -1, 1);
            GL.Vertex3(-1, 1, 1);
            GL.End();
            //ребро 4
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(1, 1, 1);
            GL.Vertex3(1, -1, 1);
            GL.Vertex3(1, -1, -1);
            GL.Vertex3(1, 1, -1);
            GL.End();
            //ребро 5
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, 1, -1);
            GL.Vertex3(-1, 1, 1);
            GL.Vertex3(1, 1, 1);
            GL.Vertex3(1, 1, -1);
            GL.End();
            //ребро 6
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, 1, 1);
            GL.Vertex3(-1, -1, 1);
            GL.Vertex3(1, -1, 1);
            GL.Vertex3(1, 1, 1);
            GL.End();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            serverButton.Enabled = addressTextBox.Enabled = connectButton.Enabled = false;
            ClientMode.Start(addressTextBox.Text);
            Text = "Лабораторная работа №1 (клиент)";
        }

        private void serverButton_Click(object sender, EventArgs e)
        {
            serverButton.Enabled = addressTextBox.Enabled = connectButton.Enabled = false;
            ServerMode.Start(this);
            Text = "Лабораторная работа №1 (сервер)";
        }
    }
}
