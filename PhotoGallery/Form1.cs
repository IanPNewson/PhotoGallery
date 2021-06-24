using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tulpep.NotificationWindow;

namespace PhotoGallery
{
    public partial class Form1 : Form
    {

        private DirectoryInfo _Dir = new DirectoryInfo(@"C:\Users\Ian\Documents\Projects\Output\StfcBot\StfcScreencopyOverlay\");

        private IEnumerable<FileInfo> ImageFiles
        {
            get
            {
                return _Dir.GetFiles("*.png").OrderBy(f => f.Name);
            }
        }

        private FileInfo _CurrentFile = null;
        private FileInfo CurrentFile
        {
            get
            {
                return _CurrentFile ?? ImageFiles.First();
            }
            set
            {
                _CurrentFile = value;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.SetImage(CurrentFile);
            this.KeyUp += Form1_KeyUp;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            var currentFile = CurrentFile;
            var nextFile = ImageFiles.SkipWhile(f => f.Name != currentFile.Name).Skip(1).FirstOrDefault();
            nextFile = nextFile ?? ImageFiles.First();
            var previousFile = ImageFiles.OrderByDescending(F => F.Name)
                .SkipWhile(f => f.Name != currentFile.Name).Skip(1).FirstOrDefault();
            previousFile = previousFile ?? ImageFiles.Last();
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    this.SetImage(nextFile);
                    CurrentFile = nextFile;
                    Toast($"Deleted {currentFile.Name}");
                    break;
                case Keys.Right:
                    this.SetImage(nextFile);
                    CurrentFile = nextFile;
                    break;
                case Keys.Left:
                    this.SetImage(previousFile);
                    CurrentFile = previousFile;
                    break;
            }
        }

        private void SetImage(FileInfo file)
        {
            this.pictureBox1.Image = Image.FromFile(file.FullName);
            this.Text = file.Name;
        }

        private void Toast(string msg)
        {
            var popupNotifier = new PopupNotifier();
            popupNotifier.TitleText = msg;
            popupNotifier.Popup();
        }

    }
}
