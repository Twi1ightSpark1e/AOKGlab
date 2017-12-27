using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OpenTK;
using System.Diagnostics;

namespace Client
{
    class _3dsReader
    {
        public static List<(string filename, _3dsReader reader)> readers = new List<(string filename, _3dsReader reader)>();

        public string FileName { get; private set; } // имя файла модели
        public Vector3[] Vertices { get; private set; } // массив вершин
        public ushort[] Indices { get; private set; } // массив индексов
        public Vector3[] Normals { get; private set; } // массив нормалей
        public Vector2[] TexCoords { get; private set; } // массив текстурных координат
        private BinaryReader fileReader; // считыватель из файла

        // словарик ID чанков, которые не надо пропускать, и действий, которые надо выполнить при обнаружении каждого чанка
        private Dictionary<ushort, Action> dontSkip = new Dictionary<ushort, Action>(); 

        /// <summary>
        /// Загружает модель из файла .3ds
        /// </summary>
        /// <param name="filename">Путь к файлу</param>
        /// <param name="roughNormals">Режим грубого восстановления нормалей; используется для угловатых поверхностей</param>
        public _3dsReader(string filename, bool roughNormals)
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
            dontSkip.Add(0x4140, new Action(() => // текстурные координаты
            {
                ushort coordinates = fileReader.ReadUInt16();
                TexCoords = new Vector2[coordinates];
                for (ushort i = 0; i < coordinates; i++)
                {
                    float s = fileReader.ReadSingle();
                    float t = fileReader.ReadSingle();
                    TexCoords[i] = new Vector2(s, t);
                }
            }));
            ReadFromFile(roughNormals);
        }

        public void ReadFromFile(bool roughNormals)
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
            // Вычисление нормалей
            if (roughNormals)
            {
                Vector3[] SortedVertices = Indices.Select((index) => Vertices[index]).ToArray();
                ushort[] SortedIndices = Enumerable.Range(0, Indices.Length).Select((index) => (ushort)index).ToArray();
                Vector2[] SortedTexCoords = Indices.Select((index) => TexCoords[index]).ToArray();
                Normals = new Vector3[SortedVertices.Length];
                Vertices = SortedVertices;
                Indices = SortedIndices;
                TexCoords = SortedTexCoords;
                for (int i = 0; i < SortedIndices.Length; i += 3)
                {
                    Vector3 v1 = SortedVertices[i];
                    Vector3 v2 = SortedVertices[i + 1];
                    Vector3 v3 = SortedVertices[i + 2];

                    Vector3 cross1 = v3 - v2;
                    Vector3 cross2 = v1 - v2;

                    Normals[i] = Vector3.Cross(cross1, cross2);
                    Normals[i + 1] = Vector3.Cross(cross1, cross2);
                    Normals[i + 2] = Vector3.Cross(cross1, cross2);
                }
            }
            else
            {
                Normals = new Vector3[Vertices.Length];
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
                for (int i = 0; i < Normals.Length; i++)
                    Normals[i].Normalize();
            }
        }

        public static _3dsReader GetReaderByFilename(string filename, bool roughNormals)
        {
            var search = readers.Where((reader) => reader.filename == filename);
            if (search.Count() == 1)
            {
                return search.First().reader;
            }
            else
            {
                var reader = new _3dsReader(filename, roughNormals);
                readers.Add((filename, reader));
                return reader;
            }
        }
    }
}
