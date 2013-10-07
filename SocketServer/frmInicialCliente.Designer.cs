namespace ServerComunicacionesALUTEL
{
    partial class frmInicialCliente
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmInicialCliente));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblOnGuardStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.btnConfiguration = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.tmrRebindClient = new System.Windows.Forms.Timer(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.tmrUpdateList = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ServerComunicacionesALUTEL.Properties.Resources.ALUTELlogo;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(99, 42);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 61;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 20);
            this.label1.TabIndex = 62;
            this.label1.Text = "OnGuard Server:";
            // 
            // lblServerStatus
            // 
            this.lblServerStatus.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.lblServerStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServerStatus.Location = new System.Drawing.Point(143, 69);
            this.lblServerStatus.Name = "lblServerStatus";
            this.lblServerStatus.Size = new System.Drawing.Size(137, 25);
            this.lblServerStatus.TabIndex = 63;
            this.lblServerStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 20);
            this.label2.TabIndex = 64;
            this.label2.Text = "Device Server:";
            // 
            // lblOnGuardStatus
            // 
            this.lblOnGuardStatus.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.lblOnGuardStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOnGuardStatus.Location = new System.Drawing.Point(143, 104);
            this.lblOnGuardStatus.Name = "lblOnGuardStatus";
            this.lblOnGuardStatus.Size = new System.Drawing.Size(137, 25);
            this.lblOnGuardStatus.TabIndex = 63;
            this.lblOnGuardStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(144, 20);
            this.label4.TabIndex = 65;
            this.label4.Text = "Registered devices";
            // 
            // listViewDevices
            // 
            this.listViewDevices.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewDevices.FullRowSelect = true;
            this.listViewDevices.Location = new System.Drawing.Point(16, 179);
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(341, 161);
            this.listViewDevices.TabIndex = 66;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            // 
            // btnConfiguration
            // 
            this.btnConfiguration.Location = new System.Drawing.Point(16, 346);
            this.btnConfiguration.Name = "btnConfiguration";
            this.btnConfiguration.Size = new System.Drawing.Size(81, 31);
            this.btnConfiguration.TabIndex = 84;
            this.btnConfiguration.Text = "Configuration";
            this.btnConfiguration.UseVisualStyleBackColor = true;
            this.btnConfiguration.Click += new System.EventHandler(this.btnConfiguration_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(287, 346);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(70, 31);
            this.button1.TabIndex = 85;
            this.button1.Text = "Exit";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnRestart
            // 
            this.btnRestart.BackColor = System.Drawing.SystemColors.HighlightText;
            this.btnRestart.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRestart.BackgroundImage")));
            this.btnRestart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRestart.Location = new System.Drawing.Point(297, 69);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(60, 60);
            this.btnRestart.TabIndex = 86;
            this.btnRestart.UseVisualStyleBackColor = false;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "ALUTEL Mobility Server";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // tmrRebindClient
            // 
            this.tmrRebindClient.Enabled = true;
            this.tmrRebindClient.Interval = 400;
            this.tmrRebindClient.Tick += new System.EventHandler(this.tmrRebindClient_Tick);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(297, 46);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 20);
            this.label6.TabIndex = 87;
            this.label6.Text = "Restart";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // tmrUpdateList
            // 
            this.tmrUpdateList.Interval = 500;
            this.tmrUpdateList.Tick += new System.EventHandler(this.tmrUpdateList_Tick);
            // 
            // frmInicialCliente
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 388);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnConfiguration);
            this.Controls.Add(this.listViewDevices);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblOnGuardStatus);
            this.Controls.Add(this.lblServerStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "frmInicialCliente";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ALUTEL Mobility Server";
            this.Load += new System.EventHandler(this.frmInicialCliente_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmInicialCliente_FormClosing);
            this.Resize += new System.EventHandler(this.frmInicialCliente_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblOnGuardStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.Button btnConfiguration;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Timer tmrUpdateList;
        public System.Windows.Forms.Timer tmrRebindClient;
    }
}