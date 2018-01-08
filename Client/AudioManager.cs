using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Audio.OpenAL;
using System.Windows.Forms;
using System.Diagnostics;

namespace Client
{
    class AudioManager
    {
        public static Vector3 ListenerPosition { get; set; }
        public static Vector3[] ListenerOrientation { get; set; }

        private static Dictionary<SoundType, int> sources = new Dictionary<SoundType, int>();
        private static Dictionary<SoundType, float> gains = new Dictionary<SoundType, float>();
        private static IntPtr soundDevice;
        private static ContextHandle soundContext;
        private static (int x, int z) mapSize;

        public static void Initialize((int x, int z) mapSize)
        {
            AudioManager.mapSize = mapSize;
            if (soundDevice == IntPtr.Zero)
                soundDevice = Alc.OpenDevice(null);
            if (soundDevice == IntPtr.Zero)
            {
                MessageBox.Show("Не удалось открыть устройство вывода звука");
                return;
            }
            else
            {
                if (soundContext == ContextHandle.Zero)
                    soundContext = Alc.CreateContext(soundDevice, new int[0] { });
                if (soundContext == ContextHandle.Zero)
                {
                    MessageBox.Show("Не удалось создать звуковой контекст");
                    return;
                }
            }
            Alc.MakeContextCurrent(soundContext);
            if (sources.Count != 0)
                UnloadAll();
            gains[SoundType.Ambient] = .5f;
            gains[SoundType.Death] = 1.5f;
            gains[SoundType.Explosion] = 1.5f;
            gains[SoundType.Shift] = 4f;
            foreach (var wave in WavLoader.Waves)
            {
                int buffer = AL.GenBuffer();
                var source = AL.GenSource();
                sources.Add(wave.Key, source);
                ALFormat format;
                if (wave.Value.Channels == 1)
                    format = wave.Value.Bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                else format = wave.Value.Bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                AL.BufferData(buffer, format, wave.Value.RawData, wave.Value.RawData.Length, wave.Value.Rate);
                AL.Source(source, ALSourcei.Buffer, buffer);
                AL.Source(source, ALSourcef.Gain, gains[wave.Key]);
            }
        }

        public static void UnloadAll()
        {
            for (int i = 0; i < sources.Count; i++)
            {
                AL.SourceStop(sources.Values.ElementAt(i));
            }
            AL.DeleteSources(sources.Values.ToArray());
            sources.Clear();
        }

        public static void SetListener(Vector3 position, float radianX, float radianY)
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

        public static void Play(SoundType sound, Vector2 position)
        {
            Vector3 sourcePosition;
            if (!sources.Keys.Contains(sound))
            {
                Debug.WriteLine($"Cannot play {sound}");
                return;
            }
            int source = sources[sound];
            if (sound == SoundType.Ambient)
            {
                sourcePosition = new Vector3(position.X, 0, position.Y);
                AL.Source(source, ALSourceb.SourceRelative, true);
                AL.Source(source, ALSourceb.Looping, true);
            }
            else
            {
                sourcePosition = new Vector3((position[0] - mapSize.x / 2) * 2, 0, (position[1] - mapSize.z / 2) * 2);
                AL.Source(source, ALSourceb.SourceRelative, false);
            }
            AL.Source(source, ALSource3f.Position, ref sourcePosition);
            AL.SourcePlay(source);
        }
    }
}
