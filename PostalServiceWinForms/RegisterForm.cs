// RegisterForm.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Registration form for new users.
// Validates all fields including Gmail-only email validation.
// Calls RegisterUser stored procedure on successful validation.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PostalServiceWinForms
{
    public class RegisterForm : Form
    {
        private TextBox txtName, txtPhone, txtEmail, txtAddress, txtPostCode, txtPass, txtConfirm;
        private ComboBox cboCity;
        private DatabaseHelper db;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);

        public RegisterForm()
        {
            db = new DatabaseHelper();
            this.Text = "PostalMS -- Create Account";
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
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(left.ClientRectangle,
                    Color.FromArgb(160, 20, 20), Color.FromArgb(210, 60, 40),
                    System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal))
                    e.Graphics.FillRectangle(b, left.ClientRectangle);
            };
            this.Controls.Add(left);

            left.Controls.Add(new Label { Text = "PostalMS", Font = new Font("Segoe UI", 30, FontStyle.Bold), ForeColor = Color.White, Location = new Point(88, 100), Size = new Size(224, 50), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleCenter });
            left.Controls.Add(new Label { Text = "Create Your Free Account", Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(255, 210, 210), Location = new Point(50, 154), Size = new Size(300, 24), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleCenter });
            left.Controls.Add(new Panel { Location = new Point(60, 190), Size = new Size(280, 1), BackColor = Color.FromArgb(220, 120, 120) });

            string[] benefits = { "  Free to register -- no credit card", "  Send domestically and internationally", "  Track all your parcels in one place", "  Refund protection on all shipments", "  AI-powered parcel assistant included" };
            int fy = 207;
            foreach (string b2 in benefits) { left.Controls.Add(new Label { Text = b2, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(255, 210, 210), Location = new Point(60, fy), Size = new Size(290, 24), BackColor = Color.Transparent }); fy += 27; }
            left.Controls.Add(new Label { Text = "PostalMS -- Middlesex University 2026", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(220, 165, 165), Location = new Point(40, 600), Size = new Size(320, 18), BackColor = Color.Transparent });

            // Right form panel
            Panel right = new Panel { Location = new Point(400, 0), Size = new Size(700, 640), BackColor = Color.White };
            this.Controls.Add(right);

            right.Controls.Add(new Label { Text = "Create Account", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Red, Location = new Point(70, 26), Size = new Size(560, 40) });
            right.Controls.Add(new Label { Text = "Fill in your details to get started with PostalMS", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(70, 68), Size = new Size(560, 22) });

            int y = 106;

            // Name and Phone
            AL(right, "FULL NAME", 70, y); AL(right, "PHONE NUMBER", 350, y); y += 18;
            txtName = ATB(right, "John Smith", 70, y, 260);
            txtPhone = ATB(right, "07700000000", 350, y, 280); y += 50;

            // Gmail only email
            AL(right, "EMAIL ADDRESS (must be @gmail.com)", 70, y); y += 18;
            txtEmail = ATB(right, "yourname@gmail.com", 70, y, 560);
            txtEmail.Leave += (s, e) =>
            {
                if (txtEmail.ForeColor != Color.LightGray && !ValidGmail(txtEmail.Text))
                    txtEmail.BackColor = Color.FromArgb(255, 235, 235);
                else
                    txtEmail.BackColor = Color.White;
            };
            y += 50;

            // Address
            AL(right, "HOME ADDRESS", 70, y); y += 18;
            txtAddress = ATB(right, "123 Main Street, London", 70, y, 560); y += 50;

            // Postcode and City -- labels first then inputs
            AL(right, "POSTCODE", 70, y);
            right.Controls.Add(new Label { Text = "LOCATION", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(100, 100, 100), Location = new Point(290, y), Size = new Size(280, 16) });
            y += 18;
            txtPostCode = ATB(right, "N1 2AB", 70, y, 200);
            cboCity = new ComboBox { Location = new Point(290, y), Size = new Size(280, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboCity.Items.AddRange(new object[] { "London", "Manchester", "Birmingham", "Leeds", "Bristol", "Edinburgh", "Glasgow" });
            cboCity.SelectedIndex = 0;
            right.Controls.Add(cboCity);
            y += 50;

            // Password and Confirm
            AL(right, "PASSWORD", 70, y); AL(right, "CONFIRM PASSWORD", 350, y); y += 18;
            txtPass = ATB(right, "", 70, y, 260, true);
            txtConfirm = ATB(right, "", 350, y, 280, true); y += 52;

            // Create button
            Button btnCreate = new Button { Text = "Create Account ->", Location = new Point(70, y), Size = new Size(560, 48), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.MouseEnter += (s, e) => btnCreate.BackColor = DarkRed;
            btnCreate.MouseLeave += (s, e) => btnCreate.BackColor = Red;
            btnCreate.Click += BtnCreate_Click;
            right.Controls.Add(btnCreate);
            y += 58;

            right.Controls.Add(new Panel { Location = new Point(70, y), Size = new Size(560, 1), BackColor = Color.FromArgb(220, 220, 220) });
            y += 10;

            right.Controls.Add(new Label { Text = "Already have an account?", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(70, y), Size = new Size(200, 22) });
            LinkLabel lnk = new LinkLabel { Text = "Sign in here", Font = new Font("Segoe UI", 10, FontStyle.Bold), LinkColor = Red, Location = new Point(275, y), Size = new Size(110, 22) };
            lnk.Click += (s, e) => this.Close();
            right.Controls.Add(lnk);
        }

        // Validates that the email is a proper Gmail address
        private bool ValidGmail(string email)
        {
            return email.EndsWith("@gmail.com") && email.Length > "@gmail.com".Length && !email.StartsWith("@");
        }

        private void AL(Panel p, string t, int x, int y) =>
            p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(100, 100, 100), Location = new Point(x, y), Size = new Size(280, 16), AutoSize = false });

        private TextBox ATB(Panel p, string ph, int x, int y, int w, bool pwd = false)
        {
            TextBox tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 34), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            if (pwd) { tb.UseSystemPasswordChar = true; }
            else if (!string.IsNullOrEmpty(ph))
            {
                tb.Text = ph; tb.ForeColor = Color.LightGray;
                tb.Enter += (s, e) => { if (tb.ForeColor == Color.LightGray) { tb.Text = ""; tb.ForeColor = Color.Black; } };
                tb.Leave += (s, e) => { if (tb.Text == "") { tb.Text = ph; tb.ForeColor = Color.LightGray; } };
            }
            p.Controls.Add(tb);
            return tb;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            // Validate name
            if (txtName.ForeColor == Color.LightGray || string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Please enter your full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // Validate Gmail
            if (txtEmail.ForeColor == Color.LightGray || string.IsNullOrWhiteSpace(txtEmail.Text))
            { MessageBox.Show("Please enter your Gmail address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (!ValidGmail(txtEmail.Text.Trim()))
            { MessageBox.Show("Email must be a valid Gmail address.\nExample: yourname@gmail.com", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // Validate password
            if (string.IsNullOrEmpty(txtPass.Text))
            { MessageBox.Show("Please enter a password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (txtPass.Text != txtConfirm.Text)
            { MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (txtPass.Text.Length < 6)
            { MessageBox.Show("Password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // Register user
            string uid = "USER-" + (db.GetUserCount() + 1).ToString("D3");
            string res = db.RegisterUser(uid, txtName.Text.Trim(), txtEmail.Text.Trim(),
                txtPhone.Text.Trim(), txtAddress.Text.Trim(), txtPostCode.Text.Trim(), "Customer", txtPass.Text, cboCity.SelectedItem?.ToString() ?? "London");

            MessageBox.Show(res, "Registration", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (res.Contains("successfully")) this.Close();
        }
    }
}