using GhostscriptSharp;
using GhostscriptSharp.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ZXing;

namespace DecodeImages
{
    public partial class DecodeImages : Form
    {
        public const string REGEX_MP_URL = @"https\:\/\/\S*\/pos\/(\d{@L})";
        public const string REGEX_MP_LENGTH = @"com\.mercadolibre01(\d*)(http\S?\:\/\/\S*\/)";
        public const int MP_OFFSET = 40;

        public DecodeImages()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                textBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (FileInfo directoryInfoFile in GetFiles())
            {
                if (directoryInfoFile.Extension.Contains("pdf"))
                {
                    string pdf = directoryInfoFile.FullName;
                    GhostscriptSettings settings = new GhostscriptSettings
                    {
                        Resolution = new Size(100, 100),
                        Device = GhostscriptSharp.Settings.GhostscriptDevices.bmpgray,
                        Size = new GhostscriptPageSize() { Manual = new Size(100, 100) },
                    };
                    GhostscriptSharp.GhostscriptWrapper.GenerateOutput(pdf, directoryInfoFile.FullName.Replace("pdf", "jpg"), settings);
                    directoryInfoFile.Delete();
                }
            }

            foreach (FileInfo directoryInfoFile in GetFiles())
            {
                // create a barcode reader instance
                IBarcodeReader reader = new BarcodeReader();
                // load a bitmap
                var barcodeBitmap = (Bitmap)Image.FromFile(directoryInfoFile.FullName);
                // detect and decode the barcode inside the bitmap
                var result = reader.Decode(barcodeBitmap);
                // do something with the result
                if (result != null)
                {
                    richTextBox.AppendText(directoryInfoFile.Directory.Name + "\t" + directoryInfoFile.Name + "\t" + GetQrInfo(result.Text) + "\t" + (result.Text) + "\r\n");
                }
            }
        }

        private IEnumerable<FileInfo> GetFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(textBox.Text);
            List<FileInfo> directoryInfoFiles = new List<FileInfo>();
            directoryInfoFiles.AddRange(directoryInfo.EnumerateFiles().ToList());

            foreach (var directoryInfoFolder in directoryInfo.EnumerateDirectories())
            {
                directoryInfoFiles.AddRange(directoryInfoFolder.EnumerateFiles().ToList());
            }
            return directoryInfoFiles;
        }

        private string GetQrInfo(string qrData)
        {
            try
            {
                Match match = Regex.Match(qrData, REGEX_MP_LENGTH);

                if (match.Success)
                {
                    int urlLength = Convert.ToInt32(match.Groups[1].Value);
                    int subUrlLength = urlLength - match.Groups[2].Value.Length;
                    match = Regex.Match(qrData, REGEX_MP_URL.Replace("@L", subUrlLength.ToString()));

                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }

                    return qrData.Substring(MP_OFFSET, urlLength);
                }

                return string.Empty;

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
