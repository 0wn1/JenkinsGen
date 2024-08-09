using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JenkinsGen
{
    public partial class Form1 : Form
    {
        private Dictionary<int, string> StoredHashs;
        private bool isUpdating = false;
        private readonly string[] iniUrls = new string[]
        {
            "https://raw.githubusercontent.com/DurtyFree/gta-v-data-dumps/master/ObjectList.ini",
            "https://raw.githubusercontent.com/DurtyFree/gta-v-data-dumps/master/VehicleList.ini",
            "https://raw.githubusercontent.com/DurtyFree/gta-v-data-dumps/master/WeaponList.ini",
            "https://raw.githubusercontent.com/DurtyFree/gta-v-data-dumps/master/PedList.ini"
        };
        private const string hashListPath = "hashList.ini";

        public Form1()
        {
            InitializeComponent();
            SetupEventHandlers();
            CheckHashListFile();
            InitializeRichTextBoxes();
            DoubleClickSelect(richTextBox1);
            DoubleClickSelect(richTextBox2);
            DoubleClickSelect(richTextBox3);
            DoubleClickSelect(richTextBox4);

            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox2.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox3.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox4.SelectionAlignment = HorizontalAlignment.Center;
        }
        private void DoubleClickSelect(RichTextBox richTextBox)
        {
            richTextBox.MouseDoubleClick += (sender, e) =>
            {
                richTextBox.SelectAll();
            };
        }

        private void InitializeRichTextBoxes()
        {
            AddContextMenu(richTextBox1);
            AddContextMenu(richTextBox2);
            AddContextMenu(richTextBox3);
            AddContextMenu(richTextBox4);

            RegisterHotkeys(richTextBox1);
            RegisterHotkeys(richTextBox2);
            RegisterHotkeys(richTextBox3);
            RegisterHotkeys(richTextBox4);
        }

        private void AddContextMenu(RichTextBox richTextBox)
        {
            richTextBox.ContextMenuStrip = CreateContextMenu(richTextBox);
        }

        private ContextMenuStrip CreateContextMenu(RichTextBox richTextBox)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Copy", null, (sender, e) => richTextBox.Copy());
            contextMenu.Items.Add("Cut", null, (sender, e) => richTextBox.Cut());
            contextMenu.Items.Add("Paste", null, (sender, e) => richTextBox.Paste());
            contextMenu.Items.Add("Select All", null, (sender, e) => richTextBox.SelectAll());
            return contextMenu;
        }

        private void RegisterHotkeys(RichTextBox richTextBox)
        {
            richTextBox.KeyDown += (sender, e) =>
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.A:
                            richTextBox.SelectAll();
                            e.SuppressKeyPress = true;
                            break;
                        case Keys.C:
                            richTextBox.Copy();
                            e.SuppressKeyPress = true;
                            break;
                        case Keys.X:
                            richTextBox.Cut();
                            e.SuppressKeyPress = true;
                            break;
                        case Keys.V:
                            richTextBox.Paste();
                            e.SuppressKeyPress = true;
                            break;
                    }
                }
            };
        }
        private void CheckHashListFile()
        {
            if (!File.Exists(hashListPath))
            {
                DialogResult result = MessageBox.Show(
                    "Hash list file not found. Do you want to generate it now?",
                    "Generate Hash List",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    GenerateHashList();
                }
            }
            else
            {
                LoadStoredHashs();
            }
        }

        private async void GenerateHashList()
        {
            try
            {
                await DownloadAndUpdateObjectList();
                LoadStoredHashs();
                MessageBox.Show("Hash list generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating hash list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStoredHashs()
        {
            StoredHashs = new Dictionary<int, string>();
            if (File.Exists(hashListPath))
            {
                string[] lines = File.ReadAllLines(hashListPath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string modelName = parts[0].Trim();
                        string hashValue = parts[1].Trim();

                        if (int.TryParse(hashValue, out int signedHash))
                        {
                            StoredHashs[signedHash] = modelName;
                        }
                        else if (uint.TryParse(hashValue, out uint unsignedHash))
                        {
                            signedHash = unchecked((int)unsignedHash);
                            StoredHashs[signedHash] = modelName;
                        }
                    }
                }
            }
        }

        private async Task DownloadAndUpdateObjectList()
        {
            using (HttpClient client = new HttpClient())
            {
                var allLines = new List<string>();

                foreach (var url in iniUrls)
                {
                    string content = await client.GetStringAsync(url);
                    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();

                        if (!string.IsNullOrWhiteSpace(trimmedLine))
                        {
                            if (url.Contains("ObjectList.ini") && !trimmedLine.Contains("="))
                            {
                                uint hash = Jenkins.Hash(trimmedLine.ToLower());
                                allLines.Add($"{trimmedLine}={hash}");
                            }
                            else if (trimmedLine.Contains("="))
                            {
                                allLines.Add(trimmedLine);
                            }
                        }
                    }
                }

                File.WriteAllLines(hashListPath, allLines.Distinct());
            }
        }

        private void SetupEventHandlers()
        {
            richTextBox1.TextChanged += RichTextBox1_TextChanged;
            richTextBox2.TextChanged += RichTextBox2_TextChanged;
            richTextBox3.TextChanged += RichTextBox3_TextChanged;
            richTextBox4.TextChanged += RichTextBox4_TextChanged;
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating) return;

            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                ClearTextBoxes();
                return;
            }

            isUpdating = true;
            uint unsignedHash = Jenkins.Hash(richTextBox1.Text.ToLower());
            int signedHash = unchecked((int)unsignedHash);
            UpdateTextBoxes(signedHash, false);
            isUpdating = false;
        }

        private void RichTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating) return;

            if (string.IsNullOrWhiteSpace(richTextBox2.Text))
            {
                ClearTextBoxes();
                return;
            }

            isUpdating = true;
            string hexValue = richTextBox2.Text.TrimStart('0', 'x');
            if (uint.TryParse(hexValue, NumberStyles.HexNumber, null, out uint unsignedHash))
            {
                int signedHash = unchecked((int)unsignedHash);
                UpdateTextBoxes(signedHash, true);
            }
            isUpdating = false;
        }

        private void RichTextBox3_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating) return;

            if (string.IsNullOrWhiteSpace(richTextBox3.Text))
            {
                ClearTextBoxes();
                return;
            }

            isUpdating = true;
            if (uint.TryParse(richTextBox3.Text, out uint unsignedHash))
            {
                int signedHash = unchecked((int)unsignedHash);
                UpdateTextBoxes(signedHash, true);
            }
            isUpdating = false;
        }

        private void RichTextBox4_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating) return;

            if (string.IsNullOrWhiteSpace(richTextBox4.Text))
            {
                ClearTextBoxes();
                return;
            }

            isUpdating = true;
            if (int.TryParse(richTextBox4.Text, out int signedHash))
            {
                UpdateTextBoxes(signedHash, true);
            }
            isUpdating = false;
        }

        private void ClearTextBoxes()
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            richTextBox3.Clear();
            richTextBox4.Clear();
        }

        private void UpdateTextBoxes(int signedHash, bool performReverse)
        {
            uint unsignedHash = unchecked((uint)signedHash);

            richTextBox2.Text = $"0x{unsignedHash:X8}";
            richTextBox3.Text = unsignedHash.ToString();
            richTextBox4.Text = signedHash.ToString();

            if (performReverse)
            {
                richTextBox1.Text = ReverseJenkins(signedHash);
            }
        }

        private string ReverseJenkins(int hash)
        {
            if (StoredHashs == null)
            {
                LoadStoredHashs();
            }

            if (StoredHashs.TryGetValue(hash, out string Value))
            {
                return Value;
            }

            uint unsignedHash = unchecked((uint)hash);
            string hexHash = $"0x{unsignedHash:X8}";

            if (StoredHashs.Values.Contains(hexHash))
            {
                return StoredHashs.FirstOrDefault(kvp => kvp.Value == hexHash).Value;
            }

            return "Not found";
        }
        private void CopyTextToClipboard(RichTextBox richTextBox)
        {
            if (!string.IsNullOrEmpty(richTextBox.Text))
            {
                Clipboard.SetText(richTextBox.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CopyTextToClipboard(richTextBox1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CopyTextToClipboard(richTextBox2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CopyTextToClipboard(richTextBox3);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CopyTextToClipboard(richTextBox4);
        }
    }

    public static class Jenkins
    {
        public static uint Hash(string input)
        {
            uint hash = 0;
            foreach (char c in input)
            {
                hash += c;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }
    }
}
