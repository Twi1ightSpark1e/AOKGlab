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
    class GraphicObject
    {
        public Model CurrentModel { get; set; }
        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                modelMatrix = Matrix4.Mult(Matrix4.Mult(Matrix4.Identity, Matrix4.CreateTranslation(value)), Matrix4.CreateRotationY(angle));
            }
        }

        private float angle;
        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                modelMatrix = Matrix4.Mult(Matrix4.Mult(Matrix4.Identity, Matrix4.CreateTranslation(position)), Matrix4.CreateRotationY(value));
            }
        }

        private Matrix4 modelMatrix;
        //private List<Model> models = new List<Model>();
        
        public GraphicObject(Model model, Vector3 position, float angle)
        {
            CurrentModel = model;
            this.position = position; //не пересчитывать матрицу модели
            Angle = angle; //пересчитать матрицу модели
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
