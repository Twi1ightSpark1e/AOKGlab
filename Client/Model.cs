using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;

namespace Client
{
    enum ShapeMode
    {
        Player = -1, Empty, LightBarrier, HeavyBarrier, Wall
    }

    enum OutputMode
    {
        Triangles, Lines
    }

    struct ModelPoint
    {
        Vector3 coordinates, colors;

        public ModelPoint((float x, float y, float z) coordinates, (float r, float g, float b) colors)
        {
            this.coordinates = new Vector3(coordinates.x, coordinates.y, coordinates.z);
            this.colors = new Vector3(colors.r, colors.g, colors.b);
        }

        public static int Size()
        {
            return Vector3.SizeInBytes * 2;
        }

        public static IntPtr CoordinatesOffset()
        {
            return Marshal.OffsetOf<ModelPoint>("coordinates");
        }

        public static IntPtr ColorsOffset()
        {
            return Marshal.OffsetOf<ModelPoint>("colors");
        }
    }

    class Model
    {
        private ModelPoint[] points;
        private uint[] indices;
        public int IdVertexBuffer { get; private set; }
        public int IdIndexBuffer { get; private set; }
        public static OutputMode OutputMode { get; set; }
        public bool IsMovable { get; private set; }
        public ShapeMode Shape { get; private set; }

        public Model(ModelPoint[] points, uint[] indices, ShapeMode shape)
        {
            this.points = points;
            this.indices = indices;
            Shape = shape;
            switch (shape)
            {
                case ShapeMode.Empty:
                case ShapeMode.Wall:
                case ShapeMode.HeavyBarrier:
                    IsMovable = false;
                    break;
                case ShapeMode.Player:
                case ShapeMode.LightBarrier:
                    IsMovable = true;
                    break;
            }
            InitializeVBO();
        }

        public static Model CreateLightBarrier()
        {
            return new Model(new ModelPoint[]
            {
                //front
                new ModelPoint((-1, -1 , 1), (1, 1, 1)),
                new ModelPoint((1, -1, 1), (1, 1, 1)),
                new ModelPoint((1, 1, 1), (1, 1, 1)),
                new ModelPoint((-1, 1, 1), (1, 1, 1)),
                //right
                new ModelPoint((1, 1, 1), (1, 0, 0)),
                new ModelPoint((1, 1, -1), (1, 0, 0)),
                new ModelPoint((1, -1, -1), (1, 0, 0)),
                new ModelPoint((1, -1, 1), (1, 0, 0)),
                //back
                new ModelPoint((-1, -1, -1), (0, 1, 0)),
                new ModelPoint((1, -1, -1), (0, 1, 0)),
                new ModelPoint(( 1, 1, -1), (0, 1, 0)),
                new ModelPoint((-1, 1, -1), (0, 1, 0)),
                //left
                new ModelPoint((-1, -1, -1), (0, 0, 1)),
                new ModelPoint((-1, -1, 1), (0, 0, 1)),
                new ModelPoint((-1, 1, 1), (0, 0, 1)),
                new ModelPoint((-1, 1, -1), (0, 0, 1)),
                //upper
                new ModelPoint(( 1, 1, 1), (1, 1, 0)),
                new ModelPoint((-1, 1, 1), (1, 1, 0)),
                new ModelPoint((-1, 1, -1), (1, 1, 0)),
                new ModelPoint((1, 1, -1), (1, 1, 0)),
                //bottom
                new ModelPoint((-1, -1, -1), (1, 0, 1)),
                new ModelPoint((1, -1, -1), (1, 0, 1)),
                new ModelPoint(( 1, -1, 1), (1, 0, 1)),
                new ModelPoint((-1, -1, 1), (1, 0, 1)),
            }, 
            new uint[] 
            {
                0,  1,  2,  0,  2,  3,   //front
                6,  5,  4,  7,  6,  4,   //right
                10, 9,  8,  11, 10, 8,   //back
                12, 13, 14, 12, 14, 15,  //left
                18, 17, 16, 19, 18, 16,  //upper
                20, 21, 22, 20, 22, 23   //bottom
            }, ShapeMode.LightBarrier);
        }

