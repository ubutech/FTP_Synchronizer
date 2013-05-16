using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTP_Synchronizer
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            textBox1.Text = Properties.Settings.Default.Host;
            textBox2.Text = Properties.Settings.Default.Username;
            textBox3.Text = Properties.Settings.Default.Password;
            textBox4.Text = Properties.Settings.Default.port;
            textBox5.Text = Properties.Settings.Default.Path;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.FTP;
            label6.Text = ""; 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string host = textBox1.Text;
            if (host.IndexOf("://") < 0)
                host = "ftp://" + host;
            Properties.Settings.Default.Host = host;
            Properties.Settings.Default.Username = textBox2.Text;
            Properties.Settings.Default.Password = textBox3.Text;
            Properties.Settings.Default.port = textBox4.Text;
            Properties.Settings.Default.Path = textBox5.Text;
            Properties.Settings.Default.Changed = "Yes";
            Properties.Settings.Default.Save();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = this.folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string foldername = folderBrowserDialog1.SelectedPath;
                textBox5.Text = foldername;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
