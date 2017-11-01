﻿using System;
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
        Bomb = -2, Player, Empty, LightBarrier, HeavyBarrier, Wall
    }

    enum OutputMode
    {
        Triangles, Lines
    }

    struct ModelPoint
    {
        Vector3 coordinates, normals;

        public ModelPoint((float x, float y, float z) coordinates, (float r, float g, float b) colors, (float x, float y, float z) normals)
        {
            this.coordinates = new Vector3(coordinates.x, coordinates.y, coordinates.z);
            this.normals = new Vector3(normals.x, normals.y, normals.z);
        }

        public ModelPoint(Vector3 coordinates, (float r, float g, float b) colors, Vector3 normals)
        {
            this.coordinates = coordinates;
            this.normals = normals;
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

        public static IntPtr NormalsOffset()
        {
            return Marshal.OffsetOf<ModelPoint>("normals");
        }
    }

    class Model
    {
        private ModelPoint[] points;
        private ushort[] indices;
        public int IdVertexBuffer { get; private set; }
        public int IdIndexBuffer { get; private set; }
        public static OutputMode OutputMode { get; set; }
        public bool IsMovable { get; private set; }
        public ShapeMode Shape { get; private set; }

        private static _3dsReader boxReader = new _3dsReader("models/Box.3DS");
        private static _3dsReader chamferBoxReader = new _3dsReader("models/ChamferBox.3DS");
        private static _3dsReader sphereReader = new _3dsReader("models/Sphere.3DS");

        public Model(ModelPoint[] points, ushort[] indices, ShapeMode shape)
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
            ModelPoint[] points = new ModelPoint[chamferBoxReader.Vertices.Length];
            int i = 0;
            foreach (Vector3 vertex in chamferBoxReader.Vertices)
            {
                points[i] = new ModelPoint(vertex * 2, (1f, .82f, .09f), chamferBoxReader.Normals[i]);
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
                points[i] = new ModelPoint(vertex * 2, (.4f, .4f, .4f), chamferBoxReader.Normals[i]);
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
                points[i] = new ModelPoint(vertex * 2, (.2f, .8f, 1f), sphereReader.Normals[i]);
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
                points[i] = new ModelPoint(vertex * 2, (.2f, .8f, 1f), sphereReader.Normals[i]);
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
                points[i] = new ModelPoint(vertex * 2, (.2f, .2f, .2f), boxReader.Normals[i]);
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
                    points.Add(new ModelPoint((x - 1, y, z - 1), (0.75f, 0.75f, 0.75f), (0, 1, 0)));
                    points.Add(new ModelPoint((x - 1, y, z + 1), (0.75f, 0.75f, 0.75f), (0, 1, 0)));
                    points.Add(new ModelPoint((x + 1, y, z - 1), (0.75f, 0.75f, 0.75f), (0, 1, 0)));
                    points.Add(new ModelPoint((x + 1, y, z + 1), (0.75f, 0.75f, 0.75f), (0, 1, 0)));
                    indices.AddRange(new ushort[6] { startIndex, (ushort)(startIndex + 1), (ushort)(startIndex + 2), (ushort)(startIndex + 2), (ushort)(startIndex + 1), (ushort)(startIndex + 3) });
                    startIndex += 4;
                }
            }
            return new Model(points.ToArray(), indices.ToArray(), ShapeMode.Empty);
        }
        
        public void Show()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
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
            GL.DisableClientState(ArrayCap.ColorArray);
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
