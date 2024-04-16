using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace simulation.PL
{
    public partial class parameterInputForm : Form
    {
        public int NumDelaysRequired { get; private set; }
        public int QueueSize { get; private set; }

        public parameterInputForm()
        {
            InitializeComponent();
        }


        private void okBtn_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(numDelaysTextBox.Text, out int numDelays) || numDelays <= 0)
            {
                MessageBox.Show("Invalid input for number of delays required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(queueSizeTextBox.Text, out int queueSize) || queueSize <= 0)
            {
                MessageBox.Show("Invalid input for queue size.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            NumDelaysRequired = numDelays;
            QueueSize = queueSize;

            DialogResult = DialogResult.OK;
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
