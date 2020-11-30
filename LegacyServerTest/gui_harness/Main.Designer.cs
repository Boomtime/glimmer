namespace ShadowCreatures.Glimmer {
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
			this.cDetailsGrid = new System.Windows.Forms.PropertyGrid();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.cDevices = new System.Windows.Forms.FlowLayoutPanel();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.cFuncGroup = new System.Windows.Forms.GroupBox();
			this.cFuncFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cBtnLoad = new System.Windows.Forms.Button();
			this.cBtnSave = new System.Windows.Forms.Button();
			this.cFrameLatency = new System.Windows.Forms.PictureBox();
			this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
			this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
			this.cUpdatesPerSecond = new System.Windows.Forms.NumericUpDown();
			this.labelHz = new System.Windows.Forms.Label();
			this.cAutoHunt = new System.Windows.Forms.CheckBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabGlimDetails = new System.Windows.Forms.TabPage();
			this.tabSequenceControls = new System.Windows.Forms.TabPage();
			this.ctlSequenceControlsPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.tabEventLog = new System.Windows.Forms.TabPage();
			this.ctlEventLog = new ShadowCreatures.Glimmer.Controls.RichText();
			this.ctl_status = new System.Windows.Forms.StatusStrip();
			tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel3.SuspendLayout();
			this.cFuncGroup.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cFrameLatency)).BeginInit();
			this.flowLayoutPanel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cUpdatesPerSecond)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabGlimDetails.SuspendLayout();
			this.tabSequenceControls.SuspendLayout();
			this.tabEventLog.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel2
			// 
			tableLayoutPanel2.ColumnCount = 2;
			tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			tableLayoutPanel2.Controls.Add(this.cDetailsGrid, 0, 0);
			tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 1;
			tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			tableLayoutPanel2.Size = new System.Drawing.Size(1554, 451);
			tableLayoutPanel2.TabIndex = 13;
			// 
			// cDetailsGrid
			// 
			this.cDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cDetailsGrid.HelpVisible = false;
			this.cDetailsGrid.Location = new System.Drawing.Point(3, 3);
			this.cDetailsGrid.Name = "cDetailsGrid";
			this.cDetailsGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
			this.cDetailsGrid.Size = new System.Drawing.Size(771, 445);
			this.cDetailsGrid.TabIndex = 12;
			this.cDetailsGrid.ToolbarVisible = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.19195F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.80805F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 393F));
			this.tableLayoutPanel1.Controls.Add(this.cDevices, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel4, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel5, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(17, 18);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52.73973F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.26027F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1574, 1103);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// cDevices
			// 
			this.cDevices.AutoScroll = true;
			this.cDevices.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cDevices.Location = new System.Drawing.Point(4, 5);
			this.cDevices.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cDevices.Name = "cDevices";
			this.cDevices.Size = new System.Drawing.Size(549, 548);
			this.cDevices.TabIndex = 1;
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.Controls.Add(this.cFuncGroup);
			this.flowLayoutPanel3.Controls.Add(this.panel1);
			this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel3.Location = new System.Drawing.Point(561, 5);
			this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(615, 548);
			this.flowLayoutPanel3.TabIndex = 5;
			// 
			// cFuncGroup
			// 
			this.cFuncGroup.Controls.Add(this.cFuncFlow);
			this.cFuncGroup.Location = new System.Drawing.Point(4, 5);
			this.cFuncGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cFuncGroup.Name = "cFuncGroup";
			this.cFuncGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cFuncGroup.Size = new System.Drawing.Size(249, 437);
			this.cFuncGroup.TabIndex = 7;
			this.cFuncGroup.TabStop = false;
			this.cFuncGroup.Text = "Function";
			// 
			// cFuncFlow
			// 
			this.cFuncFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cFuncFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.cFuncFlow.Location = new System.Drawing.Point(4, 27);
			this.cFuncFlow.Name = "cFuncFlow";
			this.cFuncFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.cFuncFlow.Size = new System.Drawing.Size(241, 405);
			this.cFuncFlow.TabIndex = 5;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cBtnLoad);
			this.panel1.Controls.Add(this.cBtnSave);
			this.panel1.Controls.Add(this.cFrameLatency);
			this.panel1.Location = new System.Drawing.Point(260, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(316, 434);
			this.panel1.TabIndex = 11;
			// 
			// cBtnLoad
			// 
			this.cBtnLoad.Location = new System.Drawing.Point(4, 240);
			this.cBtnLoad.Name = "cBtnLoad";
			this.cBtnLoad.Size = new System.Drawing.Size(175, 56);
			this.cBtnLoad.TabIndex = 10;
			this.cBtnLoad.Text = "Load Custom ...";
			this.cBtnLoad.UseVisualStyleBackColor = true;
			// 
			// cBtnSave
			// 
			this.cBtnSave.Location = new System.Drawing.Point(4, 178);
			this.cBtnSave.Name = "cBtnSave";
			this.cBtnSave.Size = new System.Drawing.Size(175, 56);
			this.cBtnSave.TabIndex = 9;
			this.cBtnSave.Text = "Save Custom ...";
			this.cBtnSave.UseVisualStyleBackColor = true;
			// 
			// cFrameLatency
			// 
			this.cFrameLatency.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.cFrameLatency.Location = new System.Drawing.Point(4, 381);
			this.cFrameLatency.Name = "cFrameLatency";
			this.cFrameLatency.Padding = new System.Windows.Forms.Padding(1);
			this.cFrameLatency.Size = new System.Drawing.Size(300, 50);
			this.cFrameLatency.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.cFrameLatency.TabIndex = 6;
			this.cFrameLatency.TabStop = false;
			// 
			// flowLayoutPanel4
			// 
			this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel4.Location = new System.Drawing.Point(560, 1061);
			this.flowLayoutPanel4.Name = "flowLayoutPanel4";
			this.flowLayoutPanel4.Size = new System.Drawing.Size(617, 39);
			this.flowLayoutPanel4.TabIndex = 6;
			// 
			// flowLayoutPanel5
			// 
			this.flowLayoutPanel5.Controls.Add(this.cUpdatesPerSecond);
			this.flowLayoutPanel5.Controls.Add(this.labelHz);
			this.flowLayoutPanel5.Controls.Add(this.cAutoHunt);
			this.flowLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel5.Location = new System.Drawing.Point(3, 1061);
			this.flowLayoutPanel5.Name = "flowLayoutPanel5";
			this.flowLayoutPanel5.Size = new System.Drawing.Size(551, 39);
			this.flowLayoutPanel5.TabIndex = 7;
			// 
			// cUpdatesPerSecond
			// 
			this.cUpdatesPerSecond.Location = new System.Drawing.Point(3, 3);
			this.cUpdatesPerSecond.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.cUpdatesPerSecond.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.cUpdatesPerSecond.Name = "cUpdatesPerSecond";
			this.cUpdatesPerSecond.ReadOnly = true;
			this.cUpdatesPerSecond.Size = new System.Drawing.Size(85, 29);
			this.cUpdatesPerSecond.TabIndex = 6;
			this.cUpdatesPerSecond.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
			// 
			// labelHz
			// 
			this.labelHz.Location = new System.Drawing.Point(94, 0);
			this.labelHz.Name = "labelHz";
			this.labelHz.Size = new System.Drawing.Size(56, 39);
			this.labelHz.TabIndex = 7;
			this.labelHz.Text = "Hz";
			this.labelHz.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cAutoHunt
			// 
			this.cAutoHunt.AutoSize = true;
			this.cAutoHunt.Checked = true;
			this.cAutoHunt.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cAutoHunt.Location = new System.Drawing.Point(156, 3);
			this.cAutoHunt.Name = "cAutoHunt";
			this.cAutoHunt.Size = new System.Drawing.Size(124, 29);
			this.cAutoHunt.TabIndex = 5;
			this.cAutoHunt.Text = "Auto-hunt";
			this.cAutoHunt.UseVisualStyleBackColor = true;
			// 
			// tabControl1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.tabControl1, 3);
			this.tabControl1.Controls.Add(this.tabGlimDetails);
			this.tabControl1.Controls.Add(this.tabSequenceControls);
			this.tabControl1.Controls.Add(this.tabEventLog);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(3, 561);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(1568, 494);
			this.tabControl1.TabIndex = 9;
			// 
			// tabGlimDetails
			// 
			this.tabGlimDetails.Controls.Add(tableLayoutPanel2);
			this.tabGlimDetails.Location = new System.Drawing.Point(4, 33);
			this.tabGlimDetails.Name = "tabGlimDetails";
			this.tabGlimDetails.Padding = new System.Windows.Forms.Padding(3);
			this.tabGlimDetails.Size = new System.Drawing.Size(1560, 457);
			this.tabGlimDetails.TabIndex = 0;
			this.tabGlimDetails.Text = "Glim Details";
			this.tabGlimDetails.UseVisualStyleBackColor = true;
			// 
			// tabSequenceControls
			// 
			this.tabSequenceControls.Controls.Add(this.ctlSequenceControlsPanel);
			this.tabSequenceControls.Location = new System.Drawing.Point(4, 33);
			this.tabSequenceControls.Name = "tabSequenceControls";
			this.tabSequenceControls.Padding = new System.Windows.Forms.Padding(3);
			this.tabSequenceControls.Size = new System.Drawing.Size(1560, 457);
			this.tabSequenceControls.TabIndex = 1;
			this.tabSequenceControls.Text = "Sequence Controls";
			this.tabSequenceControls.UseVisualStyleBackColor = true;
			// 
			// ctlSequenceControlsPanel
			// 
			this.ctlSequenceControlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctlSequenceControlsPanel.Location = new System.Drawing.Point(3, 3);
			this.ctlSequenceControlsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.ctlSequenceControlsPanel.Name = "ctlSequenceControlsPanel";
			this.ctlSequenceControlsPanel.Size = new System.Drawing.Size(1554, 451);
			this.ctlSequenceControlsPanel.TabIndex = 3;
			// 
			// tabEventLog
			// 
			this.tabEventLog.Controls.Add(this.ctlEventLog);
			this.tabEventLog.Location = new System.Drawing.Point(4, 33);
			this.tabEventLog.Name = "tabEventLog";
			this.tabEventLog.Padding = new System.Windows.Forms.Padding(3);
			this.tabEventLog.Size = new System.Drawing.Size(1560, 457);
			this.tabEventLog.TabIndex = 2;
			this.tabEventLog.Text = "Event Log";
			this.tabEventLog.UseVisualStyleBackColor = true;
			// 
			// ctlEventLog
			// 
			this.ctlEventLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctlEventLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctlEventLog.Location = new System.Drawing.Point(3, 3);
			this.ctlEventLog.MaxLines = 5;
			this.ctlEventLog.Name = "ctlEventLog";
			this.ctlEventLog.Size = new System.Drawing.Size(1554, 451);
			this.ctlEventLog.TabIndex = 8;
			// 
			// ctl_status
			// 
			this.ctl_status.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.ctl_status.Location = new System.Drawing.Point(0, 1138);
			this.ctl_status.Name = "ctl_status";
			this.ctl_status.Padding = new System.Windows.Forms.Padding(2, 0, 19, 0);
			this.ctl_status.Size = new System.Drawing.Size(1607, 22);
			this.ctl_status.TabIndex = 1;
			this.ctl_status.Text = "statusStrip1";
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1607, 1160);
			this.Controls.Add(this.ctl_status);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "Main";
			this.Text = "Beta Crucis";
			this.Load += new System.EventHandler(this.XMainLoad);
			tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel3.ResumeLayout(false);
			this.cFuncGroup.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.cFrameLatency)).EndInit();
			this.flowLayoutPanel5.ResumeLayout(false);
			this.flowLayoutPanel5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.cUpdatesPerSecond)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabGlimDetails.ResumeLayout(false);
			this.tabSequenceControls.ResumeLayout(false);
			this.tabEventLog.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.StatusStrip ctl_status;
		private System.Windows.Forms.FlowLayoutPanel ctlSequenceControlsPanel;
		private System.Windows.Forms.CheckBox cAutoHunt;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
		private System.Windows.Forms.NumericUpDown cUpdatesPerSecond;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
		private System.Windows.Forms.Label labelHz;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.GroupBox cFuncGroup;
		private System.Windows.Forms.FlowLayoutPanel cFuncFlow;
		private System.Windows.Forms.PictureBox cFrameLatency;
		private System.Windows.Forms.FlowLayoutPanel cDevices;
		private System.Windows.Forms.Button cBtnSave;
		private System.Windows.Forms.Button cBtnLoad;
		private System.Windows.Forms.Panel panel1;
		private Controls.RichText ctlEventLog;
		private System.Windows.Forms.PropertyGrid cDetailsGrid;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabGlimDetails;
		private System.Windows.Forms.TabPage tabSequenceControls;
		private System.Windows.Forms.TabPage tabEventLog;
	}
}

