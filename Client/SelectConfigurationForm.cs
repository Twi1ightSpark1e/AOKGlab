using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class SelectConfigurationForm : Form
    {
        public SelectConfigurationForm()
        {
            InitializeComponent();

            configurationComboBox.Items.AddRange(Directory.GetFiles(Application.StartupPath).Select(filename => Path.GetFileNameWithoutExtension(filename)).Where((str) => str.StartsWith("configuration-")).Select((str) => str.Replace("configuration-", "")).ToArray());
            configurationComboBox.SelectedIndex = 0;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Configuration.Configuration.Load($"configuration-{configurationComboBox.Text}.json");
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
