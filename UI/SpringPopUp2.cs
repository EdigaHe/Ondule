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

        // QUANTITATIVE CHARACTERISTICS
        // Simple variables
        private double coilD = 1;
        private double springD = 3;
        private double turns = 3;
        private double pitch = 3;
        private double width = 2;
        private double height = 1;
        private double length;
        private double G = 1.287e9;
		// Complex variables
		private double[] kRange = { 9.31e4, 2.5602e5, 4.1895e5, 5.8187e5, 7.4479e5 };
		private double k;
		private double kCoeff;
		private double C;
		private double S;

        // QUALITATIVE CHARACTERISTICS
        private double relStiffness = 3;
        private double springIndex = 3;
        private double slendernessRatio = 9;

        // Public constructor
        public SpringPopUp2()
        {
            InitializeComponent();
        }

        // Run when the plugin is selected by the user
        private void SpringPopUp2_Load(object sender, EventArgs e)
        {

            // Print values to the command line for verification
            Rhino.RhinoApp.WriteLine("Type: " + type);
            Rhino.RhinoApp.WriteLine("Mode: " + mode);
            
            // Activate the Osnap functionality (optional)
            String snapString = String.Format("Osnap E Enter");
            Rhino.RhinoApp.RunScript(snapString, false);

            // Restrict the user's selection to a curve
            const Rhino.DocObjects.ObjectType objFilter = Rhino.DocObjects.ObjectType.Curve;

            // Instruct the user to select a curve with the cursor
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one Curve", false, objFilter, out obj);

            // Create an array to hold ID values for the path curve and the cross-sectional area curve
            Guid[] numArr = new Guid[2];
            length = obj.Curve().GetLength();

            // Execute different commands depending on the type of spring desired
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
            else if (type == "Cantilever")
            {
                numArr = controller.zCurve(obj, numCurve, numSweep, width, height);
                numCurve = numArr[0];
                numSweep = numArr[1];
            }

        }

		// SLIDERS
		// _________________________________________________________________

		private void slider_coilD_Scroll(object sender, EventArgs e)
        {
            lb_coilDVal.Text = slider_coilD.Value.ToString(); // Update label as slider is moved
			coilD = slider_coilD.Value; // Update class variable as slider is moved
			update();
        }

        private void slider_springD_Scroll(object sender, EventArgs e)
        {
            lb_springDVal.Text = slider_springD.Value.ToString(); // Update label as slider is moved						  
			springD = slider_springD.Value; // Update class variable as slider is moved
			update();
        }

        private void slider_turns_Scroll(object sender, EventArgs e)
        {
            lb_turnsVal.Text = slider_turns.Value.ToString(); // Update label as slider is moved
			turns = slider_turns.Value; // Update class variable as slider is moved
			update2();
        }

        private void slider_stiffness_Scroll(object sender, EventArgs e)
        {
            lb_stiffnessVal.Text = slider_stiffness.Value.ToString();  // Update label as slider is moved
			relStiffness = slider_turns.Value; // Update class variable as slider is moved
            update2();
        }

        private void slider_slenderness_Scroll(object sender, EventArgs e)
        {
            lb_slendernessVal.Text = slider_slenderness.Value.ToString(); // Update label as slider is moved											  
			slendernessRatio = slider_slenderness.Value; // Update class variable as slider is moved
			update2();
        }

        private void slider_index_Scroll(object sender, EventArgs e)
        {
            lb_indexVal.Text = (slider_index.Value / 2).ToString(); // Update label as slider is moved
			springIndex = slider_index.Value / 2; // Update class variable as slider is moved
			update2();
        }


        // OK & UPDATE
        // _______________________________________________________________

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // The purpose of this method is to administer real time updates by
        // calling external methods to reflect the changes in the viewport
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
            else if (type == "Cantilever")
            {
                controller.zCurve(obj, numCurve, numSweep, width, height);
            }
        }

        // An unfinished modification to the original update method that would 
        // incorporate complex variables 
        private void update2()
        {
            if (type == "Helical")
            {
                coilD = length / C / S;

                springD = length / S;

                kCoeff = kRange[Convert.ToInt32(relStiffness - 1)];

                k = coilD * G / (8 * Math.Pow(C,3) * turns); 

                // kRange = 

                // controller.helicalCurve(obj, numCurve, numPipe, pitch, turns, springD, coilD);
            }
        }
    }
}

