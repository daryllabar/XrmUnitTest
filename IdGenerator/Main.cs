using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace IdGenerator
{
    public partial class Main : Form
    {
        private readonly IdGeneratorSettings _settings;

        public Main()
        {
            InitializeComponent();
            _settings = new IdGeneratorSettings().Load();
            PropertyGrid.SelectedObject = _settings;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EntitiesTxtBox.Text = _settings.Entities;
            UpdateGridLabelWidth();
        }

        private void UpdateGridLabelWidth()
        {
            // Set the label (name) column width to 200 pixels
            var gridView = PropertyGrid.Controls[2]; // The internal grid view is usually at index 1
            var method = gridView.GetType().GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(gridView, new object[] { _settings.PropertyGridLabelWidth }); // Set to desired width in pixels
            }
        }

        private void Execute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EntitiesTxtBox.Text))
            {
                MessageBox.Show("Input is Requird!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var logic = new Logic(_settings, new GuidGenerator());
            Dictionary<string, IdInfo> idsByType;
            try
            {
                idsByType = logic.ParseEntityTypes(EntitiesTxtBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error Parsing Input: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                OutputTxtBox.Text = ex.ToString();
                return;
            }

            try { 
                OutputTxtBox.Text = logic.GenerateOutput(idsByType.Values);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error Generating Output: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                OutputTxtBox.Text = ex.ToString();
                return;
            }

            _settings.Entities = EntitiesTxtBox.Text;
            _settings.Save();
        }

        private void ParseBtn_Click(object sender, EventArgs e)
        {
            var results = IdFieldInfo.ParseIdFields(OutputTxtBox.Text);
            var output = (from @group in results.GroupBy(r => r.StructName ?? r.IdType)
                          let structName = @group.First().StructName ?? string.Empty
                          let names = (string.IsNullOrWhiteSpace(structName)
                              ? ","
                              : "," + @group.First().StructName + ",") + string.Join(",", @group.Select(g => g.FieldName))
                          select @group.First().IdType + names).ToList();
            EntitiesTxtBox.Text = string.Join(Environment.NewLine, output);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Get current splitter position before saving
            var gridView = PropertyGrid.Controls[2];
            var field = gridView.GetType().GetField("_labelWidth", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                _settings.PropertyGridLabelWidth = (int)field.GetValue(gridView);
            }
            _settings.Save();
        }

        private void EntitiesTxtBox_TextChanged(object sender, EventArgs e)
        {
            ExecuteBtn.Enabled = !string.IsNullOrEmpty(EntitiesTxtBox.Text);
        }

        private void OutputTxtBox_TextChanged(object sender, EventArgs e)
        {
            ParseBtn.Enabled = !string.IsNullOrEmpty(OutputTxtBox.Text);
            RegenBtn.Enabled = ParseBtn.Enabled;
        }

        private void RegenBtn_Click(object sender, EventArgs e)
        {
            RegenerateExisting(OutputTxtBox.Text);
        }

        private void RegenerateExisting(string existingIds)
        {
            var lines = existingIds.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var output = new List<string>();
            foreach (var line in lines)
            {
                var i = line.IndexOf("(\"") + 2;
                if (i > 2)
                {
                    output.Add(line.Substring(0, i) + $"{Guid.NewGuid()}\");");
                }
                else
                {
                    output.Add(line);
                }
            }

            OutputTxtBox.Text = string.Join(Environment.NewLine, output);
        }

    }
}
