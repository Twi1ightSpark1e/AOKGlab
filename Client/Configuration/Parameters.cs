using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Client.Configuration
{
    class Parameters
    {
        // фоновая отражающая способность материала
        private float[] _ambient;
        public float[] ambient
        {
            get => _ambient;
            set
            {
                _ambient = value;
                AmbientVector = new Vector3(value[0], value[1], value[2]);
            }
        }
        public Vector3 AmbientVector { get; set; }

        // диффузная отражающая способность материала
        private float[] _diffuse;
        public float[] diffuse
        {
            get => _diffuse;
            set
            {
                _diffuse = value;
                DiffuseVector = new Vector3(value[0], value[1], value[2]);
            }
        }
        public Vector3 DiffuseVector { get; set; }

        // зеркальная отражающая способность материала
        private float[] _specular;
        public float[] specular
        {
            get => _specular;
            set
            {
                _specular = value;
                SpecularVector = new Vector3(value[0], value[1], value[2]);
            }
        }
        public Vector3 SpecularVector { get; set; }

        // излучающая способность материала
        private float[] _emission;
        public float[] emission
        {
            get => _emission;
            set
            {
                _emission = value;
                EmissionVector = new Vector3(value[0], value[1], value[2]);
            }
        }
        public Vector3 EmissionVector { get; set; }

        public float shininess; // самосвечение
    }
}
