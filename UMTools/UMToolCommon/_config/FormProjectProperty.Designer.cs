namespace UMTools.Common
{
	partial class FormProjectProperty
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProjectProperty));
			this.pg_project_globaltype = new System.Windows.Forms.PropertyGrid();
			this.label1 = new System.Windows.Forms.Label();
			this.cb_projectselect = new System.Windows.Forms.ComboBox();
			this.cb_globaltype = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btn_apply = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.pg_global = new System.Windows.Forms.PropertyGrid();
			this.tab_control = new System.Windows.Forms.TabControl();
			this.tap_project = new System.Windows.Forms.TabPage();
			this.pg_project = new System.Windows.Forms.PropertyGrid();
			this.tap_globaltype = new System.Windows.Forms.TabPage();
			this.tab_global = new System.Windows.Forms.TabPage();
			this.tab_control.SuspendLayout();
			this.tap_project.SuspendLayout();
			this.tap_globaltype.SuspendLayout();
			this.tab_global.SuspendLayout();
			this.SuspendLayout();
			// 
			// pg_project_globaltype
			// 
			this.pg_project_globaltype.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pg_project_globaltype.BackColor = System.Drawing.SystemColors.Control;
			this.pg_project_globaltype.CommandsForeColor = System.Drawing.SystemColors.ControlText;
			this.pg_project_globaltype.LineColor = System.Drawing.SystemColors.ControlLight;
			this.pg_project_globaltype.Location = new System.Drawing.Point(6, 6);
			this.pg_project_globaltype.Name = "pg_project_globaltype";
			this.pg_project_globaltype.Size = new System.Drawing.Size(614, 564);
			this.pg_project_globaltype.TabIndex = 0;
			this.pg_project_globaltype.ToolbarVisible = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 12);
			this.label1.TabIndex = 1;
			this.label1.Text = "Project Select :";
			// 
			// cb_projectselect
			// 
			this.cb_projectselect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cb_projectselect.FormattingEnabled = true;
			this.cb_projectselect.Location = new System.Drawing.Point(109, 10);
			this.cb_projectselect.MaxDropDownItems = 30;
			this.cb_projectselect.Name = "cb_projectselect";
			this.cb_projectselect.Size = new System.Drawing.Size(129, 20);
			this.cb_projectselect.TabIndex = 2;
			this.cb_projectselect.SelectedIndexChanged += new System.EventHandler(this.cb_projectselect_SelectedIndexChanged);
			// 
			// cb_globaltype
			// 
			this.cb_globaltype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cb_globaltype.FormattingEnabled = true;
			this.cb_globaltype.Location = new System.Drawing.Point(332, 10);
			this.cb_globaltype.MaxDropDownItems = 30;
			this.cb_globaltype.Name = "cb_globaltype";
			this.cb_globaltype.Size = new System.Drawing.Size(123, 20);
			this.cb_globaltype.TabIndex = 4;
			this.cb_globaltype.SelectedIndexChanged += new System.EventHandler(this.cb_globaltype_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(244, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(82, 12);
			this.label2.TabIndex = 3;
			this.label2.Text = "Global Type :";
			// 
			// btn_apply
			// 
			this.btn_apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_apply.Location = new System.Drawing.Point(467, 8);
			this.btn_apply.Name = "btn_apply";
			this.btn_apply.Size = new System.Drawing.Size(179, 27);
			this.btn_apply.TabIndex = 5;
			this.btn_apply.Text = "적 용";
			this.btn_apply.UseVisualStyleBackColor = true;
			this.btn_apply.Click += new System.EventHandler(this.btn_apply_Click);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label3.Location = new System.Drawing.Point(363, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(283, 12);
			this.label3.TabIndex = 6;
			this.label3.Text = "* 프로젝트 선택/설정은 모든 툴에 반영됩니다.";
			// 
			// pg_global
			// 
			this.pg_global.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pg_global.BackColor = System.Drawing.SystemColors.Control;
			this.pg_global.CommandsForeColor = System.Drawing.SystemColors.ControlText;
			this.pg_global.LineColor = System.Drawing.SystemColors.ControlLight;
			this.pg_global.Location = new System.Drawing.Point(3, 3);
			this.pg_global.Name = "pg_global";
			this.pg_global.Size = new System.Drawing.Size(620, 570);
			this.pg_global.TabIndex = 7;
			this.pg_global.ToolbarVisible = false;
			// 
			// tab_control
			// 
			this.tab_control.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tab_control.Controls.Add(this.tap_project);
			this.tab_control.Controls.Add(this.tap_globaltype);
			this.tab_control.Controls.Add(this.tab_global);
			this.tab_control.Location = new System.Drawing.Point(12, 63);
			this.tab_control.Name = "tab_control";
			this.tab_control.SelectedIndex = 0;
			this.tab_control.Size = new System.Drawing.Size(634, 632);
			this.tab_control.TabIndex = 10;
			this.tab_control.SelectedIndexChanged += new System.EventHandler(this.tab_control_SelectedIndexChanged);
			// 
			// tap_project
			// 
			this.tap_project.Controls.Add(this.pg_project);
			this.tap_project.Location = new System.Drawing.Point(4, 22);
			this.tap_project.Name = "tap_project";
			this.tap_project.Padding = new System.Windows.Forms.Padding(3);
			this.tap_project.Size = new System.Drawing.Size(626, 606);
			this.tap_project.TabIndex = 0;
			this.tap_project.Text = "프로젝트";
			this.tap_project.UseVisualStyleBackColor = true;
			// 
			// pg_project
			// 
			this.pg_project.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pg_project.BackColor = System.Drawing.SystemColors.Control;
			this.pg_project.CommandsForeColor = System.Drawing.SystemColors.ControlText;
			this.pg_project.LineColor = System.Drawing.SystemColors.ControlLight;
			this.pg_project.Location = new System.Drawing.Point(6, 6);
			this.pg_project.Name = "pg_project";
			this.pg_project.Size = new System.Drawing.Size(614, 594);
			this.pg_project.TabIndex = 8;
			this.pg_project.ToolbarVisible = false;
			// 
			// tap_globaltype
			// 
			this.tap_globaltype.Controls.Add(this.pg_project_globaltype);
			this.tap_globaltype.Location = new System.Drawing.Point(4, 22);
			this.tap_globaltype.Name = "tap_globaltype";
			this.tap_globaltype.Padding = new System.Windows.Forms.Padding(3);
			this.tap_globaltype.Size = new System.Drawing.Size(626, 606);
			this.tap_globaltype.TabIndex = 1;
			this.tap_globaltype.Text = "글로벌타입";
			this.tap_globaltype.UseVisualStyleBackColor = true;
			// 
			// tab_global
			// 
			this.tab_global.Controls.Add(this.pg_global);
			this.tab_global.Location = new System.Drawing.Point(4, 22);
			this.tab_global.Name = "tab_global";
			this.tab_global.Size = new System.Drawing.Size(626, 606);
			this.tab_global.TabIndex = 2;
			this.tab_global.Text = "모든툴공용";
			this.tab_global.UseVisualStyleBackColor = true;
			// 
			// FormProjectProperty
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(654, 707);
			this.Controls.Add(this.tab_control);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btn_apply);
			this.Controls.Add(this.cb_globaltype);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cb_projectselect);
			this.Controls.Add(this.label1);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(670, 699);
			this.Name = "FormProjectProperty";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Project";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormProjectConfig_FormClosed);
			this.tab_control.ResumeLayout(false);
			this.tap_project.ResumeLayout(false);
			this.tap_globaltype.ResumeLayout(false);
			this.tab_global.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PropertyGrid pg_project_globaltype;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cb_projectselect;
		private System.Windows.Forms.ComboBox cb_globaltype;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btn_apply;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PropertyGrid pg_global;
		private System.Windows.Forms.TabControl tab_control;
		private System.Windows.Forms.TabPage tap_project;
		private System.Windows.Forms.TabPage tap_globaltype;
		private System.Windows.Forms.TabPage tab_global;
		private System.Windows.Forms.PropertyGrid pg_project;
	}
}