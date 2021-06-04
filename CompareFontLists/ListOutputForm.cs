using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IronXL;
using CreateFontList.SharedFunctionality;

namespace CompareFontLists
{
    public partial class ListOutputForm : Form
    {
        public string sourceLocation { get; set; }
        public string targetLocation { get; set; }
        public List<string> uniqueSourceList { get; set; }
        public List<string> uniqueTargetList { get; set; }
        
        public ListOutputForm()
        {
            sourceLocation = string.Empty;
            targetLocation = string.Empty;
            uniqueSourceList = new List<string>();
            uniqueTargetList = new List<string>();
            InitializeComponent();
        }

        public void ShowLists(string sourceName, List<string> sourceList, string targetName,
            List<string> targetList)
        {
            label1.Text = sourceName.Replace('\\', '/');
            label2.Text = targetName.Replace('\\', '/');

            sourceLocation = label1.Text;
            targetLocation = label2.Text;

            label3.Text = $"{sourceList.Count} Entries";
            label4.Text = $"{targetList.Count} Entries";


            listView1.Columns.Add("Font", -2, HorizontalAlignment.Left);

            TrimEndOfAllEntries(ref sourceList);
            TrimEndOfAllEntries(ref targetList);

            foreach (var entry in sourceList)
            {
                if (entry == string.Empty)
                    continue;

                listView1.Items.Add(entry.Trim());

                if (!targetList.Contains(entry))
                    uniqueSourceList.Add(entry);
            }
            label5.Text = $"{uniqueSourceList.Count} Unique Entries";

            foreach (var entry in targetList)
            {
                if (entry == string.Empty)
                    continue;

                listView2.Items.Add(entry);
                if (!sourceList.Contains(entry))
                    uniqueTargetList.Add(entry);
            }
            label6.Text = $"{uniqueTargetList.Count} Unique Entries";
        }

        private void TrimEndOfAllEntries(ref List<string> sourceList)
        {
            for (int i = 0; i < sourceList.Count; i++)
            {
                sourceList[i] = sourceList[i].TrimEnd();
            }
        }

        private void sourceTransferButton_Click(object sender, System.EventArgs e)
        {
            var folderLocation = $"{label2.Text.Replace("FontList.txt", string.Empty)}UniqueFonts";

            var csv = "G:/Apps/MISFontList/fonts.csv";

            var workbook = WorkBook.Load(csv.Trim());

            var sheet = workbook.WorkSheets[0];

            Cell[] sheetRow;

            string[] locationElements;

            if (Directory.Exists($"{folderLocation}"))
                Shared_Functions.DeleteDirectory($"{folderLocation}/");

            Directory.CreateDirectory($"{folderLocation}");

            for (int row = 0; row < sheet.RowCount; row++)
            {
                sheetRow = sheet.GetRow(row).ToArray();

                if (!uniqueSourceList.Contains(sheetRow[1].Text))
                    continue;

                locationElements = sheetRow[0].Text.Split('/');

                File.Copy(sheetRow[0].Text, $"{folderLocation}/{locationElements.Last()}");
            }
        }

        private void targetTransferButton_Click(object sender, System.EventArgs e)
        {
            /*var folderLocation = $"{label2.Text.Replace("FontList.txt", string.Empty)}UniqueFonts";

            var csv = "G:/Apps/MISFontList/fonts.csv";

            var workbook = WorkBook.Load(csv.Trim());

            var sheet = workbook.WorkSheets[0];

            Cell[] sheetRow;

            string[] locationElements;

            if (Directory.Exists($"{folderLocation}"))
                Shared_Functions.DeleteDirectory($"{folderLocation}/");

            Directory.CreateDirectory($"{folderLocation}");

            for (int row = 0; row < sheet.RowCount; row++)
            {
                sheetRow = sheet.GetRow(row).ToArray();

                if (!uniqueSourceList.Contains(sheetRow[1].Text))
                    continue;

                locationElements = sheetRow[0].Text.Split('/');

                File.Copy(sheetRow[0].Text, $"{folderLocation}/{locationElements.Last()}");
            }*/
        }

        private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label5.Visible = true;
                for(int i = 0; i < listView1.Items.Count; i++)
                {
                    if(uniqueSourceList.Contains(listView1.Items[i].Text))
                        listView1.Items[i].BackColor = Color.Aqua;
                }
                label6.Visible = true;

                for (int i = 0; i < listView2.Items.Count; i++)
                {
                    if (uniqueTargetList.Contains(listView2.Items[i].Text))
                        listView2.Items[i].BackColor = Color.Aqua;
                }
            }
            else
            {
                label5.Visible = false;
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[i].BackColor = Color.Empty;
                }

                label6.Visible = false;
                for (int i = 0; i < listView2.Items.Count; i++)
                {
                    listView2.Items[i].BackColor = Color.Empty;
                }
            }
            listView1.Update();
            listView2.Update();
        }
    }
}
