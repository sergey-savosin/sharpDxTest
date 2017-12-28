using Common;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Ch03_02CubeWithTexture
{
    public class AxisLinesRenderer : RendererBase
    {
        // The vertex buffer for axis lines
        Buffer axisLinesVertices;

        // The binding structure of the axis lines vertex buffer
        VertexBufferBinding axisLinesBinding;

        protected override void CreateDeviceDependentResources()
        {
            //base.CreateDeviceDependentResources();

            RemoveAndDispose(ref axisLinesVertices);

            // Retrieve our Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            // Create xyz-axis arrows
            // X is red, Y is Green, Z is blue
            axisLinesVertices = ToDispose(Buffer.Create(
                device,
                BindFlags.VertexBuffer,
                new[]
                {
                    /* Vertex Position - Vertex Color */
                    new Vector4(-1f, 0f, 0f, 1f), (Vector4)Color.Red, // - x-axis
                    new Vector4(1f, 0f, 0f, 1f), (Vector4)Color.Red, // + x-axis
                    new Vector4(0.9f, -0.05f, 0f, 1f), (Vector4)Color.Red, // head start
                    new Vector4(1f, 0f, 0f, 1f), (Vector4)Color.Red,
                    new Vector4(0.9f, 0.05f, 0f, 1f), (Vector4)Color.Red, //
                    new Vector4(1f, 0f, 0f, 1f), (Vector4)Color.Red, // head end

                    new Vector4(0f, -1f, 0f, 1f), (Vector4)Color.Lime, // - y-axis
                    new Vector4(0f, 1f, 0f, 1f), (Vector4)Color.Lime, // + y-axis
                    new Vector4(-0.05f, 0.9f, 0f, 1f), (Vector4)Color.Lime, // head start
                    new Vector4(0f, 1f, 0f, 1f), (Vector4)Color.Lime, //
                    new Vector4(0.05f, 0.9f, 0f, 1f), (Vector4)Color.Lime, //
                    new Vector4(0f, 1f, 0f, 1f), (Vector4)Color.Lime, // head end

                    new Vector4(0f, 0f, -1f, 1f), (Vector4)Color.Blue, // - z-axis
                    new Vector4(0f, 0f, 1f, 1f), (Vector4)Color.Blue, // + z-axis
                    new Vector4(0f, -0.05f, 0.9f, 1f), (Vector4)Color.Blue, // head start
                    new Vector4(0f, 0f, 1f, 1f), (Vector4)Color.Blue, //
                    new Vector4(0f, 0.05f, 0.9f, 1f), (Vector4)Color.Blue, //
                    new Vector4(0f, 0f, 1f, 1f), (Vector4)Color.Blue, // head end
                }));

            axisLinesBinding = new VertexBufferBinding(axisLinesVertices,
                Utilities.SizeOf<Vector4>() * 2,
                0);
        }

        protected override void DoRender()
        {
            // Get the context ref
            var context = this.DeviceManager.Direct3DContext;

            // Render the Axis lines
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            // Pass in the line vertices
            context.InputAssembler.SetVertexBuffers(0, axisLinesBinding);
            // Draw the 18 vertices of our xyz-axis arrows
            context.Draw(18, 0);

        }
    }
}
