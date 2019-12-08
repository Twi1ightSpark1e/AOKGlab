using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace Client.Materials
{
    class ColorTexturedMaterial : ColorMaterial
    {
        public Texture Texture { get; set; }

        public override void Apply()
        {
            base.Apply();
            GL.Enable(EnableCap.Texture2D);
            Texture.Apply();
        }
    }
}
