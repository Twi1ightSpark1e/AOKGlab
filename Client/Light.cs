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
        public Vector3 Position { get; set; }
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }
        public Vector3 Specular { get; set; }
        public LightName LightName { get; set; }

        public void Apply()
        {
            GL.Enable((EnableCap)LightName);
            GL.Light(LightName, LightParameter.Position, new float[] { Position.X, Position.Y, Position.Z });
            GL.Light(LightName, LightParameter.Ambient, new float[] { Ambient.X, Ambient.Y, Ambient.Z, 1 });
            GL.Light(LightName, LightParameter.Diffuse, new float[] { Diffuse.X, Diffuse.Y, Diffuse.Z, 1 });
            GL.Light(LightName, LightParameter.Specular, new float[] { Specular.X, Specular.Y, Specular.Z, 1 });
        }
    }
}
