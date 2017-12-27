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
    class PhongTexturedMaterial : PhongMaterial
    {
        public Texture Texture { get; set; }

        #region фабрика материалов
        new public static PhongTexturedMaterial CreateWall()
        {
            PhongMaterial parentMaterial = PhongMaterial.CreateWall();
            return new PhongTexturedMaterial()
            {
                Ambient = parentMaterial.Ambient,
                Diffuse = parentMaterial.Diffuse,
                Emission = parentMaterial.Emission,
                Shininess = parentMaterial.Shininess,
                Specular = parentMaterial.Specular,
                Texture = Texture.GetTextureByName(Wall.material.texture?.fileName)
            };
        }

        new public static PhongTexturedMaterial CreateLightBarrier()
        {
            PhongMaterial parentMaterial = PhongMaterial.CreateLightBarrier();
            return new PhongTexturedMaterial()
            {
                Ambient = parentMaterial.Ambient,
                Diffuse = parentMaterial.Diffuse,
                Emission = parentMaterial.Emission,
                Shininess = parentMaterial.Shininess,
                Specular = parentMaterial.Specular,
                Texture = Texture.GetTextureByName(LightObject.material.texture?.fileName)
            };
        }

        new public static PhongTexturedMaterial CreateHeavyBarrier()
        {
            PhongMaterial parentMaterial = PhongMaterial.CreateHeavyBarrier();
            return new PhongTexturedMaterial()
            {
                Ambient = parentMaterial.Ambient,
                Diffuse = parentMaterial.Diffuse,
                Emission = parentMaterial.Emission,
                Shininess = parentMaterial.Shininess,
                Specular = parentMaterial.Specular,
                Texture = Texture.GetTextureByName(HeavyObject.material.texture?.fileName)
            };
        }

        new public static PhongTexturedMaterial CreateFlat()
        {
            PhongMaterial parentMaterial = PhongMaterial.CreateFlat();
            return new PhongTexturedMaterial()
            {
                Ambient = parentMaterial.Ambient,
                Diffuse = parentMaterial.Diffuse,
                Emission = parentMaterial.Emission,
                Shininess = parentMaterial.Shininess,
                Specular = parentMaterial.Specular,
                Texture = Texture.GetTextureByName(Flat.material.texture?.fileName)
            };
        }

        new public static PhongTexturedMaterial CreateBomb()
        {
            PhongMaterial parentMaterial = PhongMaterial.CreateBomb();
            return new PhongTexturedMaterial()
            {
                Ambient = parentMaterial.Ambient,
                Diffuse = parentMaterial.Diffuse,
                Emission = parentMaterial.Emission,
                Shininess = parentMaterial.Shininess,
                Specular = parentMaterial.Specular,
                Texture = Texture.GetTextureByName(Bomb.material.texture?.fileName)
            };
        }
        #endregion

        public override void Apply()
        {
            base.Apply();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)All.Modulate);
            Texture?.Apply();
        }
    }
}
