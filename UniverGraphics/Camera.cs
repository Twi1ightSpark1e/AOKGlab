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
        public Vector3 Eye { get; set; }
        public Vector3 EyeOld { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Angle { get; set; }
        public string ChangedCoordinates { get; private set; }

        public Directions CurrentDirection { get; set; }
        public float Speed => 5f;

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
            Vector3 target = Target;
            Vector3 vNewView;
            Vector3 vView;

            // Получим наш вектор взгляда (направление, куда мы смотрим)
            vView.X = target.X - eye.X;    //направление по X
            vView.Y = target.Y - eye.Y;    //направление по Y
            vView.Z = target.Z - eye.Z;    //направление по Z

            ChangedCoordinates = string.Empty;
            if (CurrentDirection.HasFlag(Directions.Up))
            {
                eye.Y += Speed * millisecondsElapsed / 1000; //вверх
                ChangedCoordinates += $"y={eye.Y};";
            }
            if (CurrentDirection.HasFlag(Directions.Down))
            {
                eye.Y -= Speed * millisecondsElapsed / 1000; //вниз
                ChangedCoordinates += $"y={eye.Y};";
            }
            if (CurrentDirection.HasFlag(Directions.Left))
            {
                float angle = Speed * millisecondsElapsed / 1000 * 2;
                float x = 0;
                float y = 1;
                float z = 0;
                float cosTheta = (float)Math.Cos(angle);
                float sinTheta = (float)Math.Sin(angle);
                // Найдем позицию X
                eye.X = (cosTheta + (1 - cosTheta) * x * x) * vView.X;
                eye.X += ((1 - cosTheta) * x * y - z * sinTheta) * vView.Y;
                eye.X += ((1 - cosTheta) * x * z + y * sinTheta) * vView.Z;

                // Найдем позицию Y
                eye.Y = ((1 - cosTheta) * x * y + z * sinTheta) * vView.X;
                eye.Y += (cosTheta + (1 - cosTheta) * y * y) * vView.Y;
                eye.Y += ((1 - cosTheta) * y * z - x * sinTheta) * vView.Z;

                // И позицию Z
                eye.Z = ((1 - cosTheta) * x * z - y * sinTheta) * vView.X;
                eye.Z += ((1 - cosTheta) * y * z + x * sinTheta) * vView.Y;
                eye.Z += (cosTheta + (1 - cosTheta) * z * z) * vView.Z;
                ChangedCoordinates += $"x={eye.X};";
                ChangedCoordinates += $"y={eye.Y};";
                ChangedCoordinates += $"z={eye.Z};";
            }
            if (CurrentDirection.HasFlag(Directions.Right))
            {
                eye.Z -= Speed * millisecondsElapsed / 1000; //идём вправо
                ChangedCoordinates += $"z={eye.Z};";
            }
            if (CurrentDirection.HasFlag(Directions.Forward))
            {
                eye.X -= Speed * millisecondsElapsed / 1000; //приближаемся
                ChangedCoordinates += $"x={eye.X};";
            }
            if (CurrentDirection.HasFlag(Directions.Backward))
            {
                eye.X += Speed * millisecondsElapsed / 1000; //отдаляемся
                ChangedCoordinates += $"x={eye.X};";
            }
            ChangedCoordinates = ChangedCoordinates.Trim(';');
            Eye = eye;
            Target = target;
        }
    }
}
