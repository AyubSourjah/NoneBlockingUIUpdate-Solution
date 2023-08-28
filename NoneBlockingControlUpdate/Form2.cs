using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoneBlockingControlUpdate
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            DoWork();
        }

        private void DoWork()
        {
            for (int i = 1; i < 1000000; i++)
            {
                progressBar1.Value = i;
            }
        }
    }
}
