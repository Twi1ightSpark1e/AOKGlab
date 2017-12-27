using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Client.Materials
{
    class ColorMaterial : IMaterial
    {
        public Vector4 Color { get; set; }

        public void Apply()
        {
            GL.Disable(EnableCap.Lighting);
            Texture.Disable();
            GL.Color4(Color);
        }
    }
}
