// HelpView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Help page with frequently asked questions and usage guidance.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PostalServiceWinForms.Forms
{
    public class HelpView : UserControl
    {
        private Color Red = Color.FromArgb(180, 30, 30);

        public HelpView()
        {
            this.Dock = DockStyle.Fill; this.BackColor = Color.FromArgb(245, 245, 245); this.AutoScroll = true;
            Build();
        }

        private void Build()
        {
            // Header
            Panel hdr = new Panel { Location = new Point(0, 0), Size = new Size(1400, 130), BackColor = Red };
            hdr.Paint += (s, e) => { using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(hdr.ClientRectangle, Color.FromArgb(155, 18, 18), Color.FromArgb(215, 65, 40), System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal)) e.Graphics.FillRectangle(b, hdr.ClientRectangle); };
            hdr.Controls.Add(new Label { Text = "", Font = new Font("Segoe UI", 38), ForeColor = Color.White, Location = new Point(35, 24), Size = new Size(70, 65), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "Help & Support", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, Location = new Point(115, 30), Size = new Size(500, 40), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "We're here to help -- contact us any time", Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(255, 210, 210), Location = new Point(115, 74), Size = new Size(500, 22), BackColor = Color.Transparent });
            this.Controls.Add(hdr);

            int y = 145;

            SH("Contact Our Team", y); y += 44;
            Panel contacts = new Panel { Location = new Point(0, y), Size = new Size(1200, 155), BackColor = Color.Transparent };
            this.Controls.Add(contacts);
            CCard(contacts, "", "Email Support", "help@postalms.com", "We respond within 24 hours.\nMark urgent emails as URGENT.", 0);
            CCard(contacts, "", "Bug Reports", "bugs@postalms.com", "Found a bug? Let us know!\nInclude steps to reproduce.", 310);
            CCard(contacts, "", "Feature Requests", "ideas@postalms.com", "Have an idea to improve PostalMS?\nWe love hearing from users!", 620);
            CCard(contacts, "", "Developer Team", "dev@postalms.com", "For technical or API enquiries,\ncontact our dev team directly.", 930);
            y += 170;

            SH("Meet the Development Team", y); y += 44;
            Panel team = new Panel { Location = new Point(0, y), Size = new Size(1200, 155), BackColor = Color.Transparent };
            this.Controls.Add(team);
            TCard(team, "", "Team Leader", "SCRUM Master -- leads sprints, removes blockers, ensures on-time delivery", 0);
            TCard(team, "", "Secretary", "Administrative support -- meeting minutes, documentation, organisation", 310);
            TCard(team, "", "Developer 1", "C# WinForms -- data structures, UI design, implementation", 620);
            TCard(team, "", "Developer 2", "SQL Server -- stored procedures, views, schema design", 930);
            y += 170;

            Panel team2 = new Panel { Location = new Point(0, y), Size = new Size(620, 155), BackColor = Color.Transparent };
            this.Controls.Add(team2);
            TCard(team2, "", "Tester / QA", "Quality assurance -- unit testing, integration testing, bug reporting", 0);
            y += 170;

            SH("Frequently Asked Questions", y); y += 44;
            string[,] faqs = {
                { "How do I track my parcel?",                   "Go to Parcels -> My Dashboard. Click any row to see full tracking details and delivery journey." },
                { "What if my parcel is lost or damaged?",       "Click 'Request Refund' on any delivered or failed parcel in your Dashboard. Reviewed within 3-5 days." },
                { "Can I send internationally?",                  "Yes! Go to Parcels -> Send Parcel / Mail, select 'International' and choose your destination country." },
                { "How long does delivery take?",                 "Standard: 3-5 days  |  Express: 1-2 days  |  Next Day: next working day. International varies." },
                { "What items are prohibited?",                  "Weapons, illegal goods, hazardous materials, liquids in mail. See guidelines on the Send Parcel page." },
                { "How do I edit my account details?",            "Click your name in the top right nav bar to access your Profile page." },
                { "What is the AI Assistant?",                    "The  AI Chat button opens an offline AI. Ask it to track parcels, estimate prices or predict delivery." },
                { "I forgot my password. What do I do?",         "Contact help@postalms.com with your registered email -- we'll reset it within 24 hours." },
            };

            Panel faqPanel = new Panel { Location = new Point(0, y), Size = new Size(1200, faqs.GetLength(0) * 70 + 10), BackColor = Color.White };
            this.Controls.Add(faqPanel);
            for (int i = 0; i < faqs.GetLength(0); i++)
            {
                Panel row = new Panel { Location = new Point(0, i * 70), Size = new Size(1200, 68), BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(255, 248, 248) };
                row.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(4, 68), BackColor = Red });
                row.Controls.Add(new Label { Text = "Q: " + faqs[i, 0], Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(18, 8), Size = new Size(1160, 22) });
                row.Controls.Add(new Label { Text = "A: " + faqs[i, 1], Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(50, 60, 80), Location = new Point(18, 32), Size = new Size(1160, 30) });
                faqPanel.Controls.Add(row);
            }
        }

        private void SH(string t, int y) { this.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Red, Location = new Point(0, y), Size = new Size(600, 30) }); this.Controls.Add(new Panel { Location = new Point(0, y + 32), Size = new Size(1200, 2), BackColor = Color.FromArgb(230, 180, 180) }); }

        private void CCard(Panel p, string icon, string title, string contact, string desc, int x)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(295, 150), BackColor = Color.White };
            c.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(295, 5), BackColor = Red });
            c.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI", 22), Location = new Point(12, 14), Size = new Size(44, 38), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 54), Size = new Size(270, 22) });
            c.Controls.Add(new Label { Text = contact, Font = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Underline), ForeColor = Color.FromArgb(30, 80, 180), Location = new Point(12, 78), Size = new Size(270, 18) });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray, Location = new Point(12, 100), Size = new Size(270, 44) });
            p.Controls.Add(c);
        }

        private void TCard(Panel p, string icon, string role, string desc, int x)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(295, 150), BackColor = Color.FromArgb(255, 248, 248) };
            c.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(295, 5), BackColor = Red });
            c.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI", 22), Location = new Point(12, 14), Size = new Size(44, 38), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = role, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 54), Size = new Size(270, 22) });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray, Location = new Point(12, 78), Size = new Size(270, 60) });
            p.Controls.Add(c);
        }
    }
}
