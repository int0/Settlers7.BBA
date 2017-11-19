namespace Settlers6.BBA
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( Form1 ) );
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnMake = new System.Windows.Forms.Button();
            this.btnSaveAll = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip( this.components );
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 192 ) ) ) ) );
            this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView1.Location = new System.Drawing.Point( 12, 12 );
            this.treeView1.Name = "treeView1";
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.Size = new System.Drawing.Size( 545, 365 );
            this.treeView1.TabIndex = 0;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point( 351, 521 );
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size( 100, 40 );
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler( this.btnOpen_Click );
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point( 457, 521 );
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size( 100, 40 );
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler( this.btnClose_Click );
            // 
            // btnMake
            // 
            this.btnMake.Location = new System.Drawing.Point( 117, 521 );
            this.btnMake.Name = "btnMake";
            this.btnMake.Size = new System.Drawing.Size( 100, 40 );
            this.btnMake.TabIndex = 3;
            this.btnMake.Text = "Make";
            this.btnMake.UseVisualStyleBackColor = true;
            this.btnMake.Click += new System.EventHandler( this.button3_Click );
            // 
            // btnSaveAll
            // 
            this.btnSaveAll.Location = new System.Drawing.Point( 11, 521 );
            this.btnSaveAll.Name = "btnSaveAll";
            this.btnSaveAll.Size = new System.Drawing.Size( 100, 40 );
            this.btnSaveAll.TabIndex = 4;
            this.btnSaveAll.Text = "Save All";
            this.btnSaveAll.UseVisualStyleBackColor = true;
            this.btnSaveAll.Click += new System.EventHandler( this.button4_Click );
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.Color.Black;
            this.progressBar1.ForeColor = System.Drawing.Color.Lime;
            this.progressBar1.Location = new System.Drawing.Point( 12, 484 );
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size( 545, 23 );
            this.progressBar1.TabIndex = 5;
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.Black;
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox1.Font = new System.Drawing.Font( "Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 204 ) ) );
            this.listBox1.ForeColor = System.Drawing.Color.Lime;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 14;
            this.listBox1.Location = new System.Drawing.Point( 12, 383 );
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size( 545, 100 );
            this.listBox1.TabIndex = 6;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Settlers 6 (*.bba)|*.bba";
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Settlers 6 (*.bba)|*.bba";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point( 189, 535 );
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size( 15, 14 );
            this.checkBox1.TabIndex = 7;
            this.toolTip1.SetToolTip( this.checkBox1, "Whats this:\r\nPacker automatically compress only text files, scripts, etc...,\r\nto " +
                    "force compression for ALL files check this checkbox." );
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange( new object[] {
            "Retail",
            "Demo"} );
            this.comboBox1.Location = new System.Drawing.Point( 223, 521 );
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size( 121, 21 );
            this.comboBox1.TabIndex = 8;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler( this.comboBox1_SelectedIndexChanged );
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 569, 573 );
            this.Controls.Add( this.comboBox1 );
            this.Controls.Add( this.checkBox1 );
            this.Controls.Add( this.listBox1 );
            this.Controls.Add( this.progressBar1 );
            this.Controls.Add( this.btnSaveAll );
            this.Controls.Add( this.btnMake );
            this.Controls.Add( this.btnClose );
            this.Controls.Add( this.btnOpen );
            this.Controls.Add( this.treeView1 );
            this.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 204 ) ) );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ( (System.Drawing.Icon)( resources.GetObject( "$this.Icon" ) ) );
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settlers 7 *RETAIL* .BBA Tool v1.01";
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMake;
        private System.Windows.Forms.Button btnSaveAll;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

