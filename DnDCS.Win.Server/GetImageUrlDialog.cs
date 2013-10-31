using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DnDCS.Libs;

namespace DnDCS.Win.Server
{
    public partial class GetImageUrlDialog : Form
    {
        public string LoadedImageUrl { get; private set; }

        public Image LoadedImage
        {
            get { return (Image)pbxPreview.Image.Clone(); }
        }

        public GetImageUrlDialog()
        {
            InitializeComponent();
        }

        private void GetImageUrlDialog_Load(object sender, EventArgs e)
        {
            var serverData = Persistence.LoadServerData();
            if (serverData != null && serverData.ServerImageUrlHistory != null)
            {
                foreach (var imageUrl in serverData.ServerImageUrlHistory)
                    lboHistory.Items.Add(imageUrl);
                if (lboHistory.Items.Count > 0)
                    lboHistory.SelectedIndex = 0;
            }

            Closed += new EventHandler(GetImageUrlDialog_Closed);
        }

        private void GetImageUrlDialog_Closed(object sender, EventArgs e)
        {
            var serverData = Persistence.LoadServerData();
            // Put the recently chosen one at the top. If none was chosen, then they will all be ordered by 'false' and be in the same order they were.
            serverData.ServerImageUrlHistory = lboHistory.Items.OfType<string>().OrderBy(i => i == LoadedImageUrl).ToArray();
            Persistence.SaveServerData(serverData);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Launch a popup to get a png
            using (var openFile = new OpenFileDialog())
            {
                openFile.Title = "Select a .JPG or .PNG image.";
                openFile.CheckFileExists = true;
                openFile.CheckPathExists = true;
                openFile.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";

                var result = openFile.ShowDialog(this);
                if (result == DialogResult.OK)
                    tboUrl.Text = openFile.FileName;
            }
        }

        private void lboHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lboHistory.SelectedIndex >= 0)
            {
                tboUrl.TextChanged -= new EventHandler(tboUrl_TextChanged);
                tboUrl.Text = (string)lboHistory.SelectedItem;
                TrySetImage(tboUrl.Text);
                tboUrl.TextChanged += new EventHandler(tboUrl_TextChanged);

                btnDelete.Enabled = true;
            }
            else
            {
                btnDelete.Enabled = false;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lboHistory.SelectedIndex < 0)
                return;
            lboHistory.Items.RemoveAt(lboHistory.SelectedIndex);
        }

        private void tboUrl_TextChanged(object sender, EventArgs e)
        {
            lboHistory.SelectedItem = tboUrl.Text;
            TrySetImage(tboUrl.Text);
        }

        private void TrySetImage(string url)
        {
            if (Regex.IsMatch(tboUrl.Text, @".*\.((png)|(jpg)|(jpeg)|(bmp))"))
            {
                pbxPreview.ImageLocation = LoadedImageUrl = tboUrl.Text;
                btnOK.Enabled = true;
            }
            else
            {
                pbxPreview.ImageLocation = LoadedImageUrl = null;
                btnOK.Enabled = false;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!lboHistory.Items.Contains(LoadedImageUrl))
                lboHistory.Items.Insert(0, LoadedImageUrl);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
