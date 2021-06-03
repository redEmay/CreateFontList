using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CompareFontLists
{
    public partial class FileSelectionForm : Form
    {
        public FileSelectionForm()
        {
            InitializeComponent();
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
                    if (!File.Exists("S:/FontList/FontList.txt"))
                        MessageBox.Show("S:/FontList/FontList.txt doesn't exist");
                    else
                        sourceList = File.ReadAllLines("S:/FontList/FontList.txt").ToList();

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
                    form.ShowLists("S:/FontList/FontList.txt", sourceList, textBox1.Text, targetList);
                }
                catch { MessageBox.Show("Couldn't create lists"); }
            }
        }
    }
}
