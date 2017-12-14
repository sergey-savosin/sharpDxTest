using Common;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ch02_01DirectX
{
    public class AxisLinesRenderer : RendererBase
    {
        // The vertex buffer for axis lines
        Buffer axisLinesVertices;

        // The binding structure of the axis lines vertex buffer
        VertexBufferBinding axisLinesBinding;

        // Shader texture resource
        ShaderResourceView textureView;

        // Control sampling behavior with this state
        SamplerState samplerState;

        protected override void CreateDeviceDependentResources()
        {
            //base.CreateDeviceDependentResources();

            RemoveAndDispose(ref axisLinesVertices);

            // Retrieve our Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            // Load texture
            var imagingFactory = DeviceManager.WICFactory;
            var texture = LoadTexture.LoadFromFile(device, imagingFactory, "Texture.png");
            textureView = ToDispose(
                new ShaderResourceView(device, texture));

            // Create our sampler state
            samplerState = ToDispose(
                new SamplerState(
                    device, new SamplerStateDescription()
                    {
                        AddressU = TextureAddressMode.Wrap,
                        AddressV = TextureAddressMode.Wrap,
                        AddressW = TextureAddressMode.Wrap,
                        Filter = Filter.MinMagMipLinear,
                    }));

            // Create xyz-axis arrows
            // X is red, Y is Green, Z is blue
            axisLinesVertices = ToDispose(Buffer.Create(
                device,
                BindFlags.VertexBuffer,
                new[]
                {
                    /* Vertex Position - TextureUV */
                    -1f, 0f, 0f, 1f,      0.1757f, 0.039f, // - x-axis
                    1f, 0f, 0f, 1f,       0.1757f, 0.039f, // + x-axis
                    0.9f, -0.05f, 0f, 1f, 0.1757f, 0.039f, // head start
                    1f, 0f, 0f, 1f,       0.1757f, 0.039f,
                    0.9f, 0.05f, 0f, 1f,  0.1757f, 0.039f, //
                    1f, 0f, 0f, 1f,       0.1757f, 0.039f, // head end

                    0f, -1f, 0f, 1f,      0.5273f, 0.136f, // - y-axis
                    0f, 1f, 0f, 1f,       0.5273f, 0.136f, // + y-axis
                    -0.05f, 0.9f, 0f, 1f, 0.5273f, 0.136f, // head start
                    0f, 1f, 0f, 1f,       0.5273f, 0.136f, //
                    0.05f, 0.9f, 0f, 1f,  0.5273f, 0.136f, //
                    0f, 1f, 0f, 1f,       0.5273f, 0.136f, // head end

                    0f, 0f, -1f, 1f,      0.859f, 0.976f, // - z-axis
                    0f, 0f, 1f, 1f,       0.859f, 0.976f, // + z-axis
                    0f, -0.05f, 0.9f, 1f, 0.859f, 0.976f, // head start
                    0f, 0f, 1f, 1f,       0.859f, 0.976f, //
                    0f, 0.05f, 0.9f, 1f,  0.859f, 0.976f, //
                    0f, 0f, 1f, 1f,       0.859f, 0.976f, // head end
                }));

            axisLinesBinding = new VertexBufferBinding(axisLinesVertices,
                Utilities.SizeOf<float>() * 6,
                0);

        }

        protected override void DoRender()
        {
            // Get the context ref
            var context = this.DeviceManager.Direct3DContext;

            // Set the shader resource
            context.PixelShader.SetShaderResource(0, textureView);

            // Set the sampler state
            context.PixelShader.SetSampler(0, samplerState);

            // Render the Axis lines
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            // Pass in the line vertices
            context.InputAssembler.SetVertexBuffers(0, axisLinesBinding);
            // Draw the 18 vertices of our xyz-axis arrows
            context.Draw(18, 0);

        }
    }
}
