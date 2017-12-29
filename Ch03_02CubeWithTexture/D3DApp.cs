using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;
using SharpDX.Windows;
using System.Windows.Forms;

namespace Ch03_02CubeWithTexture
{
    public class D3DApp : D3DApplicationDesktop
    {
        // Vertex shader
        VertexShader vertexShader;

        // Pixel shader
        PixelShader pixelShader;

        // A pixel shader that renders the depth (black closer, white further away)
        PixelShader depthPixelShader;

        // The Lambert shader
        PixelShader lambertShader;

        PixelShader phongShader;

        PixelShader blinnPhongShader;

        // Vertex layout for the IA
        InputLayout vertexLayout;

        // A buffer that will be used to update the worldViewProjection 
        // constant buffer of the vertex shader
        Buffer perObjectBuffer;

        // A buffer that will be used to update the lights
        Buffer perFrameBuffer;

        // A buffer that will be used to update object materials
        Buffer perMaterialBuffer;

        SamplerState sampler;

        // Depth stencil
        DepthStencilState depthStencilState;

        public D3DApp(System.Windows.Forms.Form window)
            : base(window)
        { }

        protected override SwapChainDescription1 CreateSwapChainDescription()
        {
            return base.CreateSwapChainDescription();
        }

        protected override void CreateDeviceDependentResources(DeviceManager deviceManager)
        {
            base.CreateDeviceDependentResources(deviceManager);

            // Release all resources
            RemoveAndDispose(ref vertexShader);
            RemoveAndDispose(ref pixelShader);

            RemoveAndDispose(ref depthPixelShader);

            RemoveAndDispose(ref lambertShader);
            RemoveAndDispose(ref phongShader);
            RemoveAndDispose(ref blinnPhongShader);

            RemoveAndDispose(ref vertexLayout);
            RemoveAndDispose(ref perFrameBuffer);
            RemoveAndDispose(ref perObjectBuffer);
            RemoveAndDispose(ref perMaterialBuffer);

            RemoveAndDispose(ref sampler); //?

            RemoveAndDispose(ref depthStencilState);

            // Get a reference to the Device1 instance and context
            var device = deviceManager.Direct3DDevice;
            var context = deviceManager.Direct3DContext;

            // Compile and create the vertex shader and input layout
            using (var vertexShaderBytecode = HLSLCompiler.CompileFromFile(@"Shaders\VS.hlsl", "VSMain", "vs_5_0"))
            {
                vertexShader = ToDispose(new VertexShader(device, vertexShaderBytecode));
                // Layout from VertexShader input signature
                vertexLayout = ToDispose(new InputLayout(device,
                    vertexShaderBytecode.GetPart(ShaderBytecodePart.InputSignatureBlob),
                    //?ShaderSignature.GetInputSignature(vertexShaderBytecode),
                new[]
                {
                    // "SV_Position" = vertex coordinate in object space
                    new InputElement("SV_Position", 0, Format.R32G32B32_Float, 0, 0),
                    // "NORMAL" = the vertex normal
                    new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                    // "COLOR"
                    new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 24, 0),
                    // ?
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 28, 0),
                }));
            }

            // Compile and create the pixel shader
            using (var bytecode = HLSLCompiler.CompileFromFile(@"Shaders\SimplePS.hlsl", "PSMain", "ps_5_0"))
                pixelShader = ToDispose(new PixelShader(device, bytecode));

            // Compile and create the depth vertex and pixel shaders
            // This shader is for checking what the depth buffer would look like
            using (var bytecode = HLSLCompiler.CompileFromFile(@"Shaders\DepthPS.hlsl", "PSMain", "ps_5_0"))
                depthPixelShader = ToDispose(new PixelShader(device, bytecode));

            using (var bytecode = HLSLCompiler.CompileFromFile(@"Shaders\CubeMapDiffusePS.hlsl", "PSMain", "ps_5_0"))
                lambertShader = ToDispose(new PixelShader(device, bytecode));

            using (var bytecode = HLSLCompiler.CompileFromFile(@"Shaders\CubeMapBlinnPhongPS.hlsl", "PSMain", "ps_5_0"))
                phongShader = ToDispose(new PixelShader(device, bytecode));

            using (var bytecode = HLSLCompiler.CompileFromFile(@"Shaders\CubeMapPhongPS.hlsl", "PSMain", "ps_5_0"))
                blinnPhongShader = ToDispose(new PixelShader(device, bytecode));

            // Create the buffer that will store our WVP matrix
            perObjectBuffer = ToDispose(
                new SharpDX.Direct3D11.Buffer(
                    device,
                    Utilities.SizeOf<ConstantBuffers.PerObject>(),
                    ResourceUsage.Default,
                    BindFlags.ConstantBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None,
                    0
                    ));

