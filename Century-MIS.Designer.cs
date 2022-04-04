
namespace Century_MIS
{
    partial class Century_MIS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Century_MIS));
            this.panel_MainTop = new DevExpress.XtraEditors.PanelControl();
            this.lbl_ServerURL = new DevExpress.XtraEditors.LabelControl();
            this.txt_ServerURL = new DevExpress.XtraEditors.TextEdit();
            this.txt_Password = new DevExpress.XtraEditors.TextEdit();
            this.btn_Connect = new DevExpress.XtraEditors.SimpleButton();
            this.txt_Username = new DevExpress.XtraEditors.TextEdit();
            this.middle_panel = new DevExpress.XtraEditors.PanelControl();
            this.lst_messages = new DevExpress.XtraEditors.ListBoxControl();
            ((System.ComponentModel.ISupportInitialize)(this.panel_MainTop)).BeginInit();
            this.panel_MainTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_ServerURL.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_Password.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_Username.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.middle_panel)).BeginInit();
            this.middle_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lst_messages)).BeginInit();
            this.SuspendLayout();
            // 
            // panel_MainTop
            // 
            this.panel_MainTop.Controls.Add(this.lbl_ServerURL);
            this.panel_MainTop.Controls.Add(this.txt_ServerURL);
            this.panel_MainTop.Controls.Add(this.txt_Password);
            this.panel_MainTop.Controls.Add(this.btn_Connect);
            this.panel_MainTop.Controls.Add(this.txt_Username);
            this.panel_MainTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_MainTop.Location = new System.Drawing.Point(0, 0);
            this.panel_MainTop.MaximumSize = new System.Drawing.Size(800, 44);
            this.panel_MainTop.Name = "panel_MainTop";
            this.panel_MainTop.Size = new System.Drawing.Size(621, 44);
            this.panel_MainTop.TabIndex = 10;
            // 
            // lbl_ServerURL
            // 
            this.lbl_ServerURL.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.25F);
            this.lbl_ServerURL.Appearance.Options.UseFont = true;
            this.lbl_ServerURL.Location = new System.Drawing.Point(12, 12);
            this.lbl_ServerURL.Name = "lbl_ServerURL";
            this.lbl_ServerURL.Size = new System.Drawing.Size(78, 19);
            this.lbl_ServerURL.TabIndex = 4;
            this.lbl_ServerURL.Text = "Server URL : ";
            // 
            // txt_ServerURL
            // 
            this.txt_ServerURL.EditValue = "";
            this.txt_ServerURL.Location = new System.Drawing.Point(96, 9);
            this.txt_ServerURL.Name = "txt_ServerURL";
            this.txt_ServerURL.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.25F);
            this.txt_ServerURL.Properties.Appearance.Options.UseFont = true;
            this.txt_ServerURL.Size = new System.Drawing.Size(156, 26);
            this.txt_ServerURL.TabIndex = 2;
            // 
            // txt_Password
            // 
            this.txt_Password.EditValue = "";
            this.txt_Password.Location = new System.Drawing.Point(324, 9);
            this.txt_Password.Name = "txt_Password";
            this.txt_Password.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.25F);
            this.txt_Password.Properties.Appearance.Options.UseFont = true;
            this.txt_Password.Size = new System.Drawing.Size(102, 26);
            this.txt_Password.TabIndex = 7;
            // 
            // btn_Connect
            // 
            this.btn_Connect.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.25F);
            this.btn_Connect.Appearance.Options.UseFont = true;
            this.btn_Connect.Location = new System.Drawing.Point(432, 8);
            this.btn_Connect.LookAndFeel.SkinName = "DevExpress Dark Style";
            this.btn_Connect.LookAndFeel.UseDefaultLookAndFeel = false;
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(77, 27);
            this.btn_Connect.TabIndex = 3;
            this.btn_Connect.Text = "Connect";
            // 
            // txt_Username
            // 
            this.txt_Username.EditValue = "";
            this.txt_Username.Location = new System.Drawing.Point(258, 9);
            this.txt_Username.Name = "txt_Username";
            this.txt_Username.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.25F);
            this.txt_Username.Properties.Appearance.Options.UseFont = true;
            this.txt_Username.Size = new System.Drawing.Size(60, 26);
            this.txt_Username.TabIndex = 5;
            // 
            // middle_panel
            // 
            this.middle_panel.Controls.Add(this.lst_messages);
            this.middle_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.middle_panel.Location = new System.Drawing.Point(0, 44);
            this.middle_panel.Name = "middle_panel";
            this.middle_panel.Size = new System.Drawing.Size(621, 228);
            this.middle_panel.TabIndex = 12;
            // 
            // lst_messages
            // 
            this.lst_messages.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lst_messages.Appearance.ForeColor = System.Drawing.Color.Black;
            this.lst_messages.Appearance.Options.UseFont = true;
            this.lst_messages.Appearance.Options.UseForeColor = true;
            this.lst_messages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lst_messages.HorizontalScrollbar = true;
            this.lst_messages.Location = new System.Drawing.Point(2, 2);
            this.lst_messages.Name = "lst_messages";
            this.lst_messages.Size = new System.Drawing.Size(617, 224);
            this.lst_messages.TabIndex = 0;
            // 
            // Century_MIS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(621, 272);
            this.Controls.Add(this.middle_panel);
            this.Controls.Add(this.panel_MainTop);
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("Century_MIS.IconOptions.Image")));
            this.Name = "Century_MIS";
            this.Text = "CenturyMIS Order Dump";
            this.Load += new System.EventHandler(this.Century_MIS_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panel_MainTop)).EndInit();
            this.panel_MainTop.ResumeLayout(false);
            this.panel_MainTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_ServerURL.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_Password.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_Username.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.middle_panel)).EndInit();
            this.middle_panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lst_messages)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panel_MainTop;
        private DevExpress.XtraEditors.LabelControl lbl_ServerURL;
        private DevExpress.XtraEditors.TextEdit txt_ServerURL;
        private DevExpress.XtraEditors.TextEdit txt_Password;
        private DevExpress.XtraEditors.SimpleButton btn_Connect;
        private DevExpress.XtraEditors.TextEdit txt_Username;
        private DevExpress.XtraEditors.PanelControl middle_panel;
        private DevExpress.XtraEditors.ListBoxControl lst_messages;
    }
}