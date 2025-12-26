namespace UMTools.Distribution
{
	partial class FormDistribution
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDistribution));
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tp_dev = new System.Windows.Forms.TabPage();
			this.btn_i18ntext_m_compare_client = new System.Windows.Forms.Button();
			this.btn_i18ntext_t_compare_client = new System.Windows.Forms.Button();
			this.btn_artsvncommit = new System.Windows.Forms.Button();
			this.btn_artsvnupdate = new System.Windows.Forms.Button();
			this.btn_docsvncommit = new System.Windows.Forms.Button();
			this.btn_docsvnupdate = new System.Windows.Forms.Button();
			this.btn_trunksvncommt = new System.Windows.Forms.Button();
			this.btn_trunksvnupdate = new System.Windows.Forms.Button();
			this.btn_devsvncommit = new System.Windows.Forms.Button();
			this.btn_config_compare_all = new System.Windows.Forms.Button();
			this.btn_devsvnupdate = new System.Windows.Forms.Button();
			this.btn_dev_update_dev_patch = new System.Windows.Forms.Button();
			this.btn_tbl_svn_commit = new System.Windows.Forms.Button();
			this.lbl_tbl_bin_copy_count = new System.Windows.Forms.Label();
			this.pgbar_tbl_bin_copy = new System.Windows.Forms.ProgressBar();
			this.btn_tbl_localizeall_compare_all = new System.Windows.Forms.Button();
			this.btn_tbl_data_compare_all = new System.Windows.Forms.Button();
			this.btn_dev_svc_server_update = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btn_dev_svc_server_commit = new System.Windows.Forms.Button();
			this.btn_tbl_svn_update = new System.Windows.Forms.Button();
			this.btn_tbl_bin_copy = new System.Windows.Forms.Button();
			this.tp_qa = new System.Windows.Forms.TabPage();
			this.btn_qa_updatelive = new System.Windows.Forms.Button();
			this.btn_qa_updaterv = new System.Windows.Forms.Button();
			this.btn_qa_updateqa2 = new System.Windows.Forms.Button();
			this.btn_qa_updateqa = new System.Windows.Forms.Button();
			this.btn_qa_update_tag_dev = new System.Windows.Forms.Button();
			this.btn_qa_commit_tag_dev = new System.Windows.Forms.Button();
			this.btn_qa_merge_tag_dev = new System.Windows.Forms.Button();
			this.btn_qa_commitqa2_patch = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.btn_qa_commitqa2 = new System.Windows.Forms.Button();
			this.btn_qa_switch_all = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.tb_qa_tagcreate_ver = new System.Windows.Forms.TextBox();
			this.btn_qa_tagcreate = new System.Windows.Forms.Button();
			this.btn_apple_ios_qa_upload = new System.Windows.Forms.Button();
			this.btn_qa_commitlive_patch = new System.Windows.Forms.Button();
			this.btn_qa_commitqa_patch = new System.Windows.Forms.Button();
			this.tb_qa_commitlive_version = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btn_qa_commitlive = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.btn_qa_commitrv = new System.Windows.Forms.Button();
			this.btn_qa_commitqa = new System.Windows.Forms.Button();
			this.lbl_qa_tag_label = new System.Windows.Forms.Label();
			this.btn_open_jenkins = new System.Windows.Forms.Button();
			this.rtb_log = new System.Windows.Forms.RichTextBox();
			this.btn_close = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.btn_client_work_commit = new System.Windows.Forms.Button();
			this.btn_server_work_commit = new System.Windows.Forms.Button();
			this.btn_server_work_update = new System.Windows.Forms.Button();
			this.btn_client_work_update = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btn_svnlog_tag_get = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.btn_svnlog_get = new System.Windows.Forms.Button();
			this.tb_svnlog_get_revision_end = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.tb_svnlog_get_revision_start = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.btn_dev_work_commit = new System.Windows.Forms.Button();
			this.btn_dev_work_update = new System.Windows.Forms.Button();
			this.btn_dev_tool_update = new System.Windows.Forms.Button();
			this.btn_dev_tool_commit = new System.Windows.Forms.Button();
			this.btn_dev_tool_distribution = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.btn_dev_umfunity_ext_commit = new System.Windows.Forms.Button();
			this.btn_dev_umf_distribution = new System.Windows.Forms.Button();
			this.btn_dev_umf_update = new System.Windows.Forms.Button();
			this.btn_dev_umf_commit = new System.Windows.Forms.Button();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.lbl_projectname = new System.Windows.Forms.Label();
			this.btn_project_select = new System.Windows.Forms.Button();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.tabControl.SuspendLayout();
			this.tp_dev.SuspendLayout();
			this.tp_qa.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tp_dev);
			this.tabControl.Controls.Add(this.tp_qa);
			this.tabControl.Location = new System.Drawing.Point(12, 123);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(768, 507);
			this.tabControl.TabIndex = 0;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// tp_dev
			// 
			this.tp_dev.Controls.Add(this.btn_i18ntext_m_compare_client);
			this.tp_dev.Controls.Add(this.btn_i18ntext_t_compare_client);
			this.tp_dev.Controls.Add(this.btn_artsvncommit);
			this.tp_dev.Controls.Add(this.btn_artsvnupdate);
			this.tp_dev.Controls.Add(this.btn_docsvncommit);
			this.tp_dev.Controls.Add(this.btn_docsvnupdate);
			this.tp_dev.Controls.Add(this.btn_trunksvncommt);
			this.tp_dev.Controls.Add(this.btn_trunksvnupdate);
			this.tp_dev.Controls.Add(this.btn_devsvncommit);
			this.tp_dev.Controls.Add(this.btn_config_compare_all);
			this.tp_dev.Controls.Add(this.btn_devsvnupdate);
			this.tp_dev.Controls.Add(this.btn_dev_update_dev_patch);
			this.tp_dev.Controls.Add(this.btn_tbl_svn_commit);
			this.tp_dev.Controls.Add(this.lbl_tbl_bin_copy_count);
			this.tp_dev.Controls.Add(this.pgbar_tbl_bin_copy);
			this.tp_dev.Controls.Add(this.btn_tbl_localizeall_compare_all);
			this.tp_dev.Controls.Add(this.btn_tbl_data_compare_all);
			this.tp_dev.Controls.Add(this.btn_dev_svc_server_update);
			this.tp_dev.Controls.Add(this.label5);
			this.tp_dev.Controls.Add(this.label4);
			this.tp_dev.Controls.Add(this.btn_dev_svc_server_commit);
			this.tp_dev.Controls.Add(this.btn_tbl_svn_update);
			this.tp_dev.Controls.Add(this.btn_tbl_bin_copy);
			this.tp_dev.Location = new System.Drawing.Point(4, 22);
			this.tp_dev.Name = "tp_dev";
			this.tp_dev.Padding = new System.Windows.Forms.Padding(3);
			this.tp_dev.Size = new System.Drawing.Size(760, 481);
			this.tp_dev.TabIndex = 1;
			this.tp_dev.Text = "DEV";
			this.tp_dev.UseVisualStyleBackColor = true;
			// 
			// btn_i18ntext_m_compare_client
			// 
			this.btn_i18ntext_m_compare_client.Location = new System.Drawing.Point(349, 158);
			this.btn_i18ntext_m_compare_client.Name = "btn_i18ntext_m_compare_client";
			this.btn_i18ntext_m_compare_client.Size = new System.Drawing.Size(142, 23);
			this.btn_i18ntext_m_compare_client.TabIndex = 29;
			this.btn_i18ntext_m_compare_client.Text = "클라번역비교(merged)";
			this.btn_i18ntext_m_compare_client.UseVisualStyleBackColor = true;
			this.btn_i18ntext_m_compare_client.Click += new System.EventHandler(this.btn_i18ntext_m_compare_client_Click);
			// 
			// btn_i18ntext_t_compare_client
			// 
			this.btn_i18ntext_t_compare_client.Location = new System.Drawing.Point(211, 158);
			this.btn_i18ntext_t_compare_client.Name = "btn_i18ntext_t_compare_client";
			this.btn_i18ntext_t_compare_client.Size = new System.Drawing.Size(132, 23);
			this.btn_i18ntext_t_compare_client.TabIndex = 28;
			this.btn_i18ntext_t_compare_client.Text = "클라번역비교(export)";
			this.btn_i18ntext_t_compare_client.UseVisualStyleBackColor = true;
			this.btn_i18ntext_t_compare_client.Click += new System.EventHandler(this.btn_i18ntext_t_compare_client_Click);
			// 
			// btn_artsvncommit
			// 
			this.btn_artsvncommit.Location = new System.Drawing.Point(271, 330);
			this.btn_artsvncommit.Name = "btn_artsvncommit";
			this.btn_artsvncommit.Size = new System.Drawing.Size(121, 23);
			this.btn_artsvncommit.TabIndex = 27;
			this.btn_artsvncommit.Text = "SVN Commit ART";
			this.btn_artsvncommit.UseVisualStyleBackColor = true;
			this.btn_artsvncommit.Click += new System.EventHandler(this.btn_artsvncommit_Click);
			// 
			// btn_artsvnupdate
			// 
			this.btn_artsvnupdate.Location = new System.Drawing.Point(271, 301);
			this.btn_artsvnupdate.Name = "btn_artsvnupdate";
			this.btn_artsvnupdate.Size = new System.Drawing.Size(121, 23);
			this.btn_artsvnupdate.TabIndex = 26;
			this.btn_artsvnupdate.Text = "SVN Update ART";
			this.btn_artsvnupdate.UseVisualStyleBackColor = true;
			this.btn_artsvnupdate.Click += new System.EventHandler(this.btn_artsvnupdate_Click);
			// 
			// btn_docsvncommit
			// 
			this.btn_docsvncommit.Location = new System.Drawing.Point(141, 330);
			this.btn_docsvncommit.Name = "btn_docsvncommit";
			this.btn_docsvncommit.Size = new System.Drawing.Size(121, 23);
			this.btn_docsvncommit.TabIndex = 25;
			this.btn_docsvncommit.Text = "SVN Commit DOC";
			this.btn_docsvncommit.UseVisualStyleBackColor = true;
			this.btn_docsvncommit.Click += new System.EventHandler(this.btn_docsvncommit_Click);
			// 
			// btn_docsvnupdate
			// 
			this.btn_docsvnupdate.Location = new System.Drawing.Point(141, 301);
			this.btn_docsvnupdate.Name = "btn_docsvnupdate";
			this.btn_docsvnupdate.Size = new System.Drawing.Size(121, 23);
			this.btn_docsvnupdate.TabIndex = 24;
			this.btn_docsvnupdate.Text = "SVN Update DOC";
			this.btn_docsvnupdate.UseVisualStyleBackColor = true;
			this.btn_docsvnupdate.Click += new System.EventHandler(this.btn_docsvnupdate_Click);
			// 
			// btn_trunksvncommt
			// 
			this.btn_trunksvncommt.Location = new System.Drawing.Point(211, 272);
			this.btn_trunksvncommt.Name = "btn_trunksvncommt";
			this.btn_trunksvncommt.Size = new System.Drawing.Size(193, 23);
			this.btn_trunksvncommt.TabIndex = 23;
			this.btn_trunksvncommt.Text = "SVN Commit ALL";
			this.btn_trunksvncommt.UseVisualStyleBackColor = true;
			this.btn_trunksvncommt.Click += new System.EventHandler(this.btn_trunksvncommt_Click);
			// 
			// btn_trunksvnupdate
			// 
			this.btn_trunksvnupdate.Location = new System.Drawing.Point(12, 272);
			this.btn_trunksvnupdate.Name = "btn_trunksvnupdate";
			this.btn_trunksvnupdate.Size = new System.Drawing.Size(193, 23);
			this.btn_trunksvnupdate.TabIndex = 22;
			this.btn_trunksvnupdate.Text = "SVN Update ALL";
			this.btn_trunksvnupdate.UseVisualStyleBackColor = true;
			this.btn_trunksvnupdate.Click += new System.EventHandler(this.btn_trunksvnupdate_Click);
			// 
			// btn_devsvncommit
			// 
			this.btn_devsvncommit.Location = new System.Drawing.Point(12, 330);
			this.btn_devsvncommit.Name = "btn_devsvncommit";
			this.btn_devsvncommit.Size = new System.Drawing.Size(121, 23);
			this.btn_devsvncommit.TabIndex = 21;
			this.btn_devsvncommit.Text = "SVN Commit DEV";
			this.btn_devsvncommit.UseVisualStyleBackColor = true;
			this.btn_devsvncommit.Click += new System.EventHandler(this.btn_devsvncommit_Click);
			// 
			// btn_config_compare_all
			// 
			this.btn_config_compare_all.Location = new System.Drawing.Point(18, 187);
			this.btn_config_compare_all.Name = "btn_config_compare_all";
			this.btn_config_compare_all.Size = new System.Drawing.Size(182, 23);
			this.btn_config_compare_all.TabIndex = 20;
			this.btn_config_compare_all.Text = "Config Compare ALL";
			this.btn_config_compare_all.UseVisualStyleBackColor = true;
			this.btn_config_compare_all.Click += new System.EventHandler(this.btn_config_compare_all_Click);
			// 
			// btn_devsvnupdate
			// 
			this.btn_devsvnupdate.Location = new System.Drawing.Point(12, 301);
			this.btn_devsvnupdate.Name = "btn_devsvnupdate";
			this.btn_devsvnupdate.Size = new System.Drawing.Size(121, 23);
			this.btn_devsvnupdate.TabIndex = 19;
			this.btn_devsvnupdate.Text = "SVN Update DEV";
			this.btn_devsvnupdate.UseVisualStyleBackColor = true;
			this.btn_devsvnupdate.Click += new System.EventHandler(this.btn_devsvnupdate_Click);
			// 
			// btn_dev_update_dev_patch
			// 
			this.btn_dev_update_dev_patch.Location = new System.Drawing.Point(603, 29);
			this.btn_dev_update_dev_patch.Name = "btn_dev_update_dev_patch";
			this.btn_dev_update_dev_patch.Size = new System.Drawing.Size(151, 23);
			this.btn_dev_update_dev_patch.TabIndex = 19;
			this.btn_dev_update_dev_patch.Text = "Update DEV Patch";
			this.btn_dev_update_dev_patch.UseVisualStyleBackColor = true;
			this.btn_dev_update_dev_patch.Click += new System.EventHandler(this.btn_dev_update_dev_patch_Click);
			// 
			// btn_tbl_svn_commit
			// 
			this.btn_tbl_svn_commit.Location = new System.Drawing.Point(6, 58);
			this.btn_tbl_svn_commit.Name = "btn_tbl_svn_commit";
			this.btn_tbl_svn_commit.Size = new System.Drawing.Size(194, 23);
			this.btn_tbl_svn_commit.TabIndex = 18;
			this.btn_tbl_svn_commit.Text = "TBL SVN Commit";
			this.btn_tbl_svn_commit.UseVisualStyleBackColor = true;
			this.btn_tbl_svn_commit.Click += new System.EventHandler(this.btn_tbl_svn_commit_Click);
			// 
			// lbl_tbl_bin_copy_count
			// 
			this.lbl_tbl_bin_copy_count.AutoSize = true;
			this.lbl_tbl_bin_copy_count.Location = new System.Drawing.Point(139, 111);
			this.lbl_tbl_bin_copy_count.Name = "lbl_tbl_bin_copy_count";
			this.lbl_tbl_bin_copy_count.Size = new System.Drawing.Size(11, 12);
			this.lbl_tbl_bin_copy_count.TabIndex = 17;
			this.lbl_tbl_bin_copy_count.Text = "-";
			// 
			// pgbar_tbl_bin_copy
			// 
			this.pgbar_tbl_bin_copy.Location = new System.Drawing.Point(18, 111);
			this.pgbar_tbl_bin_copy.Name = "pgbar_tbl_bin_copy";
			this.pgbar_tbl_bin_copy.Size = new System.Drawing.Size(115, 12);
			this.pgbar_tbl_bin_copy.TabIndex = 16;
			// 
			// btn_tbl_localizeall_compare_all
			// 
			this.btn_tbl_localizeall_compare_all.Location = new System.Drawing.Point(18, 158);
			this.btn_tbl_localizeall_compare_all.Name = "btn_tbl_localizeall_compare_all";
			this.btn_tbl_localizeall_compare_all.Size = new System.Drawing.Size(182, 23);
			this.btn_tbl_localizeall_compare_all.TabIndex = 15;
			this.btn_tbl_localizeall_compare_all.Text = "TBL I18NText Compare ALL";
			this.btn_tbl_localizeall_compare_all.UseVisualStyleBackColor = true;
			this.btn_tbl_localizeall_compare_all.Click += new System.EventHandler(this.btn_tbl_localizeall_compare_all_Click);
			// 
			// btn_tbl_data_compare_all
			// 
			this.btn_tbl_data_compare_all.Location = new System.Drawing.Point(18, 129);
			this.btn_tbl_data_compare_all.Name = "btn_tbl_data_compare_all";
			this.btn_tbl_data_compare_all.Size = new System.Drawing.Size(182, 23);
			this.btn_tbl_data_compare_all.TabIndex = 14;
			this.btn_tbl_data_compare_all.Text = "TBL Export Compare ALL";
			this.btn_tbl_data_compare_all.UseVisualStyleBackColor = true;
			this.btn_tbl_data_compare_all.Click += new System.EventHandler(this.btn_tbl_data_compare_all_Click);
			// 
			// btn_dev_svc_server_update
			// 
			this.btn_dev_svc_server_update.Location = new System.Drawing.Point(8, 408);
			this.btn_dev_svc_server_update.Name = "btn_dev_svc_server_update";
			this.btn_dev_svc_server_update.Size = new System.Drawing.Size(161, 23);
			this.btn_dev_svc_server_update.TabIndex = 12;
			this.btn_dev_svc_server_update.Text = "DEV Server Bin Update";
			this.btn_dev_svc_server_update.UseVisualStyleBackColor = true;
			this.btn_dev_svc_server_update.Click += new System.EventHandler(this.btn_dev_svc_server_update_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 14);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(28, 12);
			this.label5.TabIndex = 9;
			this.label5.Text = "TBL";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 257);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(98, 12);
			this.label4.TabIndex = 8;
			this.label4.Text = "TRUNK 업데이트";
			// 
			// btn_dev_svc_server_commit
			// 
			this.btn_dev_svc_server_commit.Location = new System.Drawing.Point(8, 379);
			this.btn_dev_svc_server_commit.Name = "btn_dev_svc_server_commit";
			this.btn_dev_svc_server_commit.Size = new System.Drawing.Size(161, 23);
			this.btn_dev_svc_server_commit.TabIndex = 7;
			this.btn_dev_svc_server_commit.Text = "DEV Server Bin Commit";
			this.btn_dev_svc_server_commit.UseVisualStyleBackColor = true;
			this.btn_dev_svc_server_commit.Click += new System.EventHandler(this.btn_dev_svc_server_commit_Click);
			// 
			// btn_tbl_svn_update
			// 
			this.btn_tbl_svn_update.Location = new System.Drawing.Point(6, 29);
			this.btn_tbl_svn_update.Name = "btn_tbl_svn_update";
			this.btn_tbl_svn_update.Size = new System.Drawing.Size(194, 23);
			this.btn_tbl_svn_update.TabIndex = 1;
			this.btn_tbl_svn_update.Text = "TBL SVN Update";
			this.btn_tbl_svn_update.UseVisualStyleBackColor = true;
			this.btn_tbl_svn_update.Click += new System.EventHandler(this.btn_tbl_svn_update_Click);
			// 
			// btn_tbl_bin_copy
			// 
			this.btn_tbl_bin_copy.Location = new System.Drawing.Point(18, 88);
			this.btn_tbl_bin_copy.Name = "btn_tbl_bin_copy";
			this.btn_tbl_bin_copy.Size = new System.Drawing.Size(182, 23);
			this.btn_tbl_bin_copy.TabIndex = 0;
			this.btn_tbl_bin_copy.Text = "TBL Export To Client";
			this.btn_tbl_bin_copy.UseVisualStyleBackColor = true;
			this.btn_tbl_bin_copy.Click += new System.EventHandler(this.btn_tbl_bin_copy_Click);
			// 
			// tp_qa
			// 
			this.tp_qa.Controls.Add(this.btn_qa_updatelive);
			this.tp_qa.Controls.Add(this.btn_qa_updaterv);
			this.tp_qa.Controls.Add(this.btn_qa_updateqa2);
			this.tp_qa.Controls.Add(this.btn_qa_updateqa);
			this.tp_qa.Controls.Add(this.btn_qa_update_tag_dev);
			this.tp_qa.Controls.Add(this.btn_qa_commit_tag_dev);
			this.tp_qa.Controls.Add(this.btn_qa_merge_tag_dev);
			this.tp_qa.Controls.Add(this.btn_qa_commitqa2_patch);
			this.tp_qa.Controls.Add(this.label6);
			this.tp_qa.Controls.Add(this.btn_qa_commitqa2);
			this.tp_qa.Controls.Add(this.btn_qa_switch_all);
			this.tp_qa.Controls.Add(this.label1);
			this.tp_qa.Controls.Add(this.tb_qa_tagcreate_ver);
			this.tp_qa.Controls.Add(this.btn_qa_tagcreate);
			this.tp_qa.Controls.Add(this.btn_apple_ios_qa_upload);
			this.tp_qa.Controls.Add(this.btn_qa_commitlive_patch);
			this.tp_qa.Controls.Add(this.btn_qa_commitqa_patch);
			this.tp_qa.Controls.Add(this.tb_qa_commitlive_version);
			this.tp_qa.Controls.Add(this.label3);
			this.tp_qa.Controls.Add(this.btn_qa_commitlive);
			this.tp_qa.Controls.Add(this.label2);
			this.tp_qa.Controls.Add(this.btn_qa_commitrv);
			this.tp_qa.Controls.Add(this.btn_qa_commitqa);
			this.tp_qa.Controls.Add(this.lbl_qa_tag_label);
			this.tp_qa.Location = new System.Drawing.Point(4, 22);
			this.tp_qa.Name = "tp_qa";
			this.tp_qa.Size = new System.Drawing.Size(760, 481);
			this.tp_qa.TabIndex = 2;
			this.tp_qa.Text = "QA";
			this.tp_qa.UseVisualStyleBackColor = true;
			// 
			// btn_qa_updatelive
			// 
			this.btn_qa_updatelive.Location = new System.Drawing.Point(177, 316);
			this.btn_qa_updatelive.Name = "btn_qa_updatelive";
			this.btn_qa_updatelive.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_updatelive.TabIndex = 40;
			this.btn_qa_updatelive.Text = "Update LIVE Server";
			this.btn_qa_updatelive.UseVisualStyleBackColor = true;
			this.btn_qa_updatelive.Click += new System.EventHandler(this.btn_qa_updatelive_Click);
			// 
			// btn_qa_updaterv
			// 
			this.btn_qa_updaterv.Location = new System.Drawing.Point(177, 252);
			this.btn_qa_updaterv.Name = "btn_qa_updaterv";
			this.btn_qa_updaterv.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_updaterv.TabIndex = 39;
			this.btn_qa_updaterv.Text = "Update Review Server";
			this.btn_qa_updaterv.UseVisualStyleBackColor = true;
			this.btn_qa_updaterv.Click += new System.EventHandler(this.btn_qa_updaterv_Click);
			// 
			// btn_qa_updateqa2
			// 
			this.btn_qa_updateqa2.Location = new System.Drawing.Point(177, 216);
			this.btn_qa_updateqa2.Name = "btn_qa_updateqa2";
			this.btn_qa_updateqa2.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_updateqa2.TabIndex = 38;
			this.btn_qa_updateqa2.Text = "Update QA2 Server";
			this.btn_qa_updateqa2.UseVisualStyleBackColor = true;
			this.btn_qa_updateqa2.Click += new System.EventHandler(this.btn_qa_updateqa2_Click);
			// 
			// btn_qa_updateqa
			// 
			this.btn_qa_updateqa.Location = new System.Drawing.Point(177, 187);
			this.btn_qa_updateqa.Name = "btn_qa_updateqa";
			this.btn_qa_updateqa.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_updateqa.TabIndex = 37;
			this.btn_qa_updateqa.Text = "Update QA Server";
			this.btn_qa_updateqa.UseVisualStyleBackColor = true;
			this.btn_qa_updateqa.Click += new System.EventHandler(this.btn_qa_updateqa_Click);
			// 
			// btn_qa_update_tag_dev
			// 
			this.btn_qa_update_tag_dev.Location = new System.Drawing.Point(141, 131);
			this.btn_qa_update_tag_dev.Name = "btn_qa_update_tag_dev";
			this.btn_qa_update_tag_dev.Size = new System.Drawing.Size(140, 23);
			this.btn_qa_update_tag_dev.TabIndex = 36;
			this.btn_qa_update_tag_dev.Text = "TAG DEV Update";
			this.btn_qa_update_tag_dev.UseVisualStyleBackColor = true;
			this.btn_qa_update_tag_dev.Click += new System.EventHandler(this.btn_qa_update_tag_dev_Click);
			// 
			// btn_qa_commit_tag_dev
			// 
			this.btn_qa_commit_tag_dev.Location = new System.Drawing.Point(141, 73);
			this.btn_qa_commit_tag_dev.Name = "btn_qa_commit_tag_dev";
			this.btn_qa_commit_tag_dev.Size = new System.Drawing.Size(140, 52);
			this.btn_qa_commit_tag_dev.TabIndex = 35;
			this.btn_qa_commit_tag_dev.Text = "TAG DEV Commit";
			this.btn_qa_commit_tag_dev.UseVisualStyleBackColor = true;
			this.btn_qa_commit_tag_dev.Click += new System.EventHandler(this.btn_qa_commit_tag_dev_Click);
			// 
			// btn_qa_merge_tag_dev
			// 
			this.btn_qa_merge_tag_dev.Location = new System.Drawing.Point(20, 73);
			this.btn_qa_merge_tag_dev.Name = "btn_qa_merge_tag_dev";
			this.btn_qa_merge_tag_dev.Size = new System.Drawing.Size(115, 81);
			this.btn_qa_merge_tag_dev.TabIndex = 34;
			this.btn_qa_merge_tag_dev.Text = "TAG DEV Merge";
			this.btn_qa_merge_tag_dev.UseVisualStyleBackColor = true;
			this.btn_qa_merge_tag_dev.Click += new System.EventHandler(this.btn_qa_merge_tag_dev_Click);
			// 
			// btn_qa_commitqa2_patch
			// 
			this.btn_qa_commitqa2_patch.Location = new System.Drawing.Point(601, 216);
			this.btn_qa_commitqa2_patch.Name = "btn_qa_commitqa2_patch";
			this.btn_qa_commitqa2_patch.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_commitqa2_patch.TabIndex = 30;
			this.btn_qa_commitqa2_patch.Text = "Commit QA2 Patch";
			this.btn_qa_commitqa2_patch.UseVisualStyleBackColor = true;
			this.btn_qa_commitqa2_patch.Click += new System.EventHandler(this.btn_qa_commitqa2_patch_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(20, 376);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(37, 12);
			this.label6.TabIndex = 29;
			this.label6.Text = "Apple";
			// 
			// btn_qa_commitqa2
			// 
			this.btn_qa_commitqa2.Location = new System.Drawing.Point(20, 216);
			this.btn_qa_commitqa2.Name = "btn_qa_commitqa2";
			this.btn_qa_commitqa2.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_commitqa2.TabIndex = 28;
			this.btn_qa_commitqa2.Text = "Commit QA2 Server";
			this.btn_qa_commitqa2.UseVisualStyleBackColor = true;
			this.btn_qa_commitqa2.Click += new System.EventHandler(this.btn_qa_commitqa2_Click);
			// 
			// btn_qa_switch_all
			// 
			this.btn_qa_switch_all.Location = new System.Drawing.Point(364, 36);
			this.btn_qa_switch_all.Name = "btn_qa_switch_all";
			this.btn_qa_switch_all.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_switch_all.TabIndex = 27;
			this.btn_qa_switch_all.Text = "TAG Switch";
			this.btn_qa_switch_all.UseVisualStyleBackColor = true;
			this.btn_qa_switch_all.Click += new System.EventHandler(this.btn_qa_switch_all_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(20, 41);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 12);
			this.label1.TabIndex = 26;
			this.label1.Text = "TAG VER";
			// 
			// tb_qa_tagcreate_ver
			// 
			this.tb_qa_tagcreate_ver.Location = new System.Drawing.Point(84, 38);
			this.tb_qa_tagcreate_ver.Name = "tb_qa_tagcreate_ver";
			this.tb_qa_tagcreate_ver.Size = new System.Drawing.Size(117, 21);
			this.tb_qa_tagcreate_ver.TabIndex = 25;
			// 
			// btn_qa_tagcreate
			// 
			this.btn_qa_tagcreate.Location = new System.Drawing.Point(207, 36);
			this.btn_qa_tagcreate.Name = "btn_qa_tagcreate";
			this.btn_qa_tagcreate.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_tagcreate.TabIndex = 24;
			this.btn_qa_tagcreate.Text = "TAG Create";
			this.btn_qa_tagcreate.UseVisualStyleBackColor = true;
			this.btn_qa_tagcreate.Click += new System.EventHandler(this.btn_qa_tagcreate_Click);
			// 
			// btn_apple_ios_qa_upload
			// 
			this.btn_apple_ios_qa_upload.Location = new System.Drawing.Point(20, 391);
			this.btn_apple_ios_qa_upload.Name = "btn_apple_ios_qa_upload";
			this.btn_apple_ios_qa_upload.Size = new System.Drawing.Size(151, 23);
			this.btn_apple_ios_qa_upload.TabIndex = 20;
			this.btn_apple_ios_qa_upload.Text = "Apple Uploader(QA)";
			this.btn_apple_ios_qa_upload.UseVisualStyleBackColor = true;
			this.btn_apple_ios_qa_upload.Click += new System.EventHandler(this.btn_apple_ios_qa_upload_Click);
			// 
			// btn_qa_commitlive_patch
			// 
			this.btn_qa_commitlive_patch.Location = new System.Drawing.Point(601, 316);
			this.btn_qa_commitlive_patch.Name = "btn_qa_commitlive_patch";
			this.btn_qa_commitlive_patch.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_commitlive_patch.TabIndex = 18;
			this.btn_qa_commitlive_patch.Text = "Commit Live Patch";
			this.btn_qa_commitlive_patch.UseVisualStyleBackColor = true;
			this.btn_qa_commitlive_patch.Click += new System.EventHandler(this.btn_qa_commitlive_patch_Click);
			// 
			// btn_qa_commitqa_patch
			// 
			this.btn_qa_commitqa_patch.Location = new System.Drawing.Point(601, 187);
			this.btn_qa_commitqa_patch.Name = "btn_qa_commitqa_patch";
			this.btn_qa_commitqa_patch.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_commitqa_patch.TabIndex = 17;
			this.btn_qa_commitqa_patch.Text = "Commit QA Patch";
			this.btn_qa_commitqa_patch.UseVisualStyleBackColor = true;
			this.btn_qa_commitqa_patch.Click += new System.EventHandler(this.btn_qa_commitqa_patch_Click);
			// 
			// tb_qa_commitlive_version
			// 
			this.tb_qa_commitlive_version.Location = new System.Drawing.Point(104, 288);
			this.tb_qa_commitlive_version.Name = "tb_qa_commitlive_version";
			this.tb_qa_commitlive_version.Size = new System.Drawing.Size(163, 21);
			this.tb_qa_commitlive_version.TabIndex = 16;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(20, 293);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(78, 12);
			this.label3.TabIndex = 15;
			this.label3.Text = "LIVE Version";
			// 
			// btn_qa_commitlive
			// 
			this.btn_qa_commitlive.Location = new System.Drawing.Point(20, 316);
			this.btn_qa_commitlive.Name = "btn_qa_commitlive";
			this.btn_qa_commitlive.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_commitlive.TabIndex = 14;
			this.btn_qa_commitlive.Text = "Commit LIVE Server";
			this.btn_qa_commitlive.UseVisualStyleBackColor = true;
			this.btn_qa_commitlive.Click += new System.EventHandler(this.btn_qa_commitlive_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(18, 172);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(47, 12);
			this.label2.TabIndex = 13;
			this.label2.Text = "Service";
			// 
			// btn_qa_commitrv
			// 
			this.btn_qa_commitrv.Location = new System.Drawing.Point(20, 252);
			this.btn_qa_commitrv.Name = "btn_qa_commitrv";
			this.btn_qa_commitrv.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_commitrv.TabIndex = 12;
			this.btn_qa_commitrv.Text = "Commit Review Server";
			this.btn_qa_commitrv.UseVisualStyleBackColor = true;
			this.btn_qa_commitrv.Click += new System.EventHandler(this.btn_qa_commitrv_Click);
			// 
			// btn_qa_commitqa
			// 
			this.btn_qa_commitqa.Location = new System.Drawing.Point(20, 187);
			this.btn_qa_commitqa.Name = "btn_qa_commitqa";
			this.btn_qa_commitqa.Size = new System.Drawing.Size(151, 23);
			this.btn_qa_commitqa.TabIndex = 11;
			this.btn_qa_commitqa.Text = "Commit QA Server";
			this.btn_qa_commitqa.UseVisualStyleBackColor = true;
			this.btn_qa_commitqa.Click += new System.EventHandler(this.btn_qa_commitqa_Click);
			// 
			// lbl_qa_tag_label
			// 
			this.lbl_qa_tag_label.AutoSize = true;
			this.lbl_qa_tag_label.Location = new System.Drawing.Point(18, 14);
			this.lbl_qa_tag_label.Name = "lbl_qa_tag_label";
			this.lbl_qa_tag_label.Size = new System.Drawing.Size(30, 12);
			this.lbl_qa_tag_label.TabIndex = 0;
			this.lbl_qa_tag_label.Text = "TAG";
			// 
			// btn_open_jenkins
			// 
			this.btn_open_jenkins.Location = new System.Drawing.Point(11, 20);
			this.btn_open_jenkins.Name = "btn_open_jenkins";
			this.btn_open_jenkins.Size = new System.Drawing.Size(161, 23);
			this.btn_open_jenkins.TabIndex = 4;
			this.btn_open_jenkins.Text = "Open Jenkins";
			this.btn_open_jenkins.UseVisualStyleBackColor = true;
			this.btn_open_jenkins.Click += new System.EventHandler(this.btn_open_jenkins_Click);
			// 
			// rtb_log
			// 
			this.rtb_log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_log.Location = new System.Drawing.Point(16, 638);
			this.rtb_log.Name = "rtb_log";
			this.rtb_log.ReadOnly = true;
			this.rtb_log.Size = new System.Drawing.Size(1146, 96);
			this.rtb_log.TabIndex = 1;
			this.rtb_log.Text = "";
			// 
			// btn_close
			// 
			this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_close.Location = new System.Drawing.Point(1019, 12);
			this.btn_close.Name = "btn_close";
			this.btn_close.Size = new System.Drawing.Size(143, 42);
			this.btn_close.TabIndex = 2;
			this.btn_close.Text = "CLOSE";
			this.btn_close.UseVisualStyleBackColor = true;
			this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
			// 
			// btn_client_work_commit
			// 
			this.btn_client_work_commit.BackColor = System.Drawing.SystemColors.ButtonShadow;
			this.btn_client_work_commit.Location = new System.Drawing.Point(145, 19);
			this.btn_client_work_commit.Name = "btn_client_work_commit";
			this.btn_client_work_commit.Size = new System.Drawing.Size(161, 23);
			this.btn_client_work_commit.TabIndex = 4;
			this.btn_client_work_commit.Text = "Client WORK Commit";
			this.btn_client_work_commit.UseVisualStyleBackColor = false;
			this.btn_client_work_commit.Click += new System.EventHandler(this.btn_client_work_commit_Click);
			// 
			// btn_server_work_commit
			// 
			this.btn_server_work_commit.BackColor = System.Drawing.SystemColors.ButtonShadow;
			this.btn_server_work_commit.Location = new System.Drawing.Point(312, 19);
			this.btn_server_work_commit.Name = "btn_server_work_commit";
			this.btn_server_work_commit.Size = new System.Drawing.Size(161, 23);
			this.btn_server_work_commit.TabIndex = 6;
			this.btn_server_work_commit.Text = "Server WORK Commit";
			this.btn_server_work_commit.UseVisualStyleBackColor = false;
			this.btn_server_work_commit.Click += new System.EventHandler(this.btn_server_work_commit_Click);
			// 
			// btn_server_work_update
			// 
			this.btn_server_work_update.BackColor = System.Drawing.SystemColors.ButtonShadow;
			this.btn_server_work_update.Location = new System.Drawing.Point(312, 48);
			this.btn_server_work_update.Name = "btn_server_work_update";
			this.btn_server_work_update.Size = new System.Drawing.Size(161, 23);
			this.btn_server_work_update.TabIndex = 11;
			this.btn_server_work_update.Text = "Server WORK Update";
			this.btn_server_work_update.UseVisualStyleBackColor = false;
			this.btn_server_work_update.Click += new System.EventHandler(this.btn_server_work_update_Click);
			// 
			// btn_client_work_update
			// 
			this.btn_client_work_update.BackColor = System.Drawing.SystemColors.ButtonShadow;
			this.btn_client_work_update.Location = new System.Drawing.Point(145, 48);
			this.btn_client_work_update.Name = "btn_client_work_update";
			this.btn_client_work_update.Size = new System.Drawing.Size(161, 23);
			this.btn_client_work_update.TabIndex = 10;
			this.btn_client_work_update.Text = "Client WORK Update";
			this.btn_client_work_update.UseVisualStyleBackColor = false;
			this.btn_client_work_update.Click += new System.EventHandler(this.btn_client_work_update_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.btn_svnlog_tag_get);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.btn_svnlog_get);
			this.groupBox2.Controls.Add(this.tb_svnlog_get_revision_end);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.tb_svnlog_get_revision_start);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Location = new System.Drawing.Point(786, 144);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(376, 179);
			this.groupBox2.TabIndex = 24;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "SVN LOG 가져오기";
			// 
			// btn_svnlog_tag_get
			// 
			this.btn_svnlog_tag_get.Location = new System.Drawing.Point(11, 94);
			this.btn_svnlog_tag_get.Name = "btn_svnlog_tag_get";
			this.btn_svnlog_tag_get.Size = new System.Drawing.Size(277, 23);
			this.btn_svnlog_tag_get.TabIndex = 26;
			this.btn_svnlog_tag_get.Text = "TAG 로그가져오기";
			this.btn_svnlog_tag_get.UseVisualStyleBackColor = true;
			this.btn_svnlog_tag_get.Click += new System.EventHandler(this.btn_svnlog_tag_get_Click);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(132, 22);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(27, 12);
			this.label9.TabIndex = 25;
			this.label9.Text = "End";
			// 
			// btn_svnlog_get
			// 
			this.btn_svnlog_get.Location = new System.Drawing.Point(11, 65);
			this.btn_svnlog_get.Name = "btn_svnlog_get";
			this.btn_svnlog_get.Size = new System.Drawing.Size(277, 23);
			this.btn_svnlog_get.TabIndex = 24;
			this.btn_svnlog_get.Text = "TRUNK 로그가져오기";
			this.btn_svnlog_get.UseVisualStyleBackColor = true;
			this.btn_svnlog_get.Click += new System.EventHandler(this.btn_svnlog_get_Click);
			// 
			// tb_svnlog_get_revision_end
			// 
			this.tb_svnlog_get_revision_end.Location = new System.Drawing.Point(134, 38);
			this.tb_svnlog_get_revision_end.Name = "tb_svnlog_get_revision_end";
			this.tb_svnlog_get_revision_end.Size = new System.Drawing.Size(100, 21);
			this.tb_svnlog_get_revision_end.TabIndex = 24;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(117, 41);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(11, 12);
			this.label7.TabIndex = 23;
			this.label7.Text = "-";
			// 
			// tb_svnlog_get_revision_start
			// 
			this.tb_svnlog_get_revision_start.Location = new System.Drawing.Point(11, 38);
			this.tb_svnlog_get_revision_start.Name = "tb_svnlog_get_revision_start";
			this.tb_svnlog_get_revision_start.Size = new System.Drawing.Size(100, 21);
			this.tb_svnlog_get_revision_start.TabIndex = 21;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(9, 23);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(82, 12);
			this.label8.TabIndex = 22;
			this.label8.Text = "Revision Start";
			// 
			// btn_dev_work_commit
			// 
			this.btn_dev_work_commit.Location = new System.Drawing.Point(6, 19);
			this.btn_dev_work_commit.Name = "btn_dev_work_commit";
			this.btn_dev_work_commit.Size = new System.Drawing.Size(133, 25);
			this.btn_dev_work_commit.TabIndex = 25;
			this.btn_dev_work_commit.Text = "DEV WORK Commit";
			this.btn_dev_work_commit.UseVisualStyleBackColor = true;
			this.btn_dev_work_commit.Click += new System.EventHandler(this.btn_dev_work_commit_Click);
			// 
			// btn_dev_work_update
			// 
			this.btn_dev_work_update.Location = new System.Drawing.Point(6, 50);
			this.btn_dev_work_update.Name = "btn_dev_work_update";
			this.btn_dev_work_update.Size = new System.Drawing.Size(133, 25);
			this.btn_dev_work_update.TabIndex = 26;
			this.btn_dev_work_update.Text = "DEV WORK Update";
			this.btn_dev_work_update.UseVisualStyleBackColor = true;
			this.btn_dev_work_update.Click += new System.EventHandler(this.btn_dev_work_update_Click);
			// 
			// btn_dev_tool_update
			// 
			this.btn_dev_tool_update.Location = new System.Drawing.Point(612, 46);
			this.btn_dev_tool_update.Name = "btn_dev_tool_update";
			this.btn_dev_tool_update.Size = new System.Drawing.Size(114, 25);
			this.btn_dev_tool_update.TabIndex = 28;
			this.btn_dev_tool_update.Text = "UMTOOL Update";
			this.btn_dev_tool_update.UseVisualStyleBackColor = true;
			this.btn_dev_tool_update.Click += new System.EventHandler(this.btn_dev_tool_update_Click);
			// 
			// btn_dev_tool_commit
			// 
			this.btn_dev_tool_commit.Location = new System.Drawing.Point(612, 15);
			this.btn_dev_tool_commit.Name = "btn_dev_tool_commit";
			this.btn_dev_tool_commit.Size = new System.Drawing.Size(114, 25);
			this.btn_dev_tool_commit.TabIndex = 27;
			this.btn_dev_tool_commit.Text = "UMTOOL Commit";
			this.btn_dev_tool_commit.UseVisualStyleBackColor = true;
			this.btn_dev_tool_commit.Click += new System.EventHandler(this.btn_dev_tool_commit_Click);
			// 
			// btn_dev_tool_distribution
			// 
			this.btn_dev_tool_distribution.Location = new System.Drawing.Point(612, 77);
			this.btn_dev_tool_distribution.Name = "btn_dev_tool_distribution";
			this.btn_dev_tool_distribution.Size = new System.Drawing.Size(114, 25);
			this.btn_dev_tool_distribution.TabIndex = 29;
			this.btn_dev_tool_distribution.Text = "UMTOOL 배포";
			this.btn_dev_tool_distribution.UseVisualStyleBackColor = true;
			this.btn_dev_tool_distribution.Click += new System.EventHandler(this.btn_dev_tool_distribution_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.btn_open_jenkins);
			this.groupBox1.Location = new System.Drawing.Point(786, 329);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(376, 297);
			this.groupBox1.TabIndex = 30;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Application";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.btn_dev_umfunity_ext_commit);
			this.groupBox4.Controls.Add(this.btn_dev_umf_distribution);
			this.groupBox4.Controls.Add(this.btn_dev_umf_update);
			this.groupBox4.Controls.Add(this.btn_dev_umf_commit);
			this.groupBox4.Controls.Add(this.btn_dev_work_update);
			this.groupBox4.Controls.Add(this.btn_client_work_commit);
			this.groupBox4.Controls.Add(this.btn_dev_tool_distribution);
			this.groupBox4.Controls.Add(this.btn_server_work_commit);
			this.groupBox4.Controls.Add(this.btn_dev_tool_update);
			this.groupBox4.Controls.Add(this.btn_dev_tool_commit);
			this.groupBox4.Controls.Add(this.btn_client_work_update);
			this.groupBox4.Controls.Add(this.btn_server_work_update);
			this.groupBox4.Controls.Add(this.btn_dev_work_commit);
			this.groupBox4.Location = new System.Drawing.Point(281, 12);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(732, 126);
			this.groupBox4.TabIndex = 31;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Working";
			// 
			// btn_dev_umfunity_ext_commit
			// 
			this.btn_dev_umfunity_ext_commit.Location = new System.Drawing.Point(145, 80);
			this.btn_dev_umfunity_ext_commit.Name = "btn_dev_umfunity_ext_commit";
			this.btn_dev_umfunity_ext_commit.Size = new System.Drawing.Size(161, 25);
			this.btn_dev_umfunity_ext_commit.TabIndex = 33;
			this.btn_dev_umfunity_ext_commit.Text = "UMF.Unity Commit";
			this.btn_dev_umfunity_ext_commit.UseVisualStyleBackColor = true;
			this.btn_dev_umfunity_ext_commit.Click += new System.EventHandler(this.btn_dev_umfunity_ext_commit_Click);
			// 
			// btn_dev_umf_distribution
			// 
			this.btn_dev_umf_distribution.Location = new System.Drawing.Point(492, 77);
			this.btn_dev_umf_distribution.Name = "btn_dev_umf_distribution";
			this.btn_dev_umf_distribution.Size = new System.Drawing.Size(114, 25);
			this.btn_dev_umf_distribution.TabIndex = 32;
			this.btn_dev_umf_distribution.Text = "UMF 배포";
			this.btn_dev_umf_distribution.UseVisualStyleBackColor = true;
			this.btn_dev_umf_distribution.Click += new System.EventHandler(this.btn_dev_umf_distribution_Click);
			// 
			// btn_dev_umf_update
			// 
			this.btn_dev_umf_update.Location = new System.Drawing.Point(492, 46);
			this.btn_dev_umf_update.Name = "btn_dev_umf_update";
			this.btn_dev_umf_update.Size = new System.Drawing.Size(114, 25);
			this.btn_dev_umf_update.TabIndex = 31;
			this.btn_dev_umf_update.Text = "UMF Update";
			this.btn_dev_umf_update.UseVisualStyleBackColor = true;
			this.btn_dev_umf_update.Click += new System.EventHandler(this.btn_dev_umf_update_Click);
			// 
			// btn_dev_umf_commit
			// 
			this.btn_dev_umf_commit.Location = new System.Drawing.Point(492, 15);
			this.btn_dev_umf_commit.Name = "btn_dev_umf_commit";
			this.btn_dev_umf_commit.Size = new System.Drawing.Size(114, 25);
			this.btn_dev_umf_commit.TabIndex = 30;
			this.btn_dev_umf_commit.Text = "UMF Commit";
			this.btn_dev_umf_commit.UseVisualStyleBackColor = true;
			this.btn_dev_umf_commit.Click += new System.EventHandler(this.btn_dev_umf_commit_Click);
			// 
			// groupBox5
			// 
			this.groupBox5.AutoSize = true;
			this.groupBox5.Controls.Add(this.lbl_projectname);
			this.groupBox5.Controls.Add(this.btn_project_select);
			this.groupBox5.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.groupBox5.Location = new System.Drawing.Point(12, 12);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(263, 105);
			this.groupBox5.TabIndex = 32;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Project";
			// 
			// lbl_projectname
			// 
			this.lbl_projectname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbl_projectname.AutoEllipsis = true;
			this.lbl_projectname.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lbl_projectname.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lbl_projectname.Font = new System.Drawing.Font("굴림", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.lbl_projectname.Location = new System.Drawing.Point(9, 18);
			this.lbl_projectname.Name = "lbl_projectname";
			this.lbl_projectname.Size = new System.Drawing.Size(245, 40);
			this.lbl_projectname.TabIndex = 30;
			this.lbl_projectname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btn_project_select
			// 
			this.btn_project_select.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_project_select.BackColor = System.Drawing.Color.DarkRed;
			this.btn_project_select.ForeColor = System.Drawing.Color.White;
			this.btn_project_select.Location = new System.Drawing.Point(6, 62);
			this.btn_project_select.Name = "btn_project_select";
			this.btn_project_select.Size = new System.Drawing.Size(248, 37);
			this.btn_project_select.TabIndex = 29;
			this.btn_project_select.Text = "프로젝트 선택/설정";
			this.btn_project_select.UseVisualStyleBackColor = false;
			this.btn_project_select.Click += new System.EventHandler(this.btn_project_select_Click);
			// 
			// FormDistribution
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1174, 746);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.btn_close);
			this.Controls.Add(this.rtb_log);
			this.Controls.Add(this.tabControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(1047, 698);
			this.Name = "FormDistribution";
			this.Text = "UM Distribution";
			this.tabControl.ResumeLayout(false);
			this.tp_dev.ResumeLayout(false);
			this.tp_dev.PerformLayout();
			this.tp_qa.ResumeLayout(false);
			this.tp_qa.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tp_dev;
		private System.Windows.Forms.RichTextBox rtb_log;
		private System.Windows.Forms.Button btn_close;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Button btn_tbl_bin_copy;
		private System.Windows.Forms.Button btn_tbl_svn_update;
		private System.Windows.Forms.TabPage tp_qa;
		private System.Windows.Forms.Label lbl_qa_tag_label;
		private System.Windows.Forms.Button btn_open_jenkins;
		private System.Windows.Forms.Button btn_qa_commitrv;
		private System.Windows.Forms.Button btn_qa_commitqa;
		private System.Windows.Forms.Button btn_client_work_commit;
		private System.Windows.Forms.Button btn_dev_svc_server_commit;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tb_qa_commitlive_version;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btn_qa_commitlive;
		private System.Windows.Forms.Button btn_qa_commitlive_patch;
		private System.Windows.Forms.Button btn_qa_commitqa_patch;
		private System.Windows.Forms.Button btn_apple_ios_qa_upload;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tb_qa_tagcreate_ver;
		private System.Windows.Forms.Button btn_qa_tagcreate;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btn_qa_switch_all;
		private System.Windows.Forms.Button btn_qa_commitqa2;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btn_dev_svc_server_update;
		private System.Windows.Forms.Button btn_qa_commitqa2_patch;
		private System.Windows.Forms.Button btn_server_work_commit;
		private System.Windows.Forms.Button btn_tbl_localizeall_compare_all;
		private System.Windows.Forms.Button btn_tbl_data_compare_all;
		private System.Windows.Forms.Button btn_server_work_update;
		private System.Windows.Forms.Button btn_client_work_update;
		private System.Windows.Forms.ProgressBar pgbar_tbl_bin_copy;
		private System.Windows.Forms.Label lbl_tbl_bin_copy_count;
		private System.Windows.Forms.Button btn_tbl_svn_commit;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btn_svnlog_tag_get;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Button btn_svnlog_get;
		private System.Windows.Forms.TextBox tb_svnlog_get_revision_end;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tb_svnlog_get_revision_start;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btn_dev_work_commit;
		private System.Windows.Forms.Button btn_dev_work_update;
		private System.Windows.Forms.Button btn_qa_update_tag_dev;
		private System.Windows.Forms.Button btn_qa_commit_tag_dev;
		private System.Windows.Forms.Button btn_qa_merge_tag_dev;
		private System.Windows.Forms.Button btn_devsvnupdate;
		private System.Windows.Forms.Button btn_dev_tool_update;
		private System.Windows.Forms.Button btn_dev_tool_commit;
		private System.Windows.Forms.Button btn_dev_tool_distribution;
		private System.Windows.Forms.Button btn_qa_updatelive;
		private System.Windows.Forms.Button btn_qa_updaterv;
		private System.Windows.Forms.Button btn_qa_updateqa2;
		private System.Windows.Forms.Button btn_qa_updateqa;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Button btn_project_select;
		private System.Windows.Forms.Button btn_dev_update_dev_patch;
		private System.Windows.Forms.ToolTip tooltip;
		private System.Windows.Forms.Label lbl_projectname;
		private System.Windows.Forms.Button btn_dev_umf_distribution;
		private System.Windows.Forms.Button btn_dev_umf_update;
		private System.Windows.Forms.Button btn_dev_umf_commit;
		private System.Windows.Forms.Button btn_dev_umfunity_ext_commit;
		private System.Windows.Forms.Button btn_config_compare_all;
		private System.Windows.Forms.Button btn_devsvncommit;
		private System.Windows.Forms.Button btn_trunksvncommt;
		private System.Windows.Forms.Button btn_trunksvnupdate;
		private System.Windows.Forms.Button btn_artsvncommit;
		private System.Windows.Forms.Button btn_artsvnupdate;
		private System.Windows.Forms.Button btn_docsvncommit;
		private System.Windows.Forms.Button btn_docsvnupdate;
		private System.Windows.Forms.Button btn_i18ntext_m_compare_client;
		private System.Windows.Forms.Button btn_i18ntext_t_compare_client;
	}
}

