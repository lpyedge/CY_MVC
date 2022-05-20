using System;
using System.Windows.Forms;

namespace KeyGenerate
{
    public partial class FormB : Form
    {
        public FormB()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("CHUyang"))
            {
                FormA fa = new FormA();
                this.Visible = false;
                fa.ShowDialog(this);
                this.Close();
            }
        }
    }
}