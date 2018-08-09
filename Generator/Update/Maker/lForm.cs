using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Cyclic.Redundancy.Check;

namespace Update.Maker
{
    public partial class lForm : Form
    {
        public lForm()
        {
            this.InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(".\\\\update");
            this.StartBrowsing();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            this.SaveList();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            this.RemoveFromPath(this.filePath.Text);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Files = this.GetFiles(e.Argument);
            for (int i = 0; i < this.Files.Length; i++)
            {
                this.backgroundWorker.ReportProgress(i + 1, this.GetFileData(this.Files[i]));
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.UpdateResult(e.UserState);
            this.UpdateProgressBar(this.ComputeProgress(e.ProgressPercentage));
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.EnableButtons();
        }

        private void DisableButtons()
        {
            this.Progress.Value = 0;
            this.Result.Clear();
            this.saveButton.Enabled = false;
            this.removeButton.Enabled = false;
            this.browseButton.Enabled = false;
        }

        private void EnableButtons()
        {
            this.saveButton.Enabled = true;
            this.removeButton.Enabled = true;
            this.browseButton.Enabled = true;
        }

        public string[] GetFiles(object Path)
        {
            return Directory.GetFiles(Path.ToString(), "*.*", SearchOption.AllDirectories);
        }

        public int GetFilesCount(string[] Files)
        {
            return Files.Length;
        }

        public string GetFileData(string File)
        {
            FileInfo fileInfo = new FileInfo(File);
            return string.Concat(new object[]
            {
                File,
                ".rar;",
                this.GetHash(File),
                ";",
                fileInfo.Length
            });
        }

        private string GetHash(string Name)
        {
            if (Name == string.Empty)
            {
                return null;
            }
            CRC crc = new CRC();
            string text = string.Empty;
            try
            {
                using (FileStream fileStream = File.Open(Name, FileMode.Open))
                {
                    foreach (byte b in crc.ComputeHash(fileStream))
                    {
                        text += b.ToString("x2").ToUpper();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Can't open: " + Name);
            }
            return text;
        }

        private void UpdateResult(object Data)
        {
            if (!this.Result.IsDisposed)
            {
                string text = Data.ToString().Replace("\\", "/").Split(new char[]
                {
                    ';'
                })[0].Replace(this.filePath.Text, string.Empty);
                if (text.Contains("/"))
                {
                    Directory.CreateDirectory("./update/" + Path.GetDirectoryName(text));
                }
                File.Copy(Data.ToString().Replace("\\", "/").Split(new char[]
                {
                    ';'
                })[0].Replace(".rar", string.Empty), Directory.GetCurrentDirectory() + "/update/" + text, true);
                this.Result.AppendText(Data.ToString().Replace("\\", "/").Replace(this.filePath.Text, string.Empty) + Environment.NewLine);
            }
        }

        private int ComputeProgress(int Percent)
        {
            return 100 * Percent / this.Files.Length;
        }

        private void UpdateProgressBar(int Percent)
        {
            if (Percent < 0 || Percent > 100)
            {
                return;
            }
            if (!this.Progress.IsDisposed)
            {
                this.Progress.Value = Percent;
            }
        }

        private void RemoveFromPath(string Remove)
        {
            if (Remove == string.Empty)
            {
                return;
            }
            this.Result.Text = this.Result.Text.Replace(Remove, string.Empty);
            this.filePath.Text = this.filePath.Text.Replace(Remove, string.Empty);
        }

        private void StartBrowsing()
        {
            if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.DisableButtons();
                this.filePath.Text = this.folderBrowserDialog.SelectedPath.Replace("\\", "/") + "/";
                if (!this.backgroundWorker.IsBusy)
                {
                    this.backgroundWorker.RunWorkerAsync(this.folderBrowserDialog.SelectedPath);
                }
            }
        }

        private void SaveList()
        {
            this.saveFileDialog.FileName = "update.txt";
            this.saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\update\\";
            this.saveFileDialog.RestoreDirectory = true;
            this.saveFileDialog.Filter = "Text files (*.txt)|*.txt|Every file (*.*)|*.*";
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter streamWriter = new StreamWriter(this.saveFileDialog.FileName))
                {
                    streamWriter.Write(this.Result.Text);
                }
            }
        }

        private void Progress_Click(object sender, EventArgs e)
        {
        }

        private void filePath_TextChanged(object sender, EventArgs e)
        {
        }

        private string[] Files;
    }
}
