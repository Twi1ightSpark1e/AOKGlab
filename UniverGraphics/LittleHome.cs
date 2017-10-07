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
    enum ShapeMode
    {
        Flat, Cube, Parallelepiped
    }

    class LittleHome
    {
        public int Angle { get; set; }
        public (byte red, byte green, byte blue) Color { get; set; }
        public Vector3 TranslateVector { get; set; }
        public ShapeMode Shape { get; set; }
        public ModelPoint[] Vertexes { get; private set; }
        public uint[] Indices { get; private set; }
        public int IdVb { get; private set; }
        public int IdIb { get; private set; }

        public LittleHome(int angle, (byte red, byte green, byte blue) color, Vector3 translate, ShapeMode shape)
        {
            Angle = angle;
            Color = color;
            TranslateVector = translate;
            Shape = shape;
            switch (shape)
            {
                case ShapeMode.Flat:
                    Vertexes = ShapeArrays.FlatPoints;
                    Indices = ShapeArrays.FlatIndices;
                    break;
                case ShapeMode.Cube:
                    Vertexes = ShapeArrays.CubePoints;
                    Indices = ShapeArrays.CubeIndices;
                    break;
                case ShapeMode.Parallelepiped:
                    Vertexes = ShapeArrays.ParallelepipedPoints;
                    Indices = ShapeArrays.ParallelepipedIndices;
                    break;
                default:
                    Vertexes = ShapeArrays.FlatPoints;
                    Indices = ShapeArrays.FlatIndices;
                    break;
            }
            InitializeVBO();
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, IdVb);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IdIb);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, ModelPoint.Size(), ModelPoint.CoordinatesOffset());
            GL.ColorPointer(3, ColorPointerType.Float, ModelPoint.Size(), ModelPoint.ColorsOffset());
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);

            //PaintCube();
            GL.PopMatrix();
        }

        public void InitializeVBO()
        {
            IdVb = GL.GenBuffer();
            IdIb = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, IdVb);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertexes.Length * ModelPoint.Size()), Vertexes, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IdIb);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, ModelPoint.Size(), 0);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(uint)), Indices, BufferUsageHint.StaticDraw);
        }
    }
}
