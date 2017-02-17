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
            trackBar1.Minimum = 1;
            trackBar1.Maximum = 30;
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
            dist = cb_distribution.Checked;
            clockw = cb_clockwise.Checked;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            Rhino.RhinoApp.WriteLine("Coil Diameter: " + lb_coilDiameter.Text + " mm");
            Rhino.RhinoApp.WriteLine("Pitch: " + lb_pitchValue.Text + " mm");
            Rhino.RhinoApp.WriteLine("Length: " + lb_length.Text + " cm");
            Rhino.RhinoApp.WriteLine("Spring Diamter: " + lb_springDiameter.Text + " mm");
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
    }
}
