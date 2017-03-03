using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PluginBar.UI //UI
{
    public partial class PropertyWindow : Form
    {
        private Boolean alongCurve = false;

        public PropertyWindow()
        {
            InitializeComponent();
        }

        private void PropertyWindow_Load(object sender, EventArgs e)
        {
            // Do nothing?
        }

        public void setCoilMaxiumHeight(double height)
        {
            double turnsNum = (height-5) / 0.3;
            trackBar1.Maximum = (int)turnsNum;
        }

        public int getCoilPerLayerData()
        {
            return trackBar1.Value;
        }

        public int getLayerData()
        {
            return trackBar2.Value;
        }

        //public void getCheckBoxValue(out bool dist, out bool clockw)
        //{
            
            //clockw = cb_clockwise.Checked;
        //}

        private void btn_OK_Click(object sender, EventArgs e)
        {
            double force;
            double springD;
            int turns;
            double shearMod;
            double coilD;

            // Create strings to hold input data
            String scriptString = "";
            String start = "0,0,0";
            String end = "0,"+lb_length.Text+",0";
            String startPT = "-5,5,0";

            // Print input data to the command line
            Rhino.RhinoApp.WriteLine("Coil Diameter: " + lb_coilDiameter.Text + " mm");
            Rhino.RhinoApp.WriteLine("Pitch: " + lb_pitchValue.Text + " mm");
            Rhino.RhinoApp.WriteLine("Length: " + lb_length.Text + " cm");
            Rhino.RhinoApp.WriteLine("Spring Diamter: " + lb_springDiameter.Text + " mm");

            // Helix Command
            if (alongCurve) // If user decides to create helix on existing curve
            {
                scriptString = String.Format("Helix A Pause T {0} {1} {2}", lb_pitchValue.Text, lb_springDiameter.Text, startPT);
                //scriptString = String.Format("Helix A Pause P {0} {1} {2}", lb_pitchValue.Text, lb_springDiameter.Text, startPT);
                Rhino.RhinoApp.SetFocusToMainWindow(); // Change the focus so the user can select a curve
                Rhino.RhinoApp.RunScript(scriptString, false); // Send command to command line
            }
            else // If user decides to create helix without existing curve
            {
                scriptString = String.Format("Helix {0} {1} T {2} {3} {4}", start, end, lb_pitchValue.Text, lb_springDiameter.Text, startPT);
                Rhino.RhinoApp.RunScript(scriptString, false); // Send command to command line
            }

            // Pipe Command
            String pipeString = String.Format("SelLast Pipe {0} {1} {2}", lb_coilDiameter.Text, lb_coilDiameter.Text, "Enter");
            Rhino.RhinoApp.RunScript(pipeString, false); // Send command to command line
        }




        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lb_coilDiameter.Text = trackBar1.Value.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            lb_pitchValue.Text = trackBar2.Value.ToString();
        }
        
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            lb_length.Text = trackBar3.Value.ToString();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            lb_springDiameter.Text = trackBar4.Value.ToString();
        }


        // Define what to do when checkboxes are changed
        private void cb_clockwise_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void cb_alongCurve_CheckedChanged(object sender, EventArgs e)
        {
            alongCurve = !alongCurve;
            trackBar3.Enabled = !trackBar3.Enabled;
        }

    }
}
