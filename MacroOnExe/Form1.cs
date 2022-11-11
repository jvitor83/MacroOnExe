using Newtonsoft.Json;
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

namespace MacroOnExe
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = openFileDialog1.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                this.ExecutablePathTextBox.Text = openFileDialog1.FileName;
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            bool anyModifierWasPressed = e.Control || e.Alt || e.Shift;
            bool thereIsAnotherKeyThanTheModifiersPressed = e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.Alt;

            if (!anyModifierWasPressed || anyModifierWasPressed && thereIsAnotherKeyThanTheModifiersPressed)
            {
                IEnumerable<DataGridViewCell> enumerable = dataGridView1.SelectedCells.Cast<DataGridViewCell>();
                if (enumerable.All(c => c.ColumnIndex == 0))
                {
                    foreach (var dataGridViewCell in enumerable)
                    {
                        dataGridViewCell.Value = e.KeyData;
                    }
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BindingSource dataSource = (BindingSource)dataGridView1.DataSource;
            List<KeyTime> source = ((BindingList<KeyTime>)dataSource.List).ToList();

            var executableKeyTime = new ExecutableKeyTime()
            {
                ExecutablePath = ExecutablePathTextBox.Text,
                Keys = source
            };

            var serialized = JsonConvert.SerializeObject(
                executableKeyTime, new JsonSerializerSettings() { }
            );

            File.WriteAllText("config.json", serialized);
            this.Close();
        }
    }
}
