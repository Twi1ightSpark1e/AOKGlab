using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace UniverGraphics
{
    [Flags]
    enum Directions
    {
        None = 0x1,
        Left = 0x2,
        Right = 0x4,
        Up = 0x8,
        Down = 0x10,
        Forward = 0x20,
        Backward = 0x40
    }

    class Camera
    {
        public float RadianX { get; set; }
        public float RadianY { get; set; }
        public float Radius { get; set; }
        public Vector3 Eye { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public string ChangedCoordinates { get; private set; }

        public Directions CurrentDirection { get; set; }
        public float Speed => 3f;

        public void SetCamera()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            
            Matrix4 modelview = Matrix4.LookAt(Eye, Target, Up);
            GL.LoadMatrix(ref modelview);
        }

        public void Simulate(float millisecondsElapsed)
        {
            Vector3 eye = Eye;
            ChangedCoordinates = string.Empty;
            if (CurrentDirection.HasFlag(Directions.Up)) //вверх
            { 
                if (RadianY < 1.396)
                    RadianY += 1.57f * millisecondsElapsed / 1000;
            }
            if (CurrentDirection.HasFlag(Directions.Down)) //вниз
            {
                if (RadianY > 0.0873)
                    RadianY -= 1.57f * millisecondsElapsed / 1000;
            }
            if (CurrentDirection.HasFlag(Directions.Left)) //влево
            {
                RadianX += ((float)Math.PI / 2) * millisecondsElapsed / 1000;
            }
            if (CurrentDirection.HasFlag(Directions.Right)) //вправо
            { 
                RadianX -= ((float)Math.PI / 2) * millisecondsElapsed / 1000;
            }
            if (CurrentDirection.HasFlag(Directions.Forward)) //приближаемся
            {
                if (Radius > 10)
                    Radius -= Speed * millisecondsElapsed / 1000;
            }
            if (CurrentDirection.HasFlag(Directions.Backward)) //отдаляемся
            {
                if (Radius < 60)
                {
                    Radius += Speed * millisecondsElapsed / 1000;
                }
            }
            Eye = new Vector3((float)(Math.Cos(RadianY) * Radius * Math.Cos(RadianX)),
                              (float)(Math.Sin(RadianY) * Radius),
                              (float)(Math.Cos(RadianY) * Radius * Math.Sin(RadianX)));
            ChangedCoordinates = $"x={eye.X};y={eye.Y};z={eye.Z}";
        }
    }
}
