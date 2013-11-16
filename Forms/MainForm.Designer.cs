﻿namespace Elmanager.Forms
{
	public partial class MainForm : System.Windows.Forms.Form
		{
		
		//Form overrides dispose to clean up the component list.
		protected override void Dispose(bool disposing)
			{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//Required by the Windows Form Designer
		private System.ComponentModel.IContainer components = null;
		
		//The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		private void InitializeComponent()
			{
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
                this.rmButton = new System.Windows.Forms.Button();
                this.levelEditorButton = new System.Windows.Forms.Button();
                this.titleLabel = new System.Windows.Forms.Label();
                this.byLabel = new System.Windows.Forms.Label();
                this.versionLabel = new System.Windows.Forms.Label();
                this.homePageLabel = new System.Windows.Forms.LinkLabel();
                this.configButton = new System.Windows.Forms.Button();
                this.SuspendLayout();
                // 
                // rmButton
                // 
                this.rmButton.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                this.rmButton.Location = new System.Drawing.Point(87, 122);
                this.rmButton.Name = "rmButton";
                this.rmButton.Size = new System.Drawing.Size(138, 23);
                this.rmButton.TabIndex = 1;
                this.rmButton.Text = "Replay manager";
                this.rmButton.UseVisualStyleBackColor = true;
                this.rmButton.Click += new System.EventHandler(this.OpenReplayManager);
                // 
                // levelEditorButton
                // 
                this.levelEditorButton.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                this.levelEditorButton.Location = new System.Drawing.Point(87, 151);
                this.levelEditorButton.Name = "levelEditorButton";
                this.levelEditorButton.Size = new System.Drawing.Size(138, 23);
                this.levelEditorButton.TabIndex = 2;
                this.levelEditorButton.Text = "SLE";
                this.levelEditorButton.UseVisualStyleBackColor = true;
                this.levelEditorButton.Click += new System.EventHandler(this.OpenLevelEditor);
                // 
                // titleLabel
                // 
                this.titleLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                this.titleLabel.Font = new System.Drawing.Font("Arial Rounded MT Bold", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
                this.titleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
                this.titleLabel.Location = new System.Drawing.Point(12, 9);
                this.titleLabel.Name = "titleLabel";
                this.titleLabel.Size = new System.Drawing.Size(288, 55);
                this.titleLabel.TabIndex = 3;
                this.titleLabel.Text = "Elmanager";
                this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // 
                // byLabel
                // 
                this.byLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                this.byLabel.Font = new System.Drawing.Font("Arial Rounded MT Bold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
                this.byLabel.ForeColor = System.Drawing.SystemColors.ControlText;
                this.byLabel.Location = new System.Drawing.Point(12, 229);
                this.byLabel.Name = "byLabel";
                this.byLabel.Size = new System.Drawing.Size(288, 32);
                this.byLabel.TabIndex = 4;
                this.byLabel.Text = "by Smibu";
                this.byLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // 
                // versionLabel
                // 
                this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                this.versionLabel.Location = new System.Drawing.Point(12, 76);
                this.versionLabel.Name = "versionLabel";
                this.versionLabel.Size = new System.Drawing.Size(288, 13);
                this.versionLabel.TabIndex = 5;
                this.versionLabel.Text = "Version";
                this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // 
                // homePageLabel
                // 
                this.homePageLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                this.homePageLabel.Location = new System.Drawing.Point(12, 98);
                this.homePageLabel.Name = "homePageLabel";
                this.homePageLabel.Size = new System.Drawing.Size(288, 13);
                this.homePageLabel.TabIndex = 6;
                this.homePageLabel.TabStop = true;
                this.homePageLabel.Text = "http://users.jyu.fi/~mikkalle/Elma";
                this.homePageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                this.homePageLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HomePageClicked);
                // 
                // configButton
                // 
                this.configButton.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                this.configButton.Location = new System.Drawing.Point(87, 180);
                this.configButton.Name = "configButton";
                this.configButton.Size = new System.Drawing.Size(138, 23);
                this.configButton.TabIndex = 7;
                this.configButton.Text = "Configuration";
                this.configButton.UseVisualStyleBackColor = true;
                this.configButton.Click += new System.EventHandler(this.ConfigButtonClick);
                // 
                // MainForm
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(312, 265);
                this.Controls.Add(this.configButton);
                this.Controls.Add(this.homePageLabel);
                this.Controls.Add(this.versionLabel);
                this.Controls.Add(this.byLabel);
                this.Controls.Add(this.titleLabel);
                this.Controls.Add(this.levelEditorButton);
                this.Controls.Add(this.rmButton);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
                this.MaximizeBox = false;
                this.Name = "MainForm";
                this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                this.Text = "Elmanager";
                this.Load += new System.EventHandler(this.StartUp);
                this.ResumeLayout(false);

		}
		internal System.Windows.Forms.Button rmButton;
		internal System.Windows.Forms.Button levelEditorButton;
		internal System.Windows.Forms.Label titleLabel;
		internal System.Windows.Forms.Label byLabel;
		internal System.Windows.Forms.Label versionLabel;
		internal System.Windows.Forms.LinkLabel homePageLabel;
		internal System.Windows.Forms.Button configButton;
	}
	
}