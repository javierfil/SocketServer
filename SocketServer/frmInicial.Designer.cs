namespace ServerComunicacionesALUTEL
{
    partial class frmInicial
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmInicial));
            this.label1 = new System.Windows.Forms.Label();
            this.lstAlutrackLOG = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.btnConfig = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnDevices = new System.Windows.Forms.Button();
            this.btnUsuarios = new System.Windows.Forms.Button();
            this.btnVirtualGate = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.tmrRebind = new System.Windows.Forms.Timer(this.components);
            this.lblPuertos = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lstLenelLOG = new System.Windows.Forms.ListBox();
            this.btnStartAlutrackServer = new System.Windows.Forms.Button();
            this.btnStopAlutrackServer = new System.Windows.Forms.Button();
            this.btnStartLENELServer = new System.Windows.Forms.Button();
            this.btnStopLENELServer = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.chkVerboseLOGAlutrack = new System.Windows.Forms.CheckBox();
            this.chkVerboseLOGLenel = new System.Windows.Forms.CheckBox();
            this.button4 = new System.Windows.Forms.Button();
            this.lstAlutrackServerLOG = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnStopAlutrackServerWeb = new System.Windows.Forms.Button();
            this.btnStartAlutrackServerWeb = new System.Windows.Forms.Button();
            this.chkVerboseLOGAlutrackServerWeb = new System.Windows.Forms.CheckBox();
            this.btnClearAlutrack = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.label1.Font = new System.Drawing.Font("Calibri", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImageKey = "(none)";
            this.label1.Location = new System.Drawing.Point(31, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(241, 39);
            this.label1.TabIndex = 1;
            this.label1.Text = "Socket Server 1.6";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lstAlutrackLOG
            // 
            this.lstAlutrackLOG.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstAlutrackLOG.FormattingEnabled = true;
            this.lstAlutrackLOG.HorizontalScrollbar = true;
            this.lstAlutrackLOG.ItemHeight = 16;
            this.lstAlutrackLOG.Location = new System.Drawing.Point(34, 185);
            this.lstAlutrackLOG.Name = "lstAlutrackLOG";
            this.lstAlutrackLOG.Size = new System.Drawing.Size(360, 324);
            this.lstAlutrackLOG.TabIndex = 54;
            this.lstAlutrackLOG.SelectedIndexChanged += new System.EventHandler(this.lstAlutrackLOG_SelectedIndexChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ServerComunicacionesALUTEL.Properties.Resources.ALUTELlogo;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(1054, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(99, 42);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 60;
            this.pictureBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Image = global::ServerComunicacionesALUTEL.Properties.Resources.report_iconCh;
            this.button2.Location = new System.Drawing.Point(883, 103);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(141, 46);
            this.button2.TabIndex = 52;
            this.button2.Text = "Reports";
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.Image = global::ServerComunicacionesALUTEL.Properties.Resources.icon_accessCh;
            this.button5.Location = new System.Drawing.Point(505, 103);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(192, 46);
            this.button5.TabIndex = 9;
            this.button5.Text = "Access Rules";
            this.button5.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // btnConfig
            // 
            this.btnConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfig.Image = global::ServerComunicacionesALUTEL.Properties.Resources.ConfigurationIconCh;
            this.btnConfig.Location = new System.Drawing.Point(1030, 103);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(123, 46);
            this.btnConfig.TabIndex = 8;
            this.btnConfig.Text = "Config";
            this.btnConfig.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Image = global::ServerComunicacionesALUTEL.Properties.Resources.user_group_iconch;
            this.button1.Location = new System.Drawing.Point(350, 103);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(149, 46);
            this.button1.TabIndex = 7;
            this.button1.Text = "Groups";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnDevices
            // 
            this.btnDevices.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDevices.Image = global::ServerComunicacionesALUTEL.Properties.Resources.device_Icon2;
            this.btnDevices.Location = new System.Drawing.Point(194, 103);
            this.btnDevices.Name = "btnDevices";
            this.btnDevices.Size = new System.Drawing.Size(150, 46);
            this.btnDevices.TabIndex = 6;
            this.btnDevices.Text = "Devices";
            this.btnDevices.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDevices.UseVisualStyleBackColor = true;
            this.btnDevices.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnUsuarios
            // 
            this.btnUsuarios.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUsuarios.Image = global::ServerComunicacionesALUTEL.Properties.Resources.User1ch1;
            this.btnUsuarios.Location = new System.Drawing.Point(38, 103);
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Size = new System.Drawing.Size(150, 46);
            this.btnUsuarios.TabIndex = 5;
            this.btnUsuarios.Text = "Users";
            this.btnUsuarios.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUsuarios.UseVisualStyleBackColor = true;
            this.btnUsuarios.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnVirtualGate
            // 
            this.btnVirtualGate.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVirtualGate.Image = global::ServerComunicacionesALUTEL.Properties.Resources.icon_accessCh;
            this.btnVirtualGate.Location = new System.Drawing.Point(703, 103);
            this.btnVirtualGate.Name = "btnVirtualGate";
            this.btnVirtualGate.Size = new System.Drawing.Size(174, 46);
            this.btnVirtualGate.TabIndex = 78;
            this.btnVirtualGate.Text = "Virtual Gate";
            this.btnVirtualGate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnVirtualGate.UseVisualStyleBackColor = true;
            this.btnVirtualGate.Click += new System.EventHandler(this.btnVirtualGate_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(1189, 510);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(63, 26);
            this.btnClear.TabIndex = 80;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(29, 155);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(180, 28);
            this.label13.TabIndex = 81;
            this.label13.Text = "HH server";
            // 
            // tmrRebind
            // 
            this.tmrRebind.Interval = 200;
            this.tmrRebind.Tick += new System.EventHandler(this.tmrRebind_Tick);
            // 
            // lblPuertos
            // 
            this.lblPuertos.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPuertos.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblPuertos.Location = new System.Drawing.Point(998, 54);
            this.lblPuertos.Name = "lblPuertos";
            this.lblPuertos.Size = new System.Drawing.Size(155, 19);
            this.lblPuertos.TabIndex = 82;
            this.lblPuertos.Text = "lblPuertos";
            this.lblPuertos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(38, 61);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(70, 36);
            this.button3.TabIndex = 83;
            this.button3.Text = "Test";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_4);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(395, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 28);
            this.label2.TabIndex = 92;
            this.label2.Text = "Lenel Server";
            // 
            // lstLenelLOG
            // 
            this.lstLenelLOG.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstLenelLOG.FormattingEnabled = true;
            this.lstLenelLOG.HorizontalScrollbar = true;
            this.lstLenelLOG.ItemHeight = 16;
            this.lstLenelLOG.Location = new System.Drawing.Point(400, 184);
            this.lstLenelLOG.Name = "lstLenelLOG";
            this.lstLenelLOG.Size = new System.Drawing.Size(395, 324);
            this.lstLenelLOG.TabIndex = 91;
            this.lstLenelLOG.SelectedIndexChanged += new System.EventHandler(this.lstLenelLOG_SelectedIndexChanged);
            // 
            // btnStartAlutrackServer
            // 
            this.btnStartAlutrackServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStartAlutrackServer.BackgroundImage")));
            this.btnStartAlutrackServer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStartAlutrackServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartAlutrackServer.Location = new System.Drawing.Point(34, 511);
            this.btnStartAlutrackServer.Name = "btnStartAlutrackServer";
            this.btnStartAlutrackServer.Size = new System.Drawing.Size(40, 40);
            this.btnStartAlutrackServer.TabIndex = 94;
            this.btnStartAlutrackServer.UseVisualStyleBackColor = true;
            this.btnStartAlutrackServer.Click += new System.EventHandler(this.btnSTARTAlutrackServer_Click);
            // 
            // btnStopAlutrackServer
            // 
            this.btnStopAlutrackServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStopAlutrackServer.BackgroundImage")));
            this.btnStopAlutrackServer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStopAlutrackServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStopAlutrackServer.Location = new System.Drawing.Point(34, 511);
            this.btnStopAlutrackServer.Name = "btnStopAlutrackServer";
            this.btnStopAlutrackServer.Size = new System.Drawing.Size(40, 40);
            this.btnStopAlutrackServer.TabIndex = 95;
            this.btnStopAlutrackServer.UseVisualStyleBackColor = true;
            this.btnStopAlutrackServer.Visible = false;
            this.btnStopAlutrackServer.Click += new System.EventHandler(this.btnSTOPAlutrackServer_Click);
            // 
            // btnStartLENELServer
            // 
            this.btnStartLENELServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStartLENELServer.BackgroundImage")));
            this.btnStartLENELServer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStartLENELServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartLENELServer.Location = new System.Drawing.Point(400, 508);
            this.btnStartLENELServer.Name = "btnStartLENELServer";
            this.btnStartLENELServer.Size = new System.Drawing.Size(40, 40);
            this.btnStartLENELServer.TabIndex = 96;
            this.btnStartLENELServer.UseVisualStyleBackColor = true;
            this.btnStartLENELServer.Click += new System.EventHandler(this.btnStartLenelServer_Click);
            // 
            // btnStopLENELServer
            // 
            this.btnStopLENELServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStopLENELServer.BackgroundImage")));
            this.btnStopLENELServer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStopLENELServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStopLENELServer.Location = new System.Drawing.Point(400, 508);
            this.btnStopLENELServer.Name = "btnStopLENELServer";
            this.btnStopLENELServer.Size = new System.Drawing.Size(40, 40);
            this.btnStopLENELServer.TabIndex = 97;
            this.btnStopLENELServer.UseVisualStyleBackColor = true;
            this.btnStopLENELServer.Visible = false;
            this.btnStopLENELServer.Click += new System.EventHandler(this.btnStopLENELServer_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(720, 508);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 20);
            this.button6.TabIndex = 98;
            this.button6.Text = "Clear";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(308, 511);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 99;
            this.button7.Text = "Clear";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(540, 526);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(113, 25);
            this.button8.TabIndex = 100;
            this.button8.Text = "Reset servidorDatos";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click_1);
            // 
            // chkVerboseLOGAlutrack
            // 
            this.chkVerboseLOGAlutrack.AutoSize = true;
            this.chkVerboseLOGAlutrack.Location = new System.Drawing.Point(315, 540);
            this.chkVerboseLOGAlutrack.Name = "chkVerboseLOGAlutrack";
            this.chkVerboseLOGAlutrack.Size = new System.Drawing.Size(68, 17);
            this.chkVerboseLOGAlutrack.TabIndex = 100;
            this.chkVerboseLOGAlutrack.Text = "Shut Up!";
            this.chkVerboseLOGAlutrack.UseVisualStyleBackColor = true;
            // 
            // chkVerboseLOGLenel
            // 
            this.chkVerboseLOGLenel.AutoSize = true;
            this.chkVerboseLOGLenel.Location = new System.Drawing.Point(720, 534);
            this.chkVerboseLOGLenel.Name = "chkVerboseLOGLenel";
            this.chkVerboseLOGLenel.Size = new System.Drawing.Size(68, 17);
            this.chkVerboseLOGLenel.TabIndex = 101;
            this.chkVerboseLOGLenel.Text = "Shut Up!";
            this.chkVerboseLOGLenel.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(118, 61);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(70, 36);
            this.button4.TabIndex = 102;
            this.button4.Text = "Inyectar GPS";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_2);
            // 
            // lstAlutrackServerLOG
            // 
            this.lstAlutrackServerLOG.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstAlutrackServerLOG.FormattingEnabled = true;
            this.lstAlutrackServerLOG.HorizontalScrollbar = true;
            this.lstAlutrackServerLOG.ItemHeight = 16;
            this.lstAlutrackServerLOG.Location = new System.Drawing.Point(801, 184);
            this.lstAlutrackServerLOG.Name = "lstAlutrackServerLOG";
            this.lstAlutrackServerLOG.Size = new System.Drawing.Size(346, 324);
            this.lstAlutrackServerLOG.TabIndex = 103;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 15.75F);
            this.label3.Location = new System.Drawing.Point(824, 155);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 26);
            this.label3.TabIndex = 104;
            this.label3.Text = "Alutrack Web";
            // 
            // btnStopAlutrackServerWeb
            // 
            this.btnStopAlutrackServerWeb.Location = new System.Drawing.Point(869, 509);
            this.btnStopAlutrackServerWeb.Name = "btnStopAlutrackServerWeb";
            this.btnStopAlutrackServerWeb.Size = new System.Drawing.Size(65, 33);
            this.btnStopAlutrackServerWeb.TabIndex = 105;
            this.btnStopAlutrackServerWeb.Text = "Stop";
            this.btnStopAlutrackServerWeb.UseVisualStyleBackColor = true;
            this.btnStopAlutrackServerWeb.Visible = false;
            this.btnStopAlutrackServerWeb.Click += new System.EventHandler(this.btnStopAlutrackServerWeb_Click_1);
            // 
            // btnStartAlutrackServerWeb
            // 
            this.btnStartAlutrackServerWeb.Location = new System.Drawing.Point(800, 509);
            this.btnStartAlutrackServerWeb.Name = "btnStartAlutrackServerWeb";
            this.btnStartAlutrackServerWeb.Size = new System.Drawing.Size(63, 33);
            this.btnStartAlutrackServerWeb.TabIndex = 106;
            this.btnStartAlutrackServerWeb.Text = "Start";
            this.btnStartAlutrackServerWeb.UseVisualStyleBackColor = true;
            this.btnStartAlutrackServerWeb.Click += new System.EventHandler(this.btnStartAlutrackServerWeb_Click);
            // 
            // chkVerboseLOGAlutrackServerWeb
            // 
            this.chkVerboseLOGAlutrackServerWeb.AutoSize = true;
            this.chkVerboseLOGAlutrackServerWeb.Location = new System.Drawing.Point(1075, 535);
            this.chkVerboseLOGAlutrackServerWeb.Name = "chkVerboseLOGAlutrackServerWeb";
            this.chkVerboseLOGAlutrackServerWeb.Size = new System.Drawing.Size(68, 17);
            this.chkVerboseLOGAlutrackServerWeb.TabIndex = 107;
            this.chkVerboseLOGAlutrackServerWeb.Text = "Shut Up!";
            this.chkVerboseLOGAlutrackServerWeb.UseVisualStyleBackColor = true;
            // 
            // btnClearAlutrack
            // 
            this.btnClearAlutrack.Location = new System.Drawing.Point(1072, 509);
            this.btnClearAlutrack.Name = "btnClearAlutrack";
            this.btnClearAlutrack.Size = new System.Drawing.Size(75, 20);
            this.btnClearAlutrack.TabIndex = 108;
            this.btnClearAlutrack.Text = "Clear";
            this.btnClearAlutrack.UseVisualStyleBackColor = true;
            this.btnClearAlutrack.Click += new System.EventHandler(this.btnClearAlutrack_Click);
            // 
            // frmInicial
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1159, 567);
            this.Controls.Add(this.btnClearAlutrack);
            this.Controls.Add(this.chkVerboseLOGAlutrackServerWeb);
            this.Controls.Add(this.btnStartAlutrackServerWeb);
            this.Controls.Add(this.btnStopAlutrackServerWeb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lstAlutrackServerLOG);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.chkVerboseLOGLenel);
            this.Controls.Add(this.chkVerboseLOGAlutrack);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.lstAlutrackLOG);
            this.Controls.Add(this.btnStartLENELServer);
            this.Controls.Add(this.btnStopLENELServer);
            this.Controls.Add(this.btnStartAlutrackServer);
            this.Controls.Add(this.lstLenelLOG);
            this.Controls.Add(this.btnStopAlutrackServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.lblPuertos);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.btnVirtualGate);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDevices);
            this.Controls.Add(this.btnUsuarios);
            this.Controls.Add(this.label1);
            this.Name = "frmInicial";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mobile Access Control System";
            this.Load += new System.EventHandler(this.frmConfiguration_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmInicial_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmInicial_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUsuarios;
        private System.Windows.Forms.Button btnDevices;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListBox lstAlutrackLOG;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnVirtualGate;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Timer tmrRebind;
        private System.Windows.Forms.Label lblPuertos;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstLenelLOG;
        private System.Windows.Forms.Button btnStartAlutrackServer;
        private System.Windows.Forms.Button btnStopAlutrackServer;
        private System.Windows.Forms.Button btnStartLENELServer;
        private System.Windows.Forms.Button btnStopLENELServer;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckBox chkVerboseLOGAlutrack;
        private System.Windows.Forms.CheckBox chkVerboseLOGLenel;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ListBox lstAlutrackServerLOG;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnStopAlutrackServerWeb;
        private System.Windows.Forms.Button btnStartAlutrackServerWeb;
        private System.Windows.Forms.CheckBox chkVerboseLOGAlutrackServerWeb;
        private System.Windows.Forms.Button btnClearAlutrack;
    }
}