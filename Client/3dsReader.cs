using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Client
{
    class _3dsReader
    {
        public string FileName { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public ushort[] Indices { get; private set; }
        public Vector3[] Normals { get; private set; }
        private BinaryReader fileReader;
        private Dictionary<ushort, Action> dontSkip = new Dictionary<ushort, Action>();

        public _3dsReader(string filename)
        {
            FileName = filename;
            dontSkip.Add(0x4d4d, null);
            dontSkip.Add(0x3d3d, null);
            dontSkip.Add(0x4000, new Action(() =>
            {
                string objectName = string.Empty;
                char c;
                do
                {
                    c = fileReader.ReadChar();
                    if (c != (char)0)
                        objectName += c;
                }
                while (c != (char)0);
            }));
            dontSkip.Add(0x4100, null);
            dontSkip.Add(0x4110, new Action(() =>
            {
                ushort vertices = fileReader.ReadUInt16();
                Vertices = new Vector3[vertices];
                for (ushort i = 0; i < vertices; i++)
                {
                    float x = fileReader.ReadSingle();
                    float y = fileReader.ReadSingle();
                    float z = fileReader.ReadSingle();
                    Vertices[i] = new Vector3(x, y, z);
                }
            }));
            dontSkip.Add(0x4120, new Action(() =>
            {
                ushort indices = fileReader.ReadUInt16();
                Indices = new ushort[indices * 3];
                for (ushort i = 0; i < indices * 3; i+=3)
                {
                    Indices[i] = fileReader.ReadUInt16();
                    Indices[i + 1] = fileReader.ReadUInt16();
                    Indices[i + 2] = fileReader.ReadUInt16();
                    fileReader.ReadUInt16(); //пропускаем дополнительные флаги
                }
            }));
            ReadFromFile();
        }

        public void ReadFromFile()
        {
            fileReader = new BinaryReader(File.OpenRead(FileName));
            while (fileReader.BaseStream.Position < fileReader.BaseStream.Length)
            {
                ushort chunkId = fileReader.ReadUInt16();
                uint chunkLength = fileReader.ReadUInt32();
                if (dontSkip.ContainsKey(chunkId))
                    dontSkip[chunkId]?.Invoke();
                else fileReader.ReadBytes((int)chunkLength - 6);
            }
            fileReader.Close();
            // Вычисление нормалей, избавление от индексов
            Normals = new Vector3[Indices.Length];
            //Vector3[] newVertices = new Vector3[Indices.Length];
            for (int i = 0; i < Indices.Length; i += 3)
            {
                Vector3 v1 = Vertices[Indices[i]];
                Vector3 v2 = Vertices[Indices[i + 1]];
                Vector3 v3 = Vertices[Indices[i + 2]];

                Normals[Indices[i]] += Vector3.Cross(v2 - v1, v3 - v1);
                Normals[Indices[i + 1]] += Vector3.Cross(v2 - v1, v3 - v1);
                Normals[Indices[i + 2]] += Vector3.Cross(v2 - v1, v3 - v1);
            }
            for (int i = 0; i < Indices.Length; i++)
                Normals[i].Normalize();
            //Vertices = newVertices;
        }
    }
}
