using Common;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Ch03_01Cube
{
    public class TriangleRenderer : RendererBase
    {
        // The triangle vertex buffer
        Buffer triangleVerteces;

        // The vertex buffer binding structure for the triangle
        VertexBufferBinding triangleBinging;

        protected override void CreateDeviceDependentResources()
        {
            base.CreateDeviceDependentResources();
            RemoveAndDispose(ref triangleVerteces);

            // Retrieve Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            // Create a triangle
            triangleVerteces = ToDispose(Buffer.Create(
                device,
                BindFlags.VertexBuffer,
                new[]
                {
                    /* Vertex position - Vertex color */
                    new Vector4(0.0f, 0.0f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Base-right
                    new Vector4(-0.5f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Base-left
                    new Vector4(-0.25f, 1.0f, 0.25f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Apex
                }));

            triangleBinging = new VertexBufferBinding(triangleVerteces, Utilities.SizeOf<Vector4>() * 2, 0);
        }

        protected override void DoRender()
        {
            // Get the context reference
            var context = this.DeviceManager.Direct3DContext;

            // Render the triangle
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            // Pass in the triangle vertices
            context.InputAssembler.SetVertexBuffers(0, triangleBinging);

            // Draw the 3 vertices of our triangle
            context.Draw(3, 0);
        }
    }
}
