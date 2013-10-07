namespace ServerComunicacionesALUTEL
{
    partial class frmVirtualGate
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
            this.label1 = new System.Windows.Forms.Label();
            this.webBrowser2 = new System.Windows.Forms.WebBrowser();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCrearZona = new System.Windows.Forms.Button();
            this.btnAddPoints = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDeletePoint = new System.Windows.Forms.Button();
            this.listViewPoints = new System.Windows.Forms.ListView();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbHandHeld = new System.Windows.Forms.ComboBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.listViewAccesosVirtualGate = new System.Windows.Forms.ListView();
            this.label6 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDeleteAll = new System.Windows.Forms.Button();
            this.listViewGates = new System.Windows.Forms.ListView();
            this.listViewZones = new System.Windows.Forms.ListView();
            this.btnAccess = new System.Windows.Forms.Button();
            this.btnNoAccess = new System.Windows.Forms.Button();
            this.btnDeleteZone = new System.Windows.Forms.Button();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.trStart = new System.Windows.Forms.TrackBar();
            this.trEnd = new System.Windows.Forms.TrackBar();
            this.optOptimize = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trEnd)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.label1.Font = new System.Drawing.Font("Calibri", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(65, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(279, 36);
            this.label1.TabIndex = 13;
            this.label1.Text = "Virtual Gate Definition";
            // 
            // webBrowser2
            // 
            this.webBrowser2.Location = new System.Drawing.Point(12, 98);
            this.webBrowser2.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser2.Name = "webBrowser2";
            this.webBrowser2.Size = new System.Drawing.Size(650, 392);
            this.webBrowser2.TabIndex = 14;
            this.webBrowser2.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser2_DocumentCompleted);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ServerComunicacionesALUTEL.Properties.Resources.user_group_iconch;
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Location = new System.Drawing.Point(12, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(38, 35);
            this.pictureBox1.TabIndex = 91;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(669, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 20);
            this.label2.TabIndex = 92;
            this.label2.Text = "Zones";
            // 
            // btnCrearZona
            // 
            this.btnCrearZona.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCrearZona.Location = new System.Drawing.Point(568, 66);
            this.btnCrearZona.Name = "btnCrearZona";
            this.btnCrearZona.Size = new System.Drawing.Size(80, 26);
            this.btnCrearZona.TabIndex = 95;
            this.btnCrearZona.Text = "Create Zone";
            this.btnCrearZona.UseVisualStyleBackColor = false;
            this.btnCrearZona.Click += new System.EventHandler(this.btnCrearZona_Click);
            // 
            // btnAddPoints
            // 
            this.btnAddPoints.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnAddPoints.Location = new System.Drawing.Point(497, 66);
            this.btnAddPoints.Name = "btnAddPoints";
            this.btnAddPoints.Size = new System.Drawing.Size(65, 26);
            this.btnAddPoints.TabIndex = 94;
            this.btnAddPoints.Text = "Add points";
            this.btnAddPoints.UseVisualStyleBackColor = false;
            this.btnAddPoints.Click += new System.EventHandler(this.btnAddPoint_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(669, 193);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 20);
            this.label3.TabIndex = 97;
            this.label3.Text = "Gates";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(668, 358);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 20);
            this.label4.TabIndex = 99;
            this.label4.Text = "Points";
            // 
            // btnDeletePoint
            // 
            this.btnDeletePoint.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDeletePoint.Location = new System.Drawing.Point(812, 496);
            this.btnDeletePoint.Name = "btnDeletePoint";
            this.btnDeletePoint.Size = new System.Drawing.Size(87, 26);
            this.btnDeletePoint.TabIndex = 101;
            this.btnDeletePoint.Text = "Delete point";
            this.btnDeletePoint.UseVisualStyleBackColor = false;
            this.btnDeletePoint.Click += new System.EventHandler(this.btnDeletePoint_Click);
            // 
            // listViewPoints
            // 
            this.listViewPoints.FullRowSelect = true;
            this.listViewPoints.GridLines = true;
            this.listViewPoints.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewPoints.Location = new System.Drawing.Point(672, 381);
            this.listViewPoints.Name = "listViewPoints";
            this.listViewPoints.Size = new System.Drawing.Size(227, 109);
            this.listViewPoints.TabIndex = 102;
            this.listViewPoints.UseCompatibleStateImageBehavior = false;
            this.listViewPoints.SelectedIndexChanged += new System.EventHandler(this.listViewPoints_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(12, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 16);
            this.label5.TabIndex = 107;
            this.label5.Text = "Track Handheld:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbHandHeld
            // 
            this.cmbHandHeld.FormattingEnabled = true;
            this.cmbHandHeld.Location = new System.Drawing.Point(126, 74);
            this.cmbHandHeld.Name = "cmbHandHeld";
            this.cmbHandHeld.Size = new System.Drawing.Size(144, 21);
            this.cmbHandHeld.TabIndex = 108;
            this.cmbHandHeld.SelectedIndexChanged += new System.EventHandler(this.cmbHandHeld_SelectedIndexChanged);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(276, 66);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(98, 26);
            this.btnReset.TabIndex = 109;
            this.btnReset.Text = "resetearTrack";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // listViewAccesosVirtualGate
            // 
            this.listViewAccesosVirtualGate.FullRowSelect = true;
            this.listViewAccesosVirtualGate.GridLines = true;
            this.listViewAccesosVirtualGate.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewAccesosVirtualGate.Location = new System.Drawing.Point(12, 528);
            this.listViewAccesosVirtualGate.Name = "listViewAccesosVirtualGate";
            this.listViewAccesosVirtualGate.Size = new System.Drawing.Size(888, 150);
            this.listViewAccesosVirtualGate.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listViewAccesosVirtualGate.TabIndex = 110;
            this.listViewAccesosVirtualGate.UseCompatibleStateImageBehavior = false;
            this.listViewAccesosVirtualGate.SelectedIndexChanged += new System.EventHandler(this.listViewAccesosVirtualGate_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(8, 505);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(155, 20);
            this.label6.TabIndex = 111;
            this.label6.Text = "Zone Access Events";
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(824, 783);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 32);
            this.btnClose.TabIndex = 112;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDeleteAll
            // 
            this.btnDeleteAll.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDeleteAll.Location = new System.Drawing.Point(417, 66);
            this.btnDeleteAll.Name = "btnDeleteAll";
            this.btnDeleteAll.Size = new System.Drawing.Size(65, 26);
            this.btnDeleteAll.TabIndex = 113;
            this.btnDeleteAll.Text = "Delete all";
            this.btnDeleteAll.UseVisualStyleBackColor = false;
            this.btnDeleteAll.Click += new System.EventHandler(this.btnDeleteAll_Click);
            // 
            // listViewGates
            // 
            this.listViewGates.FullRowSelect = true;
            this.listViewGates.GridLines = true;
            this.listViewGates.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewGates.Location = new System.Drawing.Point(673, 216);
            this.listViewGates.Name = "listViewGates";
            this.listViewGates.Size = new System.Drawing.Size(227, 112);
            this.listViewGates.TabIndex = 114;
            this.listViewGates.UseCompatibleStateImageBehavior = false;
            this.listViewGates.SelectedIndexChanged += new System.EventHandler(this.listViewGates_SelectedIndexChanged);
            // 
            // listViewZones
            // 
            this.listViewZones.FullRowSelect = true;
            this.listViewZones.GridLines = true;
            this.listViewZones.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewZones.Location = new System.Drawing.Point(673, 101);
            this.listViewZones.Name = "listViewZones";
            this.listViewZones.Size = new System.Drawing.Size(226, 77);
            this.listViewZones.TabIndex = 115;
            this.listViewZones.UseCompatibleStateImageBehavior = false;
            this.listViewZones.SelectedIndexChanged += new System.EventHandler(this.listViewZones_SelectedIndexChanged);
            // 
            // btnAccess
            // 
            this.btnAccess.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnAccess.Location = new System.Drawing.Point(744, 334);
            this.btnAccess.Name = "btnAccess";
            this.btnAccess.Size = new System.Drawing.Size(75, 26);
            this.btnAccess.TabIndex = 116;
            this.btnAccess.Text = "Access";
            this.btnAccess.UseVisualStyleBackColor = false;
            this.btnAccess.Click += new System.EventHandler(this.btnGrant_Click);
            // 
            // btnNoAccess
            // 
            this.btnNoAccess.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnNoAccess.Location = new System.Drawing.Point(825, 334);
            this.btnNoAccess.Name = "btnNoAccess";
            this.btnNoAccess.Size = new System.Drawing.Size(75, 26);
            this.btnNoAccess.TabIndex = 117;
            this.btnNoAccess.Text = "No access";
            this.btnNoAccess.UseVisualStyleBackColor = false;
            this.btnNoAccess.Click += new System.EventHandler(this.btnDeny_Click);
            // 
            // btnDeleteZone
            // 
            this.btnDeleteZone.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDeleteZone.Location = new System.Drawing.Point(824, 184);
            this.btnDeleteZone.Name = "btnDeleteZone";
            this.btnDeleteZone.Size = new System.Drawing.Size(75, 26);
            this.btnDeleteZone.TabIndex = 118;
            this.btnDeleteZone.Text = "Delete";
            this.btnDeleteZone.UseVisualStyleBackColor = false;
            this.btnDeleteZone.Click += new System.EventHandler(this.btnDeleteZone_Click);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button1.Location = new System.Drawing.Point(819, 69);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 26);
            this.button1.TabIndex = 119;
            this.button1.Text = "View Zones";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // trStart
            // 
            this.trStart.LargeChange = 1;
            this.trStart.Location = new System.Drawing.Point(350, 9);
            this.trStart.Name = "trStart";
            this.trStart.Size = new System.Drawing.Size(259, 45);
            this.trStart.TabIndex = 120;
            this.trStart.Scroll += new System.EventHandler(this.trStart_Scroll);
            // 
            // trEnd
            // 
            this.trEnd.LargeChange = 1;
            this.trEnd.Location = new System.Drawing.Point(615, 9);
            this.trEnd.Name = "trEnd";
            this.trEnd.Size = new System.Drawing.Size(266, 45);
            this.trEnd.TabIndex = 121;
            this.trEnd.Scroll += new System.EventHandler(this.trEnd_Scroll);
            // 
            // optOptimize
            // 
            this.optOptimize.AutoSize = true;
            this.optOptimize.Location = new System.Drawing.Point(350, 43);
            this.optOptimize.Name = "optOptimize";
            this.optOptimize.Size = new System.Drawing.Size(69, 17);
            this.optOptimize.TabIndex = 122;
            this.optOptimize.Text = "Optimizar";
            this.optOptimize.UseVisualStyleBackColor = true;
            this.optOptimize.CheckedChanged += new System.EventHandler(this.optOptimize_CheckedChanged);
            // 
            // frmVirtualGate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(911, 713);
            this.Controls.Add(this.optOptimize);
            this.Controls.Add(this.trEnd);
            this.Controls.Add(this.trStart);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDeleteZone);
            this.Controls.Add(this.btnNoAccess);
            this.Controls.Add(this.btnAccess);
            this.Controls.Add(this.listViewZones);
            this.Controls.Add(this.listViewGates);
            this.Controls.Add(this.btnDeleteAll);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.listViewAccesosVirtualGate);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.cmbHandHeld);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.listViewPoints);
            this.Controls.Add(this.btnDeletePoint);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCrearZona);
            this.Controls.Add(this.btnAddPoints);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.webBrowser2);
            this.Controls.Add(this.label1);
            this.Name = "frmVirtualGate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Virtual Gate Definition";
            this.Load += new System.EventHandler(this.frmVirtualGate_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trEnd)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.WebBrowser webBrowser2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCrearZona;
        private System.Windows.Forms.Button btnAddPoints;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnDeletePoint;
        private System.Windows.Forms.ListView listViewPoints;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbHandHeld;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.ListView listViewAccesosVirtualGate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDeleteAll;
        private System.Windows.Forms.ListView listViewGates;
        private System.Windows.Forms.ListView listViewZones;
        private System.Windows.Forms.Button btnAccess;
        private System.Windows.Forms.Button btnNoAccess;
        private System.Windows.Forms.Button btnDeleteZone;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TrackBar trStart;
        private System.Windows.Forms.TrackBar trEnd;
        private System.Windows.Forms.CheckBox optOptimize;
    }
}