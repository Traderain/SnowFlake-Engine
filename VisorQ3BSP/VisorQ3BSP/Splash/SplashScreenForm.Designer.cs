﻿namespace VisorQ3BSP.Splash
{
    partial class SplashScreenForm
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
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreenForm));
        	this.progressBar1 = new System.Windows.Forms.ProgressBar();
        	this.label1 = new System.Windows.Forms.Label();
        	this.pictureBox1 = new System.Windows.Forms.PictureBox();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// progressBar1
        	// 
        	this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.progressBar1.Location = new System.Drawing.Point(0, 139);
        	this.progressBar1.MarqueeAnimationSpeed = 50;
        	this.progressBar1.Name = "progressBar1";
        	this.progressBar1.Size = new System.Drawing.Size(502, 10);
        	this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
        	this.progressBar1.TabIndex = 0;
        	// 
        	// label1
        	// 
        	this.label1.AutoSize = true;
        	this.label1.BackColor = System.Drawing.Color.Transparent;
        	this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.label1.ForeColor = System.Drawing.Color.Green;
        	this.label1.Location = new System.Drawing.Point(12, 117);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(153, 19);
        	this.label1.TabIndex = 1;
        	this.label1.Text = "Loading BSP map ...";
        	// 
        	// pictureBox1
        	// 
        	this.pictureBox1.BackColor = System.Drawing.Color.Black;
        	this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
        	this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        	this.pictureBox1.Cursor = System.Windows.Forms.Cursors.WaitCursor;
        	this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        	this.pictureBox1.Name = "pictureBox1";
        	this.pictureBox1.Size = new System.Drawing.Size(502, 139);
        	this.pictureBox1.TabIndex = 2;
        	this.pictureBox1.TabStop = false;
        	// 
        	// SplashScreenForm
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.Azure;
        	this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
        	this.ClientSize = new System.Drawing.Size(502, 149);
        	this.Controls.Add(this.label1);
        	this.Controls.Add(this.pictureBox1);
        	this.Controls.Add(this.progressBar1);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        	this.Name = "SplashScreenForm";
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "SplashForm";
        	this.TopMost = true;
        	this.TransparencyKey = System.Drawing.Color.Transparent;
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SplashForm_FormClosing);
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}