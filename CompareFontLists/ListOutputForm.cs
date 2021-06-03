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

            label3.Text = $"{sourceList.Count}";
            label4.Text = $"{targetList.Count}";


            listView1.Columns.Add("Font", -2, HorizontalAlignment.Left);

            TrimEndOfAllEntries(ref sourceList);
            TrimEndOfAllEntries(ref targetList);

            foreach (var entry in sourceList)
            {
                if (entry == string.Empty)
                    continue;

                listView1.Items.Add(entry.Trim());

                if (!targetList.Contains(entry))
                {
                    listView1.Items[listView1.Items.Count - 1].BackColor = Color.Aqua;
                    uniqueSourceList.Add(entry);
                }
            }

            foreach (var entry in targetList)
            {
                if (entry == string.Empty)
                    continue;

                listView2.Items.Add(entry);
                if (!sourceList.Contains(entry))
                {
                    listView2.Items[listView2.Items.Count - 1].BackColor = Color.Aqua;
                    uniqueTargetList.Add(entry);
                }
            }
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
    }
}
