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

namespace Ch02_01DirectX
{
    public class D3DApp : D3DApplicationDesktop
    {
        // Vertex shader
        ShaderBytecode vertexShaderBytecode;
        VertexShader vertexShader;

        // Pixel shader
        ShaderBytecode pixelShaderBytecode;
        PixelShader pixelShader;

        // A vertex shader that gives depth info to pixel shader
        ShaderBytecode depthVertexShaderBytecode;
        VertexShader depthVertexShader;

        // A pixel shader that renders the depth (black closer, white further away)
        ShaderBytecode depthPixelShaderBytecode;
        PixelShader depthPixelShader;

        // Vertex layout for the IA
        InputLayout vertexLayout;

        // A buffer to update constant buffer
        // used by vertex shader
        Buffer worldViewProjectionBuffer;

        // Depth stencil
        DepthStencilState depthStencilState;

        public D3DApp(System.Windows.Forms.Form window)
            : base(window)
        { }

        protected override SwapChainDescription1 CreateSwapChainDescription()
        {
            var description = base.CreateSwapChainDescription();
            description.SampleDescription.Count = 4;
            description.SampleDescription.Quality = 0;
            //? description.SampleDescription = new SampleDescription(4, 0);
            return description;
        }

        protected override void CreateDeviceDependentResources(DeviceManager deviceManager)
        {
            base.CreateDeviceDependentResources(deviceManager);

            // Release all resources
            RemoveAndDispose(ref vertexShader);
            RemoveAndDispose(ref vertexShaderBytecode);
            RemoveAndDispose(ref pixelShader);
            RemoveAndDispose(ref pixelShaderBytecode);

            RemoveAndDispose(ref depthPixelShader);
            RemoveAndDispose(ref depthPixelShaderBytecode);
            RemoveAndDispose(ref depthVertexShader);
            RemoveAndDispose(ref depthVertexShaderBytecode);

            RemoveAndDispose(ref vertexLayout);
            RemoveAndDispose(ref worldViewProjectionBuffer);
            RemoveAndDispose(ref depthStencilState);

            // Get a reference to the Device1 instance and context
            var device = deviceManager.Direct3DDevice;
            var context = deviceManager.Direct3DContext;

            ShaderFlags shaderFlags = ShaderFlags.None;
#if DEBUG
            shaderFlags = ShaderFlags.Debug;
#endif

            // Compile and create the vertex shader
            //--var res = ShaderBytecode.CompileFromFile("Simple.hlsl", "VSMain", "vs_5_0", shaderFlags);
            vertexShaderBytecode = ToDispose(
                ShaderBytecode.CompileFromFile("Simple.hlsl", "VSMain", "vs_5_0", shaderFlags));
            vertexShader = ToDispose(new VertexShader(device, vertexShaderBytecode));

            // Compile and create the pixel shader
            pixelShaderBytecode = ToDispose(
                ShaderBytecode.CompileFromFile("Simple.hlsl", "PSMain", "ps_5_0", shaderFlags));
            pixelShader = ToDispose(new PixelShader(device, pixelShaderBytecode));

            // Compile and create the depth vertex and pixel shaders
            // These shaders are for checking what the depth buffer should look like
            depthVertexShaderBytecode = ToDispose(ShaderBytecode.CompileFromFile("Depth.hlsl", "VSMain", "vs_5_0", shaderFlags));
            depthVertexShader = ToDispose(new VertexShader(device, depthVertexShaderBytecode));
            depthPixelShaderBytecode = ToDispose(ShaderBytecode.CompileFromFile("Depth.hlsl", "PSMain", "ps_5_0", shaderFlags));
            depthPixelShader = ToDispose(new PixelShader(device, depthPixelShaderBytecode));

            // Layout from VertexShader input signature
            vertexLayout = ToDispose(
                new InputLayout(
                    device,
                    vertexShaderBytecode.GetPart(ShaderBytecodePart.InputSignatureBlob),
                    new[]
                    {
                        // input semantic SV_Position = vertex coord in object space
                        new InputElement("SV_Position", 0, Format.R32G32B32A32_Float, 0, 0),
                        // input semantic TEXTCOORD = vertex texture coordinate
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
                        //new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)

                    }
                ));

            // Create the buffer that will store our WVP matrix
            worldViewProjectionBuffer = ToDispose(
                new SharpDX.Direct3D11.Buffer(
                    device,
                    Utilities.SizeOf<Matrix>(),
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
            context.VertexShader.SetConstantBuffer(0, worldViewProjectionBuffer);

            // Set the vertex shader to run
            context.VertexShader.Set(vertexShader);

            // Set the pixel shader to run
            context.PixelShader.Set(pixelShader);

            // Set our depth stencil state
            context.OutputMerger.DepthStencilState = depthStencilState;
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
                    + "\nPress Z to show/hide depth buffer",
                    rotation,
                    viewMatrix.TranslationVector,
                    DeviceManager.Direct3DDevice.NativePointer);
            };

            bool useDepthShaders = false;

            // Support keyboard/mouse input
            var moveFactor = 0.02f;
            var shiftKey = false;
            var ctrlKey = false;

            Window.KeyDown += (s, e) =>
            {
                shiftKey = e.Shift;
                ctrlKey = e.Control;

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
                        var context = DeviceManager.Direct3DContext;
                        useDepthShaders = !useDepthShaders;
                        if (useDepthShaders)
                        {
                            context.VertexShader.Set(depthVertexShader);
                            context.PixelShader.Set(depthPixelShader);
                        }
                        else
                        {
                            context.VertexShader.Set(vertexShader);
                            context.PixelShader.Set(pixelShader);
                        }
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

                // If Keys.CtrlKey is down, auto rotate viewProjection based on time
                if (ctrlKey)
                {
                    var time = clock.ElapsedMilliseconds / 1000.0f;
                    viewProjection = Matrix.RotationY(time * 1.8f) * Matrix.RotationZ(time * 1f) * Matrix.RotationZ(time * 0.6f) * viewProjection;
                }

                // WorldViewProjection matrix
                var worldViewProjection = worldMatrix * viewProjection;

                // HLSL defaults to "column-major" order matrices
                // SharpDx uses row-major matrices
                worldViewProjection.Transpose();

                // Write the WorldViewProjection to constant buffer
                context.UpdateSubresource(ref worldViewProjection, worldViewProjectionBuffer);

                // Render the primitives
                axisLines.Render();
                triangle.Render();
                quad.Render();

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
