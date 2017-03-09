using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using RMA.UI;

namespace PluginBar.UI
{
    [System.Runtime.InteropServices.Guid("dbc3ef6e-35c1-46fe-84c6-f759017306fd")]

    public class PopUpCommand2 : Rhino.Commands.Command
    {
        public static Rhino.RhinoDoc rhinoDoc = null;
        static PopUpCommand2 m_thecommand;

        public SpringPopUp2 Form
        {
            get;
            set;
        }

        public PopUpCommand2()
        {
            // Rhino only creates one instance of each command class defined in a plug-in, so it is
            // safe to hold on to a static reference.
            m_thecommand = this;
        }

        ///<summary>The one and only instance of this command</summary>
        public static PopUpCommand2 TheCommand
        {
            get { return m_thecommand; }
        }

        ///<returns>The command name as it appears on the Rhino command line</returns>
        public override string EnglishName
        {
            get { return "Spring2"; }
        }

        protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
        {
            if (null == Form)
            {
                Form = new SpringPopUp2();
                Form.FormClosed += new System.Windows.Forms.FormClosedEventHandler(Form_FormClosed);
                Form.Show(RhinoApp.MainWindow());
            }

            return Rhino.Commands.Result.Success;
        }

        void Form_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            Form.Dispose();
            Form = null;
        }
    }
}

