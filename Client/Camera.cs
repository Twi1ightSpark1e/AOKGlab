using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Client
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

        public Directions CurrentDirection { get; set; }
        public float Speed => 2f;

        public void SetCamera()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            SetEye(RadianX, RadianY, Radius);
            Matrix4 modelview = Matrix4.LookAt(Eye, Target, Up);
            GL.LoadMatrix(ref modelview);
        }
        public int ViewPosition()
        {
            while (RadianX > 6.28f)
                RadianX -= 6.28f;
            while (RadianX < 0)
                RadianX += 6.28f;
            if ((RadianX > 3.14 * 5 / 4) && (RadianX <= 3.14 * 7 / 4))
                return 1;
            else if ((RadianX > 3.14 * 7 / 4) || (RadianX <= 3.14 * 1 / 4))
                return 2;
            else if ((RadianX > 3.14 * 1 / 4) && (RadianX <= 3.14 * 3 / 4))
                return 3;
            else if ((RadianX > 3.14 * 3 / 4) && (RadianX <= 3.14 * 5 / 4))
                return 4;
            return 1;
        }
        public void SetEye(float radianX, float radianY, float radius)
        {
            var eye = Eye;
            eye.X = (float)(Math.Cos(RadianY) * Radius * Math.Cos(RadianX));
            eye.Y = (float)(Math.Sin(RadianY) * Radius);
            eye.Z = (float)(Math.Cos(RadianY) * Radius * Math.Sin(RadianX));
            Eye = eye;
        }

        public void Simulate(float secondsElapsed)
        {
            if (CurrentDirection.HasFlag(Directions.Up)) //вверх
            { 
                if (RadianY < 1.396)
                    RadianY += (float)Math.Sqrt(Speed) * secondsElapsed;
            }
            if (CurrentDirection.HasFlag(Directions.Down)) //вниз
            {
                if (RadianY > 0.0873)
                    RadianY -= (float)Math.Sqrt(Speed) * secondsElapsed;
            }
            if (CurrentDirection.HasFlag(Directions.Left)) //влево
            {
                RadianX += ((float)Math.PI / 2) * Speed * secondsElapsed;
            }
            if (CurrentDirection.HasFlag(Directions.Right)) //вправо
            { 
                RadianX -= ((float)Math.PI / 2) * Speed * secondsElapsed;
            }
            if (CurrentDirection.HasFlag(Directions.Forward)) //приближаемся
            {
                if (Radius > 10)
                    Radius -= (float)Math.Pow(Speed, 4) * secondsElapsed;
            }
            if (CurrentDirection.HasFlag(Directions.Backward)) //отдаляемся
            {
                if (Radius < 100)
                    Radius += (float)Math.Pow(Speed, 4) * secondsElapsed;
            }
        }
    }
}