        public static Model CreateHeavyBarrier()
        {
            return new Model(new ModelPoint[]
            {
                //front
                new ModelPoint((-1, -1 , 1), (0, 0, 0)),
                new ModelPoint((1, -1, 1), (0, 0, 0)),
                new ModelPoint((1, 1, 1), (0, 0, 0)),
                new ModelPoint((-1, 1, 1), (0, 0, 0)),
                //right
                new ModelPoint((1, 1, 1), (0, 0, 0)),
                new ModelPoint((1, 1, -1), (0, 0, 0)),
                new ModelPoint((1, -1, -1), (0, 0, 0)),
                new ModelPoint((1, -1, 1), (0, 0, 0)),
                //back
                new ModelPoint((-1, -1, -1), (0, 0, 0)),
                new ModelPoint((1, -1, -1), (0, 0, 0)),
                new ModelPoint(( 1, 1, -1), (0, 0, 0)),
                new ModelPoint((-1, 1, -1), (0, 0, 0)),
                //left
                new ModelPoint((-1, -1, -1), (0, 0, 0)),
                new ModelPoint((-1, -1, 1), (0, 0, 0)),
                new ModelPoint((-1, 1, 1), (0, 0, 0)),
                new ModelPoint((-1, 1, -1), (0, 0, 0)),
                //upper
                new ModelPoint(( 1, 1, 1), (0, 0, 0)),
                new ModelPoint((-1, 1, 1), (0, 0, 0)),
                new ModelPoint((-1, 1, -1), (0, 0, 0)),
                new ModelPoint((1, 1, -1), (0, 0, 0)),
                //bottom
                new ModelPoint((-1, -1, -1), (0, 0, 0)),
                new ModelPoint((1, -1, -1), (0, 0, 0)),
                new ModelPoint(( 1, -1, 1), (0, 0, 0)),
                new ModelPoint((-1, -1, 1), (0, 0, 0)),
            },
            new uint[]
            {
                0,  1,  2,  0,  2,  3,   //front
                6,  5,  4,  7,  6,  4,   //right
                10, 9,  8,  11, 10, 8,   //back
                12, 13, 14, 12, 14, 15,  //left
                18, 17, 16, 19, 18, 16,  //upper
                20, 21, 22, 20, 22, 23   //bottom
            }, ShapeMode.HeavyBarrier);
        }

        public static Model CreatePlayer()
        {
            return new Model(new ModelPoint[]
            {
                //front
                new ModelPoint((-0.9f, -0.9f , 0.9f), (0, 1, 0)),
                new ModelPoint((0.9f, -0.9f, 0.9f), (0, 1, 0)),
                new ModelPoint((0.9f, 0.9f, 0.9f), (0, 1, 0)),
                new ModelPoint((-0.9f, 0.9f, 0.9f), (0, 1, 0)),
                //right
                new ModelPoint((0.9f, 0.9f, 0.9f), (0, 0, 1)),
                new ModelPoint((0.9f, 0.9f, -0.9f), (0, 0, 1)),
                new ModelPoint((0.9f, -0.9f, -0.9f), (0, 0, 1)),
                new ModelPoint((0.9f, -0.9f, 0.9f), (0, 0, 1)),
                //back
                new ModelPoint((-0.9f, -0.9f, -0.9f), (1, 1, 1)),
                new ModelPoint((0.9f, -0.9f, -0.9f), (1, 1, 1)),
                new ModelPoint(( 0.9f, 0.9f, -0.9f), (1, 1, 1)),
                new ModelPoint((-0.9f, 0.9f, -0.9f), (1, 1, 1)),
                //left
                new ModelPoint((-0.9f, -0.9f, -0.9f), (1, 0, 0)),
                new ModelPoint((-0.9f, -0.9f, 0.9f), (1, 0, 0)),
                new ModelPoint((-0.9f, 0.9f, 0.9f), (1, 0, 0)),
                new ModelPoint((-0.9f, 0.9f, -0.9f), (1, 0, 0)),
                //upper
                new ModelPoint(( 0.9f, 0.9f, 0.9f), (1, 0, 1)),
                new ModelPoint((-0.9f, 0.9f, 0.9f), (1, 0, 1)),
                new ModelPoint((-0.9f, 0.9f, -0.9f), (1, 0, 1)),
                new ModelPoint((0.9f, 0.9f, -0.9f), (1, 0, 1)),
                //bottom
                new ModelPoint((-0.9f, -0.9f, -0.9f), (1, 1, 1)),
                new ModelPoint((0.9f, -0.9f, -0.9f), (1, 1, 1)),
                new ModelPoint(( 0.9f, -0.9f, 0.9f), (1, 1, 1)),
                new ModelPoint((-0.9f, -0.9f, 0.9f), (1, 1, 1)),
            },
            new uint[]
            {
                0,  1,  2,  0,  2,  3,   //front
                6,  5,  4,  7,  6,  4,   //right
                10, 9,  8,  11, 10, 8,   //back
                12, 13, 14, 12, 14, 15,  //left
                18, 17, 16, 19, 18, 16,  //upper
                20, 21, 22, 20, 22, 23   //bottom
            }, ShapeMode.Player);
        }

