using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.DXGI;

namespace Ch03_01Cube
{
    public class CubeRenderer : RendererBase
    {
        Buffer vertexBuffer;
        Buffer indexBuffer;
        VertexBufferBinding vertexBinding;

        protected override void CreateDeviceDependentResources()
        {
            //base.CreateDeviceDependentResources();

            RemoveAndDispose(ref vertexBuffer);
            RemoveAndDispose(ref indexBuffer);

            var device = DeviceManager.Direct3DDevice;

            // Create vertex buffer for cube
            vertexBuffer = ToDispose(Buffer.Create(
                device,
                BindFlags.VertexBuffer,
                new Vertex[]
                {
                    /* Vertex position - Color */
                    new Vertex(-0.5f, 0.5f, -0.5f, Color.Gray), // 0-Top-Left
                    new Vertex(0.5f, 0.5f, -0.5f, Color.Gray), // 1-Top-Right
                    new Vertex(0.5f, -0.5f, -0.5f, Color.Gray), // 2-Base-right
                    new Vertex(-0.5f, -0.5f, -0.5f, Color.Gray), // 3-Base-Left

                    new Vertex(-0.5f, 0.5f, 0.5f, Color.Gray), // 4-Top-Left
                    new Vertex(0.5f, 0.5f, 0.5f, Color.Gray), // 5-Top-right
                    new Vertex(0.5f, -0.5f, 0.5f, Color.Gray), // 6-Base-right
                    new Vertex(-0.5f, -0.5f, 0.5f, Color.Gray), // 7-Base-left
                }));

            vertexBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0);

            indexBuffer = ToDispose(Buffer.Create(
                device,
                BindFlags.IndexBuffer
                , new ushort[]
                {
                    0, 1, 2, // Front A
                    0, 2, 3, // Front B
                    1, 5, 6, // Right
                    1, 6, 2,
                    1, 0, 4, // Top
                    1, 4, 5,
                    5, 4, 7, // Back
                    5, 7, 6,
                    4, 0, 3, // Left
                    4, 3, 7,
                    3, 2, 6, // Bottom
                    3, 6, 7,
                }));
        }

        protected override void DoRender()
        {
            var context = this.DeviceManager.Direct3DContext;

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            // Set the index buffer
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);

            // Pass in the quad vertices (only 4 vertices)
            context.InputAssembler.SetVertexBuffers(0, vertexBinding);

            // Draw the 6 vertices that make up the two triangles in the quad
            context.DrawIndexed(36, 0, 0);

        }
    }
}
