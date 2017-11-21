using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Light
    {
        public Vector3 Position { get; set; } // координаты источника света
        public Vector3 Ambient { get; set; } // фоновая составляющая источника света
        public Vector3 Diffuse { get; set; } // диффузная составляющая источника света
        public Vector3 Specular { get; set; } // зеркальная составляющая источника света
        public LightName LightName { get; set; } // имя источника света
        public static LightMode LightMode { get; set; } // текущий режим освещения

        public void Apply()
        {
            // задаём составляющие в зависимости от текущего режима освещения
            Vector3 emptyVector = new Vector3();
            Vector3 currentAmbient = Ambient;
            Vector3 currentDiffuse = Diffuse;
            Vector3 currentSpecular = Specular;
            switch (LightMode)
            {
                case LightMode.OnlyAmbient:
                    currentDiffuse = emptyVector;
                    currentSpecular = emptyVector;
                    break;
                case LightMode.OnlyDiffuse:
                    currentAmbient = emptyVector;
                    currentSpecular = emptyVector;
                    break;
                case LightMode.OnlySpecular:
                    currentAmbient = emptyVector;
                    currentDiffuse = emptyVector;
                    break;
            }
            // "применяем" источник света
            GL.Enable((EnableCap)LightName);
            GL.Light(LightName, LightParameter.Position, new float[] { Position.X, Position.Y, Position.Z });
            GL.Light(LightName, LightParameter.Ambient, new float[] { currentAmbient.X, currentAmbient.Y, currentAmbient.Z, 1 });
            GL.Light(LightName, LightParameter.Diffuse, new float[] { currentDiffuse.X, currentDiffuse.Y, currentDiffuse.Z, 1 });
            GL.Light(LightName, LightParameter.Specular, new float[] { currentSpecular.X, currentSpecular.Y, currentSpecular.Z, 1 });
        }
    }
}
