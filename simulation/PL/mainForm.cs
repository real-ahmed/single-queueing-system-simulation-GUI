using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using simulation.BL;
using simulation.PL;

namespace simulation
{
    public partial class mainForm : Form
    {
        QueueSimulation queue; // Capitalize the class name

        public mainForm()
        {
            InitializeComponent();
        }
        private void runBtn_Click(object sender, EventArgs e)
        {
            parameterInputForm inputForm = new parameterInputForm();
            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                queue = new QueueSimulation(inputForm.NumDelaysRequired, inputForm.QueueSize, this);
                queue.Start();

                this.runBtn.Hide();
                this.stopBtn.Show();
            }
        }

        private void stopBTN_Click(object sender, EventArgs e)
        {
            this.stopBtn.Hide();
            this.runBtn.Show();
            queue.Stop(); 
        }
    }
}
