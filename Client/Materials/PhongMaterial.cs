using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using static Client.Configuration.Configuration;

namespace Client.Materials
{
    class PhongMaterial : IMaterial
    {
        public Vector3 Ambient { get; set; } // фоновая отражающая способность материала
        public Vector3 Diffuse { get; set; } // диффузная отражающая способность материала
        public Vector3 Specular { get; set; } // зеркальная отражающая способность материала
        public Vector3 Emission { get; set; } // излучающая способность материала
        public float Shininess { get; set; } // самосвечение
        #region фабрика материалов
        public static PhongMaterial CreateWall()
        {
            return new PhongMaterial()
            {
                //Ambient = new Vector3(.5f, .5f, .5f),
                //Diffuse = new Vector3(1f, 1f, 1f),
                //Shininess = 1
                Ambient = Wall.material.parameters.AmbientVector,
                Diffuse = Wall.material.parameters.DiffuseVector,
                Emission = Wall.material.parameters.EmissionVector,
                Specular = Wall.material.parameters.SpecularVector,
                Shininess = Wall.material.parameters.shininess
            };
        }

        public static PhongMaterial CreatePlayer()
        {
            return new PhongMaterial()
            {
                //Ambient = new Vector3(0, .55f, .8f),
                //Diffuse = new Vector3(.4f, .4f, .4f),
                //Emission = new Vector3(.05f, .05f, .05f)
                Ambient = Player.material.parameters.AmbientVector,
                Diffuse = Player.material.parameters.DiffuseVector,
                Emission = Player.material.parameters.EmissionVector,
                Specular = Player.material.parameters.SpecularVector,
                Shininess = Player.material.parameters.shininess
            };
        }

        public static PhongMaterial CreateBomb()
        {
            return new PhongMaterial()
            {
                //Specular = new Vector3(.5f, .5f, .5f),
                //Shininess = 7f
                Ambient = Bomb.material.parameters.AmbientVector,
                Diffuse = Bomb.material.parameters.DiffuseVector,
                Emission = Bomb.material.parameters.EmissionVector,
                Specular = Bomb.material.parameters.SpecularVector,
                Shininess = Bomb.material.parameters.shininess
            };
        }

        public static PhongMaterial CreateLightBarrier()
        {
            return new PhongMaterial()
            {
                //Ambient = new Vector3(.8f, .8f, .8f),
                //Diffuse = new Vector3(.4f, .4f, .4f),
                //Shininess = 1
                Ambient = LightObject.material.parameters.AmbientVector,
                Diffuse = LightObject.material.parameters.DiffuseVector,
                Emission = LightObject.material.parameters.EmissionVector,
                Specular = LightObject.material.parameters.SpecularVector,
                Shininess = LightObject.material.parameters.shininess
            };
        }

        public static PhongMaterial CreateHeavyBarrier()
        {
            return new PhongMaterial()
            {
                //Ambient = new Vector3(.51f, .51f, .51f),
                //Diffuse = new Vector3(.85f, .85f, .85f),
                //Shininess = 10
                Ambient = HeavyObject.material.parameters.AmbientVector,
                Diffuse = HeavyObject.material.parameters.DiffuseVector,
                Emission = HeavyObject.material.parameters.EmissionVector,
                Specular = HeavyObject.material.parameters.SpecularVector,
                Shininess = HeavyObject.material.parameters.shininess
            };
        }

        public static PhongMaterial CreateFlat()
        {
            return new PhongMaterial()
            {
                //Ambient = new Vector3(.27f, .27f, .27f),
                //Diffuse = new Vector3(.9f, .9f, .9f),
                //Shininess = 1
                Ambient = Flat.material.parameters.AmbientVector,
                Diffuse = Flat.material.parameters.DiffuseVector,
                Emission = Flat.material.parameters.EmissionVector,
                Specular = Flat.material.parameters.SpecularVector,
                Shininess = Flat.material.parameters.shininess
            };
        }
        #endregion

        public virtual void Apply()
        {
            Texture.Disable();
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, new float[] { Ambient.X, Ambient.Y, Ambient.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, new float[] { Diffuse.X, Diffuse.Y, Diffuse.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new float[] { Specular.X, Specular.Y, Specular.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { Emission.X, Emission.Y, Emission.Z, 1 });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, Shininess);
        }
    }
}
