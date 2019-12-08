using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Materials;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Client
{
    class Particle
    {
        public Vector3 position, speed;
        public float size, timeToLive, elapsedTime, distance;
        public Vector4 startColor, endColor;
        public bool alive;
    }

    class ParticleSystem
    {
        const int DefaultParticleAmount = 100;
        const float DefaultSize = 1f;
        static readonly Vector4 DefaultStartColor = new Vector4(0f, 0f, 0f, 1f);
        static readonly Vector4 DefaultEndColor = new Vector4(.3f, .3f, .3f, 1f);
        const float DefaultTimeToLive = 1f;
        const int TextureCount = 64;
        const int TexturesInARow = 8;

        private const float anglePart = (float)Math.PI / DefaultParticleAmount * 2;
        private static readonly Vector3 falseUp = new Vector3(0f, 1f, 0f);
        private static readonly uint[] indices =
        {
            0, 2, 1, 1, 2, 3
        };

        private static List<Particle> particles = new List<Particle>();
        public static IMaterial Material { get; set; }

        public static void Create(Vector2 position, float radius)
        {
            Random rnd = new Random();
            for (int i = 0; i < DefaultParticleAmount; i++)
            {
                float angleVertical = (float)((double)rnd.Next() / int.MaxValue * Math.PI - Math.PI / 2);
                float angleHorizontal = (float)((double)rnd.Next() / int.MaxValue * Math.PI * 2);
                float positionRadius = (float)((double)rnd.Next() / int.MaxValue * radius * Math.Cos(angleVertical));
                Vector3 disposition = new Vector3(
                    positionRadius * (float)Math.Cos(angleHorizontal),
                    radius * (float)Math.Sin(angleVertical),
                    positionRadius * (float)Math.Sin(angleVertical));
                Particle current = new Particle()
                {
                    position = new Vector3(position.X - 1, .5f, position.Y - 1) + disposition,
                    size = DefaultSize,
                    startColor = DefaultStartColor,
                    endColor = DefaultEndColor,
                    timeToLive = DefaultTimeToLive,
                    elapsedTime = 0,
                    speed = new Vector3((float)Math.Sin(anglePart * i), -.1f, (float)Math.Cos(anglePart * i)),
                    alive = true
                };
            }
        }

        public static void Draw(Vector3 position)
        {
            for (int i = 0; i < particles.Count; i++)
                particles[i].distance = Math.Abs((particles[i].position - position).Length);

            particles.Sort(new Comparison<Particle>((p1, p2) => (int)(p1.distance - p2.distance)));

            Material.Apply();
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.ClientActiveTexture(TextureUnit.Texture0);
            foreach (var particle in particles)
            {
                Vector3 forward = (particle.position - position).Normalized();
                Vector3 right = Vector3.Cross(forward, falseUp).Normalized();
                Vector3 up = Vector3.Cross(right, forward).Normalized();

                float progress = particle.elapsedTime / particle.timeToLive;
                int textureNumber = (int)(progress * TextureCount);
                Vector4 color = particle.startColor * (1 - progress) + particle.endColor * progress;
                int textureX = textureNumber % TexturesInARow;
                int textureY = textureNumber / TexturesInARow;
                ParticlePoint[] points =
                {
                    new ParticlePoint()
                    {
                        color = color,
                        coord = (-right + up) * particle.size * .5f,
                        texCoord = new Vector2(
                            textureX / TexturesInARow, 
                            textureY / TexturesInARow)
                    },
                    new ParticlePoint()
                    {
                        color = color,
                        coord = ( right + up) * particle.size * .5f,
                        texCoord = new Vector2(
                            (textureX + 1) / TexturesInARow, 
                            textureY / TexturesInARow)
                    },
                    new ParticlePoint()
                    {
                        color = color,
                        coord = (-right - up) * particle.size * .5f,
                        texCoord = new Vector2(
                            textureX / TexturesInARow, 
                            (textureY + 1) / TexturesInARow)
                    },
                    new ParticlePoint()
                    {
                        color = color,
                        coord = ( right - up) * particle.size * .5f,
                        texCoord = new Vector2(
                            (textureX + 1) / TexturesInARow, 
                            (textureY + 1) / TexturesInARow)
                    }
                };
                float[] matrix =
                {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    particle.position[0],
                    particle.position[1],
                    particle.position[2],
                    1
                };

                GL.PushMatrix();
                GL.MultMatrix(matrix);
                IntPtr ptr = Marshal.AllocHGlobal((Vector4.SizeInBytes + Vector3.SizeInBytes + Vector2.SizeInBytes) * 4);
                GL.ColorPointer(4, ColorPointerType.Float, Vector4.SizeInBytes + Vector3.SizeInBytes + Vector2.SizeInBytes, points);
                //GL.VertexPointer(3, VertexPointerType.Float, Vector4.SizeInBytes + Vector3.SizeInBytes + Vector2.SizeInBytes, )
            }
        }

        public static void Simulate(float elapsedTime)
        {

        }
    }
}
