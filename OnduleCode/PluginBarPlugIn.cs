using System.Drawing;
using Rhino.PlugIns;
using Rhino.UI;
using RMA.UI;

namespace OndulePlugin
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
        public OnduleTopBarControl DockBarUserControl
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
            DockBarUserControl = new OnduleTopBarControl();
            RhinoModel rhinoModel = new IncRhinoModel();

            Controller control = new IncController(DockBarUserControl, rhinoModel);

            // Create our dock bar
            DockBarDialog = new PluginBarDialog(PluginBarDialog.DockBarId(), "Ondule Plugin Topbar", DockBarUserControl);

            // Register our dock bar
            MRhinoDockBarManager.CreateRhinoDockBar(
              this,
              DockBarDialog,
              false, // Don't show yet...
              MRhinoUiDockBar.DockLocation.right,
              MRhinoUiDockBar.DockStyle.right,
              new Point(0, 0)
              );

            
            DockBarDialog.Initialize();
            DockBarDialog.ResizeFloating(new Size(400, 622));
            DockBarDialog.SetInitialSizeDockedVert(new Size(400, 622));
            DockBarDialog.SetSizeRange(MRhinoUiDockBar.DockLocation.right, new Size(368, 100), new Size(368, 622));
            MRhinoDockBarManager.ShowDockBar(PluginBarDialog.DockBarId(), true, false);

            return LoadReturnCode.Success;
        }
    }
}