using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    class WavLoader
    {
        public static Dictionary<SoundType, WavLoader> Waves = new Dictionary<SoundType, WavLoader>();

        public string Filename { get; private set; }
        public int Channels { get; private set; }
        public int Bits { get; private set; }
        public int Rate { get; private set; }
        public byte[] RawData { get; private set; }

        public WavLoader(string filename)
        {
            Filename = filename;
            Load(File.Open($"sounds/{filename}", FileMode.Open));
        }

        public void Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                Channels = reader.ReadInt16();
                Rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                Bits = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                RawData = reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static void InitializeWaves()
        {
            List<string> files = Directory.GetFiles($"{Application.StartupPath}\\sounds\\").Select(filename => Path.GetFileName(filename)).Where(filename => filename.EndsWith(".wav")).ToList();
            string[] sounds = Enum.GetNames(typeof(SoundType));
            foreach (var sound in sounds)
            {
                if (files.Contains($"{sound}.wav"))
                    Waves.Add((SoundType)Enum.Parse(typeof(SoundType), sound), new WavLoader($"{sound}.wav"));
                else MessageBox.Show($"Не удалось загрузить {sound} звук");
            }
        }
    }
}
