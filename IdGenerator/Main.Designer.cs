
namespace IdGenerator
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            OutputTxtBox = new System.Windows.Forms.TextBox();
            ExecuteTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            ExecuteBtn = new System.Windows.Forms.Button();
            ParseBtn = new System.Windows.Forms.Button();
            EntitiesTxtBox = new System.Windows.Forms.TextBox();
            MainSplitContainer = new System.Windows.Forms.SplitContainer();
            EntitiesPropertiesSplitContainer = new System.Windows.Forms.SplitContainer();
            PropertyGrid = new System.Windows.Forms.PropertyGrid();
            RegenBtn = new System.Windows.Forms.Button();
            ExecuteTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MainSplitContainer).BeginInit();
            MainSplitContainer.Panel1.SuspendLayout();
            MainSplitContainer.Panel2.SuspendLayout();
            MainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)EntitiesPropertiesSplitContainer).BeginInit();
            EntitiesPropertiesSplitContainer.Panel1.SuspendLayout();
            EntitiesPropertiesSplitContainer.Panel2.SuspendLayout();
            EntitiesPropertiesSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // OutputTxtBox
            // 
            OutputTxtBox.Dock = System.Windows.Forms.DockStyle.Fill;
            OutputTxtBox.Font = new System.Drawing.Font("Consolas", 11.25F);
            OutputTxtBox.Location = new System.Drawing.Point(0, 0);
            OutputTxtBox.Multiline = true;
            OutputTxtBox.Name = "OutputTxtBox";
            OutputTxtBox.Size = new System.Drawing.Size(857, 484);
            OutputTxtBox.TabIndex = 2;
            OutputTxtBox.TextChanged += OutputTxtBox_TextChanged;
            // 
            // ExecuteTableLayoutPanel
            // 
            ExecuteTableLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ExecuteTableLayoutPanel.ColumnCount = 5;
            ExecuteTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            ExecuteTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            ExecuteTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            ExecuteTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            ExecuteTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            ExecuteTableLayoutPanel.Controls.Add(ExecuteBtn, 1, 0);
            ExecuteTableLayoutPanel.Controls.Add(ParseBtn, 2, 0);
            ExecuteTableLayoutPanel.Controls.Add(RegenBtn, 3, 0);
            ExecuteTableLayoutPanel.Location = new System.Drawing.Point(12, 502);
            ExecuteTableLayoutPanel.Name = "ExecuteTableLayoutPanel";
            ExecuteTableLayoutPanel.RowCount = 1;
            ExecuteTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            ExecuteTableLayoutPanel.Size = new System.Drawing.Size(1227, 32);
            ExecuteTableLayoutPanel.TabIndex = 1;
            // 
            // ExecuteBtn
            // 
            ExecuteBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            ExecuteBtn.Location = new System.Drawing.Point(436, 3);
            ExecuteBtn.Name = "ExecuteBtn";
            ExecuteBtn.Size = new System.Drawing.Size(114, 26);
            ExecuteBtn.TabIndex = 1;
            ExecuteBtn.Text = "Execute";
            ExecuteBtn.UseVisualStyleBackColor = true;
            ExecuteBtn.Click += Execute_Click;
            // 
            // ParseBtn
            // 
            ParseBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            ParseBtn.Enabled = false;
            ParseBtn.Location = new System.Drawing.Point(556, 3);
            ParseBtn.Name = "ParseBtn";
            ParseBtn.Size = new System.Drawing.Size(114, 26);
            ParseBtn.TabIndex = 2;
            ParseBtn.Text = "Parse";
            ParseBtn.UseVisualStyleBackColor = true;
            ParseBtn.Click += ParseBtn_Click;
            // 
            // EntitiesTxtBox
            // 
            EntitiesTxtBox.Dock = System.Windows.Forms.DockStyle.Fill;
            EntitiesTxtBox.Font = new System.Drawing.Font("Consolas", 11.25F);
            EntitiesTxtBox.Location = new System.Drawing.Point(0, 0);
            EntitiesTxtBox.Multiline = true;
            EntitiesTxtBox.Name = "EntitiesTxtBox";
            EntitiesTxtBox.Size = new System.Drawing.Size(366, 354);
            EntitiesTxtBox.TabIndex = 0;
            EntitiesTxtBox.TextChanged += EntitiesTxtBox_TextChanged;
            // 
            // MainSplitContainer
            // 
            MainSplitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            MainSplitContainer.Location = new System.Drawing.Point(12, 12);
            MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            MainSplitContainer.Panel1.Controls.Add(EntitiesPropertiesSplitContainer);
            // 
            // MainSplitContainer.Panel2
            // 
            MainSplitContainer.Panel2.Controls.Add(OutputTxtBox);
            MainSplitContainer.Size = new System.Drawing.Size(1227, 484);
            MainSplitContainer.SplitterDistance = 366;
            MainSplitContainer.TabIndex = 3;
            // 
            // EntitiesPropertiesSplitContainer
            // 
            EntitiesPropertiesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            EntitiesPropertiesSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            EntitiesPropertiesSplitContainer.Location = new System.Drawing.Point(0, 0);
            EntitiesPropertiesSplitContainer.Name = "EntitiesPropertiesSplitContainer";
            EntitiesPropertiesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // EntitiesPropertiesSplitContainer.Panel1
            // 
            EntitiesPropertiesSplitContainer.Panel1.Controls.Add(EntitiesTxtBox);
            // 
            // EntitiesPropertiesSplitContainer.Panel2
            // 
            EntitiesPropertiesSplitContainer.Panel2.Controls.Add(PropertyGrid);
            EntitiesPropertiesSplitContainer.Size = new System.Drawing.Size(366, 484);
            EntitiesPropertiesSplitContainer.SplitterDistance = 354;
            EntitiesPropertiesSplitContainer.TabIndex = 0;
            // 
            // PropertyGrid
            // 
            PropertyGrid.BackColor = System.Drawing.SystemColors.Control;
            PropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            PropertyGrid.Location = new System.Drawing.Point(0, 0);
            PropertyGrid.Name = "PropertyGrid";
            PropertyGrid.Size = new System.Drawing.Size(366, 126);
            PropertyGrid.TabIndex = 3;
            PropertyGrid.ToolbarVisible = false;
            // 
            // RegenBtn
            // 
            RegenBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            RegenBtn.Enabled = false;
            RegenBtn.Location = new System.Drawing.Point(676, 3);
            RegenBtn.Name = "RegenBtn";
            RegenBtn.Size = new System.Drawing.Size(114, 26);
            RegenBtn.TabIndex = 3;
            RegenBtn.Text = "Regenerate";
            RegenBtn.UseVisualStyleBackColor = true;
            RegenBtn.Click += RegenBtn_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1251, 546);
            Controls.Add(MainSplitContainer);
            Controls.Add(ExecuteTableLayoutPanel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Main";
            Text = "Id Generator";
            FormClosing += Main_FormClosing;
            Load += Form1_Load;
            ExecuteTableLayoutPanel.ResumeLayout(false);
            MainSplitContainer.Panel1.ResumeLayout(false);
            MainSplitContainer.Panel2.ResumeLayout(false);
            MainSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MainSplitContainer).EndInit();
            MainSplitContainer.ResumeLayout(false);
            EntitiesPropertiesSplitContainer.Panel1.ResumeLayout(false);
            EntitiesPropertiesSplitContainer.Panel1.PerformLayout();
            EntitiesPropertiesSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)EntitiesPropertiesSplitContainer).EndInit();
            EntitiesPropertiesSplitContainer.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox OutputTxtBox;
        private System.Windows.Forms.TableLayoutPanel ExecuteTableLayoutPanel;
        private System.Windows.Forms.Button ExecuteBtn;
        private System.Windows.Forms.TextBox EntitiesTxtBox;
        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.SplitContainer EntitiesPropertiesSplitContainer;
        private System.Windows.Forms.PropertyGrid PropertyGrid;
        private System.Windows.Forms.Button ParseBtn;
        private System.Windows.Forms.Button RegenBtn;
    }
}

