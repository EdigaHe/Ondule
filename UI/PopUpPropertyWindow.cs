using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PluginBar.UI
{
    public partial class PropertyWindow : Form
    {
        public PropertyWindow()
        {
            InitializeComponent();
            //trackBar1.Minimum = 1;
            //trackBar1.Maximum = 30;
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

        public void getCheckBoxValue(out bool dist, out bool clockw)
        {
            dist = cb_vertical.Checked;
            clockw = cb_clockwise.Checked;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            // Helix Options
            String opt1 = "";
            String start = "0,0,0";
            String end = "0,"+lb_length.Text+",0";
            String diameter = lb_springDiameter.Text;
            String startPT = "-5,5,0";

            Rhino.RhinoApp.WriteLine("Coil Diameter: " + lb_coilDiameter.Text + " mm");
            Rhino.RhinoApp.WriteLine("Pitch: " + lb_pitchValue.Text + " mm");
            Rhino.RhinoApp.WriteLine("Length: " + lb_length.Text + " cm");
            Rhino.RhinoApp.WriteLine("Spring Diamter: " + lb_springDiameter.Text + " mm");

            if (cb_vertical.Enabled)
            {
                opt1 = "Vertical";
            }

            String scriptString = String.Format("Helix {0} {1} P {2} {3} {4}", start, end, lb_pitchValue.Text, diameter, startPT);

            //Rhino.RhinoApp.RunScript("_-Helix", false);
            //Rhino.RhinoApp.RunScript(start, false);
            //Rhino.RhinoApp.RunScript(end, false);
            Rhino.RhinoApp.RunScript(scriptString, false);

            // Pipe Options
            String radius = lb_coilDiameter.Text;

            String pipeString = String.Format("SelLast Pipe {0} {1} {2}", radius, radius, "Enter");

            Rhino.RhinoApp.RunScript(pipeString, false);
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




        private void cb_vertical_CheckedChanged(object sender, EventArgs e)
        {
            cb_vertical.Enabled = !cb_vertical.Enabled;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            cb_vertical.Enabled = !cb_vertical.Enabled;
        }

        private void cb_clockwise_CheckedChanged(object sender, EventArgs e)
        {
            cb_clockwise.Enabled = !cb_clockwise.Enabled;
        }
    }
}
