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
    public class SphereRenderer : RendererBase
    {
        Buffer vertexBuffer;
        Buffer indexBuffer;
        VertexBufferBinding vertexBinding;

        int totalVertexCount = 0;

        protected override void CreateDeviceDependentResources()
        {
            // base.CreateDeviceDependentResources();
            RemoveAndDispose(ref vertexBuffer);
            RemoveAndDispose(ref indexBuffer);

            // Retrieve our SharpDX.Direct3D11.Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            Vertex[] vertices;
            int[] indices;
            GeometricPrimitives.GenerateSphere(out vertices, out indices, Color.Gray);

            vertexBuffer = ToDispose(Buffer.Create(device, BindFlags.VertexBuffer, vertices));
            vertexBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0);

            indexBuffer = ToDispose(Buffer.Create(device, BindFlags.IndexBuffer, indices));
            totalVertexCount = indices.Length;
        }

        protected override void DoRender()
        {
            var context = this.DeviceManager.Direct3DContext;

            // Tell the IA we are using triangles
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            // Set the index buffer
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            // Pass in the quad vertices (note: only 4 vertices)
            context.InputAssembler.SetVertexBuffers(0, vertexBinding);
            // Draw the 36 vertices that make up the two triangles in the quad
            // using the vertex indices
            context.DrawIndexed(totalVertexCount, 0, 0);
            // Note: we have called DrawIndexed so that the index buffer will be used

        }
    }
}
