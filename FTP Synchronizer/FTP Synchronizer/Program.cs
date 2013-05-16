using System;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Timers;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTP_Synchronizer
{
    public class Program
    {
        public static NotifyIcon notifyicon1 = new NotifyIcon();
        public static System.Timers.Timer aTimer = new System.Timers.Timer();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 10000;

            ContextMenu contextmenu1 = new ContextMenu();
            MenuItem menuitem1 = new MenuItem();

            contextmenu1.MenuItems.AddRange(new MenuItem[] { menuitem1 });
            menuitem1.Index = 0;
            menuitem1.Text = "E&xit";
            menuitem1.Click += new EventHandler(menuItem1_Click);

            MenuItem menuitem2 = new MenuItem();
            contextmenu1.MenuItems.AddRange(new MenuItem[] { menuitem2 });
            menuitem2.Index = menuitem2.Index - 1;
            menuitem2.Text = "P&references Server";
            menuitem2.Click += new EventHandler(menuItem2_Click);

            MenuItem menuitem3 = new MenuItem();
            contextmenu1.MenuItems.AddRange(new MenuItem[] { menuitem3 });
            menuitem3.Index = menuitem3.Index - 2;
            menuitem3.Text = "H&ome";
            menuitem3.Click += new EventHandler(menuItem3_Click);

            notifyicon1.Icon = Properties.Resources.FTP;
            notifyicon1.Text = "FTP Synchronizer";
            notifyicon1.ContextMenu = contextmenu1;
            notifyicon1.Visible = true;

            Form1 form1 = new Form1();
            Info form2 = new Info();
            form2.Show();
            

            Application.Run();
            notifyicon1.Visible = false;
        }

        public static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            
        }

        private static void menuItem1_Click(object Sender, EventArgs e)
        {
            Application.Exit();
        }

        private static void menuItem2_Click(object Sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
        }

        private static void menuItem3_Click(object Sender, EventArgs e)
        {
            Info form2 = new Info();

            if (form2.Visible == false)
                form2.Focus();
        }

        public void MakeConnection()
        {
            notifyicon1.Text = "FTP Synchronizer" + Environment.NewLine + "Establishing connection...";
        }
    }
}
