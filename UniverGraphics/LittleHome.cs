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
            //GL.Color3(Color.red, Color.green, Color.blue);
            //Нарисуем фигуру
            //PaintLittleHome();
            //Teapot.DrawWireTeapot(1.0f);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, MainForm.IdVb);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, MainForm.IdIb);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, MainForm.ModelPoint.Size(), MainForm.ModelPoint.CoordinatesOffset());
            GL.ColorPointer(3, ColorPointerType.Float, MainForm.ModelPoint.Size(), MainForm.ModelPoint.ColorsOffset());
            GL.DrawElements(PrimitiveType.Triangles, MainForm.indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);

            //PaintCube();
            GL.PopMatrix();
        }
    }
}
