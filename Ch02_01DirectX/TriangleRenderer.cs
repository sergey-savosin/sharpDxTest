using Common;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Ch02_01DirectX
{
    public class TriangleRenderer : RendererBase
    {
        // The triangle vertex buffer
        Buffer triangleVerteces;

        // The vertex buffer binding structure for the triangle
        VertexBufferBinding triangleBinging;

        // Shader texture resource
        ShaderResourceView textureView;

        // Control sampling behavior with this state
        SamplerState samplerState;

        protected override void CreateDeviceDependentResources()
        {
            base.CreateDeviceDependentResources();

            RemoveAndDispose(ref triangleVerteces);

            // Retrieve Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            // Load texture
            var imagingFactory = DeviceManager.WICFactory;
            var texture = LoadTexture.LoadFromFile(device, imagingFactory, "Texture2.png");
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

            // Create a triangle
            triangleVerteces = ToDispose(Buffer.Create(
                device,
                BindFlags.VertexBuffer,
                new[]
                {
                    /* Vertex position       -     Vertex UV */
                    0.75f, -0.75f, -0.001f, 1.0f,  1.0f, 1.0f, // Base-right
                    -0.75f, -0.75f, -0.001f, 1.0f, 0.0f, 1.0f, // Base-left
                    0.0f, 0.75f, -0.001f, 1.0f,    0.5f, 0.0f, // Apex
                }));

            triangleBinging = new VertexBufferBinding(triangleVerteces, Utilities.SizeOf<float>() * 6, 0);
        }

        protected override void DoRender()
        {
            // Get the context reference
            var context = this.DeviceManager.Direct3DContext;

            // Set the shader resource
            context.PixelShader.SetShaderResource(0, textureView);

            // Set the sampler state
            context.PixelShader.SetSampler(0, samplerState);

            // Render the triangle
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            // Pass in the triangle vertices
            context.InputAssembler.SetVertexBuffers(0, triangleBinging);

            // Draw the 3 vertices of our triangle
            context.Draw(3, 0);
        }
    }
}
