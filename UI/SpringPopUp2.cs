using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.UI;

namespace PluginBar.UI
{
    public partial class SpringPopUp2 : Form
    {
        public String type;
        public String mode;

        public RhinoDoc myDoc;
        public Controller controller;

        public Guid numCurve = Guid.Empty;
        public Guid numPipe = Guid.Empty;
        public Guid numSweep = Guid.Empty;
        Rhino.DocObjects.ObjRef obj;

        //private Boolean alongCurve = false;
        private double coilD = 1;
        private double springD = 3;
        private double turns = 3;
        private double pitch = 3;
        private double width = 2;
        private double height = 1;
        //private String startPT = "-5,5,0";

        public SpringPopUp2()
        {
            InitializeComponent();
        }


        private void SpringPopUp2_Load(object sender, EventArgs e)
        {

            // Print values to the command line for verification
            Rhino.RhinoApp.WriteLine("Type: " + type);
            Rhino.RhinoApp.WriteLine("Mode: " + mode);
            
            // Activate the Osnap functionality
            String snapString = String.Format("Osnap E Enter");
            Rhino.RhinoApp.RunScript(snapString, false);

            const Rhino.DocObjects.ObjectType objFilter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one Curve", false, objFilter, out obj);

            Guid[] numArr = new Guid[2];

            if (type == "Helical")
            {
                numArr = controller.helicalCurve(obj, numCurve, numPipe, pitch, turns, springD, coilD);
                numCurve = numArr[0];
                numPipe = numArr[1];
            }
            else if (type == "Machined")
            {
                numArr = controller.machineCurve(obj, numCurve, numSweep, pitch, turns, springD, width, height);
                numCurve = numArr[0];
                numSweep = numArr[1];
            }
            else if (type == "Z")
            {
                numArr = controller.zCurve(obj, numCurve, numSweep, width, height);
                numCurve = numArr[0];
                numSweep = numArr[1];
            }

        }


        private void slider_coilD_Scroll(object sender, EventArgs e)
        {
            // Update label as slider is moved
            lb_coilDVal.Text = slider_coilD.Value.ToString();
            // Update class variable as slider is moved
            coilD = slider_coilD.Value;

            update();
        }

        private void slider_springD_Scroll(object sender, EventArgs e)
        {
            // Update label as slider is moved
            lb_springDVal.Text = slider_springD.Value.ToString();
            // Update class variable as slider is moved
            springD = slider_springD.Value;

            update();
        }

        private void slider_turns_Scroll(object sender, EventArgs e)
        {
            // Update label as slider is moved
            lb_turnsVal.Text = slider_turns.Value.ToString();
            // Update class variable as slider is moved
            turns = slider_turns.Value;

            update();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void update()
        {
            if (type == "Helical")
            {
                controller.helicalCurve(obj, numCurve, numPipe, pitch, turns, springD, coilD);
            }
            else if (type == "Machined")
            {
                controller.machineCurve(obj, numCurve, numSweep, pitch, turns, springD, width, height);
            }
            else if (type == "Z")
            {
                controller.zCurve(obj, numCurve, numSweep, width, height);
            }
        }






        // OLD CODE

        // Default parameter. NEED TO EXPAND FOR MORE OPTIONS
        //if (type == "Helical")
        //{
        //    String scriptString = String.Format("Helix A SelLast T {0} {1} {2}", turns, springD, startPT);
        //    Rhino.RhinoApp.RunScript(scriptString, false); // Send command to command line
        //    String pipeString = String.Format("SelLast Pipe {0} {1} Enter", coilD, coilD);
        //    Rhino.RhinoApp.RunScript(pipeString, false); // Send command to command line
        //}
        //else if (type == "Machined")
        //{
        //    String scriptString = String.Format("Helix A SelLast T {0} {1} {2}", turns, springD, startPT);
        //    Rhino.RhinoApp.RunScript(scriptString, false); // Send command to command line 
        //    String areaString = String.Format("Rectangle Center Pause 2 2 Enter");
        //    Rhino.RhinoApp.RunScript(areaString, false); // Send command to command line
        //    String sweepString = String.Format("Sweep1");
        //    Rhino.RhinoApp.RunScript(sweepString, false); // Send command to command line
        //}
        //else if (type == "Z")
        //{
        //}

        // THIS IS THE SOURCE OF REFRESHING ERRORS
        //private void adjust()
        //{
        //    // Delete the last helix
        //    Rhino.RhinoApp.RunScript("SelLast Delete", false);
        //    // Create a new helix with updated characteristics
        //    String scriptString = String.Format("Helix A SelCrv T {0} {1} {2}", turns, springD, startPT);
        //    // Run the command
        //    Rhino.RhinoApp.RunScript(scriptString, false); // Send command to command line
        //    // Bring the pop-up window back into focus
        //    //this.Focus();

        //}

    }
}

