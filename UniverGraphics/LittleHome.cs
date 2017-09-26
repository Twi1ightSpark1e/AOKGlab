using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace UniverGraphics
{
    class LittleHome
    {
        public int Angle { get; set; }
        public (byte red, byte green, byte blue) Color { get; set; }
        public Vector3 TranslateVector { get; set; }

        public LittleHome(int angle, (byte red, byte green, byte blue) color, Vector3 translate)
        {
            Angle = angle;
            Color = color;
            TranslateVector = translate;
        }

        public void Show()
        {
            GL.PushMatrix();
            GL.Translate(TranslateVector);
            GL.Rotate(Angle, 0, 1, 0);
            //Установим цвет фигуры
            GL.Color3(Color.red, Color.green, Color.blue);
            //Нарисуем фигуру
            // PaintLittleHome();
            Teapot.DrawWireTeapot(1.0f);
            GL.PopMatrix();
        }

        private void PaintLittleHome()
        {
            //поверхность 1
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(1, 1, -1);
            GL.Vertex3(1, -1, -1);
            GL.Vertex3(-1, -1, -1);
            GL.Vertex3(-1, 1, -1);
            GL.End();
            //поверхность 2
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, -1, -1);
            GL.Vertex3(1, -1, -1);
            GL.Vertex3(1, -1, 1);
            GL.Vertex3(-1, -1, 1);
            GL.End();
            //поверхность 3
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, 1, -1);
            GL.Vertex3(-1, -1, -1);
            GL.Vertex3(-1, -1, 1);
            GL.Vertex3(-1, 1, 1);
            GL.End();
            //поверхность 4
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(1, 1, 1);
            GL.Vertex3(1, -1, 1);
            GL.Vertex3(1, -1, -1);
            GL.Vertex3(1, 1, -1);
            GL.End();
            //поверхность 5
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, 1, -1);
            GL.Vertex3(-1, 1, 1);
            GL.Vertex3(1, 1, 1);
            GL.Vertex3(1, 1, -1);
            GL.End();
            //поверхность 6
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(-1, 1, 1);
            GL.Vertex3(-1, -1, 1);
            GL.Vertex3(1, -1, 1);
            GL.Vertex3(1, 1, 1);
            GL.End();
            ///направляющая пирамида
            //поверхность 1
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(1, 1, 1);
            GL.Vertex3(2, 0, 0);
            GL.Vertex3(1, -1, 1);
            GL.End();
            //поверхность 2
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(1, -1, 1);
            GL.Vertex3(2, 0, 0);
            GL.Vertex3(1, -1, -1);
            GL.End();
            //поверхность 3
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(1, -1, -1);
            GL.Vertex3(2, 0, 0);
            GL.Vertex3(1, 1, -1);
            GL.End();
        }
    }
}
