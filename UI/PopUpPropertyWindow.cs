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
    public partial class PropertyWindow : MetroFramework.Forms.MetroForm
    {
        public PropertyWindow()
        {
            InitializeComponent();
            trackBar1.Minimum = 1;
            trackBar1.Maximum = 30;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lb_coilNum.Text = trackBar1.Value.ToString();
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
            
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            lb_layerNum.Text = trackBar2.Value.ToString();
        }
    }
}
