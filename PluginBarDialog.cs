using System;
using System.Drawing;
using System.Windows.Forms;
using RMA.UI;

namespace OndulePlugin
{
    public class PluginBarDialog : MRhinoUiDockBar
    {
        /// <summary>
        /// Public constructor
        /// </summary>
        public PluginBarDialog()
          : base()
        {
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        public PluginBarDialog(Guid id, string name, Control control)
          : base(id, name, control)
        {
        }

        public void Initialize()
        {
            m_border_size = 1;

            m_cy_gripper = (int)gripper_sizes.small_gripper;
            UseSmallGripper(true);

            SetDockBarFlag(control_bar_flags.cbf_show_name_when_docked_horz, false);
            SetDockBarFlag(control_bar_flags.cbf_show_name_when_docked_vert, false);

            ShowCaptionButton(caption_button.cb_close, false);

        }

        /// <summary>
        /// CanClose
        /// </summary>
        public override bool CanClose()
        {
            return false;
        }

        /// <summary>
        /// ID of this dockbar
        /// </summary>
        static public Guid DockBarId()
        {
            return new System.Guid("9FFAC36F-F6A9-4CE0-89B7-00AFFCBDCA08");
        }
    }
}
