using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ch03_01Cube
{
    public static class ConstantBuffers
    {
        /// <summary>
        /// Per Object constant buffer (matrices)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerObject
        {
            // WorldViewProjection matrix
            public Matrix WorldViewProjection;

            // We need the world matrix so that we can
            // calculate the lighting in world space
            public Matrix World;

            // Inverse transpose of World
            public Matrix WorldInverseTranspose;

            /// <summary>
            /// Transpose the matrices so that they are in row major order for HLSL
            /// </summary>
            internal void Transpose()
            {
                this.World.Transpose();
                this.WorldInverseTranspose.Transpose();
                this.WorldViewProjection.Transpose();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DirectionalLight
        {
            public Color4 Color;
            public Vector3 Direction;
            float _padding0;
        }

        /// <summary>
        /// Per frame constant buffer (camera position)
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PerFrame
        {
            public DirectionalLight Light;
            public Vector3 CameraPosition;
            float _padding0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PerMaterial
        {
            public Color4 Ambient;
            public Color4 Diffuse;
            public Color4 Specular;
            public float SpecularPower;
            public uint HasTexture; // 0 false, 1 true
            Vector2 _padding0;
            public Color4 Emissive;
            public Matrix UVTransform; // Support UV transforms
        }
    }
}
