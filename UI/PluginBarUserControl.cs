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
    public partial class PluginBarUserControl : UserControl, View, IControllerModelObserver
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

        private void PluginBarUserControl_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Limit the types of objects the user can select
            // const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Mesh | Rhino.DocObjects.ObjectType.PolysrfFilter | Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.Curve;

            // Declare a variable to hold the selected object
            // Rhino.DocObjects.ObjRef objRef;

            // Prompt the user to select an object from the command line
            // Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);
            
            // Declare a variable to hold the success status if the form is in a modal mode
            //System.Windows.Forms.DialogResult rst;
            
            // Initialize a variable to hold the form object
            PluginBar.UI.PropertyWindow springPropWindow = new UI.PropertyWindow();

            // Store the success status of the form if in modal mode
            //rst = springPropWindow.ShowDialog(Rhino.RhinoApp.MainWindow());

            // Show the form to the user, bring into current focus
            springPropWindow.Show();
            
            // Do something if the form is created successfully in modal mode
            //if(rst == System.Windows.Forms.DialogResult.OK)
            
        }

        // Code from Huaishu

        //private void btn_deform_Click(object sender, EventArgs e)
        //{
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Brep;
        //    Rhino.DocObjects.ObjRef brepRef;
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one Brep", false, filter, out brepRef);

           
        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        controller.deformBrep(brepRef);
        //        controller.setObjColor(1, ironObjRef); // 1 means iron
        //    }
        //}

        private void button2_Click(object sender, EventArgs e)
        {
            // Initialize a variable to hold the form object
            PluginBar.UI.SpringPopUp2 springPropWindow = new UI.SpringPopUp2();

            // Send the values from the plugin bar to the pop-up window
            springPropWindow.type = dd_type.SelectedItem.ToString();
            springPropWindow.mode = dd_mode.SelectedItem.ToString();

            // Show the form to the user, bring into current focus
            springPropWindow.Show();
            
        }

        

        
    }
}
