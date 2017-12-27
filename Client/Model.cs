using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;

using static Client.Configuration.Configuration;

namespace Client
{
    struct ModelPoint
    {
        Vector3 coordinates, normals;
        Vector2 texCoords;

        public ModelPoint((float x, float y, float z) coordinates, (float x, float y, float z) normals, (float s, float t) texCoords)
        {
            this.coordinates = new Vector3(coordinates.x, coordinates.y, coordinates.z);
            this.normals = new Vector3(normals.x, normals.y, normals.z);
            this.texCoords = new Vector2(texCoords.s, texCoords.t);
        }

        public ModelPoint(Vector3 coordinates, Vector3 normals, Vector2 texCoords)
        {
            this.coordinates = coordinates;
            this.normals = normals;
            this.texCoords = texCoords;
        }
        // размер структуры
        public static int Size() => Vector3.SizeInBytes * 2 + Vector2.SizeInBytes;
        // смещение поля coordinates
        public static IntPtr CoordinatesOffset() => Marshal.OffsetOf<ModelPoint>("coordinates");
        // смещение поля normals
        public static IntPtr NormalsOffset() => Marshal.OffsetOf<ModelPoint>("normals");
        // смещение поля texcoords
        public static IntPtr TexCoordsOffset() => Marshal.OffsetOf<ModelPoint>("texCoords");
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
        // public static _3dsReader boxReader;
        //private static _3dsReader chamferBoxReader = new _3dsReader("models/ChamferBox.3DS");
        // public static _3dsReader sphereReader;

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
            _3dsReader reader = _3dsReader.GetReaderByFilename(LightObject.model.fileName, LightObject.model.roughNormals);
            ModelPoint[] points = new ModelPoint[reader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in reader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, reader.Normals[i], reader.TexCoords[i]);
                i++;
            }
            return new Model(points, reader.Indices, ShapeMode.LightBarrier);
        }

        public static Model CreateHeavyBarrier()
        {
            _3dsReader reader = _3dsReader.GetReaderByFilename(HeavyObject.model.fileName, HeavyObject.model.roughNormals);
            ModelPoint[] points = new ModelPoint[reader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in reader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, reader.Normals[i], reader.TexCoords[i]);
                i++;
            }
            return new Model(points, reader.Indices, ShapeMode.HeavyBarrier);
        }

        public static Model CreatePlayer()
        {
            _3dsReader reader = _3dsReader.GetReaderByFilename(Player.model.fileName, Player.model.roughNormals);
            ModelPoint[] points = new ModelPoint[reader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in reader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, reader.Normals[i], reader.TexCoords[i]);
                i++;
            }
            return new Model(points, reader.Indices, ShapeMode.Player);
        }

        public static Model CreateBomb()
        {
            _3dsReader reader = _3dsReader.GetReaderByFilename(Bomb.model.fileName, Bomb.model.roughNormals);
            ModelPoint[] points = new ModelPoint[reader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in reader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, reader.Normals[i], reader.TexCoords[i]);
                i++;
            }
            return new Model(points, reader.Indices, ShapeMode.Bomb);
        }

        public static Model CreateWall()
        {
            _3dsReader reader = _3dsReader.GetReaderByFilename(Wall.model.fileName, Wall.model.roughNormals);
            ModelPoint[] points = new ModelPoint[reader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in reader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, reader.Normals[i], reader.TexCoords[i]);
                i++;
            }
            return new Model(points, reader.Indices, ShapeMode.Wall);
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
                    points.Add(new ModelPoint((x - 1, y, z - 1), (0, 1, 0), (0, 0)));
                    points.Add(new ModelPoint((x - 1, y, z + 1), (0, 1, 0), (0, 1)));
                    points.Add(new ModelPoint((x + 1, y, z - 1), (0, 1, 0), (1, 0)));
                    points.Add(new ModelPoint((x + 1, y, z + 1), (0, 1, 0), (1, 1)));
                    indices.AddRange(new ushort[6] { startIndex, (ushort)(startIndex + 1), (ushort)(startIndex + 2), (ushort)(startIndex + 2), (ushort)(startIndex + 1), (ushort)(startIndex + 3) });
                    startIndex += 4;
                }
            }
            return new Model(points.ToArray(), indices.ToArray(), ShapeMode.Empty);
        }

        public static Model CreateExplosion()
        {
            ModelPoint[] points = new ModelPoint[]
            {
                new ModelPoint((-1, -1, -1), (0, 1, 0), (0, 0)),
                new ModelPoint((-1, -1, +1), (0, 1, 0), (0, 1)),
                new ModelPoint((+1, -1, -1), (0, 1, 0), (1, 0)),
                new ModelPoint((+1, -1, +1), (0, 1, 0), (1, 1))
            };
            ushort[] indices = new ushort[]
            {
                0, 1, 2, 2, 1, 3
            };
            return new Model(points, indices, ShapeMode.Decal);
        }
        #endregion
        public void Show()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.ClientActiveTexture(TextureUnit.Texture0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, IdVertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IdIndexBuffer);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, ModelPoint.Size(), ModelPoint.CoordinatesOffset());
            GL.NormalPointer(NormalPointerType.Float, ModelPoint.Size(), ModelPoint.NormalsOffset());
            GL.TexCoordPointer(2, TexCoordPointerType.Float, ModelPoint.Size(), ModelPoint.TexCoordsOffset());
            switch (OutputMode)
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
            GL.DisableClientState(ArrayCap.TextureCoordArray);
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
