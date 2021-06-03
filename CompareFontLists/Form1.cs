using CreateFontList;
using System;
using System.Windows.Forms;

namespace CompareFontLists
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ListCreator.CreateList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var form = new FileSelectionForm();
            form.Show();
        }
    }
}
