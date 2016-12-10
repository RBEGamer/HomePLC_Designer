using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartPLC_NodeCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ios zählen
            //complette strings bauen
            //csv bauen
            //speichern
            int anzahl_ios = 5;
            string param_string = textBox3.Text + "%" + anzahl_ios.ToString() + "%" + richTextBox2.Text + "%" + richTextBox3.Text + "%";

            string s_string = "FALSE";
            if (STATIC.Checked)
            {
                s_string = "TRUE";
            }
            string csv_string = "node_" + textBox1.Text + ";" + textBox1.Text + ";" + textBox5.Text + ";" + textBox2.Text + ";" + s_string + ";";

        }


        //ADD INPUT
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "") { return; }
            string tmp = "[" + numericUpDown1.Value.ToString() + "," + comboBox1.SelectedItem.ToString() + "," + textBox4.Text + "],";
            richTextBox2.Text += tmp;
        }




        //ADD OUTPUT
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "") { return; }
            string tmp = "[" + numericUpDown1.Value.ToString() + "," + comboBox1.SelectedItem.ToString() + "," + textBox4.Text + "],";
            richTextBox3.Text += tmp;
        }
    }
}
