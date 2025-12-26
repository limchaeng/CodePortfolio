namespace UMTools.Distribution
{
	partial class FormExportPopup
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
			this.components = new System.ComponentModel.Container();
			this.tb_export_texts = new System.Windows.Forms.TextBox();
			this.btn_close = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.cb_export_options = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btn_all_select = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// tb_export_texts
			// 
			this.tb_export_texts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_export_texts.HideSelection = false;
			this.tb_export_texts.Location = new System.Drawing.Point(12, 12);
			this.tb_export_texts.Multiline = true;
			this.tb_export_texts.Name = "tb_export_texts";
			this.tb_export_texts.ReadOnly = true;
			this.tb_export_texts.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tb_export_texts.Size = new System.Drawing.Size(849, 591);
			this.tb_export_texts.TabIndex = 0;
			// 
			// btn_close
			// 
			this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_close.Location = new System.Drawing.Point(676, 609);
			this.btn_close.Name = "btn_close";
			this.btn_close.Size = new System.Drawing.Size(185, 50);
			this.btn_close.TabIndex = 1;
			this.btn_close.Text = "CLOSE";
			this.btn_close.UseVisualStyleBackColor = true;
			this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// cb_export_options
			// 
			this.cb_export_options.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cb_export_options.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cb_export_options.FormattingEnabled = true;
			this.cb_export_options.Location = new System.Drawing.Point(12, 630);
			this.cb_export_options.Name = "cb_export_options";
			this.cb_export_options.Size = new System.Drawing.Size(236, 20);
			this.cb_export_options.TabIndex = 2;
			this.cb_export_options.SelectedIndexChanged += new System.EventHandler(this.cb_export_options_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 610);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 12);
			this.label1.TabIndex = 3;
			this.label1.Text = "Option Select";
			// 
			// btn_all_select
			// 
			this.btn_all_select.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_all_select.Location = new System.Drawing.Point(528, 610);
			this.btn_all_select.Name = "btn_all_select";
			this.btn_all_select.Size = new System.Drawing.Size(142, 49);
			this.btn_all_select.TabIndex = 4;
			this.btn_all_select.Text = "ALL SELECT";
			this.btn_all_select.UseVisualStyleBackColor = true;
			this.btn_all_select.Click += new System.EventHandler(this.btn_all_select_Click);
			// 
			// FormExportPopup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(873, 671);
			this.Controls.Add(this.btn_all_select);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cb_export_options);
			this.Controls.Add(this.btn_close);
			this.Controls.Add(this.tb_export_texts);
			this.Name = "FormExportPopup";
			this.Text = "Export ";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormExportPopup_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tb_export_texts;
		private System.Windows.Forms.Button btn_close;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ComboBox cb_export_options;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btn_all_select;
	}
}