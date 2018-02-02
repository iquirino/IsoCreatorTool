using Caliburn.Micro;
using DiscUtils.Iso9660;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoCreatorTool.ViewModels
{
    public class ShellViewModel : Screen
    {
        private string _folder;
        public string Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                _folder = value;

                if (string.IsNullOrWhiteSpace(this.DiscName))
                {
                    this.DiscName = (new System.IO.DirectoryInfo(_folder)).Name;
                }

                this.NotifyOfPropertyChange(nameof(this.Folder));
                this.NotifyOfPropertyChange(nameof(this.CanProcess));
            }
        }

        private string _discName;
        public string DiscName
        {
            get
            {
                return _discName;
            }
            set
            {
                _discName = value.CleanString();
                this.NotifyOfPropertyChange(nameof(this.DiscName));
            }
        }

        public bool _working;
        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value;

                this.NotifyOfPropertyChange(nameof(this.Working));
                this.NotifyOfPropertyChange(nameof(this.CanChoose));
                this.NotifyOfPropertyChange(nameof(this.CanProcess));
            }
        }
        public bool CanChoose => !Working;
        public bool CanProcess => !string.IsNullOrWhiteSpace(Folder) && System.IO.Directory.Exists(Folder) && !Working;

        public ShellViewModel()
        {
            this.DisplayName = "Iso Creator Tool";
        }

        public void Choose()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                this.Folder = dialog.SelectedPath;
        }

        public void Process()
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();

            if (!string.IsNullOrWhiteSpace(this.DiscName))
                dialog.FileName = this.DiscName.CleanString(false) + ".iso";

            dialog.DefaultExt = ".iso";
            dialog.Filter = "Iso image file|*.iso";

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string filename = dialog.FileName;

                Working = true;
                Task run = Task.Factory.StartNew(() => Build(filename));
            }
        }

        private void Build(string filename)
        {
            if (System.IO.File.Exists(filename))
                System.IO.File.Delete(filename);

            System.Windows.Forms.DialogResult criticalError = System.Windows.Forms.DialogResult.None;

            do
            {
                try
                {
                    this.DisplayName = "Iso Creator Tool: Writing iso...";

                    CDBuilder builder = new CDBuilder();
                    builder.UseJoliet = true;

                    if (!string.IsNullOrWhiteSpace(this.DiscName))
                        builder.VolumeIdentifier = this.DiscName;

                    string[] directories = System.IO.Directory.GetDirectories(this.Folder, "*", System.IO.SearchOption.AllDirectories);

                    foreach (string dir in directories)
                        builder.AddDirectory(dir.Substring(this.Folder.Length + 1));

                    string[] files = System.IO.Directory.GetFiles(this.Folder, "*", System.IO.SearchOption.AllDirectories);

                    int ix = 1;
                    int fCount = files.Length;

                    foreach (string file in files)
                    {
                        System.Windows.Forms.DialogResult fileError = System.Windows.Forms.DialogResult.None;

                        this.DisplayName = string.Format("[{0}%] Iso Creator Tool: Writing iso...", Math.Round((100.0 / fCount) * ix, 0).ToString());

                        do
                        {
                            try
                            {
                                builder.AddFile(file.Substring(this.Folder.Length + 1), System.IO.File.OpenRead(file));
                            }
                            catch (Exception exFile)
                            {
                                fileError = System.Windows.Forms.MessageBox.Show(exFile.Message, "Error", System.Windows.Forms.MessageBoxButtons.AbortRetryIgnore, System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        } while (fileError == System.Windows.Forms.DialogResult.Retry);

                        if (fileError == System.Windows.Forms.DialogResult.Abort)
                            goto Abort;

                        ix++;
                    }

                    this.DisplayName = "Iso Creator Tool: Finishing iso...";

                    builder.Build(filename);

                    System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + filename + "\"");
                }
                catch (Exception ex)
                {
                    criticalError = System.Windows.Forms.MessageBox.Show(ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Error);

                    if (System.IO.File.Exists(filename))
                        System.IO.File.Delete(filename);
                }
            } while (criticalError == System.Windows.Forms.DialogResult.Retry);

            Abort:

            this.DisplayName = "Iso Creator Tool: Finish...";
            this.Working = false;
        }
    }
}
