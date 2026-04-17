// MainForm.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Main application window shown after successful login.
// Contains the navigation bar and switches between views:
// Home, Parcels, Deliveries, Help, Info
// Profile is accessible via the username button in the top right.

using System;
using System.Drawing;
using System.Windows.Forms;
using PostalServiceWinForms.Forms;

namespace PostalServiceWinForms
{
    public class MainForm : Form
    {
        private Panel pnlNav, pnlContent;
        private Button btnActive;
        public string UserName, UserRole, UserID, UserEmail;
        public DatabaseHelper DB;

        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);
        private Color ActiveRed = Color.FromArgb(210, 60, 40);
        private Color White = Color.White;
        private Color PageBg = Color.FromArgb(245, 245, 245);

        public MainForm(string name, string role, string id, string email)
        {
            UserName = name; UserRole = role; UserID = id; UserEmail = email;
            DB = new DatabaseHelper();
            this.Text = "PostalMS";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1200, 700);
            this.BackColor = PageBg;
            BuildNav();
            ShowHome();
        }

        private void BuildNav()
        {
            pnlNav = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Red };
            pnlNav.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlNav.ClientRectangle,
                    Color.FromArgb(160, 20, 20),
                    Color.FromArgb(210, 55, 35), 0f))
                    e.Graphics.FillRectangle(b, pnlNav.ClientRectangle);
            };
            this.Controls.Add(pnlNav);

            // Logo
            Label logo = new Label
            {
                Text = "  PostalMS",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = White,
                Location = new Point(18, 15),
                Size = new Size(175, 30),
                BackColor = Color.Transparent
            };
            pnlNav.Controls.Add(logo);

            // Nav buttons -- Data page REMOVED
            Button bHome = NB(" Home", 200, ShowHome);
            Button bParc = NB(" Parcels", 305, ShowParcels);
            Button bDeli = NB(" Deliveries", 415, ShowDeliveries);
            Button bHelp = NB(" Help", 520, ShowHelp);
            Button bInfo = NB("(i) Info", 615, ShowInfo);
            SetActive(bHome);

            // AI Chat button
            Button btnAI = new Button
            {
                Text = " AI Chat",
                Location = new Point(714, 11),
                Size = new Size(105, 36),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = DarkRed,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAI.FlatAppearance.BorderColor = Color.FromArgb(220, 100, 100);
            btnAI.FlatAppearance.BorderSize = 1;
            btnAI.Click += (s, e) => new AIAssistantPanel(UserID, UserName, DB).Show();
            pnlNav.Controls.Add(btnAI);

            // Profile + Logout (right side, anchored)
            Button btnProf = new Button
            {
                Text = " " + UserName.Split(' ')[0],
                Size = new Size(130, 36),
                Font = new Font("Segoe UI", 9),
                BackColor = DarkRed,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnProf.FlatAppearance.BorderColor = Color.FromArgb(220, 100, 100);
            btnProf.FlatAppearance.BorderSize = 1;
            btnProf.Click += (s, e) => ShowProfile();

            Button btnOut = new Button
            {
                Text = "Logout",
                Size = new Size(90, 36),
                Font = new Font("Segoe UI", 9),
                BackColor = DarkRed,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOut.FlatAppearance.BorderColor = Color.FromArgb(220, 100, 100);
            btnOut.FlatAppearance.BorderSize = 1;
            btnOut.Click += (s, e) => { this.Close(); new LoginForm().Show(); };

            pnlNav.Controls.AddRange(new Control[] { btnProf, btnOut });

            void Pos()
            {
                btnOut.Location = new Point(this.ClientSize.Width - 104, 11);
                btnProf.Location = new Point(this.ClientSize.Width - 244, 11);
            }
            Pos();
            this.Resize += (s, e) => Pos();

            // Content area
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = PageBg, Padding = new Padding(18) };
            this.Controls.Add(pnlContent);
        }

        private Button NB(string text, int x, Action click)
        {
            Button b = new Button
            {
                Text = text,
                Location = new Point(x, 0),
                Size = new Size(108, 58),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(255, 200, 200),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => { SetActive(b); click(); };
            pnlNav.Controls.Add(b);
            return b;
        }

        private void SetActive(Button b)
        {
            if (btnActive != null) { btnActive.BackColor = Color.Transparent; btnActive.ForeColor = Color.FromArgb(255, 200, 200); }
            b.BackColor = ActiveRed; b.ForeColor = White;
            btnActive = b;
        }

        public void Clear() => pnlContent.Controls.Clear();

        public void ShowHome() { Clear(); pnlContent.Controls.Add(new HomeView(this)); }
        public void ShowParcels() { Clear(); pnlContent.Controls.Add(new ParcelsView(UserID, UserName, DB)); }
        public void ShowDeliveries() { Clear(); pnlContent.Controls.Add(new DeliveriesView(UserID, DB)); }
        public void ShowHelp() { Clear(); pnlContent.Controls.Add(new HelpView()); }
        public void ShowInfo() { Clear(); pnlContent.Controls.Add(new InfoView()); }
        public void ShowProfile() { Clear(); pnlContent.Controls.Add(new ProfileView(UserID, UserEmail, DB)); }
    }
}
