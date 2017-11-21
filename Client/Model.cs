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
    struct ModelPoint
    {
        Vector3 coordinates, normals;

        public ModelPoint((float x, float y, float z) coordinates, (float x, float y, float z) normals)
        {
            this.coordinates = new Vector3(coordinates.x, coordinates.y, coordinates.z);
            this.normals = new Vector3(normals.x, normals.y, normals.z);
        }

        public ModelPoint(Vector3 coordinates, Vector3 normals)
        {
            this.coordinates = coordinates;
            this.normals = normals;
        }
        // размер структуры
        public static int Size() => Vector3.SizeInBytes * 2;
        // смещение поля coordinates
        public static IntPtr CoordinatesOffset() => Marshal.OffsetOf<ModelPoint>("coordinates");
        // смещение поля normals
        public static IntPtr NormalsOffset() => Marshal.OffsetOf<ModelPoint>("normals");
    }

    class Model
    {
        private ModelPoint[] points;
        private ushort[] indices;
        public int IdVertexBuffer { get; private set; } // идентификатор буфера вершин
        public int IdIndexBuffer { get; private set; } // идентификатор буфера индексов
        public static OutputMode OutputMode { get; set; }  // режим вывода
        public ShapeMode Shape { get; private set; }  // форма объекта
        // загрузчики моделей из файлов
        private static _3dsReader boxReader = new _3dsReader("models/Box.3DS");
        private static _3dsReader chamferBoxReader = new _3dsReader("models/ChamferBox.3DS");
        private static _3dsReader sphereReader = new _3dsReader("models/Sphere.3DS");

        public Model(ModelPoint[] points, ushort[] indices, ShapeMode shape)
        {
            this.points = points;
            this.indices = indices;
            Shape = shape;
            InitializeVBO();
        }
        #region фабрика моделей
        public static Model CreateLightBarrier()
        {
            ModelPoint[] points = new ModelPoint[chamferBoxReader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in chamferBoxReader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, chamferBoxReader.Normals[i]);
                i++;
            }
            return new Model(points, chamferBoxReader.Indices, ShapeMode.LightBarrier);
        }

        public static Model CreateHeavyBarrier()
        {
            ModelPoint[] points = new ModelPoint[chamferBoxReader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in chamferBoxReader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, chamferBoxReader.Normals[i]);
                i++;
            }
            return new Model(points, chamferBoxReader.Indices, ShapeMode.HeavyBarrier);
        }

        public static Model CreatePlayer()
        {
            ModelPoint[] points = new ModelPoint[sphereReader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in sphereReader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, sphereReader.Normals[i]);
                i++;
            }
            return new Model(points, sphereReader.Indices, ShapeMode.Player);
        }

        public static Model CreateBomb()
        {
            ModelPoint[] points = new ModelPoint[sphereReader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in sphereReader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, sphereReader.Normals[i]);
                i++;
            }
            return new Model(points, sphereReader.Indices, ShapeMode.Bomb);
        }

        public static Model CreateWall()
        {
            ModelPoint[] points = new ModelPoint[boxReader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in boxReader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, boxReader.Normals[i]);
                i++;
            }
            return new Model(points, boxReader.Indices, ShapeMode.Wall);
        }

        public static Model CreateFlat(int width, int height)
        {
            float y = -1f;
            List<ModelPoint> points = new List<ModelPoint>();
            List<ushort> indices = new List<ushort>();
            ushort startIndex = 0;
            for (int x = -width / 2 * 2; x <= width; x += 2)
            {
                for (int z = -height / 2 * 2; z <= height; z += 2)
                {
                    points.Add(new ModelPoint((x - 1, y, z - 1), (0, 1, 0)));
                    points.Add(new ModelPoint((x - 1, y, z + 1), (0, 1, 0)));
                    points.Add(new ModelPoint((x + 1, y, z - 1), (0, 1, 0)));
                    points.Add(new ModelPoint((x + 1, y, z + 1), (0, 1, 0)));
                    indices.AddRange(new ushort[6] { startIndex, (ushort)(startIndex + 1), (ushort)(startIndex + 2), (ushort)(startIndex + 2), (ushort)(startIndex + 1), (ushort)(startIndex + 3) });
                    startIndex += 4;
                }
            }
            return new Model(points.ToArray(), indices.ToArray(), ShapeMode.Empty);
        }
        #endregion
        public void Show()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, IdVertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IdIndexBuffer);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, ModelPoint.Size(), ModelPoint.CoordinatesOffset());
            GL.NormalPointer(NormalPointerType.Float, ModelPoint.Size(), ModelPoint.NormalsOffset());
            switch (Model.OutputMode)
            {
                case OutputMode.Triangles:
                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedShort, 0);
                    break;
                case OutputMode.Lines:
                    GL.DrawElements(PrimitiveType.Lines, indices.Length, DrawElementsType.UnsignedShort, 0);
                    break;
                default:
                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedShort, 0);
                    break;
            }
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
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
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(ushort)), indices, BufferUsageHint.StaticDraw);
        }
    }
}
