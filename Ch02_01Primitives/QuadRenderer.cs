using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D11;

using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;

namespace Ch02_01DirectX
{
    public class QuadRenderer : RendererBase
    {
        // The quad vertex buffer
        Buffer quadVertices;

        // The quad index buffer
        Buffer quadIndices;

        // The vertex buffer binding for the quad
        VertexBufferBinding quadBinding;

        protected override void CreateDeviceDependentResources()
        {
            base.CreateDeviceDependentResources();

            RemoveAndDispose(ref quadVertices);
            RemoveAndDispose(ref quadIndices);

            // Retrieve our device instance
            var device = this.DeviceManager.Direct3DDevice;

            // Create a quad (two triangles)
            quadVertices = ToDispose(Buffer.Create(
                device,
                BindFlags.VertexBuffer,
                new[]
                {
                    /* Vertex position - Vertex color */
                    new Vector4(0.25f, 0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Top-left
                    new Vector4(0.75f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Top-right
                    new Vector4(0.75f, 0.0f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Base-right
                    new Vector4(0.25f, 0.0f, -0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Base-left
                }));

            quadBinding = new VertexBufferBinding(quadVertices, Utilities.SizeOf<Vector4>() * 2, 0);

            /****************
             * v0    v1
             * |-----|
             * | \ A |
             * |  \  |
             * | B \ |
             * |-----|
             * v3    v2
             ****************/
            quadIndices = ToDispose(Buffer.Create(
                device,
                BindFlags.IndexBuffer,
                new ushort[]
                {
                     0, 1, 2, // A
                     2, 3, 0  // B
                }));
        }
        protected override void DoRender()
        {
            var context = this.DeviceManager.Direct3DContext;

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            // Set the index buffer
            context.InputAssembler.SetIndexBuffer(quadIndices, Format.R16_UInt, 0);

            // Pass in the quad vertices (only 4 vertices)
            context.InputAssembler.SetVertexBuffers(0, quadBinding);

            // Draw the 6 vertices that make up the two triangles in the quad
            context.DrawIndexed(6, 0, 0);
        }
    }
}
