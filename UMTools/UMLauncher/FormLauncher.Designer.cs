namespace UMTools.UMLauncher
{
	partial class FormLauncher
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLauncher));
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.lbl_projectname = new System.Windows.Forms.Label();
			this.btn_project_select = new System.Windows.Forms.Button();
			this.btn_close = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rtb_log = new System.Windows.Forms.RichTextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btn_dev_client_fxtool = new System.Windows.Forms.Button();
			this.tb_devserver_configtype = new System.Windows.Forms.TextBox();
			this.btn_dev_client_battletest = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.btn_launch_devclient = new System.Windows.Forms.Button();
			this.btn_devserver_close = new System.Windows.Forms.Button();
			this.cb_launch_devclient_vers = new System.Windows.Forms.ComboBox();
			this.btn_devclient_close = new System.Windows.Forms.Button();
			this.btn_launch_devserver = new System.Windows.Forms.Button();
			this.cb_client_notusedoctbl = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btn_dev_clear_prevversion = new System.Windows.Forms.Button();
			this.group_dev_server = new System.Windows.Forms.GroupBox();
			this.btn_download_cancel = new System.Windows.Forms.Button();
			this.btn_server_log_open = new System.Windows.Forms.Button();
			this.btn_client_log_open = new System.Windows.Forms.Button();
			this.cb_autochecknewver = new System.Windows.Forms.CheckBox();
			this.btn_dev_server_changelog = new System.Windows.Forms.Button();
			this.btn_dev_client_changelog = new System.Windows.Forms.Button();
			this.lbl_progress_newvertext = new System.Windows.Forms.Label();
			this.tb_server_currver = new System.Windows.Forms.TextBox();
			this.tb_client_currver = new System.Windows.Forms.TextBox();
			this.lbl_client_currver = new System.Windows.Forms.Label();
			this.btn_dev_newvercheck = new System.Windows.Forms.Button();
			this.lbl_server_currver = new System.Windows.Forms.Label();
			this.progress_newverdownload = new System.Windows.Forms.ProgressBar();
			this.timer_onesec = new System.Windows.Forms.Timer(this.components);
			this.timer_Alarm = new System.Windows.Forms.Timer(this.components);
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox5.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.group_dev_server.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox5
			// 
			this.groupBox5.AutoSize = true;
			this.groupBox5.Controls.Add(this.lbl_projectname);
			this.groupBox5.Controls.Add(this.btn_project_select);
			this.groupBox5.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.groupBox5.Location = new System.Drawing.Point(12, 12);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(517, 77);
			this.groupBox5.TabIndex = 33;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Project";
			// 
			// lbl_projectname
			// 
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
			this.btn_project_select.BackColor = System.Drawing.Color.DarkRed;
			this.btn_project_select.ForeColor = System.Drawing.Color.White;
			this.btn_project_select.Location = new System.Drawing.Point(260, 20);
			this.btn_project_select.Name = "btn_project_select";
			this.btn_project_select.Size = new System.Drawing.Size(248, 37);
			this.btn_project_select.TabIndex = 29;
			this.btn_project_select.Text = "프로젝트 선택/설정";
			this.btn_project_select.UseVisualStyleBackColor = false;
			this.btn_project_select.Click += new System.EventHandler(this.btn_project_select_Click);
			// 
			// btn_close
			// 
			this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_close.Location = new System.Drawing.Point(803, 12);
			this.btn_close.Name = "btn_close";
			this.btn_close.Size = new System.Drawing.Size(143, 42);
			this.btn_close.TabIndex = 34;
			this.btn_close.Text = "CLOSE";
			this.btn_close.UseVisualStyleBackColor = true;
			this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
			this.groupBox1.Controls.Add(this.rtb_log);
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.groupBox3);
			this.groupBox1.Controls.Add(this.group_dev_server);
			this.groupBox1.Location = new System.Drawing.Point(12, 95);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(934, 563);
			this.groupBox1.TabIndex = 35;
			this.groupBox1.TabStop = false;
			// 
			// rtb_log
			// 
			this.rtb_log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_log.Location = new System.Drawing.Point(6, 446);
			this.rtb_log.Name = "rtb_log";
			this.rtb_log.ReadOnly = true;
			this.rtb_log.Size = new System.Drawing.Size(922, 111);
			this.rtb_log.TabIndex = 35;
			this.rtb_log.Text = "";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
			this.groupBox2.Controls.Add(this.btn_dev_client_fxtool);
			this.groupBox2.Controls.Add(this.tb_devserver_configtype);
			this.groupBox2.Controls.Add(this.btn_dev_client_battletest);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.groupBox4);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.btn_launch_devclient);
			this.groupBox2.Controls.Add(this.btn_devserver_close);
			this.groupBox2.Controls.Add(this.cb_launch_devclient_vers);
			this.groupBox2.Controls.Add(this.btn_devclient_close);
			this.groupBox2.Controls.Add(this.btn_launch_devserver);
			this.groupBox2.Controls.Add(this.cb_client_notusedoctbl);
			this.groupBox2.Location = new System.Drawing.Point(9, 20);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(919, 186);
			this.groupBox2.TabIndex = 34;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Client / Server";
			// 
			// btn_dev_client_fxtool
			// 
			this.btn_dev_client_fxtool.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_dev_client_fxtool.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.btn_dev_client_fxtool.Location = new System.Drawing.Point(162, 149);
			this.btn_dev_client_fxtool.Name = "btn_dev_client_fxtool";
			this.btn_dev_client_fxtool.Size = new System.Drawing.Size(148, 31);
			this.btn_dev_client_fxtool.TabIndex = 4;
			this.btn_dev_client_fxtool.Text = "이펙트 툴 실행";
			this.btn_dev_client_fxtool.UseVisualStyleBackColor = false;
			this.btn_dev_client_fxtool.Click += new System.EventHandler(this.btn_dev_client_fxtool_Click);
			// 
			// tb_devserver_configtype
			// 
			this.tb_devserver_configtype.Location = new System.Drawing.Point(575, 102);
			this.tb_devserver_configtype.Name = "tb_devserver_configtype";
			this.tb_devserver_configtype.Size = new System.Drawing.Size(152, 21);
			this.tb_devserver_configtype.TabIndex = 36;
			// 
			// btn_dev_client_battletest
			// 
			this.btn_dev_client_battletest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_dev_client_battletest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.btn_dev_client_battletest.Location = new System.Drawing.Point(8, 148);
			this.btn_dev_client_battletest.Name = "btn_dev_client_battletest";
			this.btn_dev_client_battletest.Size = new System.Drawing.Size(148, 31);
			this.btn_dev_client_battletest.TabIndex = 3;
			this.btn_dev_client_battletest.Text = "BattleTest 실행";
			this.btn_dev_client_battletest.UseVisualStyleBackColor = false;
			this.btn_dev_client_battletest.Click += new System.EventHandler(this.btn_dev_client_battletest_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(496, 108);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(76, 12);
			this.label1.TabIndex = 35;
			this.label1.Text = "_ConfigType";
			// 
			// groupBox4
			// 
			this.groupBox4.BackColor = System.Drawing.Color.Red;
			this.groupBox4.Location = new System.Drawing.Point(461, 13);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(10, 166);
			this.groupBox4.TabIndex = 34;
			this.groupBox4.TabStop = false;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(6, 101);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(71, 28);
			this.label4.TabIndex = 21;
			this.label4.Text = "버전별 실행";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.ForeColor = System.Drawing.Color.Red;
			this.label5.Location = new System.Drawing.Point(492, 80);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(157, 12);
			this.label5.TabIndex = 22;
			this.label5.Text = "* DEV SERVER : DEFAULT";
			// 
			// btn_launch_devclient
			// 
			this.btn_launch_devclient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.btn_launch_devclient.Location = new System.Drawing.Point(8, 20);
			this.btn_launch_devclient.Name = "btn_launch_devclient";
			this.btn_launch_devclient.Size = new System.Drawing.Size(349, 51);
			this.btn_launch_devclient.TabIndex = 3;
			this.btn_launch_devclient.Text = "개발용클라이언트 실행";
			this.btn_launch_devclient.UseVisualStyleBackColor = false;
			this.btn_launch_devclient.Click += new System.EventHandler(this.btn_launch_devclient_Click);
			// 
			// btn_devserver_close
			// 
			this.btn_devserver_close.BackColor = System.Drawing.Color.Red;
			this.btn_devserver_close.ForeColor = System.Drawing.Color.White;
			this.btn_devserver_close.Location = new System.Drawing.Point(822, 20);
			this.btn_devserver_close.Name = "btn_devserver_close";
			this.btn_devserver_close.Size = new System.Drawing.Size(80, 49);
			this.btn_devserver_close.TabIndex = 17;
			this.btn_devserver_close.Text = "서버종료";
			this.btn_devserver_close.UseVisualStyleBackColor = false;
			this.btn_devserver_close.Click += new System.EventHandler(this.btn_devserver_close_Click);
			// 
			// cb_launch_devclient_vers
			// 
			this.cb_launch_devclient_vers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cb_launch_devclient_vers.FormattingEnabled = true;
			this.cb_launch_devclient_vers.ItemHeight = 12;
			this.cb_launch_devclient_vers.Location = new System.Drawing.Point(83, 106);
			this.cb_launch_devclient_vers.Name = "cb_launch_devclient_vers";
			this.cb_launch_devclient_vers.Size = new System.Drawing.Size(357, 20);
			this.cb_launch_devclient_vers.TabIndex = 20;
			// 
			// btn_devclient_close
			// 
			this.btn_devclient_close.BackColor = System.Drawing.Color.Red;
			this.btn_devclient_close.ForeColor = System.Drawing.Color.White;
			this.btn_devclient_close.Location = new System.Drawing.Point(363, 20);
			this.btn_devclient_close.Name = "btn_devclient_close";
			this.btn_devclient_close.Size = new System.Drawing.Size(80, 49);
			this.btn_devclient_close.TabIndex = 18;
			this.btn_devclient_close.Text = "클라이언트종료";
			this.btn_devclient_close.UseVisualStyleBackColor = false;
			this.btn_devclient_close.Click += new System.EventHandler(this.btn_devclient_close_Click);
			// 
			// btn_launch_devserver
			// 
			this.btn_launch_devserver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.btn_launch_devserver.Location = new System.Drawing.Point(494, 20);
			this.btn_launch_devserver.Name = "btn_launch_devserver";
			this.btn_launch_devserver.Size = new System.Drawing.Size(322, 52);
			this.btn_launch_devserver.TabIndex = 2;
			this.btn_launch_devserver.Text = "로컬서버 실행";
			this.btn_launch_devserver.UseVisualStyleBackColor = false;
			this.btn_launch_devserver.Click += new System.EventHandler(this.btn_launch_devserver_Click);
			// 
			// cb_client_notusedoctbl
			// 
			this.cb_client_notusedoctbl.AutoSize = true;
			this.cb_client_notusedoctbl.Location = new System.Drawing.Point(8, 80);
			this.cb_client_notusedoctbl.Name = "cb_client_notusedoctbl";
			this.cb_client_notusedoctbl.Size = new System.Drawing.Size(219, 16);
			this.cb_client_notusedoctbl.TabIndex = 24;
			this.cb_client_notusedoctbl.Text = "Documents의 TBL 데이터 사용안함";
			this.cb_client_notusedoctbl.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.btn_dev_clear_prevversion);
			this.groupBox3.Location = new System.Drawing.Point(8, 330);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(919, 108);
			this.groupBox3.TabIndex = 19;
			this.groupBox3.TabStop = false;
			// 
			// btn_dev_clear_prevversion
			// 
			this.btn_dev_clear_prevversion.Location = new System.Drawing.Point(6, 20);
			this.btn_dev_clear_prevversion.Name = "btn_dev_clear_prevversion";
			this.btn_dev_clear_prevversion.Size = new System.Drawing.Size(148, 31);
			this.btn_dev_clear_prevversion.TabIndex = 0;
			this.btn_dev_clear_prevversion.Text = "이전버전정리(삭제)";
			this.btn_dev_clear_prevversion.UseVisualStyleBackColor = true;
			this.btn_dev_clear_prevversion.Click += new System.EventHandler(this.btn_dev_clear_prevversion_Click);
			// 
			// group_dev_server
			// 
			this.group_dev_server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.group_dev_server.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.group_dev_server.Controls.Add(this.btn_download_cancel);
			this.group_dev_server.Controls.Add(this.btn_server_log_open);
			this.group_dev_server.Controls.Add(this.btn_client_log_open);
			this.group_dev_server.Controls.Add(this.cb_autochecknewver);
			this.group_dev_server.Controls.Add(this.btn_dev_server_changelog);
			this.group_dev_server.Controls.Add(this.btn_dev_client_changelog);
			this.group_dev_server.Controls.Add(this.lbl_progress_newvertext);
			this.group_dev_server.Controls.Add(this.tb_server_currver);
			this.group_dev_server.Controls.Add(this.tb_client_currver);
			this.group_dev_server.Controls.Add(this.lbl_client_currver);
			this.group_dev_server.Controls.Add(this.btn_dev_newvercheck);
			this.group_dev_server.Controls.Add(this.lbl_server_currver);
			this.group_dev_server.Controls.Add(this.progress_newverdownload);
			this.group_dev_server.Location = new System.Drawing.Point(9, 212);
			this.group_dev_server.Name = "group_dev_server";
			this.group_dev_server.Size = new System.Drawing.Size(919, 100);
			this.group_dev_server.TabIndex = 13;
			this.group_dev_server.TabStop = false;
			this.group_dev_server.Text = "업데이트";
			// 
			// btn_download_cancel
			// 
			this.btn_download_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_download_cancel.Location = new System.Drawing.Point(826, 69);
			this.btn_download_cancel.Name = "btn_download_cancel";
			this.btn_download_cancel.Size = new System.Drawing.Size(87, 25);
			this.btn_download_cancel.TabIndex = 21;
			this.btn_download_cancel.Text = "취소";
			this.btn_download_cancel.UseVisualStyleBackColor = true;
			this.btn_download_cancel.Click += new System.EventHandler(this.btn_download_cancel_Click);
			// 
			// btn_server_log_open
			// 
			this.btn_server_log_open.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_server_log_open.Location = new System.Drawing.Point(826, 40);
			this.btn_server_log_open.Name = "btn_server_log_open";
			this.btn_server_log_open.Size = new System.Drawing.Size(87, 25);
			this.btn_server_log_open.TabIndex = 20;
			this.btn_server_log_open.Text = "로그열기";
			this.btn_server_log_open.UseVisualStyleBackColor = true;
			this.btn_server_log_open.Click += new System.EventHandler(this.btn_server_log_open_Click);
			// 
			// btn_client_log_open
			// 
			this.btn_client_log_open.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_client_log_open.Location = new System.Drawing.Point(826, 16);
			this.btn_client_log_open.Name = "btn_client_log_open";
			this.btn_client_log_open.Size = new System.Drawing.Size(87, 25);
			this.btn_client_log_open.TabIndex = 19;
			this.btn_client_log_open.Text = "로그열기";
			this.btn_client_log_open.UseVisualStyleBackColor = true;
			this.btn_client_log_open.Click += new System.EventHandler(this.btn_client_log_open_Click);
			// 
			// cb_autochecknewver
			// 
			this.cb_autochecknewver.Location = new System.Drawing.Point(8, 61);
			this.cb_autochecknewver.Name = "cb_autochecknewver";
			this.cb_autochecknewver.Size = new System.Drawing.Size(111, 39);
			this.cb_autochecknewver.TabIndex = 18;
			this.cb_autochecknewver.Text = "자동체크";
			this.cb_autochecknewver.UseVisualStyleBackColor = true;
			this.cb_autochecknewver.CheckedChanged += new System.EventHandler(this.cb_autochecknewver_CheckedChanged);
			// 
			// btn_dev_server_changelog
			// 
			this.btn_dev_server_changelog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_dev_server_changelog.Location = new System.Drawing.Point(733, 40);
			this.btn_dev_server_changelog.Name = "btn_dev_server_changelog";
			this.btn_dev_server_changelog.Size = new System.Drawing.Size(87, 25);
			this.btn_dev_server_changelog.TabIndex = 17;
			this.btn_dev_server_changelog.Text = "변경사항";
			this.btn_dev_server_changelog.UseVisualStyleBackColor = true;
			this.btn_dev_server_changelog.Click += new System.EventHandler(this.btn_dev_server_changelog_Click);
			// 
			// btn_dev_client_changelog
			// 
			this.btn_dev_client_changelog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_dev_client_changelog.Location = new System.Drawing.Point(733, 16);
			this.btn_dev_client_changelog.Name = "btn_dev_client_changelog";
			this.btn_dev_client_changelog.Size = new System.Drawing.Size(87, 25);
			this.btn_dev_client_changelog.TabIndex = 16;
			this.btn_dev_client_changelog.Text = "변경사항";
			this.btn_dev_client_changelog.UseVisualStyleBackColor = true;
			this.btn_dev_client_changelog.Click += new System.EventHandler(this.btn_dev_client_changelog_Click);
			// 
			// lbl_progress_newvertext
			// 
			this.lbl_progress_newvertext.AutoSize = true;
			this.lbl_progress_newvertext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
			this.lbl_progress_newvertext.ForeColor = System.Drawing.Color.White;
			this.lbl_progress_newvertext.Location = new System.Drawing.Point(227, 76);
			this.lbl_progress_newvertext.Name = "lbl_progress_newvertext";
			this.lbl_progress_newvertext.Size = new System.Drawing.Size(97, 12);
			this.lbl_progress_newvertext.TabIndex = 15;
			this.lbl_progress_newvertext.Text = "update progress";
			// 
			// tb_server_currver
			// 
			this.tb_server_currver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_server_currver.Location = new System.Drawing.Point(125, 43);
			this.tb_server_currver.Name = "tb_server_currver";
			this.tb_server_currver.ReadOnly = true;
			this.tb_server_currver.Size = new System.Drawing.Size(602, 21);
			this.tb_server_currver.TabIndex = 14;
			// 
			// tb_client_currver
			// 
			this.tb_client_currver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tb_client_currver.Location = new System.Drawing.Point(125, 19);
			this.tb_client_currver.Name = "tb_client_currver";
			this.tb_client_currver.ReadOnly = true;
			this.tb_client_currver.Size = new System.Drawing.Size(602, 21);
			this.tb_client_currver.TabIndex = 13;
			this.tb_client_currver.TextChanged += new System.EventHandler(this.tb_client_currver_TextChanged);
			// 
			// lbl_client_currver
			// 
			this.lbl_client_currver.AutoSize = true;
			this.lbl_client_currver.Location = new System.Drawing.Point(6, 22);
			this.lbl_client_currver.Name = "lbl_client_currver";
			this.lbl_client_currver.Size = new System.Drawing.Size(113, 12);
			this.lbl_client_currver.TabIndex = 9;
			this.lbl_client_currver.Text = "클라이언트현재버전";
			// 
			// btn_dev_newvercheck
			// 
			this.btn_dev_newvercheck.Location = new System.Drawing.Point(125, 69);
			this.btn_dev_newvercheck.Name = "btn_dev_newvercheck";
			this.btn_dev_newvercheck.Size = new System.Drawing.Size(87, 25);
			this.btn_dev_newvercheck.TabIndex = 12;
			this.btn_dev_newvercheck.Text = "새버전체크";
			this.btn_dev_newvercheck.UseVisualStyleBackColor = true;
			this.btn_dev_newvercheck.Click += new System.EventHandler(this.btn_dev_newvercheck_Click);
			// 
			// lbl_server_currver
			// 
			this.lbl_server_currver.AutoSize = true;
			this.lbl_server_currver.Location = new System.Drawing.Point(42, 46);
			this.lbl_server_currver.Name = "lbl_server_currver";
			this.lbl_server_currver.Size = new System.Drawing.Size(77, 12);
			this.lbl_server_currver.TabIndex = 11;
			this.lbl_server_currver.Text = "서버현재버전";
			// 
			// progress_newverdownload
			// 
			this.progress_newverdownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progress_newverdownload.Location = new System.Drawing.Point(218, 70);
			this.progress_newverdownload.Name = "progress_newverdownload";
			this.progress_newverdownload.Size = new System.Drawing.Size(602, 24);
			this.progress_newverdownload.TabIndex = 10;
			// 
			// timer_onesec
			// 
			this.timer_onesec.Interval = 1000;
			this.timer_onesec.Tick += new System.EventHandler(this.timer_onesec_Tick);
			// 
			// timer_Alarm
			// 
			this.timer_Alarm.Tick += new System.EventHandler(this.timer_Alarm_Tick);
			// 
			// FormLauncher
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(958, 670);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btn_close);
			this.Controls.Add(this.groupBox5);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(974, 616);
			this.Name = "FormLauncher";
			this.Text = "UM Launcher";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLauncher_FormClosed);
			this.groupBox5.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.group_dev_server.ResumeLayout(false);
			this.group_dev_server.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label lbl_projectname;
		private System.Windows.Forms.Button btn_project_select;
		private System.Windows.Forms.Button btn_close;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox cb_client_notusedoctbl;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btn_dev_clear_prevversion;
		private System.Windows.Forms.Button btn_devclient_close;
		private System.Windows.Forms.ComboBox cb_launch_devclient_vers;
		private System.Windows.Forms.Button btn_devserver_close;
		private System.Windows.Forms.GroupBox group_dev_server;
		private System.Windows.Forms.Button btn_server_log_open;
		private System.Windows.Forms.Button btn_client_log_open;
		private System.Windows.Forms.CheckBox cb_autochecknewver;
		private System.Windows.Forms.Button btn_dev_server_changelog;
		private System.Windows.Forms.Button btn_dev_client_changelog;
		private System.Windows.Forms.Label lbl_progress_newvertext;
		private System.Windows.Forms.TextBox tb_server_currver;
		private System.Windows.Forms.TextBox tb_client_currver;
		private System.Windows.Forms.Label lbl_client_currver;
		private System.Windows.Forms.Button btn_dev_newvercheck;
		private System.Windows.Forms.Label lbl_server_currver;
		private System.Windows.Forms.ProgressBar progress_newverdownload;
		private System.Windows.Forms.Button btn_launch_devserver;
		private System.Windows.Forms.Button btn_launch_devclient;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.RichTextBox rtb_log;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Timer timer_onesec;
		private System.Windows.Forms.Timer timer_Alarm;
		private System.Windows.Forms.TextBox tb_devserver_configtype;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btn_dev_client_fxtool;
		private System.Windows.Forms.Button btn_dev_client_battletest;
		private System.Windows.Forms.Button btn_download_cancel;
		private System.Windows.Forms.ToolTip tooltip;
	}
}

