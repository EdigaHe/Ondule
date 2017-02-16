using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace PluginBar
{
    class EventWatcherHandlers
    {
        #region Members
        private readonly EventHandler<RhinoObjectEventArgs> m_add_rhino_object_handler;
        private readonly EventHandler<UndoRedoEventArgs> m_undo_redo_handler;
        private Controller controller;
        #endregion

        EventWatcherHandlers()
        {
            m_add_rhino_object_handler = new EventHandler<RhinoObjectEventArgs>(OnAddRhinoObject);
            m_undo_redo_handler = new EventHandler<UndoRedoEventArgs>(OnUndoRedo);

        }

        public bool IsEnabled { get; private set; }

        /// <summary>
        /// The one and only EventWatcherHandlers object
        /// </summary>
        static EventWatcherHandlers g_instance;

        /// <summary>
        /// Returns the one and only EventWatcherHandlers object
        /// </summary>
        public static EventWatcherHandlers Instance
        {
            get { return g_instance ?? (g_instance = new EventWatcherHandlers()); }
        }

        public void setRhinoModel(ref Controller c)
        {
            controller = c;
        }

        #region Events
        /// <summary>
        /// Triggered as long as one surface contained geometry is created.
        /// in auto mode, this will trigger the timer to decide to add it to the printing queue.
        /// if the new object is a cutter, because the cutter by itself is not a printing object,
        /// won't put it into printing list. but check the cutting timer event (which is the same as printing timer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnAddRhinoObject(object sender, Rhino.DocObjects.RhinoObjectEventArgs e)
        {

        }
        #endregion


        void OnUndoRedo(object sender, UndoRedoEventArgs e)
        {
            if(e.IsEndUndo)
            RhinoApp.WriteLine("** EVENT: Undo **");
        }

        public void Enable(bool enable)
        {
            if (enable != IsEnabled)
            {

                if (enable)
                {
                    RhinoDoc.AddRhinoObject += m_add_rhino_object_handler;
                    Command.UndoRedo += m_undo_redo_handler;
                   
                }
                else
                {
                    RhinoDoc.AddRhinoObject -= m_add_rhino_object_handler;
                    Command.UndoRedo -= m_undo_redo_handler;
                }
            }
            IsEnabled = enable;
        }
    }
}
