using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Management;

namespace WinFormsTimer
{


    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();

        }

        public void button_Click(object sender, EventArgs e)
        {
            Timers timers = new Timers(timeTextBox, logTextBox, changeTextBox, button);
            timers.Start();
            return;
        }
    }
}


