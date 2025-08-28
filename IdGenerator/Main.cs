using PluralizeService.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Reflection;

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
            EntitiesTxtBox.Text = "Account 2" + Environment.NewLine + "Contact";
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

            _settings.Save();

            var parts = EntitiesTxtBox.Text.Split(new string[] { ",", " ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var types = new List<string>();
            var previous = string.Empty;
            foreach (var t in parts)
            {
                var value = t.Trim();
                if (int.TryParse(value, out var intValue))
                {
                    for (var j = intValue; j > 1; j--)
                    {
                        types.Add(previous);
                    }
                }
                else
                {
                    previous = value;
                    types.Add(previous);
                }
            }

            OutputTxtBox.Text = string.Empty;
            var idInfos = CreateIdInfo(types);
            GenerateOutput(idInfos);
        }

        private void ParseExisting(string existingIds)
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

            OutputTxtBox.AppendText(string.Join(Environment.NewLine, output));
        }

        private List<IdInfo> CreateIdInfo(List<string> entityTypes)
        {
            entityTypes = entityTypes.OrderBy(t => t.ToLower()).ToList();
            var nameChar = (int)'a';
            var dictionary = new Dictionary<string, int>();
            var areMultiple = entityTypes.GroupBy(v => v).Count() != entityTypes.Count;
            var idInfos = new List<IdInfo>(entityTypes.Count);
            foreach (var entityType in entityTypes)
            {
                var name = entityType == "Entity" ? ((char)nameChar++).ToString() : entityType;
                if (name.Contains("_"))
                {
                    name = string.Join(string.Empty, name.Split('_').Skip(1).ToArray());
                }

                name = name.ToUpper()[0] + name.Remove(0, 1);
                dictionary.TryGetValue(name, out var count);
                dictionary[name] = count + 1;
                if (count > 0)
                {
                    name = name + count;
                }
                else if (areMultiple)
                {
                    name += " ";
                }
                idInfos.Add(new IdInfo
                {
                    EntityType = entityType,
                    Name = name,
                    Index = count
                });
            }

            return idInfos;
        }

        private void GenerateOutput(List<IdInfo> idInfos)
        {
            var output = new List<string>();
            foreach (var entityType in idInfos.GroupBy(i => i.EntityType))
            {
                var isMultiple = entityType.Count() > 1;
                if (isMultiple)
                {
                    output.Add($"public struct {PluralizationProvider.Pluralize(entityType.First(t => t.Index == 0).Name.TrimEnd())}");
                    output.Add("{");
                }
                foreach (var id in entityType)
                {
                    var newStatement = _settings.UseTargetTypedNew ? "new" : $"new Id<{id.EntityType}>";
                    if (isMultiple)
                    {
                        output.Add($"    public static readonly Id<{id.EntityType}> {IntToBase(id.Index + 1)} = {newStatement}(\"{Guid.NewGuid().ToString().ToUpper()}\");");
                    }
                    else
                    {
                        output.Add($"public static readonly Id<{id.EntityType}> {id.Name}= {newStatement}(\"{Guid.NewGuid().ToString().ToUpper()}\");");
                    }
                }

                if (isMultiple)
                {
                    output.Add("}");
                }
            }

            OutputTxtBox.AppendText(string.Join(Environment.NewLine, output));
        }

        public class IdInfo
        {
            public string EntityType { get; set; }
            public string Name { get; set; }
            public int Index { get; set; }
        }

        private static readonly char[] BaseChars =
                 "zabcdefghijklmnopqrstuvwxy".ToCharArray();
        private static readonly Dictionary<char, int> CharValues = BaseChars
                   .Select((c, i) => new { Char = c, Index = i })
                   .ToDictionary(c => c.Char, c => c.Index);

        public static string IntToBase(int value)
        {
            int targetBase = BaseChars.Length;
            // Determine exact number of characters to use.
            char[] buffer = new char[Math.Max(
                       (int)Math.Ceiling(Math.Log(value + 1, targetBase)), 1)];

            var i = (long)buffer.Length;
            do
            {
                buffer[--i] = BaseChars[value % targetBase];
                value = value / targetBase;
            }
            while (value > 0);

            if (buffer.Length > 0)
            {
                buffer[0] = buffer[0].ToString().ToUpper()[0];
            }
            return new string(buffer);
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

        // Define other methods and classes here

    }
}
