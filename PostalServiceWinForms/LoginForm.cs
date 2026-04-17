// LoginForm.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Login screen shown on application startup.
// Validates email must be a valid Gmail address ending in @gmail.com
// On success opens MainForm with the authenticated user details.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PostalServiceWinForms
{
    public class LoginForm : Form
    {
        private TextBox txtEmail, txtPassword;
        private DatabaseHelper db;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);

        public LoginForm()
        {
            db = new DatabaseHelper();
            this.Text = "PostalMS -- Login";
            this.Size = new Size(1100, 640);
            this.MinimumSize = new Size(1100, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            BuildUI();
        }

        private void BuildUI()
        {
            // Left branding panel
            Panel left = new Panel { Location = new Point(0, 0), Size = new Size(400, 640), BackColor = Red };
            left.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(
                    left.ClientRectangle, Color.FromArgb(160, 20, 20), Color.FromArgb(210, 60, 40),
                    System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal))
                    e.Graphics.FillRectangle(b, left.ClientRectangle);
            };
            this.Controls.Add(left);

            left.Controls.Add(new Label { Text = "PostalMS", Font = new Font("Segoe UI", 30, FontStyle.Bold), ForeColor = Color.White, Location = new Point(88, 120), Size = new Size(224, 50), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleCenter });
            left.Controls.Add(new Label { Text = "Your Trusted Postal Partner", Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(255, 210, 210), Location = new Point(50, 174), Size = new Size(300, 24), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleCenter });
            left.Controls.Add(new Panel { Location = new Point(60, 210), Size = new Size(280, 1), BackColor = Color.FromArgb(220, 120, 120) });

            string[] feats = { "  Send mail and packages", "  Track deliveries in real time", "  Ship to 20+ countries", "  AI-powered parcel assistant", "  Refund protection included" };
            int fy = 228;
            foreach (string f in feats) { left.Controls.Add(new Label { Text = f, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(255, 215, 215), Location = new Point(60, fy), Size = new Size(290, 24), BackColor = Color.Transparent }); fy += 28; }
            left.Controls.Add(new Label { Text = "PostalMS -- Middlesex University 2026", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(220, 170, 170), Location = new Point(40, 600), Size = new Size(320, 18), BackColor = Color.Transparent });

            // Right login panel
            Panel right = new Panel { Location = new Point(400, 0), Size = new Size(700, 640), BackColor = Color.White };
            this.Controls.Add(right);

            right.Controls.Add(new Label { Text = "Welcome Back", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Red, Location = new Point(80, 80), Size = new Size(500, 48) });
            right.Controls.Add(new Label { Text = "Sign in to your PostalMS account", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(80, 132), Size = new Size(500, 24) });

            // Email must be Gmail
            right.Controls.Add(new Label { Text = "EMAIL ADDRESS (must be @gmail.com)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(100, 100, 100), Location = new Point(80, 188), Size = new Size(500, 16) });
            txtEmail = new TextBox { Location = new Point(80, 208), Size = new Size(500, 38), Font = new Font("Segoe UI", 12), BorderStyle = BorderStyle.FixedSingle, Text = "yourname@gmail.com", ForeColor = Color.LightGray };
            txtEmail.Enter += (s, e) => { if (txtEmail.ForeColor == Color.LightGray) { txtEmail.Text = ""; txtEmail.ForeColor = Color.Black; } };
            txtEmail.Leave += (s, e) => { if (txtEmail.Text == "") { txtEmail.Text = "yourname@gmail.com"; txtEmail.ForeColor = Color.LightGray; } txtEmail.BackColor = ValidGmail(txtEmail.Text) ? Color.FromArgb(240, 255, 240) : Color.White; };
            right.Controls.Add(txtEmail);

            right.Controls.Add(new Label { Text = "PASSWORD", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(100, 100, 100), Location = new Point(80, 268), Size = new Size(500, 16) });
            txtPassword = new TextBox { Location = new Point(80, 288), Size = new Size(500, 38), Font = new Font("Segoe UI", 12), BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            right.Controls.Add(txtPassword);

            Button btnLogin = new Button { Text = "Sign In ->", Location = new Point(80, 354), Size = new Size(500, 50), Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = DarkRed;
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Red;
            btnLogin.Click += BtnLogin_Click;
            right.Controls.Add(btnLogin);

            right.Controls.Add(new Panel { Location = new Point(80, 424), Size = new Size(500, 1), BackColor = Color.FromArgb(220, 220, 220) });
            right.Controls.Add(new Label { Text = "Don't have an account?", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(80, 438), Size = new Size(200, 24) });

            LinkLabel lnkReg = new LinkLabel { Text = "Register here", Font = new Font("Segoe UI", 10, FontStyle.Bold), LinkColor = Red, Location = new Point(285, 438), Size = new Size(120, 24) };
            lnkReg.Click += (s, e) => new RegisterForm().ShowDialog();
            right.Controls.Add(lnkReg);
            right.Controls.Add(new Label { Text = "PostalMS v1.0  --  Secure and Encrypted", Font = new Font("Segoe UI", 8), ForeColor = Color.LightGray, Location = new Point(80, 560), Size = new Size(300, 18) });
        }

        // Validate that email is a proper Gmail address
        private bool ValidGmail(string email)
        {
            return email.EndsWith("@gmail.com") && email.Length > "@gmail.com".Length && !email.StartsWith("@");
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Text;

            // Check email is not empty or placeholder
            if (string.IsNullOrEmpty(email) || txtEmail.ForeColor == Color.LightGray || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Please enter your Gmail address and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Enforce Gmail only
            if (!ValidGmail(email))
            {
                MessageBox.Show("Please enter a valid Gmail address.\nExample: yourname@gmail.com", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.BackColor = Color.FromArgb(255, 240, 240);
                return;
            }

            var result = db.LoginUser(email, pass);

            if (result.Rows.Count > 0 && result.Columns[0].ColumnName != "Message")
            {
                string uid = result.Rows[0]["UserID"].ToString();
                string name = result.Rows[0]["FullName"].ToString();
                string role = result.Rows[0]["Role"].ToString();
                new MainForm(name, role, uid, email).Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid email or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
