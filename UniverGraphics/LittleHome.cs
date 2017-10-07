using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
            //Teapot.DrawWireTeapot(1.0f);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, MainForm.IdVb);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, MainForm.IdIb);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, MainForm.ModelPoint.Size(), 0);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, MainForm.points.Length);
            GL.DrawElements(PrimitiveType.Triangles, MainForm.indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DisableClientState(ArrayCap.VertexArray);

            //PaintCube();
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

        private void PaintCube()
        {
            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(-1, 0, -1);
            GL.Vertex3(-1, 0, 1);
            GL.Vertex3(1, 0, 1);
            GL.End();

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(1, 0, 1);
            GL.Vertex3(1, 0, -1);
            GL.Vertex3(-1, 0, -1);
            GL.End();
        }
    }
}
