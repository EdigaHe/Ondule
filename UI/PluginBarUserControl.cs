using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rhino.UI;

namespace PluginBar
{
    public partial class PluginBarUserControl : MetroFramework.Controls.MetroUserControl, View,  IControllerModelObserver
    {

        Controller controller;
        public void setController(Controller cont)
        {
            controller = cont;
            
            if (EventWatcherHandlers.Instance.IsEnabled == false)
            {
                EventWatcherHandlers.Instance.Enable(true);
                EventWatcherHandlers.Instance.setRhinoModel(ref controller);
            }
        }

        public PluginBarUserControl()
        {
            InitializeComponent();


        }


        private void button1_Click(object sender, EventArgs e)
        {
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Mesh | Rhino.DocObjects.ObjectType.PolysrfFilter | Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.Curve;

            Rhino.DocObjects.ObjRef objRef;

            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            
            System.Windows.Forms.DialogResult rst;
            
            PluginBar.UI.PropertyWindow coilwindow = new UI.PropertyWindow();

            rst = coilwindow.ShowDialog(Rhino.RhinoApp.MainWindow());
            if(rst == System.Windows.Forms.DialogResult.OK)
            {
        

            }

            
        }


        private void mt_Select_Click(object sender, EventArgs e)
        {
            controller.selection();
        }

<<<<<<< HEAD
=======
           
        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        controller.deformBrep(brepRef);
        //        controller.setObjColor(1, ironObjRef); // 1 means iron
        //    }
        //}
>>>>>>> origin/master

        private void mt_LinearDeform_Click(object sender, EventArgs e)
        {
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.DocObjects.ObjRef objRef;
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            if (rc == Rhino.Commands.Result.Success)
            {
                controller.linearDeform(objRef);
            }



            
        }

        private void mt_wireFrame_Click(object sender, EventArgs e)
        {
<<<<<<< HEAD

            controller.wireframe();
=======
            const Rhino.DocObjects.ObjectType objFilter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.DocObjects.ObjRef objRef;
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one Curve", false, objFilter, out objRef);

            if (rc == Rhino.Commands.Result.Success)
            {
                Rhino.RhinoApp.WriteLine("Success");
                controller.ConvertLineToSpring(objRef);
            }
            else
            {
                Rhino.RhinoApp.WriteLine("Failed2");
            }
>>>>>>> origin/master
        }
    }
}
