using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Material
    {
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }
        public Vector3 Specular { get; set; }
        public Vector3 Emission { get; set; }
        public float Shininess { get; set; }

        public static Material CreateWall()
        {
            return new Material()
            {
                Ambient = new Vector3(.1f, .1f, .1f),
                Diffuse = new Vector3(.2f, .2f, .2f)
            };
        }

        public static Material CreatePlayer()
        {
            return new Material()
            {
                Ambient = new Vector3(0, .75f, 1),
                Diffuse = new Vector3(.4f, .4f, .4f),
                Emission = new Vector3(.05f, .05f, .05f)
            };
        }

        public static Material CreateBomb()
        {
            return new Material()
            {
                Specular = new Vector3(.5f, .5f, .5f),
                Shininess = 7f
            };
        }

        public static Material CreateLightBarrier()
        {
            return new Material()
            {
                Ambient = new Vector3(1f, .82f, .09f),
                Diffuse = new Vector3(.6f, .6f, .6f)
            };
        }

        public static Material CreateHeavyBarrier()
        {
            return new Material()
            {
                Ambient = new Vector3(.4f, .4f, .4f),
                Diffuse = new Vector3(.3f, .3f, .3f),
                Specular = new Vector3(.3f, .3f, .3f),
                Shininess = 5
            };
        }

        public static Material CreateFlat()
        {
            return new Material()
            {
                Ambient = new Vector3(.1f, .1f, .1f),
                Diffuse = new Vector3(.3f, .3f, .3f),
                Specular = new Vector3(.1f, .1f, .1f)
            };
        }

        public void Apply()
        {
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, new float[] { Ambient.X, Ambient.Y, Ambient.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, new float[] { Diffuse.X, Diffuse.Y, Diffuse.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new float[] { Specular.X, Specular.Y, Specular.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { Emission.X, Emission.Y, Emission.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, Shininess);
        }
    }
}
