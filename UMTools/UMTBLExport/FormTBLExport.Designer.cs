namespace UMTools.TBLExport
{
	partial class FormTBLExport
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다.
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTBLExport));
			this.lv_filelist = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.Export = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.Version = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btn_Refresh = new System.Windows.Forms.Button();
			this.btn_export = new System.Windows.Forms.Button();
			this.progress_export = new System.Windows.Forms.ProgressBar();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rtb_log = new System.Windows.Forms.RichTextBox();
			this.lv_log = new System.Windows.Forms.ListView();
			this.ch_log = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btn_logclear = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btn_devFieldExport = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.lbl_basepath = new System.Windows.Forms.Label();
			this.btn_close = new System.Windows.Forms.Button();
			this.btn_checkEncrypt = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.cb_text_summary_detail = new System.Windows.Forms.CheckBox();
			this.btn_i18ntext_summary = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.clb_supportLanguage = new System.Windows.Forms.CheckedListBox();
			this.btn_localizeOnlyExport = new System.Windows.Forms.Button();
			this.btn_project_select = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.lbl_projectname = new System.Windows.Forms.Label();
			this.tb_pathtext = new System.Windows.Forms.TextBox();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tp_export = new System.Windows.Forms.TabPage();
			this.tp_i18ntext = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.cb_i18ntext_duplicate_include = new System.Windows.Forms.CheckBox();
			this.grp_i18ntext_export_type = new System.Windows.Forms.GroupBox();
			this.radio_i18n_xml2excel = new System.Windows.Forms.RadioButton();
			this.radio_i18n_excel2xml = new System.Windows.Forms.RadioButton();
			this.radio_i18n_merge = new System.Windows.Forms.RadioButton();
			this.btn_i18n_help = new System.Windows.Forms.Button();
			this.btn_i18n_export = new System.Windows.Forms.Button();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tp_export.SuspendLayout();
			this.tp_i18ntext.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.grp_i18ntext_export_type.SuspendLayout();
			this.SuspendLayout();
			// 
			// lv_filelist
			// 
			this.lv_filelist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lv_filelist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.Export,
            this.Version});
			this.lv_filelist.FullRowSelect = true;
			this.lv_filelist.GridLines = true;
			this.lv_filelist.HideSelection = false;
			this.lv_filelist.Location = new System.Drawing.Point(6, 23);
			this.lv_filelist.MultiSelect = false;
			this.lv_filelist.Name = "lv_filelist";
			this.lv_filelist.ShowItemToolTips = true;
			this.lv_filelist.Size = new System.Drawing.Size(523, 285);
			this.lv_filelist.TabIndex = 0;
			this.lv_filelist.UseCompatibleStateImageBehavior = false;
			this.lv_filelist.View = System.Windows.Forms.View.Details;
			this.lv_filelist.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lv_filelist_KeyDown);
			this.lv_filelist.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lv_filelist_MouseClick);
			this.lv_filelist.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lv_filelist_MouseDoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "File Name";
			this.columnHeader1.Width = 325;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "status";
			this.columnHeader2.Width = 73;
			// 
			// Export
			// 
			this.Export.Text = "Export";
			// 
			// Version
			// 
			this.Version.Text = "Version";
			// 
			// btn_Refresh
			// 
			this.btn_Refresh.Location = new System.Drawing.Point(416, 40);
			this.btn_Refresh.Name = "btn_Refresh";
			this.btn_Refresh.Size = new System.Drawing.Size(140, 33);
			this.btn_Refresh.TabIndex = 1;
			this.btn_Refresh.Text = "Refresh";
			this.btn_Refresh.UseVisualStyleBackColor = true;
			this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
			// 
			// btn_export
			// 
			this.btn_export.BackColor = System.Drawing.Color.Green;
			this.btn_export.ForeColor = System.Drawing.Color.White;
			this.btn_export.Location = new System.Drawing.Point(6, 137);
			this.btn_export.Name = "btn_export";
			this.btn_export.Size = new System.Drawing.Size(509, 48);
			this.btn_export.TabIndex = 2;
			this.btn_export.Text = "TBL EXPORT ( ALL )";
			this.btn_export.UseVisualStyleBackColor = false;
			this.btn_export.Click += new System.EventHandler(this.btn_export_Click);
			// 
			// progress_export
			// 
			this.progress_export.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progress_export.Location = new System.Drawing.Point(116, 20);
			this.progress_export.Name = "progress_export";
			this.progress_export.Size = new System.Drawing.Size(318, 23);
			this.progress_export.TabIndex = 8;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.rtb_log);
			this.groupBox1.Controls.Add(this.lv_log);
			this.groupBox1.Controls.Add(this.btn_logclear);
			this.groupBox1.Controls.Add(this.progress_export);
			this.groupBox1.Location = new System.Drawing.Point(553, 127);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 554);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Export Information";
			// 
			// rtb_log
			// 
			this.rtb_log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_log.Location = new System.Drawing.Point(10, 375);
			this.rtb_log.Name = "rtb_log";
			this.rtb_log.ReadOnly = true;
			this.rtb_log.Size = new System.Drawing.Size(424, 161);
			this.rtb_log.TabIndex = 12;
			this.rtb_log.Text = "";
			// 
			// lv_log
			// 
			this.lv_log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lv_log.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ch_log});
			this.lv_log.FullRowSelect = true;
			this.lv_log.GridLines = true;
			this.lv_log.HideSelection = false;
			this.lv_log.Location = new System.Drawing.Point(10, 51);
			this.lv_log.Name = "lv_log";
			this.lv_log.Size = new System.Drawing.Size(428, 318);
			this.lv_log.TabIndex = 11;
			this.lv_log.UseCompatibleStateImageBehavior = false;
			this.lv_log.View = System.Windows.Forms.View.Details;
			this.lv_log.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lv_log_KeyUp);
			this.lv_log.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lv_log_MouseDoubleClick);
			// 
			// ch_log
			// 
			this.ch_log.Text = "LOG";
			this.ch_log.Width = 542;
			// 
			// btn_logclear
			// 
			this.btn_logclear.Location = new System.Drawing.Point(6, 20);
			this.btn_logclear.Name = "btn_logclear";
			this.btn_logclear.Size = new System.Drawing.Size(104, 23);
			this.btn_logclear.TabIndex = 10;
			this.btn_logclear.Text = "Log Clear";
			this.btn_logclear.UseVisualStyleBackColor = true;
			this.btn_logclear.Click += new System.EventHandler(this.btn_logclear_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 339);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(187, 12);
			this.label1.TabIndex = 10;
			this.label1.Text = "(One by one when DoubleClick)";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.lv_filelist);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Location = new System.Drawing.Point(12, 355);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(535, 326);
			this.groupBox2.TabIndex = 11;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "File List";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 308);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(173, 12);
			this.label2.TabIndex = 11;
			this.label2.Text = "(Export Single : Double Click)";
			// 
			// btn_devFieldExport
			// 
			this.btn_devFieldExport.Location = new System.Drawing.Point(6, 6);
			this.btn_devFieldExport.Name = "btn_devFieldExport";
			this.btn_devFieldExport.Size = new System.Drawing.Size(151, 38);
			this.btn_devFieldExport.TabIndex = 12;
			this.btn_devFieldExport.Text = "ExcelField To ToolParam Script ALL";
			this.btn_devFieldExport.UseVisualStyleBackColor = true;
			this.btn_devFieldExport.Click += new System.EventHandler(this.btn_devFieldExport_Click);
			// 
			// lbl_basepath
			// 
			this.lbl_basepath.AutoEllipsis = true;
			this.lbl_basepath.AutoSize = true;
			this.lbl_basepath.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.lbl_basepath.Location = new System.Drawing.Point(323, 12);
			this.lbl_basepath.Name = "lbl_basepath";
			this.lbl_basepath.Size = new System.Drawing.Size(87, 12);
			this.lbl_basepath.TabIndex = 7;
			this.lbl_basepath.Text = "TBL PATH : ";
			// 
			// btn_close
			// 
			this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_close.Location = new System.Drawing.Point(866, 36);
			this.btn_close.Name = "btn_close";
			this.btn_close.Size = new System.Drawing.Size(127, 69);
			this.btn_close.TabIndex = 5;
			this.btn_close.Text = "CLOSE";
			this.btn_close.UseVisualStyleBackColor = true;
			this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
			// 
			// btn_checkEncrypt
			// 
			this.btn_checkEncrypt.Location = new System.Drawing.Point(6, 50);
			this.btn_checkEncrypt.Name = "btn_checkEncrypt";
			this.btn_checkEncrypt.Size = new System.Drawing.Size(151, 23);
			this.btn_checkEncrypt.TabIndex = 20;
			this.btn_checkEncrypt.Text = "View Bin/Encrypt XML";
			this.btn_checkEncrypt.UseVisualStyleBackColor = true;
			this.btn_checkEncrypt.Click += new System.EventHandler(this.btn_checkEncrypt_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "openFileDialog";
			// 
			// cb_text_summary_detail
			// 
			this.cb_text_summary_detail.AutoSize = true;
			this.cb_text_summary_detail.Location = new System.Drawing.Point(269, 10);
			this.cb_text_summary_detail.Name = "cb_text_summary_detail";
			this.cb_text_summary_detail.Size = new System.Drawing.Size(84, 16);
			this.cb_text_summary_detail.TabIndex = 30;
			this.cb_text_summary_detail.Text = "디테일하게";
			this.cb_text_summary_detail.UseVisualStyleBackColor = true;
			// 
			// btn_i18ntext_summary
			// 
			this.btn_i18ntext_summary.Location = new System.Drawing.Point(152, 6);
			this.btn_i18ntext_summary.Name = "btn_i18ntext_summary";
			this.btn_i18ntext_summary.Size = new System.Drawing.Size(111, 23);
			this.btn_i18ntext_summary.TabIndex = 29;
			this.btn_i18ntext_summary.Text = "Text SUMMARY";
			this.btn_i18ntext_summary.UseVisualStyleBackColor = true;
			this.btn_i18ntext_summary.Click += new System.EventHandler(this.btn_i18ntext_summary_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(380, 4);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 12);
			this.label3.TabIndex = 28;
			this.label3.Text = "지원 언어";
			// 
			// clb_supportLanguage
			// 
			this.clb_supportLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clb_supportLanguage.CheckOnClick = true;
			this.clb_supportLanguage.FormattingEnabled = true;
			this.clb_supportLanguage.Location = new System.Drawing.Point(382, 21);
			this.clb_supportLanguage.Name = "clb_supportLanguage";
			this.clb_supportLanguage.Size = new System.Drawing.Size(130, 164);
			this.clb_supportLanguage.TabIndex = 1;
			this.clb_supportLanguage.ThreeDCheckBoxes = true;
			// 
			// btn_localizeOnlyExport
			// 
			this.btn_localizeOnlyExport.Location = new System.Drawing.Point(6, 79);
			this.btn_localizeOnlyExport.Name = "btn_localizeOnlyExport";
			this.btn_localizeOnlyExport.Size = new System.Drawing.Size(151, 23);
			this.btn_localizeOnlyExport.TabIndex = 27;
			this.btn_localizeOnlyExport.Text = "Text ONLY EXPORT";
			this.btn_localizeOnlyExport.UseVisualStyleBackColor = true;
			this.btn_localizeOnlyExport.Click += new System.EventHandler(this.btn_localizeOnlyExport_Click);
			// 
			// btn_project_select
			// 
			this.btn_project_select.BackColor = System.Drawing.Color.DarkRed;
			this.btn_project_select.ForeColor = System.Drawing.Color.White;
			this.btn_project_select.Location = new System.Drawing.Point(6, 66);
			this.btn_project_select.Name = "btn_project_select";
			this.btn_project_select.Size = new System.Drawing.Size(285, 45);
			this.btn_project_select.TabIndex = 29;
			this.btn_project_select.Text = "프로젝트 선택/설정";
			this.btn_project_select.UseVisualStyleBackColor = false;
			this.btn_project_select.Click += new System.EventHandler(this.btn_project_select_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.lbl_projectname);
			this.groupBox3.Controls.Add(this.btn_project_select);
			this.groupBox3.Location = new System.Drawing.Point(12, 9);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(297, 117);
			this.groupBox3.TabIndex = 30;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Project";
			// 
			// lbl_projectname
			// 
			this.lbl_projectname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbl_projectname.AutoEllipsis = true;
			this.lbl_projectname.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lbl_projectname.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lbl_projectname.Font = new System.Drawing.Font("굴림", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.lbl_projectname.Location = new System.Drawing.Point(8, 17);
			this.lbl_projectname.Name = "lbl_projectname";
			this.lbl_projectname.Size = new System.Drawing.Size(283, 40);
			this.lbl_projectname.TabIndex = 31;
			this.lbl_projectname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_pathtext
			// 
			this.tb_pathtext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_pathtext.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.tb_pathtext.Location = new System.Drawing.Point(416, 9);
			this.tb_pathtext.Name = "tb_pathtext";
			this.tb_pathtext.ReadOnly = true;
			this.tb_pathtext.Size = new System.Drawing.Size(575, 21);
			this.tb_pathtext.TabIndex = 31;
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tp_export);
			this.tabControl.Controls.Add(this.tp_i18ntext);
			this.tabControl.Location = new System.Drawing.Point(12, 132);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(529, 217);
			this.tabControl.TabIndex = 32;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// tp_export
			// 
			this.tp_export.Controls.Add(this.btn_export);
			this.tp_export.Controls.Add(this.btn_devFieldExport);
			this.tp_export.Controls.Add(this.btn_checkEncrypt);
			this.tp_export.Controls.Add(this.btn_localizeOnlyExport);
			this.tp_export.Location = new System.Drawing.Point(4, 22);
			this.tp_export.Name = "tp_export";
			this.tp_export.Padding = new System.Windows.Forms.Padding(3);
			this.tp_export.Size = new System.Drawing.Size(521, 191);
			this.tp_export.TabIndex = 0;
			this.tp_export.Text = "EXPORT";
			this.tp_export.UseVisualStyleBackColor = true;
			// 
			// tp_i18ntext
			// 
			this.tp_i18ntext.Controls.Add(this.groupBox4);
			this.tp_i18ntext.Controls.Add(this.grp_i18ntext_export_type);
			this.tp_i18ntext.Controls.Add(this.btn_i18n_help);
			this.tp_i18ntext.Controls.Add(this.btn_i18n_export);
			this.tp_i18ntext.Controls.Add(this.clb_supportLanguage);
			this.tp_i18ntext.Controls.Add(this.label3);
			this.tp_i18ntext.Controls.Add(this.cb_text_summary_detail);
			this.tp_i18ntext.Controls.Add(this.btn_i18ntext_summary);
			this.tp_i18ntext.Location = new System.Drawing.Point(4, 22);
			this.tp_i18ntext.Name = "tp_i18ntext";
			this.tp_i18ntext.Padding = new System.Windows.Forms.Padding(3);
			this.tp_i18ntext.Size = new System.Drawing.Size(521, 191);
			this.tp_i18ntext.TabIndex = 1;
			this.tp_i18ntext.Text = "I18N TEXT";
			this.tp_i18ntext.UseVisualStyleBackColor = true;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.cb_i18ntext_duplicate_include);
			this.groupBox4.Location = new System.Drawing.Point(153, 70);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(223, 115);
			this.groupBox4.TabIndex = 37;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Options";
			// 
			// cb_i18ntext_duplicate_include
			// 
			this.cb_i18ntext_duplicate_include.AutoSize = true;
			this.cb_i18ntext_duplicate_include.Location = new System.Drawing.Point(6, 20);
			this.cb_i18ntext_duplicate_include.Name = "cb_i18ntext_duplicate_include";
			this.cb_i18ntext_duplicate_include.Size = new System.Drawing.Size(108, 16);
			this.cb_i18ntext_duplicate_include.TabIndex = 0;
			this.cb_i18ntext_duplicate_include.Text = "중복텍스트포함";
			this.cb_i18ntext_duplicate_include.UseVisualStyleBackColor = true;
			this.cb_i18ntext_duplicate_include.CheckedChanged += new System.EventHandler(this.cb_i18ntext_duplicate_include_CheckedChanged);
			// 
			// grp_i18ntext_export_type
			// 
			this.grp_i18ntext_export_type.Controls.Add(this.radio_i18n_xml2excel);
			this.grp_i18ntext_export_type.Controls.Add(this.radio_i18n_excel2xml);
			this.grp_i18ntext_export_type.Controls.Add(this.radio_i18n_merge);
			this.grp_i18ntext_export_type.Location = new System.Drawing.Point(14, 6);
			this.grp_i18ntext_export_type.Name = "grp_i18ntext_export_type";
			this.grp_i18ntext_export_type.Size = new System.Drawing.Size(132, 125);
			this.grp_i18ntext_export_type.TabIndex = 36;
			this.grp_i18ntext_export_type.TabStop = false;
			this.grp_i18ntext_export_type.Text = "Export Type";
			// 
			// radio_i18n_xml2excel
			// 
			this.radio_i18n_xml2excel.AutoSize = true;
			this.radio_i18n_xml2excel.Location = new System.Drawing.Point(6, 20);
			this.radio_i18n_xml2excel.Name = "radio_i18n_xml2excel";
			this.radio_i18n_xml2excel.Size = new System.Drawing.Size(111, 16);
			this.radio_i18n_xml2excel.TabIndex = 31;
			this.radio_i18n_xml2excel.TabStop = true;
			this.radio_i18n_xml2excel.Text = "XML -> EXCEL";
			this.radio_i18n_xml2excel.UseVisualStyleBackColor = true;
			this.radio_i18n_xml2excel.CheckedChanged += new System.EventHandler(this.radio_i18n_xml2excel_CheckedChanged);
			this.radio_i18n_xml2excel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.radio_i18n_xml2excel_MouseClick);
			// 
			// radio_i18n_excel2xml
			// 
			this.radio_i18n_excel2xml.AutoSize = true;
			this.radio_i18n_excel2xml.Location = new System.Drawing.Point(6, 42);
			this.radio_i18n_excel2xml.Name = "radio_i18n_excel2xml";
			this.radio_i18n_excel2xml.Size = new System.Drawing.Size(111, 16);
			this.radio_i18n_excel2xml.TabIndex = 32;
			this.radio_i18n_excel2xml.TabStop = true;
			this.radio_i18n_excel2xml.Text = "EXCEL -> XML";
			this.radio_i18n_excel2xml.UseVisualStyleBackColor = true;
			this.radio_i18n_excel2xml.CheckedChanged += new System.EventHandler(this.radio_i18n_excel2xml_CheckedChanged);
			this.radio_i18n_excel2xml.MouseClick += new System.Windows.Forms.MouseEventHandler(this.radio_i18n_excel2xml_MouseClick);
			// 
			// radio_i18n_merge
			// 
			this.radio_i18n_merge.AutoSize = true;
			this.radio_i18n_merge.Location = new System.Drawing.Point(6, 64);
			this.radio_i18n_merge.Name = "radio_i18n_merge";
			this.radio_i18n_merge.Size = new System.Drawing.Size(67, 16);
			this.radio_i18n_merge.TabIndex = 33;
			this.radio_i18n_merge.TabStop = true;
			this.radio_i18n_merge.Text = "MERGE";
			this.radio_i18n_merge.UseVisualStyleBackColor = true;
			this.radio_i18n_merge.CheckedChanged += new System.EventHandler(this.radio_i18n_merge_CheckedChanged);
			this.radio_i18n_merge.MouseClick += new System.Windows.Forms.MouseEventHandler(this.radio_i18n_merge_MouseClick);
			// 
			// btn_i18n_help
			// 
			this.btn_i18n_help.Location = new System.Drawing.Point(152, 35);
			this.btn_i18n_help.Name = "btn_i18n_help";
			this.btn_i18n_help.Size = new System.Drawing.Size(111, 23);
			this.btn_i18n_help.TabIndex = 35;
			this.btn_i18n_help.Text = "번역 HELP";
			this.btn_i18n_help.UseVisualStyleBackColor = true;
			this.btn_i18n_help.Click += new System.EventHandler(this.btn_i18n_help_Click);
			// 
			// btn_i18n_export
			// 
			this.btn_i18n_export.BackColor = System.Drawing.Color.DarkCyan;
			this.btn_i18n_export.ForeColor = System.Drawing.Color.White;
			this.btn_i18n_export.Location = new System.Drawing.Point(14, 137);
			this.btn_i18n_export.Name = "btn_i18n_export";
			this.btn_i18n_export.Size = new System.Drawing.Size(132, 48);
			this.btn_i18n_export.TabIndex = 34;
			this.btn_i18n_export.Text = "TEXT EXPORT (ALL)";
			this.btn_i18n_export.UseVisualStyleBackColor = false;
			this.btn_i18n_export.Click += new System.EventHandler(this.btn_i18n_export_Click);
			// 
			// FormTBLExport
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1005, 693);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.tb_pathtext);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.lbl_basepath);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btn_close);
			this.Controls.Add(this.btn_Refresh);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(970, 721);
			this.Name = "FormTBLExport";
			this.Text = "TBL Export";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tp_export.ResumeLayout(false);
			this.tp_i18ntext.ResumeLayout(false);
			this.tp_i18ntext.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.grp_i18ntext_export_type.ResumeLayout(false);
			this.grp_i18ntext_export_type.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView lv_filelist;
		private System.Windows.Forms.Button btn_Refresh;
		private System.Windows.Forms.Button btn_export;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ProgressBar progress_export;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btn_logclear;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ColumnHeader Export;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btn_devFieldExport;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label lbl_basepath;
		private System.Windows.Forms.Button btn_close;
		private System.Windows.Forms.ListView lv_log;
		private System.Windows.Forms.ColumnHeader ch_log;
		private System.Windows.Forms.Button btn_checkEncrypt;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ColumnHeader Version;
		private System.Windows.Forms.Button btn_localizeOnlyExport;
		private System.Windows.Forms.CheckedListBox clb_supportLanguage;
		private System.Windows.Forms.RichTextBox rtb_log;
		private System.Windows.Forms.Button btn_project_select;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tb_pathtext;
		private System.Windows.Forms.Label lbl_projectname;
		private System.Windows.Forms.Button btn_i18ntext_summary;
		private System.Windows.Forms.CheckBox cb_text_summary_detail;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tp_export;
		private System.Windows.Forms.TabPage tp_i18ntext;
		private System.Windows.Forms.RadioButton radio_i18n_excel2xml;
		private System.Windows.Forms.RadioButton radio_i18n_xml2excel;
		private System.Windows.Forms.RadioButton radio_i18n_merge;
		private System.Windows.Forms.Button btn_i18n_help;
		private System.Windows.Forms.Button btn_i18n_export;
		private System.Windows.Forms.GroupBox grp_i18ntext_export_type;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox cb_i18ntext_duplicate_include;
	}
}

