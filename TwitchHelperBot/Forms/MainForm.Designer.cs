﻿namespace TwitchHelperBot
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.updateChannelInfoTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.NotificationMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setupPresetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showViewerListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AudioMixerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spotifyPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NotificationMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // updateChannelInfoTimer
            // 
            this.updateChannelInfoTimer.Enabled = true;
            this.updateChannelInfoTimer.Interval = 5000;
            this.updateChannelInfoTimer.Tick += new System.EventHandler(this.updateChannelInfoTimer_Tick);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipTitle = "Channel Info Updated";
            this.notifyIcon1.ContextMenuStrip = this.NotificationMenuStrip;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Twitch Bot Helper";
            this.notifyIcon1.Visible = true;
            // 
            // NotificationMenuStrip
            // 
            this.NotificationMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setupPresetsToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.toolStripSeparator2,
            this.toolsToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.toolStripSeparator1,
            this.quitToolStripMenuItem});
            this.NotificationMenuStrip.Name = "contextMenuStrip1";
            this.NotificationMenuStrip.ShowImageMargin = false;
            this.NotificationMenuStrip.Size = new System.Drawing.Size(156, 148);
            this.NotificationMenuStrip.Opened += new System.EventHandler(this.NotificationMenuStrip_Opened);
            // 
            // setupPresetsToolStripMenuItem
            // 
            this.setupPresetsToolStripMenuItem.Name = "setupPresetsToolStripMenuItem";
            this.setupPresetsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.setupPresetsToolStripMenuItem.Text = "Preset Manager";
            this.setupPresetsToolStripMenuItem.Click += new System.EventHandler(this.setupPresetsToolStripMenuItem_Click);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.pauseToolStripMenuItem.Text = "Pause Channel Edits";
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(152, 6);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showViewerListToolStripMenuItem,
            this.AudioMixerToolStripMenuItem,
            this.spotifyPreviewToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // showViewerListToolStripMenuItem
            // 
            this.showViewerListToolStripMenuItem.Name = "showViewerListToolStripMenuItem";
            this.showViewerListToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showViewerListToolStripMenuItem.Text = "Viewer List";
            this.showViewerListToolStripMenuItem.Click += new System.EventHandler(this.showViewerListToolStripMenuItem_Click);
            // 
            // AudioMixerToolStripMenuItem
            // 
            this.AudioMixerToolStripMenuItem.Name = "AudioMixerToolStripMenuItem";
            this.AudioMixerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.AudioMixerToolStripMenuItem.Text = "Audio Mixer";
            this.AudioMixerToolStripMenuItem.Click += new System.EventHandler(this.AudioMixerToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // spotifyPreviewToolStripMenuItem
            // 
            this.spotifyPreviewToolStripMenuItem.Name = "spotifyPreviewToolStripMenuItem";
            this.spotifyPreviewToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.spotifyPreviewToolStripMenuItem.Text = "Spotify Preview";
            this.spotifyPreviewToolStripMenuItem.Click += new System.EventHandler(this.spotifyPreviewToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(134, 111);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Main";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.NotificationMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer updateChannelInfoTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip NotificationMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem setupPresetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AudioMixerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showViewerListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spotifyPreviewToolStripMenuItem;
    }
}