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
        public string FileName { get; private set; } // имя файла модели
        public Vector3[] Vertices { get; private set; } // массив вершин
        public ushort[] Indices { get; private set; } // массив индексов
        public Vector3[] Normals { get; private set; } // массив нормалей
        private BinaryReader fileReader; // считыватель из файла

        // словарик ID чанков, которые не надо пропускать, и действий, которые надо выполнить при обнаружении каждого чанка
        private Dictionary<ushort, Action> dontSkip = new Dictionary<ushort, Action>(); 

        public _3dsReader(string filename)
        {
            FileName = filename;
            // заполняем словарик
            // если действие null, то мы ничего не делаем с чанком, но и не пропускаем его
            dontSkip.Add(0x4d4d, null); // главный чанк
            dontSkip.Add(0x3d3d, null); // чанк 3D редактора
            dontSkip.Add(0x4000, new Action(() => // блок объекта, считываем имя модели
            {
                string objectName = string.Empty;
                char c;
                // считываем имя до нулевого символа
                do
                {
                    c = fileReader.ReadChar();
                    if (c != (char)0)
                        objectName += c;
                }
                while (c != (char)0);
            }));
            dontSkip.Add(0x4100, null); // чанк с треугольной сеткой
            dontSkip.Add(0x4110, new Action(() => // вершины
            {
                ushort vertices = fileReader.ReadUInt16(); // количество вершин
                Vertices = new Vector3[vertices];
                for (ushort i = 0; i < vertices; i++)
                {
                    float x = fileReader.ReadSingle();
                    float y = fileReader.ReadSingle();
                    float z = fileReader.ReadSingle();
                    Vertices[i] = new Vector3(x, y, z);
                }
            }));
            dontSkip.Add(0x4120, new Action(() => // индексы
            {
                ushort indices = fileReader.ReadUInt16(); // количество индексов
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
            // считываем содержимое файла
            while (fileReader.BaseStream.Position < fileReader.BaseStream.Length)
            {
                ushort chunkId = fileReader.ReadUInt16(); // считываем ID чанка
                uint chunkLength = fileReader.ReadUInt32(); // считываем длину чанка
                if (dontSkip.ContainsKey(chunkId)) // если его не надо пропускать
                    dontSkip[chunkId]?.Invoke();   // то выполняем действие из словарика
                else fileReader.ReadBytes((int)chunkLength - 6); // иначе пропускаем
            }
            fileReader.Close();
            // Вычисление нормалей, избавление от индексов
            Normals = new Vector3[Indices.Length];
            for (int i = 0; i < Indices.Length; i += 3)
            {
                // выбираем вершины
                Vector3 v1 = Vertices[Indices[i]];
                Vector3 v2 = Vertices[Indices[i + 1]];
                Vector3 v3 = Vertices[Indices[i + 2]];
                // увеличиваем нормали, соотвествующие выбранным вершинам
                Normals[Indices[i]] += Vector3.Cross(v2 - v1, v3 - v1);
                Normals[Indices[i + 1]] += Vector3.Cross(v2 - v1, v3 - v1);
                Normals[Indices[i + 2]] += Vector3.Cross(v2 - v1, v3 - v1);
            }
            // приводим все нормали к единичной длине
            for (int i = 0; i < Indices.Length; i++)
                Normals[i].Normalize();
        }
    }
}
