using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EthiopicCalendar
{
    public partial class Form1 : Form
    {
        private EthiopicCalendar.Calendar ec = new Calendar();

        public Form1()
        {
            InitializeComponent();
            label1.Font = new Font("Nyala",14);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var selected = this.dateTimePicker1.Value;

            label1.Text = new EthiopicDateTime(dateTimePicker1.Value).ToString();

            textBox1.Text = ec.GregorianToEthiopic(selected.Date);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dateTimePicker2.Value = ec.EthiopicToGregorian(textBox2.Text);
        }
    }
}
