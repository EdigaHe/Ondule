using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using RMA.UI;

namespace PluginBar
{
    [System.Runtime.InteropServices.Guid("679d72b6-7877-4e1a-8728-650ca4487b48")]
    public class PluginBarCommand : Command
    {
        public static Rhino.RhinoDoc rhinoDoc = null;

        public PluginBarCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PluginBarCommand Instance
        {
            get;
            private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "SlinkyPlugin"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            rhinoDoc = doc;

            var dock_bar_id = PluginBarDialog.DockBarId();
            var visible = MRhinoDockBarManager.IsDockBarVisible(dock_bar_id);

            var prompt = (visible)
                    ? "Dockbar is visible. New value"
                    : "Dockbar panel is hidden. New value";

            RhinoApp.WriteLine(prompt);

            var go = new GetOption();
            go.SetCommandPrompt("Choose option");
            var hide_index = go.AddOption("Hide");
            var show_index = go.AddOption("Show");
            var toggle_index = go.AddOption("Toggle");

            go.Get();
            if (go.CommandResult() != Rhino.Commands.Result.Success)
                return go.CommandResult();

            var option = go.Option();
            if (null == option)
                return Rhino.Commands.Result.Failure;

            var index = option.Index;
            if (index == hide_index)
            {
                if (visible)
                    MRhinoDockBarManager.ShowDockBar(dock_bar_id, false, false); ;
            }
            else if (index == show_index)
            {
                if (!visible)
                    MRhinoDockBarManager.ShowDockBar(dock_bar_id, true, false);
            }
            else if (index == toggle_index)
            {
                MRhinoDockBarManager.ShowDockBar(dock_bar_id, !visible, false);
            }

            return Result.Success;
        }
    }
}
