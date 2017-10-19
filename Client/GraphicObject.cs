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
        None = 0,
        Left = -2,
        Up = -1,
        Down = 1,
        Right = 2
    }
    class GraphicObject
    {
        internal delegate void OnSimulationFinishedDelegate();
        public event OnSimulationFinishedDelegate OnSimulationFinished;

        public bool IsPlayerObject => CurrentModel.Shape == ShapeMode.Player;
        private MoveDirection currentMoveDirection;
        public bool IsMoving => currentMoveDirection != MoveDirection.None;
        public float MoveProgress { get; private set; }
        public Model CurrentModel { get; set; }
        public float Speed => 2f;
        private (int x, int z) destination;
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
        private Vector3 Translation
        {
            set
            {
                translation = value;
                modelMatrix = Matrix4.Mult(Matrix4.Mult(Matrix4.Identity, Matrix4.CreateTranslation(translation)), Matrix4.CreateRotationY(angle));
            }
        }
        private GraphicObject nextObject;

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
            if (!CurrentModel.IsMovable || IsMoving)
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
            nextObject = null;
            foreach (GraphicObject graphicObject in MainForm.Scene)
            {
                if (graphicObject.Position.x == targetX &&
                    graphicObject.Position.z == targetZ &&
                    graphicObject.CurrentModel.Shape != ShapeMode.Empty)
                {
                    nextObject = graphicObject;
                    break;
                }
            }
            if (nextObject == null) //нет фигуры, только плоскость
                return true;
            if (!IsPlayerObject && nextObject.CurrentModel.Shape != ShapeMode.Empty)
                return false;
            if (nextObject.CanMove(direction))
                return true;
            return false;
        }

        public void Move(MoveDirection direction)
        {
            if (!IsMoving)
            {
                MoveProgress = 1;
                currentMoveDirection = direction;
                switch (currentMoveDirection)
                {
                    case MoveDirection.Up:
                    case MoveDirection.Down:
                        destination = (position.x, position.z + Math.Sign((sbyte)currentMoveDirection));
                        break;
                    case MoveDirection.Left:
                    case MoveDirection.Right:
                        destination = (position.x + Math.Sign((sbyte)currentMoveDirection), position.z);
                        break;
                }
                foreach (GraphicObject graphicObject in MainForm.Scene)
                {
                    if (graphicObject.Position.x == destination.x &&
                        graphicObject.Position.z == destination.z &&
                        graphicObject.CurrentModel.Shape != ShapeMode.Empty)
                    {
                        nextObject = graphicObject;
                        break;
                    }
                }
            }
        }

        public void Simulate(float secondsElapsed)
        {
            if (currentMoveDirection != MoveDirection.None)
            {
                MoveProgress = MoveProgress - secondsElapsed * Speed;
                Vector3 nextTranslation;

                switch (currentMoveDirection)
                {
                    case MoveDirection.Up:
                    case MoveDirection.Down:
                        Translation = new Vector3((position.x - xLength / 2) * 2, 0, (position.z - zLength / 2) * 2 + Math.Sign((sbyte)currentMoveDirection) * (1 - MoveProgress) * 2f);
                        nextTranslation = new Vector3(translation.X, 0, translation.Z + Math.Sign((sbyte)currentMoveDirection) * 2);
                        break;
                    case MoveDirection.Left:
                    case MoveDirection.Right:
                        Translation = new Vector3((position.x - xLength / 2) * 2 + Math.Sign((sbyte)currentMoveDirection) * (1 - MoveProgress) * 2f, 0, (position.z - zLength / 2) * 2);
                        nextTranslation = new Vector3(translation.X + Math.Sign((sbyte)currentMoveDirection) * 2, 0, translation.Z);
                        break;
                    default:
                        Translation = new Vector3(0, 0, 0);
                        nextTranslation = new Vector3(0, 0, 0);
                        break;
                }
                if (nextObject != null)
                    nextObject.Translation = nextTranslation;
                if (MoveProgress <= 0)
                {
                    MoveProgress = 0;
                    Position = destination;
                    if (nextObject != null)
                    {
                        switch (currentMoveDirection)
                        {
                            case MoveDirection.Up:
                            case MoveDirection.Down:
                                nextObject.Position = (position.x, position.z + Math.Sign((sbyte)currentMoveDirection));
                                break;
                            case MoveDirection.Left:
                            case MoveDirection.Right:
                                nextObject.Position = (position.x + Math.Sign((sbyte)currentMoveDirection), position.z);
                                break;
                        }
                        nextObject = null;
                    }
                    currentMoveDirection = MoveDirection.None;
                    OnSimulationFinished?.Invoke();
                }
            }
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
