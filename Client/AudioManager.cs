using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Audio.OpenAL;

namespace Client
{
    class AudioManager
    {
        public Vector3 ListenerPosition { get; set; }
        public Vector3[] ListenerOrientation { get; set; }

        public void SetListener(Vector3 position, float radianX, float radianY)
        {
            ListenerPosition = position;
            float radianYUp = radianY + ((float)Math.PI / 4);
            ListenerOrientation = new Vector3[]
            {
                new Vector3(
                    -(float)(Math.Cos(radianY) * Math.Cos(radianX)),
                    -(float)Math.Sin(radianY),
                    -(float)(Math.Cos(radianY)*Math.Sin(radianX))
                ),
                new Vector3(
                    -(float)(Math.Cos(radianY)*Math.Cos(radianX)),
                    -(float)Math.Sin(radianY),
                    -(float)(Math.Cos(radianY)*Math.Sin(radianX))
                )
            };
            AL.Listener(ALListenerfv.Orientation, ref ListenerOrientation[0], ref ListenerOrientation[1]);
            AL.Listener(ALListenerf.Gain, 1f);
        }
    }
}
