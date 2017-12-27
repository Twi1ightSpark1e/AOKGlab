using Client.Materials;

using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Client
{
    class GraphicObject
    {
        // событие, поднимаемое тогда, когда объект закончил движение
        internal delegate void OnSimulationFinishedDelegate();
        public event OnSimulationFinishedDelegate OnSimulationFinished;

        private MoveDirection currentMoveDirection; // текущее направление движения или None, если объект стоит на месте
        public bool IsPlayerObject => CurrentModel.Shape == ShapeMode.Player;
        public bool IsMoving => currentMoveDirection != MoveDirection.None;
        public float MoveProgress { get; private set; } // прогресс движения; изменяется от 0 - начальная точка, до 1 - конечная точка
        // внешний вид объекта
        public Model CurrentModel { get; set; }
        public IMaterial CurrentMaterial { get; set; }
        public float Speed => 4f; // скорость передвижения объекта
        // позиция графического объекта
        private (int x, int z) position;
        public (int x, int z) Position
        {
            get => position;
            set
            {
                position = value;
                Translation = new Vector3((value.x - xLength / 2) * 2, 0, (value.z - zLength / 2) * 2); // устанавливаем вектор переноса
            }
        }
        // вектор "переноса", используемый для отображения объекта на карте
        private Vector3 translation;
        private Vector3 Translation
        {
            set
            {
                translation = value;
                // вычисляем матрицу модели, чтобы не перемножать матрицы с рендером каждого кадра
                modelMatrix = Matrix4.Mult(
                    Matrix4.Mult(Matrix4.Identity, Matrix4.CreateTranslation(translation)), 
                    Matrix4.CreateRotationY(angle)); 
            }
        }
        private GraphicObject nextObject; // следующий за движущимся объектом объект, чтобы передвигать и его тоже
        // угол поворота объекта
        private float angle;
        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                // вычисляем матрицу модели, чтобы не перемножать матрицы с рендером каждого кадра
                modelMatrix = Matrix4.Mult(
                    Matrix4.Mult(Matrix4.Identity, Matrix4.CreateTranslation(translation)), 
                    Matrix4.CreateRotationY(angle));
            }
        }
        // размеры игрового поля для точного позиционирования всех объектов в центре
        private int xLength, zLength;
        private Matrix4 modelMatrix;

        private Queue<MoveDirection> moveQueue;

        public GraphicObject(Model model, IMaterial material, (int x, int y) position, (int xMax, int yMax) maxPosition, float angle)
        {
            CurrentModel = model;
            CurrentMaterial = material;
            this.angle = angle;
            xLength = maxPosition.xMax;
            zLength = maxPosition.yMax;
            Position = (position.x, position.y);
            moveQueue = new Queue<MoveDirection>();
        }

        public void Move(MoveDirection direction)
        {
            if (moveQueue.Count == 0 && currentMoveDirection == MoveDirection.None)
            {
                SetMoving(direction);
            }
            else moveQueue.Enqueue(direction);
        }

        private void SetMoving(MoveDirection direction)
        {
            MoveProgress = 0;
            currentMoveDirection = direction;
            // получаем координаты места, в котором окажется объект по окончанию движения
            switch (currentMoveDirection)
            {
                case MoveDirection.Up:
                case MoveDirection.Down:
                    position = (position.x, position.z + Math.Sign((sbyte)currentMoveDirection));
                    break;
                case MoveDirection.Left:
                case MoveDirection.Right:
                    position = (position.x + Math.Sign((sbyte)currentMoveDirection), position.z);
                    break;
            }
            // находим объект по полученным координатам
            foreach (GraphicObject graphicObject in MainForm.Scene)
            {
                if (graphicObject.Position.x == position.x &&
                    graphicObject.Position.z == position.z &&
                    graphicObject.CurrentModel.Shape != ShapeMode.Player &&
                    graphicObject.CurrentModel.Shape != ShapeMode.Decal)
                {
                    nextObject = graphicObject;
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
                    break;
                }
            }
        }

        public void Simulate(float secondsElapsed)
        {
            if (currentMoveDirection != MoveDirection.None)
            {
                MoveProgress = MoveProgress + secondsElapsed * Speed;
                // в зависимости от направления движения
                Vector3 nextTranslation;
                switch (currentMoveDirection)
                {
                    case MoveDirection.Up:
                    case MoveDirection.Down:
                        Translation = new Vector3((position.x - xLength / 2) * 2, 0, (position.z - zLength / 2) * 2 - Math.Sign((sbyte)currentMoveDirection) * (1 - MoveProgress) * 2f);
                        nextTranslation = new Vector3(translation.X, 0, translation.Z + Math.Sign((sbyte)currentMoveDirection) * 2);
                        break;
                    case MoveDirection.Left:
                    case MoveDirection.Right:
                        Translation = new Vector3((position.x - xLength / 2) * 2 - Math.Sign((sbyte)currentMoveDirection) * (1 - MoveProgress) * 2f, 0, (position.z - zLength / 2) * 2);
                        nextTranslation = new Vector3(translation.X + Math.Sign((sbyte)currentMoveDirection) * 2, 0, translation.Z);
                        break;
                    default:
                        Translation = new Vector3(0, 0, 0);
                        nextTranslation = new Vector3(0, 0, 0);
                        break;
                }
                if (nextObject != null)
                    nextObject.Translation = nextTranslation;
                if (MoveProgress >= 1)
                {
                    MoveProgress = 1;
                    Position = (position.x, position.z); //из-за того, что позиция немного улетает когда окно неактивно
                    if (nextObject != null)
                    {
                        nextObject.Position = (nextObject.Position.x, nextObject.Position.z); //из-за того, что позиция немного улетает когда окно неактивно
                        nextObject = null;
                    }
                    currentMoveDirection = MoveDirection.None;
                    OnSimulationFinished?.Invoke();
                    if (moveQueue.Count != 0)
                    {
                        SetMoving(moveQueue.Dequeue());
                    }
                }
            }
        }

        public void Show()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.MultMatrix(ref modelMatrix);
            CurrentMaterial.Apply();
            CurrentModel.Show();
            GL.PopMatrix();
        }
    }
}