            perFrameBuffer = ToDispose(
                new SharpDX.Direct3D11.Buffer(
                    device,
                    Utilities.SizeOf<ConstantBuffers.PerFrame>(),
                    ResourceUsage.Default,
                    BindFlags.ConstantBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None,
                    0
                    ));

            perMaterialBuffer = ToDispose(
                new SharpDX.Direct3D11.Buffer(
                    device,
                    Utilities.SizeOf<ConstantBuffers.PerMaterial>(),
                    ResourceUsage.Default,
                    BindFlags.ConstantBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None,
                    0
                    ));


            // Configure the OM to discard pixels ...
            depthStencilState = ToDispose(new DepthStencilState(
                device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthComparison = Comparison.Less,
                    DepthWriteMask = DepthWriteMask.All,
                    IsStencilEnabled = false,
                    StencilReadMask = 0xff, // no mask
                    StencilWriteMask = 0xff, // no mask
                    // Configure FrontFace depth/stencil operations
                    FrontFace = new DepthStencilOperationDescription()
                    {
                        Comparison = Comparison.Always,
                        PassOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment
                    },
                    // Configure BackFace depth/stencil operations
                    BackFace = new DepthStencilOperationDescription()
                    {
                        Comparison = Comparison.Always,
                        PassOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement
                    },

                }));

            // Tell the IA what the vertices will look like
            context.InputAssembler.InputLayout = vertexLayout;

            // Bind constant buffer to vertex shader stage
            context.VertexShader.SetConstantBuffer(0, perObjectBuffer);
            context.VertexShader.SetConstantBuffer(1, perFrameBuffer);
            context.VertexShader.SetConstantBuffer(2, perMaterialBuffer);

            // Set the vertex shader to run
            context.VertexShader.Set(vertexShader);

            // Set our pixel constant buffer
            context.PixelShader.SetConstantBuffer(1, perFrameBuffer);
            context.PixelShader.SetConstantBuffer(2, perMaterialBuffer);

            // Set the pixel shader to run
            context.PixelShader.Set(pixelShader);

            // Set our depth stencil state
            context.OutputMerger.DepthStencilState = depthStencilState;

            context.Rasterizer.State = ToDispose(new RasterizerState(device, new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                IsFrontCounterClockwise = false,
            }));
        }

        protected override void CreateSizeDependentResources(D3DApplicationBase app)
        {
            base.CreateSizeDependentResources(app);
        }

        public override void Run()
        {
            #region Create renderers

            // Axis lines renderer
            var axisLines = ToDispose(new AxisLinesRenderer());
            axisLines.Initialize(this);

            // Triangle renderer
            var triangle = ToDispose(new TriangleRenderer());
            triangle.Initialize(this);

            // Quad renderer
            var quad = ToDispose(new QuadRenderer());
            quad.Initialize(this);
            // scale 5x and translate
            quad.World = Matrix.Scaling(5f);
            quad.World.TranslationVector = new Vector3(0, -0.5f, 0);

            // Cube renderer
            var cube = ToDispose(new CubeRenderer());
            cube.Initialize(this);
            // move the cube
            cube.World = Matrix.Translation(-1, 0, 0);

            // Sphere renderer
            var sphere = ToDispose(new SphereRenderer());
            sphere.Initialize(this);
            // move the sphere
            sphere.World = Matrix.Translation(0, 0, 1.1f);

            // Create and initialize a Direct2D FPS text renderer
            var fps = ToDispose(new Common.FpsRenderer("Calibri", Color.CornflowerBlue, new Point(8, 8), 16));
            fps.Initialize(this);

            // Create and initialize a general purpose Direct2D text renderer
            // This will display some instructions and the current view and rotation offsets
            var textRenderer = ToDispose(new Common.TextRenderer("Calibri", Color.CornflowerBlue, new Point(8, 30), 12));
            textRenderer.Initialize(this);
            
            #endregion

            // World matrix
            var worldMatrix = Matrix.Identity;

            // Set camera position (x, y, -z)
            var cameraPosition = new Vector3(1, 1, -2);
            var cameraTarget = Vector3.Zero; // Looking at origin 0, 0, 0
            var cameraUp = Vector3.UnitY; // Y+ is Up

            // Create view matrix from our camera
            var viewMatrix = Matrix.LookAtLH(cameraPosition, cameraTarget, cameraUp);

            // Create the projection matrix
            // Field of view 60degrees = Pi/3 radians
            // Aspect ratio, Near clip, Far clip
            var projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 3f,
                Width / (float)Height,
                0.5f,
                100f);

            // Maintain the correct aspect ratio on resize
            Window.Resize += (s, e) =>
            {
                projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 3f,
                    Width / (float)Height,
                    0.5f,
                    100f);
            };

            #region Rotation and window event handlers
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);

            Action updateText = () =>
            {
                textRenderer.Text =
                    String.Format("World rotation ({0}) (Up/Down Left/Right Wheel +-)"
                    + "\nView ({1}) (A/D W/S Shift+Wheel)"
                    + "\nPress X to reinitialize the device and resources (device ptr: {2})"
                    + "\nPress Z to show/hide depth buffer"
                    + "\nPress F to toggle wireframe"
                    + "\nPress 1-4 to toggle shaders",
                    rotation,
                    viewMatrix.TranslationVector,
                    DeviceManager.Direct3DDevice.NativePointer);
            };

            Dictionary<Keys, bool> keyToggles = new Dictionary<Keys, bool>();
            keyToggles[Keys.Z] = false;
            keyToggles[Keys.F] = false;

            // Support keyboard/mouse input
            var moveFactor = 0.02f;
            var shiftKey = false;
            var ctrlKey = false;

            Window.KeyDown += (s, e) =>
            {
                shiftKey = e.Shift;
                ctrlKey = e.Control;
                var context = DeviceManager.Direct3DContext;

                switch (e.KeyCode)
                {
                    // WASD -> pans view
                    case Keys.A:
                        viewMatrix.TranslationVector += new Vector3(moveFactor * 2, 0f, 0f);
                        break;
                    case Keys.D:
                        viewMatrix.TranslationVector -= new Vector3(moveFactor * 2, 0f, 0f);
                        break;
                    case Keys.S:
                        if (shiftKey)
                            viewMatrix.TranslationVector += new Vector3(0f, moveFactor * 2, 0f);
                        else
                            viewMatrix.TranslationVector += new Vector3(0f, 0f, 1f) * moveFactor * 2;
                        break;
                    case Keys.W:
                        if (shiftKey)
                            viewMatrix.TranslationVector -= new Vector3(0f, moveFactor * 2, 0f);
                        else
                            viewMatrix.TranslationVector -= new Vector3(0f, 0f, 1f) * moveFactor * 2;
                        break;

                    // Up/Down and Left/Right - rotates around  X / Y
                    case Keys.Down:
                        worldMatrix *= Matrix.RotationX(-moveFactor);
                        rotation -= new Vector3(moveFactor, 0f, 0f);
                        break;
                    case Keys.Up:
                        worldMatrix *= Matrix.RotationX(moveFactor);
                        rotation += new Vector3(moveFactor, 0f, 0f);
                        break;
                    case Keys.Left:
                        worldMatrix *= Matrix.RotationY(-moveFactor);
                        rotation -= new Vector3(0f, moveFactor, 0f);
                        break;
                    case Keys.Right:
                        worldMatrix *= Matrix.RotationY(moveFactor);
                        rotation += new Vector3(0f, moveFactor, 0f);
                        break;

                    case Keys.X:
                        // To test correct resource recreation
                        // Simulate device reset or lost
                        System.Diagnostics.Debug.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
                        DeviceManager.Initialize(DeviceManager.Dpi);
                        System.Diagnostics.Debug.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
                        break;
                    case Keys.Z:
                        keyToggles[Keys.Z] = !keyToggles[Keys.Z];
                        if (keyToggles[Keys.Z])
                        {
                            context.PixelShader.Set(depthPixelShader);
                        }
                        else
                        {
                            context.PixelShader.Set(pixelShader);
                        }
                        break;
                    case Keys.F:
                        keyToggles[Keys.F] = !keyToggles[Keys.F];
                        RasterizerStateDescription rasterDesc;
                        if (context.Rasterizer.State != null)
                            rasterDesc = context.Rasterizer.State.Description;
                        else
                            rasterDesc = new RasterizerStateDescription()
                            {
                                CullMode = CullMode.Back,
                                FillMode = FillMode.Solid
                            };
                        if (keyToggles[Keys.F])
                        {
                            rasterDesc.FillMode = FillMode.Wireframe;
                            context.Rasterizer.State = ToDispose(new RasterizerState(context.Device, rasterDesc));
                        }
                        else
                        {
                            rasterDesc.FillMode = FillMode.Solid;
                            context.Rasterizer.State = ToDispose(new RasterizerState(context.Device, rasterDesc));
                        }
                        break;
                    case Keys.D1:
                        context.PixelShader.Set(pixelShader);
                        break;
                    case Keys.D2:
                        context.PixelShader.Set(lambertShader);
                        break;
                    case Keys.D3:
                        context.PixelShader.Set(phongShader);
                        break;
                    case Keys.D4:
                        context.PixelShader.Set(blinnPhongShader);
                        break;

                }

                updateText();
            };

            Window.KeyUp += (s, e) =>
            {
                // Clear shift/ctrl keys so they aren't sticky
                if (e.KeyCode == Keys.ShiftKey)
                    shiftKey = false;
                if (e.KeyCode == Keys.ControlKey)
                    ctrlKey = false;
            };

            Window.MouseWheel += (s, e) =>
            {
                if (shiftKey)
                {
                    // Zoom in/out
                    viewMatrix.TranslationVector -= new Vector3(0f, 0f, (e.Delta / 120f) * moveFactor * 2);
                }
                else
                {
                    // rotate around Z-axis
                    viewMatrix *= Matrix.RotationZ((e.Delta / 120f) * moveFactor);
                    rotation += new Vector3(0f, 0f, (e.Delta / 120f) * moveFactor);
                }

                updateText();
            };

            var lastX = 0;
            var lastY = 0;

            Window.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    lastX = e.X;
                    lastY = e.Y;
                }
            };

            Window.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    var yRotate = lastX - e.X;
                    var xRotate = lastY - e.Y;
                    lastY = e.Y;
                    lastX = e.X;

                    // Mouse move changes 
                    viewMatrix *= Matrix.RotationX(xRotate * moveFactor);
                    viewMatrix *= Matrix.RotationY(yRotate * moveFactor);

                    updateText();
                }
            };

            // Display instructions with initial values
            updateText();

            #endregion

            var clock = new System.Diagnostics.Stopwatch();
            clock.Start();

            #region Render Loop

            RenderLoop.Run(Window, () =>
            {
                // Start of frame

                var context = DeviceManager.Direct3DContext;

                // Clear depth stencil view
                context.ClearDepthStencilView(DepthStencilView,
                    DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                    1.0f,
                    0);

                // Clear render target view
                context.ClearRenderTargetView(RenderTargetView, Color.White);

                // ViewProjection matrix
                var viewProjection = Matrix.Multiply(viewMatrix, projectionMatrix);

                // Extract camera position from view
                var camPosition = Matrix.Transpose(Matrix.Invert(viewMatrix)).Column4;
                cameraPosition = new Vector3(camPosition.X, camPosition.Y, camPosition.Z);

                // If Keys.CtrlKey is down, auto rotate viewProjection based on time
                if (ctrlKey)
                {
                    var time = clock.ElapsedMilliseconds / 1000.0f;
                    viewProjection = Matrix.RotationY(time * 1.8f) * Matrix.RotationZ(time * 1f) * Matrix.RotationZ(time * 0.6f) * viewProjection;
                }

                // Update the per frame constant buffer
                var perFrame = new ConstantBuffers.PerFrame();
                perFrame.CameraPosition = cameraPosition;
                perFrame.Light.Color = Color.White;
                var lightDir = Vector3.Transform(new Vector3(1f, -1f, -1f), worldMatrix);
                perFrame.Light.Direction = new Vector3(lightDir.X, lightDir.Y, lightDir.Z);

                context.UpdateSubresource(ref perFrame, perFrameBuffer);

                // Render each object

                var perMaterial = new ConstantBuffers.PerMaterial();
                perMaterial.Ambient = new Color4(0.2f);
                perMaterial.Diffuse = Color.White;
                perMaterial.Emissive = new Color4(0);
                perMaterial.Specular = Color.White;
                perMaterial.SpecularPower = 20f;
                perMaterial.HasTexture = 0;
                perMaterial.UVTransform = Matrix.Identity;
                context.UpdateSubresource(ref perMaterial, perMaterialBuffer);

                var perObject = new ConstantBuffers.PerObject();

                // QUAD
                perObject.World = quad.World * worldMatrix;
                perObject.WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(perObject.World));
                perObject.WorldViewProjection = perObject.World * viewProjection;
                perObject.Transpose();
                context.UpdateSubresource(ref perObject, perObjectBuffer);
                quad.Render();

                // CUBE
                perObject.World = cube.World * worldMatrix;
                perObject.WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(perObject.World));
                perObject.WorldViewProjection = perObject.World * viewProjection;
                perObject.Transpose();
                context.UpdateSubresource(ref perObject, perObjectBuffer);
                cube.Render();

                // SPHERE
                perObject.World = sphere.World * worldMatrix;
                perObject.WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(perObject.World));
                perObject.WorldViewProjection = perObject.World * viewProjection;
                perObject.Transpose();
                context.UpdateSubresource(ref perObject, perObjectBuffer);
                sphere.Render();

                // Render FPS
                fps.Render();
                
                // Render instructions + position changes
                textRenderer.Render();

                // Present the frame
                Present();
            });

            #endregion
        }

    }
}
