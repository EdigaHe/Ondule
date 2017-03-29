using System.Drawing;
using Rhino.PlugIns;
using Rhino.UI;
using RMA.UI;

namespace PluginBar
{
    /// <summary>
    /// SampleCsDockBarPlugIn plug-in class.
    /// DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.
    ///</summary>
    public class PluginBarPlugIn : PlugIn
    {
        // Public constructor
        public PluginBarPlugIn()
        {
            Instance = this;
        }

        /// <summary>
        /// Returns the one and only instance of the SampleCsDockBarPlugIn plug-in.
        /// </summary>
        public static PluginBarPlugIn Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the one and only instance of the SampleCsDockBarDialog panel
        /// </summary>
        public PluginBarDialog DockBarDialog
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the one and only instance of the SampleCsDockBarUserControl panel
        /// </summary>
        public PluginBarUserControl DockBarUserControl
        {
            get;
            private set;
        }



        /// <summary>
        /// OnLoad override.
        /// Called when the plug-in is being loaded.
        /// </summary>
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            // Create our user control
            DockBarUserControl = new PluginBarUserControl();
            RhinoModel rhinoModel = new IncRhinoModel();

            Controller control = new IncController(DockBarUserControl, rhinoModel);

            // Create our dock bar
            DockBarDialog = new PluginBarDialog(PluginBarDialog.DockBarId(), "Slinky Plugin", DockBarUserControl);

            // Register our dock bar
            MRhinoDockBarManager.CreateRhinoDockBar(
              this,
              DockBarDialog,
              false, // Don't show yet...
              MRhinoUiDockBar.DockLocation.top,
              MRhinoUiDockBar.DockStyle.top,
              new Point(0, 0)
              );

            
            DockBarDialog.Initialize();
            DockBarDialog.ResizeFloating(new Size(200, 48));
            MRhinoDockBarManager.ShowDockBar(PluginBarDialog.DockBarId(), true, false);


            return LoadReturnCode.Success;
        }
    }
}