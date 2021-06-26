using INHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        public Form1(DirectoryInfo dir)
        {
            _Dir = dir;
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
                case Keys.D:
                    this.SetImage(nextFile);
                    CurrentFile = nextFile;

                    Task.Run(() =>
                    {
                        //Retry delete in case it's locked
                        var i = 0;
                        while (i < 10 && currentFile.Exists)
                        {
                            try
                            {
                                currentFile.Delete();
                                currentFile.Refresh();
                                Invoke(new Action(() =>
                                {
                                    if(!currentFile.Exists)
                                        Toast($"Deleted {currentFile.Name}");
                                }));
                            }
                            catch (Exception ex)
                            {
                                Invoke(new Action(() =>
                                {
                                    Toast($" Couldn't delete {currentFile.Name}", $"Attempts: {i}: {ex.Message}");
                                }));
                                Thread.Sleep(10_000);
                            }
                            ++i;
                        }
                    });
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
            Image imgClone;
            using (var img = Image.FromFile(file.FullName))
            {
                imgClone = new Bitmap(img);
            }
            this.pictureBox1.Image = imgClone;
            var count = ImageFiles.Count();
            var index = ImageFiles.TakeWhile(f => f.Name.CompareTo(file.Name) <= 0).Count();
            this.Text = file.Name + $" ({index}/{count}) ({_Dir.Name})";
        }

        private void Toast(string msg, string body = null)
        {
            var popupNotifier = new PopupNotifier();
            popupNotifier.TitleText = msg;
            popupNotifier.ContentText = body;
            popupNotifier.Popup();
        }

    }
}
