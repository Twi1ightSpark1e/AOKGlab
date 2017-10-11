using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Client
{
    enum MoveDirection
    {
        Left = -2,
        Up = -1,
        Down = 1,
        Right = 2
    }
    class GraphicObject
    {
        public bool IsPlayerObject => CurrentModel.Shape == ShapeMode.Player;
        public Model CurrentModel { get; set; }
        private (int x, int z) position;
        public (int x, int z) Position
        {
            get => position;
            set
            {
                position = value;
                translation = new Vector3((value.x - xLength / 2) * 2, 0, (value.z - zLength / 2) * 2);
                modelMatrix = Matrix4.Mult(Matrix4.Mult(Matrix4.Identity, Matrix4.CreateTranslation(translation)), Matrix4.CreateRotationY(angle));
            }
        }
        private Vector3 translation;

        private float angle;
        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                modelMatrix = Matrix4.Mult(Matrix4.Mult(Matrix4.Identity, Matrix4.CreateTranslation(translation)), Matrix4.CreateRotationY(angle));
            }
        }
        private int xLength, zLength;

        private Matrix4 modelMatrix;

        public GraphicObject(Model model, (int x, int y) position, (int xMax, int yMax) maxPosition, float angle)
        {
            CurrentModel = model;
            this.angle = angle;
            xLength = maxPosition.xMax;
            zLength = maxPosition.yMax;
            Position = (position.x, position.y);
        }

        public bool CanMove(MoveDirection direction)
        {
            if (!CurrentModel.IsMovable)
                return false;
            int targetX = position.x, targetZ = position.z;
            switch (direction)
            {
                case MoveDirection.Up:
                case MoveDirection.Down:
                    targetZ += Math.Sign((sbyte)direction);
                    break;
                case MoveDirection.Left:
                case MoveDirection.Right:
                    targetX += Math.Sign((sbyte)direction);
                    break;
            }
            var nextFigure = MainForm.Scene.Where((graphicObject) => 
                graphicObject.Position.x == targetX && 
                graphicObject.Position.z == targetZ && 
                graphicObject.CurrentModel.Shape != ShapeMode.Flat).FirstOrDefault();
            if (nextFigure == null) //нет фигуры, только плоскость
                return true;
            if (!IsPlayerObject && nextFigure.CurrentModel.Shape != ShapeMode.Flat)
                return false;
            if (nextFigure.CanMove(direction))
                return true;
            return false;
        }

        public void Show()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.MultMatrix(ref modelMatrix);
            CurrentModel.Show();
            GL.PopMatrix();
        }
    }
}
