﻿namespace BlazorWinFormsCustomTitleBar
{
    partial class TitleBarLessBlazorWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            blazorWebView = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
            SuspendLayout();
            // 
            // blazorWebView
            // 
            blazorWebView.BackColor = SystemColors.Control;
            blazorWebView.CausesValidation = false;
            blazorWebView.Dock = DockStyle.Fill;
            blazorWebView.Location = new Point(0, 0);
            blazorWebView.Name = "blazorWebView";
            blazorWebView.Size = new Size(782, 582);
            blazorWebView.StartPath = "/";
            blazorWebView.TabIndex = 0;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 582);
            Controls.Add(blazorWebView);
            MinimumSize = new Size(400, 300);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView;
    }
}
