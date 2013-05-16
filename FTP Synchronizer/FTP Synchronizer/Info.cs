using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace FTP_Synchronizer
{
    public partial class Info : Form
    {
        public delegate void RepopulatedHandler(object sender, EventArgs e);
        [Description("Occurs after the contents of the control have been updated.")]
        public event RepopulatedHandler Repopulated;

        private TaskbarManager windowsTaskbar = TaskbarManager.Instance;
        private JumpList jumpList;
        FtpClient _ftpClient;

        public FtpClient FtpClient
        {
            get { return _ftpClient; }
        }

        public bool AllowDirectoryNavigation { get; set; }

        public string Host
        {
            get { return _ftpClient.Host; }
            set { _ftpClient.Host = value; }
        }

        public string Username
        {
            get { return _ftpClient.Username; }
            set { _ftpClient.Username = value; }
        }

        public string Password
        {
            get { return _ftpClient.Password; }
            set { _ftpClient.Password = value; }
        }

        public Info()
        {
            InitializeComponent();
            _ftpClient = new FtpClient();
            listView1.ListViewItemSorter = new ListViewItemComparer();
            AllowDirectoryNavigation = true;
        }

        public static int GetTaskbarHeight()
        {
            return Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;

            if (m.Msg == WM_NCHITTEST)
                return;
            base.WndProc(ref m);
        }

        private void Info_Load(object sender, EventArgs e)
        {
            pictureBox1.Visible = AllowDirectoryNavigation && !_ftpClient.IsRootDirectory;
            this.DesktopLocation = new Point(Screen.PrimaryScreen.WorkingArea.Width - 420, Screen.PrimaryScreen.WorkingArea.Height - 580);
            this.Icon = Properties.Resources.FTP;
            this.Focus();

            if (Properties.Settings.Default.Changed == "No")
            {
                Form1 form1 = new Form1();
                form1.ShowDialog();
                Populate();
            }
            else
            {
                Host = Properties.Settings.Default.Host;
                Username = Properties.Settings.Default.Username;
                Password = Properties.Settings.Default.Password;
                Populate();
            }
        }

        #region Files and Folders

        public void Populate()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                listView1.Items.Clear();
                List<FtpDirectoryEntry> entries = _ftpClient.ListDirectory();
                foreach (FtpDirectoryEntry entry in entries)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = entry.Name;
                    item.Tag = entry;
                    item.SubItems.Add((entry.IsDirectory) ? "<DIR>" : FileSizeToString(entry.Size));
                    item.SubItems.Add(entry.CreateTime.ToString());
                    item.ImageIndex = entry.IsDirectory ? 0 : 1;
                    listView1.Items.Add(item);
                    pictureBox1.Visible = AllowDirectoryNavigation && !_ftpClient.IsRootDirectory;
                }
                if (Repopulated != null)
                    Repopulated(this, new EventArgs());
            }
            catch (Exception ex)
            {
                ShowError("Error listing FTP directory", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        
        public void UploadFiles(params string[] files)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _ftpClient.UploadFiles(files);
            }
            catch (Exception ex)
            {
                ShowError("Error uploading files", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            Populate();
        }

        public void DeleteFiles(params string[] files)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _ftpClient.DeleteFiles(files);
            }
            catch (Exception ex)
            {
                ShowError("Error deleting files", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            Populate();
        }

        public void CreateDirectory(string directory)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _ftpClient.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                ShowError("Error creating directory", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            Populate();
        }

        public void DeleteDirectory(string directory)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _ftpClient.DeleteDirectory(directory);
            }
            catch (Exception ex)
            {
                ShowError("Error deleting directory", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            Populate();
        }

        public void SetDirectory(string directory)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _ftpClient.ChangeDirectory(directory);
            }
            catch (Exception ex)
            {
                ShowError("Error changing the current directory", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            Populate();
        }

        public bool DirectoryExists(string directory)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                return _ftpClient.DirectoryExists(directory);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region Menu Commands

        private void parentDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDirectory("..");
        }

        private void uploadFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Upload Files";
            openFileDialog1.FileName = String.Empty;
            openFileDialog1.Filter = "All Files|*.*|Image Files|*.jpg;*.jpeg;*.gif;*.png|Zip Files|*.zip";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                UploadFiles(openFileDialog1.FileNames);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection items = listView1.SelectedItems;
            if (items.Count > 0)
            {
                string description;
                if (items.Count > 1)
                    description = String.Format("{0} items", items.Count);
                else
                    description = String.Format("'{0}'", ((FtpDirectoryEntry)items[0].Tag).Name);

                if (MessageBox.Show(String.Format("Are you sure you want to permanently delete {0}?", description),
                    "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (ListViewItem item in items)
                    {
                        FtpDirectoryEntry entry = (FtpDirectoryEntry)item.Tag;
                        if (entry.IsDirectory)
                        {
                            DeleteDirectory(entry.Name);
                        }
                        else
                        {
                            DeleteFiles(entry.Name);
                        }
                    }
                }
            }
        }

        private void createDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directory = String.Empty;
            if (InputBox.Show("Create Directory", "&New directory name:", ref directory) == DialogResult.OK)
                CreateDirectory(directory);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Populate();
        }

        #endregion

        #region Event Handlers

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                ChooseItem();
            else if (e.KeyCode == Keys.Back)
            {
                if (AllowDirectoryNavigation && !_ftpClient.IsRootDirectory)
                    SetDirectory("..");
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ChooseItem();
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                bool filesSelected = (listView1.SelectedItems.Count > 0);
                deleteToolStripMenuItem.Enabled = filesSelected;
                parentDirectoryToolStripMenuItem.Enabled = AllowDirectoryNavigation && !_ftpClient.IsRootDirectory;
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] directoryName = (string[])e.Data.GetData(DataFormats.FileDrop);
                string[] files = Directory.GetFiles(directoryName[0]);
                UploadFiles(files);
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = Properties.Resources.arrowReverse_40x40;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = Properties.Resources.arrowReverse;
            pictureBox1.Visible = AllowDirectoryNavigation && !_ftpClient.IsRootDirectory;
            SetDirectory("..");
        }

        #endregion

        #region Helper Methods

        protected void ChooseItem()
        {
            if (AllowDirectoryNavigation)
            {
                ListView.SelectedListViewItemCollection items = listView1.SelectedItems;
                if (items.Count > 0)
                {
                    FtpDirectoryEntry entry = (FtpDirectoryEntry)items[0].Tag;
                    if (entry.IsDirectory)
                        SetDirectory(entry.Name);
                }
            }
        }

        protected List<string> GetSelectedFiles()
        {
            List<string> files = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                FtpDirectoryEntry entry = (FtpDirectoryEntry)item.Tag;
                if (!entry.IsDirectory)
                    files.Add(entry.Name);
            }
            return files;
        }

        protected static string[] _suffix = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB" };
        protected static double _fileSizeDivisor = 1024.0;

        protected string FileSizeToString(long sizeInBytes)
        {
            int i = 0;
            double size = (double)sizeInBytes;

            while (size >= _fileSizeDivisor)
            {
                i++;
                size /= _fileSizeDivisor;
            }
            return String.Format("{0:#,##0.##} {1}", size, _suffix[i]);
        }

        protected void ShowError(string msg, Exception ex)
        {
            MessageBox.Show(String.Format("{0} : {1}", msg, ex.Message),
                "FTP Client Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        #endregion

    }

    #region Classes

    class ListViewItemComparer : IComparer
    {
        public int Compare(object obj1, object obj2)
        {
            FtpDirectoryEntry entry1 = ((ListViewItem)obj1).Tag as FtpDirectoryEntry;
            FtpDirectoryEntry entry2 = ((ListViewItem)obj2).Tag as FtpDirectoryEntry;

            if (entry1 == null || entry2 == null)
            {
                return 0;
            }
            else if (entry1.IsDirectory != entry2.IsDirectory)
            {
                return (entry1.IsDirectory) ? -1 : 1;
            }
            else
            {
                return String.Compare(entry1.Name, entry2.Name);
            }
        }
    }

    class InputBox
    {
        public static DialogResult Show(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }

    #endregion
}
