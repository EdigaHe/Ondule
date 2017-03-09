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
    [System.Runtime.InteropServices.Guid("ebb73a6d-3ce4-4c5b-846b-aea98cb2db38")]
    //Rhino.Commands.CommandStyle(Rhino.Commands.Style.ScriptRunner)

    public class PopUpCommand : Rhino.Commands.Command
    {
        public static Rhino.RhinoDoc rhinoDoc = null;
        static PopUpCommand m_thecommand;

        public PropertyWindow Form
        {
            get;
            set;
        }

        public PopUpCommand()
        {
            // Rhino only creates one instance of each command class defined in a plug-in, so it is
            // safe to hold on to a static reference.
            m_thecommand = this;
        }

        ///<summary>The one and only instance of this command</summary>
        public static PopUpCommand TheCommand
        {
            get { return m_thecommand; }
        }

        ///<returns>The command name as it appears on the Rhino command line</returns>
        public override string EnglishName
        {
            get { return "Spring"; }
        }

        protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
        {

            if (null == Form)
            {
                Form = new PropertyWindow();
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

