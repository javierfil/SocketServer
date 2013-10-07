namespace ServerComunicacionesALUTEL
{
    partial class frmGrupos
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
            this.lstGruposUsuarios = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lstDevicesDelGrupo = new System.Windows.Forms.ListBox();
            this.lstGruposDevices = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnEliminarGrupoUsuarios = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.btnCrearGrupoDevices = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.btnCrearGrupoUsuarios = new System.Windows.Forms.Button();
            this.listViewUsuarios = new System.Windows.Forms.ListView();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lstGruposUsuarios
            // 
            this.lstGruposUsuarios.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstGruposUsuarios.FormattingEnabled = true;
            this.lstGruposUsuarios.ItemHeight = 16;
            this.lstGruposUsuarios.Location = new System.Drawing.Point(29, 99);
            this.lstGruposUsuarios.Name = "lstGruposUsuarios";
            this.lstGruposUsuarios.Size = new System.Drawing.Size(158, 244);
            this.lstGruposUsuarios.TabIndex = 0;
            this.lstGruposUsuarios.SelectedIndexChanged += new System.EventHandler(this.lstGruposUsuarios_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(25, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "User groups";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(203, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Selected Users";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(560, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Device groups";
            // 
            // lstDevicesDelGrupo
            // 
            this.lstDevicesDelGrupo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstDevicesDelGrupo.FormattingEnabled = true;
            this.lstDevicesDelGrupo.ItemHeight = 16;
            this.lstDevicesDelGrupo.Location = new System.Drawing.Point(738, 99);
            this.lstDevicesDelGrupo.Name = "lstDevicesDelGrupo";
            this.lstDevicesDelGrupo.Size = new System.Drawing.Size(181, 244);
            this.lstDevicesDelGrupo.TabIndex = 7;
            // 
            // lstGruposDevices
            // 
            this.lstGruposDevices.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstGruposDevices.FormattingEnabled = true;
            this.lstGruposDevices.ItemHeight = 16;
            this.lstGruposDevices.Location = new System.Drawing.Point(564, 98);
            this.lstGruposDevices.Name = "lstGruposDevices";
            this.lstGruposDevices.Size = new System.Drawing.Size(168, 244);
            this.lstGruposDevices.TabIndex = 6;
            this.lstGruposDevices.SelectedIndexChanged += new System.EventHandler(this.lstGruposDevices_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(738, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "Selected devices";
            // 
            // btnEliminarGrupoUsuarios
            // 
            this.btnEliminarGrupoUsuarios.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEliminarGrupoUsuarios.Location = new System.Drawing.Point(112, 349);
            this.btnEliminarGrupoUsuarios.Name = "btnEliminarGrupoUsuarios";
            this.btnEliminarGrupoUsuarios.Size = new System.Drawing.Size(75, 33);
            this.btnEliminarGrupoUsuarios.TabIndex = 11;
            this.btnEliminarGrupoUsuarios.Text = "Delete";
            this.btnEliminarGrupoUsuarios.UseVisualStyleBackColor = true;
            this.btnEliminarGrupoUsuarios.Click += new System.EventHandler(this.button3_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.SystemColors.Control;
            this.label5.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.label5.Font = new System.Drawing.Font("Calibri", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(52, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(207, 33);
            this.label5.TabIndex = 13;
            this.label5.Text = "Groups Definition";
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(657, 349);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 33);
            this.button4.TabIndex = 16;
            this.button4.Text = "Delete";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // btnCrearGrupoDevices
            // 
            this.btnCrearGrupoDevices.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCrearGrupoDevices.Location = new System.Drawing.Point(564, 349);
            this.btnCrearGrupoDevices.Name = "btnCrearGrupoDevices";
            this.btnCrearGrupoDevices.Size = new System.Drawing.Size(75, 33);
            this.btnCrearGrupoDevices.TabIndex = 14;
            this.btnCrearGrupoDevices.Text = "Create";
            this.btnCrearGrupoDevices.UseVisualStyleBackColor = true;
            this.btnCrearGrupoDevices.Click += new System.EventHandler(this.btnCrearGrupoDevices_Click);
            // 
            // button7
            // 
            this.button7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button7.Location = new System.Drawing.Point(828, 400);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(91, 37);
            this.button7.TabIndex = 17;
            this.button7.Text = "Close";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // btnCrearGrupoUsuarios
            // 
            this.btnCrearGrupoUsuarios.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCrearGrupoUsuarios.Location = new System.Drawing.Point(29, 349);
            this.btnCrearGrupoUsuarios.Name = "btnCrearGrupoUsuarios";
            this.btnCrearGrupoUsuarios.Size = new System.Drawing.Size(75, 33);
            this.btnCrearGrupoUsuarios.TabIndex = 18;
            this.btnCrearGrupoUsuarios.Text = "Create";
            this.btnCrearGrupoUsuarios.UseVisualStyleBackColor = true;
            this.btnCrearGrupoUsuarios.Click += new System.EventHandler(this.btnCrearGrupoUsuarios_Click);
            // 
            // listViewUsuarios
            // 
            this.listViewUsuarios.Location = new System.Drawing.Point(202, 99);
            this.listViewUsuarios.Name = "listViewUsuarios";
            this.listViewUsuarios.Size = new System.Drawing.Size(343, 243);
            this.listViewUsuarios.TabIndex = 19;
            this.listViewUsuarios.UseCompatibleStateImageBehavior = false;
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ServerComunicacionesALUTEL.Properties.Resources.user_group_iconch;
            this.pictureBox1.Location = new System.Drawing.Point(12, 11);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(38, 35);
            this.pictureBox1.TabIndex = 89;
            this.pictureBox1.TabStop = false;
            // 
            // frmGrupos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 442);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.listViewUsuarios);
            this.Controls.Add(this.btnCrearGrupoUsuarios);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btnCrearGrupoDevices);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnEliminarGrupoUsuarios);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lstDevicesDelGrupo);
            this.Controls.Add(this.lstGruposDevices);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstGruposUsuarios);
            this.Name = "frmGrupos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Groups definition";
            this.Load += new System.EventHandler(this.frmGrupos_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstGruposUsuarios;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lstDevicesDelGrupo;
        private System.Windows.Forms.ListBox lstGruposDevices;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnEliminarGrupoUsuarios;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button btnCrearGrupoDevices;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button btnCrearGrupoUsuarios;
        private System.Windows.Forms.ListView listViewUsuarios;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}