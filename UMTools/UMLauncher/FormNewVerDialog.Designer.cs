namespace UMTools.UMLauncher
{
	partial class FormNewVerDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewVerDialog));
			this.rtb_newver_dialog_changedlog = new System.Windows.Forms.RichTextBox();
			this.btn_newver_dialog_install = new System.Windows.Forms.Button();
			this.btn_newver_dialog_cancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.tb_newver_dialog_info = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// rtb_newver_dialog_changedlog
			// 
			this.rtb_newver_dialog_changedlog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_newver_dialog_changedlog.Location = new System.Drawing.Point(14, 105);
			this.rtb_newver_dialog_changedlog.Name = "rtb_newver_dialog_changedlog";
			this.rtb_newver_dialog_changedlog.ReadOnly = true;
			this.rtb_newver_dialog_changedlog.Size = new System.Drawing.Size(570, 337);
			this.rtb_newver_dialog_changedlog.TabIndex = 10;
			this.rtb_newver_dialog_changedlog.Text = "";
			// 
			// btn_newver_dialog_install
			// 
			this.btn_newver_dialog_install.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_newver_dialog_install.Location = new System.Drawing.Point(349, 460);
			this.btn_newver_dialog_install.Name = "btn_newver_dialog_install";
			this.btn_newver_dialog_install.Size = new System.Drawing.Size(235, 44);
			this.btn_newver_dialog_install.TabIndex = 9;
			this.btn_newver_dialog_install.Text = "다운로드 및 설치";
			this.btn_newver_dialog_install.UseVisualStyleBackColor = true;
			this.btn_newver_dialog_install.Click += new System.EventHandler(this.btn_newver_dialog_install_Click);
			// 
			// btn_newver_dialog_cancel
			// 
			this.btn_newver_dialog_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_newver_dialog_cancel.Location = new System.Drawing.Point(119, 460);
			this.btn_newver_dialog_cancel.Name = "btn_newver_dialog_cancel";
			this.btn_newver_dialog_cancel.Size = new System.Drawing.Size(224, 44);
			this.btn_newver_dialog_cancel.TabIndex = 8;
			this.btn_newver_dialog_cancel.Text = "취 소";
			this.btn_newver_dialog_cancel.UseVisualStyleBackColor = true;
			this.btn_newver_dialog_cancel.Click += new System.EventHandler(this.btn_newver_dialog_cancel_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 85);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 12);
			this.label1.TabIndex = 7;
			this.label1.Text = "업데이트 내용";
			// 
			// tb_newver_dialog_info
			// 
			this.tb_newver_dialog_info.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_newver_dialog_info.Location = new System.Drawing.Point(14, 12);
			this.tb_newver_dialog_info.Multiline = true;
			this.tb_newver_dialog_info.Name = "tb_newver_dialog_info";
			this.tb_newver_dialog_info.ReadOnly = true;
			this.tb_newver_dialog_info.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tb_newver_dialog_info.Size = new System.Drawing.Size(570, 65);
			this.tb_newver_dialog_info.TabIndex = 6;
			// 
			// FormNewVerDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(596, 516);
			this.Controls.Add(this.rtb_newver_dialog_changedlog);
			this.Controls.Add(this.btn_newver_dialog_install);
			this.Controls.Add(this.btn_newver_dialog_cancel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tb_newver_dialog_info);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormNewVerDialog";
			this.Text = "새버전";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox rtb_newver_dialog_changedlog;
		private System.Windows.Forms.Button btn_newver_dialog_install;
		private System.Windows.Forms.Button btn_newver_dialog_cancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tb_newver_dialog_info;
	}
}