        public static Model CreateWall()
        {
            return new Model(new ModelPoint[]
            {
                //front
                new ModelPoint((-1, -1 , 1), (1, 1, 1)),
                new ModelPoint((1, -1, 1), (1, 1, 1)),
                new ModelPoint((1, 3, 1), (1, 1, 1)),
                new ModelPoint((-1, 3, 1), (1, 1, 1)),
                //right
                new ModelPoint((1, 3, 1), (1, 0, 0)),
                new ModelPoint((1, 3, -1), (1, 0, 0)),
                new ModelPoint((1, -1, -1), (1, 0, 0)),
                new ModelPoint((1, -1, 1), (1, 0, 0)),
                //back
                new ModelPoint((-1, -1, -1), (0, 1, 0)),
                new ModelPoint((1, -1, -1), (0, 1, 0)),
                new ModelPoint(( 1, 3, -1), (0, 1, 0)),
                new ModelPoint((-1, 3, -1), (0, 1, 0)),
                //left
                new ModelPoint((-1, -1, -1), (0, 0, 1)),
                new ModelPoint((-1, -1, 1), (0, 0, 1)),
                new ModelPoint((-1, 3, 1), (0, 0, 1)),
                new ModelPoint((-1, 3, -1), (0, 0, 1)),
                //upper
                new ModelPoint(( 1, 3, 1), (1, 1, 0)),
                new ModelPoint((-1, 3, 1), (1, 1, 0)),
                new ModelPoint((-1, 3, -1), (1, 1, 0)),
                new ModelPoint((1, 3, -1), (1, 1, 0)),
                //bottom
                new ModelPoint((-1, -1, -1), (1, 0, 1)),
                new ModelPoint((1, -1, -1), (1, 0, 1)),
                new ModelPoint(( 1, -1, 1), (1, 0, 1)),
                new ModelPoint((-1, -1, 1), (1, 0, 1)),
            }, 
            new uint[]
            {
                0,  1,  2,  0,  2,  3,   //front
                6,  5,  4,  7,  6,  4,   //right
                10, 9,  8,  11, 10, 8,   //back
                12, 13, 14, 12, 14, 15,  //left
                18, 17, 16, 19, 18, 16,  //upper
                20, 21, 22, 20, 22, 23   //bottom
            }, ShapeMode.Wall);
        }

        public static Model CreateFlat(int width, int height)
        {
            float y = -1f;
            List<ModelPoint> points = new List<ModelPoint>();
            List<uint> indices = new List<uint>();
            uint startIndex = 0;
            for (int x = -width / 2 * 2; x <= width; x += 2)
            {
                for (int z = -height / 2 * 2; z <= height; z += 2)
                {
                    points.Add(new ModelPoint((x - 1, y, z - 1), (0.75f, 0.75f, 0.75f)));
                    points.Add(new ModelPoint((x - 1, y, z + 1), (0.75f, 0.75f, 0.75f)));
                    points.Add(new ModelPoint((x + 1, y, z - 1), (0.75f, 0.75f, 0.75f)));
                    points.Add(new ModelPoint((x + 1, y, z + 1), (0.75f, 0.75f, 0.75f)));
                    indices.AddRange(new uint[6] { startIndex, startIndex + 1, startIndex + 2, startIndex + 2, startIndex + 1, startIndex + 3 });
                    startIndex += 4;
                }
            }
            return new Model(points.ToArray(), indices.ToArray(), ShapeMode.Empty);
        }
        
        public void Show()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, IdVertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IdIndexBuffer);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, ModelPoint.Size(), ModelPoint.CoordinatesOffset());
            GL.ColorPointer(3, ColorPointerType.Float, ModelPoint.Size(), ModelPoint.ColorsOffset());
            switch (Model.OutputMode)
            {
                case OutputMode.Triangles:
                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                case OutputMode.Lines:
                    GL.DrawElements(PrimitiveType.Lines, indices.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                default:
                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
                    break;
            }
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);
        }

        private void InitializeVBO()
        {
            IdVertexBuffer = GL.GenBuffer();
            IdIndexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, IdVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(points.Length * ModelPoint.Size()), points, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IdIndexBuffer);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, ModelPoint.Size(), 0);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
        }
    }
}
