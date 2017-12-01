using System;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Device1 = SharpDX.Direct3D11.Device1;
using SharpDX.DXGI;
using SharpDX.Windows;


namespace Ch01_03Debugging
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region Direct3D initialization

            // Create window to render to
            Form1 form = new Form1();
            form.Text = "D3DRendering - empty project";
            form.Width = 640;
            form.Height = 480;

            Device1 device;
            SwapChain1 swapChain;

            // Create D3D11 device
            using (var device11 = new Device(SharpDX.Direct3D.DriverType.Hardware,
                DeviceCreationFlags.None,
                new[]
                {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    //SharpDX.Direct3D.FeatureLevel.Level_10_1,
                }))
            {
                // Query device
                device = device11.QueryInterfaceOrNull<Device1>();
                if (device == null)
                    throw new NotSupportedException(
                        "SharpDX.Direct3D11.Device1 is not supported");
            }

            using (var dxgi = device.QueryInterface<SharpDX.DXGI.Device2>())
            using (var adapter = dxgi.Adapter)
            using (var factory = adapter.GetParent<Factory2>())
            {
                var desc1 = new SwapChainDescription1()
                {
                    Width = form.ClientSize.Width,
                    Height = form.ClientSize.Height,
                    Format = Format.R8G8B8A8_UNorm,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Scaling = Scaling.Stretch,
                    SwapEffect = SwapEffect.Discard,
                };

                swapChain = new SwapChain1(factory,
                    device,
                    form.Handle,
                    ref desc1,
                    new SwapChainFullScreenDescription()
                    {
                        RefreshRate = new Rational(60, 1),
                        Scaling = DisplayModeScaling.Centered,
                        Windowed = true
                    },
                    null);

                swapChain.Present(0, PresentFlags.None, new PresentParameters());
            }


            // create refrences
            var backbuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var renderTargetView = new RenderTargetView(device, backbuffer);

            #endregion

            #region Render loop

            RenderLoop.Run(form, () =>
            {
                // Clear

                var color = SharpDX.Color.LightBlue;
                long totalSeconds = 0;
                var lerpColor = SharpDX.Color.Lerp(
                    SharpDX.Color.LightBlue,
                    SharpDX.Color.Red,
                    (float)(1.0 * (totalSeconds % 10) / 11.0)
                    );
                device.ImmediateContext.ClearRenderTargetView(
                    renderTargetView, lerpColor);

                // Executing
                totalSeconds += 1;
                // Present the frame
                swapChain.Present(0, PresentFlags.None);
            });

            #endregion

            #region Direct3D cleanup

            renderTargetView.Dispose();
            device.Dispose();
            swapChain.Dispose();

            #endregion

        }
    }

}
