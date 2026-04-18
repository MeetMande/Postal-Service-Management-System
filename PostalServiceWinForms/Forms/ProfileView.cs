// ProfileView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// User profile page showing all account details.
// Loads user data from the database on startup.
// Allows editing name, phone, address and postcode.
// Shows initials avatar or uploaded photo.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace PostalServiceWinForms.Forms
{
    public class ProfileView : UserControl
    {
        // Display labels for profile info
        private Label valName, valID, valRole;
        private Label valEmail, valPhone;
        private Label valAddr, valPost;
        private Label valJoined, valAccType;

        // Edit fields
        private TextBox eName, ePhone, eAddr, ePost;

        private Panel pnlView, pnlEdit, pnlCard;
        private Panel avPanel;
        private Image profileImage = null;
        private DatabaseHelper db;
        private string userID;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);

        public ProfileView(string uid, string email, DatabaseHelper dbHelper)
        {
            userID = uid;
            db = dbHelper;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.AutoScroll = true;
            Build();
            LoadProfile();
        }

        private void Build()
        {
            // Page heading
            this.Controls.Add(new Label
            {
                Text = "My Profile",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(20, 50),
                Size = new Size(300, 40)
            });
            this.Controls.Add(new Label
            {
                Text = "Your personal account details",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Location = new Point(20, 94),
                Size = new Size(500, 24)
            });

            // White card panel
            pnlCard = new Panel
            {
                BackColor = Color.White,
                Size = new Size(860, 620),
                Location = new Point(20, 134)
            };
            pnlCard.Controls.Add(new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(860, 6),
                BackColor = Red
            });
            this.Controls.Add(pnlCard);

            // Avatar circle
            avPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(90, 90),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            avPanel.Paint += AvPaint;
            avPanel.Click += AvClick;
            pnlCard.Controls.Add(avPanel);

            // Change photo link
            LinkLabel changePic = new LinkLabel
            {
                Text = "Change photo",
                Font = new Font("Segoe UI", 8, FontStyle.Underline),
                LinkColor = Red,
                Location = new Point(10, 114),
                Size = new Size(110, 18),
                Cursor = Cursors.Hand
            };
            changePic.Click += AvClick;
            pnlCard.Controls.Add(changePic);

            // Name, ID and role badge
            valName = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(130, 20),
                Size = new Size(710, 36)
            };
            valID = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(130, 58),
                Size = new Size(710, 20)
            };
            valRole = new Label
            {
                Text = "...",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Red,
                Location = new Point(130, 82),
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3)
            };
            pnlCard.Controls.AddRange(new Control[] { valName, valID, valRole });

            // Divider
            pnlCard.Controls.Add(new Panel
            {
                Location = new Point(0, 142),
                Size = new Size(860, 1),
                BackColor = Color.FromArgb(230, 180, 180)
            });

            // View panel -- shows profile info
            pnlView = new Panel
            {
                Location = new Point(0, 150),
                Size = new Size(860, 440),
                BackColor = Color.White
            };
            pnlCard.Controls.Add(pnlView);
            BuildViewPanel();

            // Edit panel -- shown when Edit button clicked
            pnlEdit = new Panel
            {
                Location = new Point(0, 150),
                Size = new Size(860, 460),
                BackColor = Color.White,
                Visible = false
            };
            pnlCard.Controls.Add(pnlEdit);
            BuildEditPanel();
        }

        private void BuildViewPanel()
        {
            int y = 14;

            // Contact Information section
            SH(pnlView, "Contact Information", y); y += 38;
            Row(pnlView, "Email Address", ref y, out valEmail);
            Row(pnlView, "Phone Number", ref y, out valPhone);
            y += 10;

            // Address section
            SH(pnlView, "Address", y); y += 38;
            Row(pnlView, "Home Address", ref y, out valAddr);
            Row(pnlView, "Post Code", ref y, out valPost);
            y += 10;

            // Account section
            SH(pnlView, "Account", y); y += 38;
            Row(pnlView, "Member Since", ref y, out valJoined);
            Row(pnlView, "Account Type", ref y, out valAccType);
            y += 20;

            // Edit button
            Button btnEdit = new Button
            {
                Text = "Edit My Profile",
                Location = new Point(20, y),
                Size = new Size(200, 46),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
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
        }

        private void BuildEditPanel()
        {
            pnlEdit.Controls.Add(new Label
            {
                Text = "Edit Your Profile",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(20, 10),
                Size = new Size(500, 30)
            });
            pnlEdit.Controls.Add(new Label
            {
                Text = "Email and User ID cannot be changed.",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(20, 44),
                Size = new Size(700, 18)
            });

            int ey = 72;

            // Name and Phone
            EL(pnlEdit, "FULL NAME", 20, ey);
            EL(pnlEdit, "PHONE NUMBER", 450, ey);
            ey += 18;
            eName = ET(pnlEdit, 20, ey, 420);
            ePhone = ET(pnlEdit, 450, ey, 390);
            ey += 50;

            // Address
            EL(pnlEdit, "HOME ADDRESS", 20, ey); ey += 18;
            eAddr = ET(pnlEdit, 20, ey, 820); ey += 50;

            // Postcode
            EL(pnlEdit, "POST CODE", 20, ey); ey += 18;
            ePost = ET(pnlEdit, 20, ey, 200); ey += 52;

            // Save and Cancel buttons
            Button bSave = new Button
            {
                Text = "Save Changes",
                Location = new Point(20, ey),
                Size = new Size(180, 46),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            bSave.FlatAppearance.BorderSize = 0;
            bSave.MouseEnter += (s, e) => bSave.BackColor = DarkRed;
            bSave.MouseLeave += (s, e) => bSave.BackColor = Red;
            bSave.Click += Save_Click;

            Button bCanc = new Button
            {
                Text = "Cancel",
                Location = new Point(210, ey),
                Size = new Size(120, 46),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            bCanc.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            bCanc.Click += (s, e) =>
            {
                pnlEdit.Visible = false;
                pnlView.Visible = true;
            };

            pnlEdit.Controls.AddRange(new Control[] { bSave, bCanc });
        }

        // Load user profile from database
        private void LoadProfile()
        {
            try
            {
                // Use simple query that definitely works
                DataTable dt = db.Q_Public(
                    "SELECT UserID, FullName, Email, Phone, Address, PostCode, Role, CreatedDate FROM Users WHERE UserID=@id",
                    ("@id", userID));

                if (dt == null || dt.Rows.Count == 0)
                {
                    valName.Text = "Profile not found";
                    valID.Text = "User ID: " + userID;
                    return;
                }

                DataRow r = dt.Rows[0];

                // Set all display labels
                valName.Text = r["FullName"]?.ToString() ?? "--";
                valID.Text = "User ID: " + r["UserID"] + "     " + r["Email"];
                valRole.Text = "  " + (r["Role"]?.ToString() ?? "User") + "  ";
                valEmail.Text = r["Email"]?.ToString() ?? "--";
                valPhone.Text = r["Phone"]?.ToString() ?? "--";
                valAddr.Text = r["Address"]?.ToString() ?? "--";
                valPost.Text = r["PostCode"]?.ToString() ?? "--";
                valAccType.Text = r["Role"]?.ToString() ?? "--";

                if (DateTime.TryParse(r["CreatedDate"]?.ToString(), out DateTime joined))
                    valJoined.Text = joined.ToString("dd MMMM yyyy");
                else
                    valJoined.Text = "--";

                // Refresh the avatar with initials
                avPanel?.Invalidate();
                this.Refresh();
            }
            catch (Exception ex)
            {
                // Show error clearly so we can debug
                if (valName != null)
                    valName.Text = "Error loading profile";
                if (valID != null)
                    valID.Text = ex.Message;
            }
        }

        private void ShowEdit()
        {
            eName.Text = valName.Text;
            ePhone.Text = valPhone.Text == "--" ? "" : valPhone.Text;
            eAddr.Text = valAddr.Text == "--" ? "" : valAddr.Text;
            ePost.Text = valPost.Text == "--" ? "" : valPost.Text;
            pnlView.Visible = false;
            pnlEdit.Visible = true;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(eName.Text))
            {
                MessageBox.Show("Name cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string res = db.UpdateUserProfile(userID, eName.Text.Trim(), ePhone.Text.Trim(), eAddr.Text.Trim(), ePost.Text.Trim());
            MessageBox.Show(res, "Profile Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (res.Contains("successfully"))
            {
                pnlEdit.Visible = false;
                pnlView.Visible = true;
                LoadProfile();
            }
        }

        // Draw avatar circle with initials or photo
        private void AvPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (profileImage != null)
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, 89, 89);
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawImage(profileImage, 0, 0, 90, 90);
                    e.Graphics.ResetClip();
                }
                e.Graphics.DrawEllipse(new Pen(Red, 3), 1, 1, 87, 87);
            }
            else
            {
                // Draw initials circle
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(255, 220, 220)), 0, 0, 89, 89);
                string ini = GetInitials(valName?.Text ?? "?");
                e.Graphics.DrawString(ini, new Font("Segoe UI", 26, FontStyle.Bold), new SolidBrush(Red), 14, 20);
                e.Graphics.DrawEllipse(new Pen(Color.FromArgb(220, 160, 160), 2), 1, 1, 87, 87);
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
                    try
                    {
                        profileImage = System.Drawing.Image.FromFile(dlg.FileName);
                        avPanel.Invalidate();
                    }
                    catch { MessageBox.Show("Could not load image. Please choose a JPG or PNG file."); }
                }
            }
        }

        // Get first letter of first and last name
        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "Loading..." || name == "Error loading profile") return "?";
            var parts = name.Trim().Split(' ');
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpper();
            return name.Length > 0 ? name[0].ToString().ToUpper() : "?";
        }

        // Section heading helper
        private void SH(Panel p, string title, int y)
        {
            p.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(20, y),
                Size = new Size(500, 20)
            });
            p.Controls.Add(new Panel
            {
                Location = new Point(20, y + 22),
                Size = new Size(820, 1),
                BackColor = Color.FromArgb(230, 180, 180)
            });
        }

        // Row with label and value
        private void Row(Panel p, string lbl, ref int y, out Label val)
        {
            p.Controls.Add(new Label
            {
                Text = lbl,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(20, y),
                Size = new Size(200, 20)
            });
            val = new Label
            {
                Text = "--",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(230, y),
                Size = new Size(610, 22)
            };
            p.Controls.Add(val);
            y += 36;
        }

        // Edit label helper
        private void EL(Panel p, string t, int x, int y) =>
            p.Controls.Add(new Label
            {
                Text = t,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(x, y),
                Size = new Size(420, 16)
            });

        // Edit textbox helper
        private TextBox ET(Panel p, int x, int y, int w)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 34),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            p.Controls.Add(tb);
            return tb;
        }
    }
}