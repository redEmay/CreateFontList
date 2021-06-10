using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CreateFontList;

namespace CompareFontLists
{
    public partial class FileSelectionForm : Form
    {
        IAppSettings appsettings { get; set; }

        public FileSelectionForm()
        {
            InitializeComponent();
            appsettings = new AppSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var sourceList = new List<string>();
            var masterList = new List<string>();
            var targetList = new List<string>();
            var uniqueSourceList = new List<string>();
            var uniqueTargetList = new List<string>();

            if (!textBox1.Text.Contains(".txt"))
                MessageBox.Show($"File {textBox1} is nto a txt file.");
            if (!File.Exists(textBox1.Text))
                MessageBox.Show($"File {textBox1.Text} doesn't exist.");

            else
            {
                targetList = File.ReadAllLines(textBox1.Text).ToList();

                try
                {
                    if (!File.Exists(appsettings.Location.ShareFontListFileLocation))
                        MessageBox.Show($"{appsettings.Location.ShareFontListFileLocation} doesn't exist");
                    else
                        sourceList = File.ReadAllLines(appsettings.Location.ShareFontListFileLocation).ToList();

                    foreach (var entry in sourceList)
                    {
                        masterList.Add(entry);

                        if (!targetList.Contains(entry))
                            uniqueSourceList.Add(entry);
                    }

                    foreach (var entry in targetList)
                    {
                        masterList.Add(entry);

                        if (!targetList.Contains(entry))
                            uniqueTargetList.Add(entry);
                    }

                    var FinalList = masterList.Distinct();

                    var form = new ListOutputForm();
                    form.Show();
                    form.ShowLists(appsettings.Location.ShareFontListFileLocation, sourceList, textBox1.Text, targetList);
                }
                catch { MessageBox.Show("Couldn't create lists"); }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Height = 325;
                button1.Location = new System.Drawing.Point(214, 253);
            }
            else
            {
                Height = 150;
                button1.Location = new System.Drawing.Point(317, 74);
            }
        }
    }
}
