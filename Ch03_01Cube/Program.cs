using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ch03_01Cube
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            // Enable object tracking
            SharpDX.Configuration.EnableObjectTracking = true;
#endif
            // Create the form to render to
            var form = new Form1();
            form.Text = "D3DRendering - Primitives";
            form.ClientSize = new System.Drawing.Size(1024, 768);
            form.Show();

            // Create and initialize the new D3D application
            using (D3DApp app = new D3DApp(form))
            {
                app.VSync = true;
                app.Initialize();
                app.Run();
            }

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
