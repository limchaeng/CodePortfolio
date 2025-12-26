namespace UMTools.TBLExport
{
	partial class FormView
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
			this.btn_close = new System.Windows.Forms.Button();
			this.btn_all_select = new System.Windows.Forms.Button();
			this.rtb_textview = new System.Windows.Forms.RichTextBox();
			this.cb_wordwrap = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// btn_close
			// 
			this.btn_close.Location = new System.Drawing.Point(745, 586);
			this.btn_close.Name = "btn_close";
			this.btn_close.Size = new System.Drawing.Size(200, 49);
			this.btn_close.TabIndex = 0;
			this.btn_close.Text = "CLOSE";
			this.btn_close.UseVisualStyleBackColor = true;
			this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
			// 
			// btn_all_select
			// 
			this.btn_all_select.Location = new System.Drawing.Point(539, 586);
			this.btn_all_select.Name = "btn_all_select";
			this.btn_all_select.Size = new System.Drawing.Size(200, 49);
			this.btn_all_select.TabIndex = 2;
			this.btn_all_select.Text = "ALL SELECT";
			this.btn_all_select.UseVisualStyleBackColor = true;
			this.btn_all_select.Click += new System.EventHandler(this.btn_all_select_Click);
			// 
			// rtb_textview
			// 
			this.rtb_textview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_textview.Location = new System.Drawing.Point(12, 12);
			this.rtb_textview.Name = "rtb_textview";
			this.rtb_textview.ReadOnly = true;
			this.rtb_textview.Size = new System.Drawing.Size(933, 568);
			this.rtb_textview.TabIndex = 3;
			this.rtb_textview.Text = "";
			this.rtb_textview.WordWrap = false;
			// 
			// cb_wordwrap
			// 
			this.cb_wordwrap.AutoSize = true;
			this.cb_wordwrap.Location = new System.Drawing.Point(12, 586);
			this.cb_wordwrap.Name = "cb_wordwrap";
			this.cb_wordwrap.Size = new System.Drawing.Size(84, 16);
			this.cb_wordwrap.TabIndex = 4;
			this.cb_wordwrap.Text = "Word Wrap";
			this.cb_wordwrap.UseVisualStyleBackColor = true;
			this.cb_wordwrap.CheckedChanged += new System.EventHandler(this.cb_wordwrap_CheckedChanged);
			// 
			// FormView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(957, 646);
			this.Controls.Add(this.cb_wordwrap);
			this.Controls.Add(this.rtb_textview);
			this.Controls.Add(this.btn_all_select);
			this.Controls.Add(this.btn_close);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormView";
			this.Text = "View";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btn_close;
		private System.Windows.Forms.Button btn_all_select;
		private System.Windows.Forms.RichTextBox rtb_textview;
		private System.Windows.Forms.CheckBox cb_wordwrap;
	}
}