// ProfileView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// User profile page showing account details.
// Allows editing name, phone, address, postcode, preferred name, date of birth and bio.
// Supports profile photo upload from local files.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace PostalServiceWinForms.Forms
{
    public class ProfileView : UserControl
    {
        // All value display labels -- clearly named
        private Label valName, valID, valRole;
        private Label valEmail, valPhone;
        private Label valAddr, valPost;
        private Label valPrefName, valDOB, valBio;
        private Label valJoined, valAccType;

        // Edit fields
        private TextBox eName, ePhone, eAddr, ePost, ePrefName, eDOB, eBio;

        private Panel pnlView, pnlEdit, pnlCard;
        private Panel avPanel;
        private Image profileImage = null;
        private DatabaseHelper db;
        private string userID;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);

        public ProfileView(string uid, string email, DatabaseHelper dbHelper)
        {
            userID = uid; db = dbHelper;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.AutoScroll = true;
            Build();
            LoadProfile();
        }

        private void Build()
        {
            // Page header
            this.Controls.Add(new Label { Text = "My Profile", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Red, Location = new Point(0, 0), Size = new Size(300, 38) });
            this.Controls.Add(new Label { Text = "Your personal account details", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(0, 40), Size = new Size(500, 22) });

            // Card -- centred
            pnlCard = new Panel { BackColor = Color.White, Size = new Size(940, 700) };
            pnlCard.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(940, 6), BackColor = Red });
            this.Controls.Add(pnlCard);
            CentreCard();
            this.Resize += (s, e) => CentreCard();

            // -- Avatar --
            avPanel = new Panel { Location = new Point(20, 22), Size = new Size(96, 96), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            avPanel.Paint += AvPaint;
            avPanel.Click += AvClick;
            pnlCard.Controls.Add(avPanel);

            Label cam = new Label { Text = "", Font = new Font("Segoe UI", 11), Location = new Point(64, 68), Size = new Size(30, 26), BackColor = Color.White, ForeColor = Color.Gray, Cursor = Cursors.Hand };
            cam.Click += AvClick;
            pnlCard.Controls.Add(cam);

            Label changePic = new Label { Text = "Change photo", Font = new Font("Segoe UI", 8, FontStyle.Underline), ForeColor = Red, Location = new Point(10, 122), Size = new Size(115, 18), Cursor = Cursors.Hand };
            changePic.Click += AvClick;
            pnlCard.Controls.Add(changePic);

            // Name + ID + Role badge
            valName = new Label { Text = "...", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Red, Location = new Point(140, 22), Size = new Size(778, 34) };
            valID = new Label { Text = "...", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(140, 58), Size = new Size(778, 18) };
            valRole = new Label { Text = "...", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White, BackColor = Red, Location = new Point(140, 82), AutoSize = true, Padding = new Padding(8, 3, 8, 3) };
            pnlCard.Controls.AddRange(new Control[] { valName, valID, valRole });

            // -- VIEW PANEL --
            pnlView = new Panel { Location = new Point(0, 152), Size = new Size(940, 508), BackColor = Color.White };
            pnlCard.Controls.Add(pnlView);

            int y = 10;

            SecHead(pnlView, "Contact Information", y); y += 34;
            Row(pnlView, "Email Address", ref y, out valEmail);
            Row(pnlView, "Phone Number", ref y, out valPhone);
            y += 6;

            SecHead(pnlView, "Address", y); y += 34;
            Row(pnlView, "Home Address", ref y, out valAddr);
            Row(pnlView, "Post Code", ref y, out valPost);
            y += 6;

            SecHead(pnlView, "Personal Details", y); y += 34;
            Row(pnlView, "Preferred Name", ref y, out valPrefName);
            Row(pnlView, "Date of Birth", ref y, out valDOB);
            RowMulti(pnlView, "About Me", ref y, out valBio);
            y += 6;

            SecHead(pnlView, "Account", y); y += 34;
            Row(pnlView, "Member Since", ref y, out valJoined);
            Row(pnlView, "Account Type", ref y, out valAccType);
            y += 18;

            // -- EDIT BUTTON -- big and prominent --
            Button btnEdit = new Button
            {
                Text = "   Edit My Profile",
                Location = new Point(20, y),
                Size = new Size(220, 50),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.MouseEnter += (s, e) => btnEdit.BackColor = DarkRed;
            btnEdit.MouseLeave += (s, e) => btnEdit.BackColor = Red;
            btnEdit.Click += (s, e) => ShowEdit();
            pnlView.Controls.Add(btnEdit);

            // -- EDIT PANEL --
            pnlEdit = new Panel { Location = new Point(0, 152), Size = new Size(940, 540), BackColor = Color.White, Visible = false };
            pnlCard.Controls.Add(pnlEdit);

            pnlEdit.Controls.Add(new Label { Text = "Edit Your Profile", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(20, 10), Size = new Size(500, 30) });
            pnlEdit.Controls.Add(new Label { Text = "Email and User ID cannot be changed.", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(20, 42), Size = new Size(700, 18) });

            int ey = 68;
            EL(pnlEdit, "FULL NAME", 20, ey); EL(pnlEdit, "PHONE NUMBER", 480, ey); ey += 18;
            eName = ET(pnlEdit, 20, ey, 440); ePhone = ET(pnlEdit, 480, ey, 440); ey += 50;
            EL(pnlEdit, "HOME ADDRESS", 20, ey); ey += 18;
            eAddr = ET(pnlEdit, 20, ey, 900); ey += 50;
            EL(pnlEdit, "POST CODE", 20, ey); EL(pnlEdit, "DATE OF BIRTH (DD/MM/YYYY)", 480, ey); ey += 18;
            ePost = ET(pnlEdit, 20, ey, 440); eDOB = ET(pnlEdit, 480, ey, 440); ey += 50;
            EL(pnlEdit, "PREFERRED NAME (what should we call you?)", 20, ey); ey += 18;
            ePrefName = ET(pnlEdit, 20, ey, 900); ey += 50;
            EL(pnlEdit, "ABOUT ME (optional short bio)", 20, ey); ey += 18;
            eBio = new TextBox { Location = new Point(20, ey), Size = new Size(900, 70), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, Multiline = true };
            pnlEdit.Controls.Add(eBio); ey += 80;

            Button bSave = new Button { Text = "Save Changes", Location = new Point(20, ey), Size = new Size(180, 46), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            bSave.FlatAppearance.BorderSize = 0;
            bSave.MouseEnter += (s, e) => bSave.BackColor = DarkRed;
            bSave.MouseLeave += (s, e) => bSave.BackColor = Red;
            bSave.Click += Save_Click;

            Button bCanc = new Button { Text = "Cancel", Location = new Point(212, ey), Size = new Size(120, 46), Font = new Font("Segoe UI", 12), BackColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            bCanc.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            bCanc.Click += (s, e) => { pnlCard.Size = new Size(940, 700); pnlEdit.Visible = false; pnlView.Visible = true; CentreCard(); };
            pnlEdit.Controls.AddRange(new Control[] { bSave, bCanc });
        }

        private void CentreCard()
        {
            if (pnlCard == null) return;
            int x = Math.Max(0, (this.ClientSize.Width - pnlCard.Width) / 2);
            pnlCard.Location = new Point(x, 72);
        }

        private void SecHead(Panel p, string title, int y)
        {
            p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(20, y), Size = new Size(500, 20) });
            p.Controls.Add(new Panel { Location = new Point(20, y + 22), Size = new Size(898, 1), BackColor = Color.FromArgb(230, 180, 180) });
        }

        private void Row(Panel p, string lbl, ref int y, out Label val)
        {
            p.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(20, y), Size = new Size(220, 20) });
            val = new Label { Text = "--", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 30), Location = new Point(250, y), Size = new Size(668, 22) };
            p.Controls.Add(val); y += 34;
        }

        private void RowMulti(Panel p, string lbl, ref int y, out Label val)
        {
            p.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(20, y), Size = new Size(220, 20) });
            val = new Label { Text = "--", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(30, 30, 30), Location = new Point(250, y), Size = new Size(668, 42) };
            p.Controls.Add(val); y += 52;
        }

        private void AvPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (profileImage != null)
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, 95, 95);
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawImage(profileImage, 0, 0, 96, 96);
                    e.Graphics.ResetClip();
                }
                e.Graphics.DrawEllipse(new Pen(Red, 3), 1, 1, 93, 93);
            }
            else
            {
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 220, 220)), 0, 0, 95, 95);
                string ini = valName != null && valName.Text.Length > 1 ? Init(valName.Text) : "?";
                e.Graphics.DrawString(ini, new Font("Segoe UI", 28, FontStyle.Bold), new SolidBrush(Red), 16, 22);
                e.Graphics.DrawEllipse(new Pen(Color.FromArgb(230, 180, 180), 2), 1, 1, 93, 93);
            }
        }

        private void AvClick(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Choose Profile Photo";
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try { profileImage = System.Drawing.Image.FromFile(dlg.FileName); avPanel.Invalidate(); }
                    catch { MessageBox.Show("Could not load that image. Please choose a JPG or PNG file."); }
                }
            }
        }

        private void EL(Panel p, string t, int x, int y) =>
            p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(x, y), Size = new Size(450, 16) });

        private TextBox ET(Panel p, int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(tb); return tb;
        }

        private string Init(string name)
        {
            var parts = name.Split(' ');
            return parts.Length >= 2 ? (parts[0][0].ToString() + parts[1][0]).ToUpper() : name.Length > 0 ? name[0].ToString().ToUpper() : "?";
        }

        private void ShowEdit()
        {
            eName.Text = valName.Text;
            ePhone.Text = valPhone.Text == "--" ? "" : valPhone.Text;
            eAddr.Text = valAddr.Text == "--" ? "" : valAddr.Text;
            ePost.Text = valPost.Text == "--" ? "" : valPost.Text;
            ePrefName.Text = valPrefName.Text == "--" ? "" : valPrefName.Text;
            eDOB.Text = valDOB.Text == "--" ? "" : valDOB.Text;
            eBio.Text = valBio.Text == "--" ? "" : valBio.Text;
            pnlCard.Size = new Size(940, 780);
            pnlView.Visible = false;
            pnlEdit.Visible = true;
            CentreCard();
        }

        private void LoadProfile()
        {
            try
            {
                DataTable dt = db.GetUserByID(userID);
                if (dt.Rows.Count == 0) { valName.Text = "User not found"; return; }
                DataRow r = dt.Rows[0];

                valName.Text = r["FullName"].ToString();
                valID.Text = "User ID: " + r["UserID"] + "       " + r["Email"];
                valRole.Text = "  " + r["Role"] + "  ";
                valEmail.Text = r["Email"].ToString();
                valPhone.Text = r["Phone"].ToString();
                valAddr.Text = r["Address"].ToString();
                valPost.Text = r["PostCode"].ToString();
                valJoined.Text = DateTime.TryParse(r["CreatedDate"].ToString(), out DateTime d) ? d.ToString("dd MMMM yyyy") : "--";
                valAccType.Text = r["Role"].ToString();
                valPrefName.Text = GetExtra(dt, r, "PreferredName");
                valDOB.Text = GetExtra(dt, r, "DOB");
                valBio.Text = GetExtra(dt, r, "Bio");

                avPanel?.Invalidate();
                this.Refresh();
            }
            catch (Exception ex)
            {
                if (valName != null) valName.Text = "Error loading profile";
                MessageBox.Show("Profile error: " + ex.Message);
            }
        }

        private string GetExtra(DataTable dt, DataRow r, string col)
        {
            if (dt.Columns.Contains(col) && r[col] != DBNull.Value && !string.IsNullOrEmpty(r[col].ToString()))
                return r[col].ToString();
            return "--";
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(eName.Text))
            { MessageBox.Show("Name cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string res = db.UpdateUserProfile(userID, eName.Text.Trim(), ePhone.Text.Trim(), eAddr.Text.Trim(), ePost.Text.Trim());
            try { db.UpdateUserExtras(userID, ePrefName.Text.Trim(), eDOB.Text.Trim(), eBio.Text.Trim()); } catch { }

            MessageBox.Show(res, "Profile Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (res.Contains("successfully"))
            {
                pnlCard.Size = new Size(940, 700);
                pnlEdit.Visible = false;
                pnlView.Visible = true;
                CentreCard();
                LoadProfile();
            }
        }
    }
}