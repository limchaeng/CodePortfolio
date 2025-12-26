namespace UMTools.Distribution
{
	partial class FormAppleUploader
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
			this.btn_altools_list_ipa = new System.Windows.Forms.Button();
			this.rtb_altools_log = new System.Windows.Forms.RichTextBox();
			this.lv_altools_ipalist = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btn_altools_close = new System.Windows.Forms.Button();
			this.btn_altools_upload_ipa = new System.Windows.Forms.Button();
			this.btn_altools_verify_ipa = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.tb_work_path = new System.Windows.Forms.TextBox();
			this.btn_clear_log = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tb_apple_id = new System.Windows.Forms.TextBox();
			this.tb_apple_pw = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btn_altools_list_ipa
			// 
			this.btn_altools_list_ipa.Location = new System.Drawing.Point(12, 34);
			this.btn_altools_list_ipa.Name = "btn_altools_list_ipa";
			this.btn_altools_list_ipa.Size = new System.Drawing.Size(234, 35);
			this.btn_altools_list_ipa.TabIndex = 0;
			this.btn_altools_list_ipa.Text = "Connect && Reload IPA List";
			this.btn_altools_list_ipa.UseVisualStyleBackColor = true;
			this.btn_altools_list_ipa.Click += new System.EventHandler(this.btn_altools_list_ipa_Click);
			// 
			// rtb_altools_log
			// 
			this.rtb_altools_log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_altools_log.Location = new System.Drawing.Point(12, 493);
			this.rtb_altools_log.Name = "rtb_altools_log";
			this.rtb_altools_log.ReadOnly = true;
			this.rtb_altools_log.Size = new System.Drawing.Size(928, 112);
			this.rtb_altools_log.TabIndex = 1;
			this.rtb_altools_log.Text = "";
			// 
			// lv_altools_ipalist
			// 
			this.lv_altools_ipalist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lv_altools_ipalist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.lv_altools_ipalist.FullRowSelect = true;
			this.lv_altools_ipalist.GridLines = true;
			this.lv_altools_ipalist.Location = new System.Drawing.Point(12, 75);
			this.lv_altools_ipalist.MultiSelect = false;
			this.lv_altools_ipalist.Name = "lv_altools_ipalist";
			this.lv_altools_ipalist.Size = new System.Drawing.Size(713, 412);
			this.lv_altools_ipalist.TabIndex = 2;
			this.lv_altools_ipalist.UseCompatibleStateImageBehavior = false;
			this.lv_altools_ipalist.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "IPA";
			this.columnHeader1.Width = 689;
			// 
			// btn_altools_close
			// 
			this.btn_altools_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_altools_close.Location = new System.Drawing.Point(737, 12);
			this.btn_altools_close.Name = "btn_altools_close";
			this.btn_altools_close.Size = new System.Drawing.Size(203, 57);
			this.btn_altools_close.TabIndex = 3;
			this.btn_altools_close.Text = "Close";
			this.btn_altools_close.UseVisualStyleBackColor = true;
			this.btn_altools_close.Click += new System.EventHandler(this.btn_altools_close_Click);
			// 
			// btn_altools_upload_ipa
			// 
			this.btn_altools_upload_ipa.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_altools_upload_ipa.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.btn_altools_upload_ipa.Location = new System.Drawing.Point(737, 75);
			this.btn_altools_upload_ipa.Name = "btn_altools_upload_ipa";
			this.btn_altools_upload_ipa.Size = new System.Drawing.Size(203, 57);
			this.btn_altools_upload_ipa.TabIndex = 4;
			this.btn_altools_upload_ipa.Text = "Upload IPA to AppleStore";
			this.btn_altools_upload_ipa.UseVisualStyleBackColor = false;
			this.btn_altools_upload_ipa.Click += new System.EventHandler(this.btn_altools_upload_ipa_Click);
			// 
			// btn_altools_verify_ipa
			// 
			this.btn_altools_verify_ipa.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_altools_verify_ipa.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
			this.btn_altools_verify_ipa.Location = new System.Drawing.Point(737, 138);
			this.btn_altools_verify_ipa.Name = "btn_altools_verify_ipa";
			this.btn_altools_verify_ipa.Size = new System.Drawing.Size(203, 57);
			this.btn_altools_verify_ipa.TabIndex = 5;
			this.btn_altools_verify_ipa.Text = "Verify IPA";
			this.btn_altools_verify_ipa.UseVisualStyleBackColor = false;
			this.btn_altools_verify_ipa.Click += new System.EventHandler(this.btn_altools_verify_ipa_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 12);
			this.label1.TabIndex = 6;
			this.label1.Text = "WorkPath";
			// 
			// tb_work_path
			// 
			this.tb_work_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_work_path.Location = new System.Drawing.Point(75, 9);
			this.tb_work_path.Name = "tb_work_path";
			this.tb_work_path.ReadOnly = true;
			this.tb_work_path.Size = new System.Drawing.Size(650, 21);
			this.tb_work_path.TabIndex = 7;
			// 
			// btn_clear_log
			// 
			this.btn_clear_log.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_clear_log.Location = new System.Drawing.Point(737, 449);
			this.btn_clear_log.Name = "btn_clear_log";
			this.btn_clear_log.Size = new System.Drawing.Size(203, 38);
			this.btn_clear_log.TabIndex = 8;
			this.btn_clear_log.Text = "Clear LOG";
			this.btn_clear_log.UseVisualStyleBackColor = true;
			this.btn_clear_log.Click += new System.EventHandler(this.btn_clear_log_Click);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(735, 198);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(52, 12);
			this.label2.TabIndex = 9;
			this.label2.Text = "Apple ID";
			// 
			// tb_apple_id
			// 
			this.tb_apple_id.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_apple_id.Location = new System.Drawing.Point(737, 213);
			this.tb_apple_id.Name = "tb_apple_id";
			this.tb_apple_id.Size = new System.Drawing.Size(203, 21);
			this.tb_apple_id.TabIndex = 10;
			// 
			// tb_apple_pw
			// 
			this.tb_apple_pw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_apple_pw.Location = new System.Drawing.Point(737, 256);
			this.tb_apple_pw.Name = "tb_apple_pw";
			this.tb_apple_pw.Size = new System.Drawing.Size(203, 21);
			this.tb_apple_pw.TabIndex = 12;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(735, 241);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(98, 12);
			this.label3.TabIndex = 11;
			this.label3.Text = "Apple Password";
			// 
			// FormAppleUploader
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(952, 617);
			this.Controls.Add(this.tb_apple_pw);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tb_apple_id);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btn_clear_log);
			this.Controls.Add(this.tb_work_path);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btn_altools_verify_ipa);
			this.Controls.Add(this.btn_altools_upload_ipa);
			this.Controls.Add(this.btn_altools_close);
			this.Controls.Add(this.lv_altools_ipalist);
			this.Controls.Add(this.rtb_altools_log);
			this.Controls.Add(this.btn_altools_list_ipa);
			this.MinimumSize = new System.Drawing.Size(968, 656);
			this.Name = "FormAppleUploader";
			this.Text = "Apple Application Loader Tools";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btn_altools_list_ipa;
		private System.Windows.Forms.RichTextBox rtb_altools_log;
		private System.Windows.Forms.ListView lv_altools_ipalist;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Button btn_altools_close;
		private System.Windows.Forms.Button btn_altools_upload_ipa;
		private System.Windows.Forms.Button btn_altools_verify_ipa;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tb_work_path;
		private System.Windows.Forms.Button btn_clear_log;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tb_apple_id;
		private System.Windows.Forms.TextBox tb_apple_pw;
		private System.Windows.Forms.Label label3;
	}
}