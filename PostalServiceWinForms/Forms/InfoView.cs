// InfoView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Information page showing details about PostalMS, the team and the module.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PostalServiceWinForms.Forms
{
    public class InfoView : UserControl
    {
        private Color Red = Color.FromArgb(180, 30, 30);

        public InfoView()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.AutoScroll = true;
            Build();
        }

        private void Build()
        {
            // Header
            Panel hdr = new Panel { Location = new Point(0, 0), Size = new Size(1400, 130), BackColor = Red };
            hdr.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(hdr.ClientRectangle,
                    Color.FromArgb(155, 18, 18), Color.FromArgb(215, 65, 40),
                    System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal))
                    e.Graphics.FillRectangle(b, hdr.ClientRectangle);
            };
            hdr.Controls.Add(new Label { Text = "(i)", Font = new Font("Segoe UI", 38), ForeColor = Color.White, Location = new Point(35, 24), Size = new Size(70, 65), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "How to Use PostalMS", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, Location = new Point(115, 30), Size = new Size(600, 40), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "Everything you need to know about using this app", Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(255, 210, 210), Location = new Point(115, 74), Size = new Size(600, 22), BackColor = Color.Transparent });
            this.Controls.Add(hdr);

            int y = 145;

            // -- Quick Start Guide --
            SH("Quick Start Guide", y); y += 44;

            // FIX: Use separate string arrays instead of 2D array to avoid index errors
            string[] nums = { "1", "2", "3", "4", "5", "6", "7", "8" };
            string[] titles = {
                "Register or Login",
                "Explore the Home Page",
                "Send a Parcel or Mail",
                "Track Your Parcels",
                "View Deliveries",
                "Request a Refund",
                "Use the AI Assistant",
                "Edit Your Profile"
            };
            string[] descs = {
                "If you're new, click 'Register here' on the login screen. Fill in your details and create your account. Then sign in.",
                "After logging in you land on the Home page -- an overview of PostalMS, its features and how to get started.",
                "Click 'Parcels' -> 'Send Parcel / Mail'. Choose Mail or Package, fill in receiver details, choose size and service type.",
                "Click 'Parcels' -> 'My Dashboard' to see all your parcels and stats. Click any row for full details.",
                "Click 'Deliveries' to see all delivery records. Filter by status and click any row for full details including failure reasons.",
                "In My Dashboard, click a delivered or failed parcel row -- a 'Request Refund' button appears. Describe the issue and submit.",
                "Click the ' AI Chat' button in the top nav. Ask naturally -- track a parcel, get a price, or predict delivery. Works offline.",
                "Click your name in the top right navigation to view and edit your account details (name, phone, address, postcode)."
            };

            Panel steps = new Panel { Location = new Point(0, y), Size = new Size(1200, 320), BackColor = Color.Transparent };
            this.Controls.Add(steps);

            for (int i = 0; i < 8; i++)
            {
                int col = i < 4 ? 0 : 610;
                int row = (i % 4) * 76;

                Panel card = new Panel { Location = new Point(col, row), Size = new Size(600, 70), BackColor = Color.White };

                // Number circle -- capture i value for paint event
                int stepNum = i;
                Panel circ = new Panel { Location = new Point(12, 12), Size = new Size(40, 40) };
                circ.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(new SolidBrush(Red), 0, 0, 39, 39);
                    e.Graphics.DrawString(nums[stepNum], new Font("Segoe UI", 13, FontStyle.Bold), Brushes.White, 9, 8);
                };

                card.Controls.Add(circ);
                card.Controls.Add(new Label { Text = titles[i], Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(62, 8), Size = new Size(525, 20) });
                card.Controls.Add(new Label { Text = descs[i], Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray, Location = new Point(62, 30), Size = new Size(525, 34) });
                steps.Controls.Add(card);
            }
            y += 330;

            // -- Pricing Guide --
            SH("Pricing Guide", y); y += 44;

            Panel pricing = new Panel { Location = new Point(0, y), Size = new Size(1200, 260), BackColor = Color.White };
            this.Controls.Add(pricing);

            // Header row
            Panel phdr = new Panel { Location = new Point(0, 0), Size = new Size(1200, 32), BackColor = Red };
            string[] heads = { "Service", "Est. Time", "Details", "Rate" };
            int[] hxs = { 15, 320, 530, 1100 };
            for (int i = 0; i < heads.Length; i++)
                phdr.Controls.Add(new Label { Text = heads[i], Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White, Location = new Point(hxs[i], 8), Size = new Size(200, 16), BackColor = Color.Transparent });
            pricing.Controls.Add(phdr);

            string[] services = {
                "  UK Domestic -- Standard",
                "  UK Domestic -- Express",
                "  UK Domestic -- Next Day",
                "  Zone 1 -- Europe",
                "  Zone 2 -- N. Europe",
                "  Zone 3 -- N. America & UAE",
                "  Zone 4 -- Rest of World"
            };
            string[] times = { "3-5 working days", "1-2 working days", "Next working day", "5-7 days", "6-8 days", "8-12 days", "12-16 days" };
            string[] details = {
                "Base price: GBP2.99 + GBP1.20 per kg",
                "Base + GBP2.00 surcharge",
                "Base + GBP5.00 surcharge",
                "France, Germany, Spain, Ireland...",
                "Poland, Sweden, Norway",
                "USA, Canada, UAE",
                "Australia, Japan, India, Brazil..."
            };
            string[] rates = { "x1.0", "x2.0", "x3.0", "x1.8", "x2.2", "x3.0", "x3.5-3.8" };

            for (int i = 0; i < services.Length; i++)
            {
                Panel row = new Panel { Location = new Point(0, 32 + i * 32), Size = new Size(1200, 32), BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(255, 248, 248) };
                Color rateColor = rates[i] == "x1.0" ? Color.FromArgb(20, 110, 50) : rates[i].StartsWith("x2") ? Color.FromArgb(140, 80, 0) : Red;
                row.Controls.Add(new Label { Text = services[i], Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(30, 40, 60), Location = new Point(hxs[0], 8), Size = new Size(290, 18) });
                row.Controls.Add(new Label { Text = times[i], Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(30, 40, 60), Location = new Point(hxs[1], 8), Size = new Size(200, 18) });
                row.Controls.Add(new Label { Text = details[i], Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(30, 40, 60), Location = new Point(hxs[2], 8), Size = new Size(560, 18) });
                row.Controls.Add(new Label { Text = rates[i], Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = rateColor, Location = new Point(hxs[3], 8), Size = new Size(90, 18) });
                pricing.Controls.Add(row);
            }
            y += 275;

            // -- Prohibited Items --
            SH("Prohibited Items -- Do NOT Send:", y); y += 44;

            Panel proh = new Panel { Location = new Point(0, y), Size = new Size(1200, 80), BackColor = Color.FromArgb(255, 235, 235) };
            this.Controls.Add(proh);
            proh.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 80), BackColor = Red });

            string[] prohibited = {
                "Weapons & ammunition",
                "Illegal drugs & substances",
                "Hazardous materials",
                "Live animals",
                "Perishable food (unapproved)",
                "Counterfeit goods",
                "Cash & negotiable items",
                "Human remains",
                "Lithium batteries (in mail)"
            };

            int px = 18; int col2 = 0;
            for (int i = 0; i < prohibited.Length; i++)
            {
                proh.Controls.Add(new Label
                {
                    Text = "  " + prohibited[i],
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Red,
                    Location = new Point(px, 8 + (i % 3) * 23),
                    Size = new Size(370, 20)
                });
                if ((i + 1) % 3 == 0) { col2++; px = 18 + col2 * 380; }
            }
            y += 95;

            // -- Data Structures note --
            Panel dsNote = new Panel { Location = new Point(0, y), Size = new Size(1200, 70), BackColor = Color.FromArgb(255, 235, 235) };
            this.Controls.Add(dsNote);
            dsNote.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 70), BackColor = Red });
            dsNote.Controls.Add(new Label { Text = "  Technical Note -- Data Structures", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(18, 8), Size = new Size(600, 20), BackColor = Color.Transparent });
            dsNote.Controls.Add(new Label { Text = "PostalMS uses custom-built data structures: Hash Table (O(1) parcel lookup), Binary Search Tree (O(log n) sorted display)\nand Queue (O(1) FIFO delivery processing). Click ' Data' in the navigation to see them working live.", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 30, 30), Location = new Point(18, 30), Size = new Size(1160, 36), BackColor = Color.Transparent });
        }

        private void SH(string t, int y)
        {
            this.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Red, Location = new Point(0, y), Size = new Size(600, 30) });
            this.Controls.Add(new Panel { Location = new Point(0, y + 32), Size = new Size(1200, 2), BackColor = Color.FromArgb(230, 180, 180) });
        }
    }
}
