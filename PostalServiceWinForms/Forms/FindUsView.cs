// FindUsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Find Us page showing PostalMS office locations and contact details.
// Users can see where to visit in person to get help with their parcels.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PostalServiceWinForms.Forms
{
    public class FindUsView : UserControl
    {
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color Dark = Color.FromArgb(30, 30, 30);
        private Color Grey = Color.FromArgb(90, 90, 90);
        private Color LightBg = Color.FromArgb(245, 245, 245);

        public FindUsView()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = LightBg;
            this.AutoScroll = true;
            Build();
        }

        private void Build()
        {
            int y = 0;

            // Page header
            Panel header = new Panel { Location = new Point(0, y), Size = new Size(1400, 80), BackColor = Red };
            header.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(header.ClientRectangle,
                    Color.FromArgb(155, 18, 18), Color.FromArgb(210, 55, 35), 0f))
                    e.Graphics.FillRectangle(b, header.ClientRectangle);
            };
            header.Controls.Add(new Label { Text = "Find Us", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 18), Size = new Size(400, 42), BackColor = Color.Transparent });
            this.Controls.Add(header);
            y += 90;

            // Intro
            this.Controls.Add(new Label { Text = "Visit one of our PostalMS service centres to speak with our team in person.", Font = new Font("Segoe UI", 12), ForeColor = Grey, Location = new Point(20, y), Size = new Size(900, 28) });
            y += 44;

            // Opening hours banner
            Panel hours = new Panel { Location = new Point(20, y), Size = new Size(960, 50), BackColor = Color.FromArgb(255, 245, 220) };
            hours.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 50), BackColor = Color.FromArgb(180, 120, 0) });
            hours.Controls.Add(new Label { Text = "Opening Hours:  Monday to Friday  9:00am - 5:30pm  |  Saturday  9:00am - 1:00pm  |  Sunday  Closed", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(110, 70, 0), Location = new Point(16, 14), Size = new Size(930, 22), BackColor = Color.Transparent });
            this.Controls.Add(hours);
            y += 64;

            // Section heading
            SHead("Our Service Centres", y); y += 44;

            // Location cards
            LocationCard("PostalMS -- Hendon Campus Centre",
                "The Burroughs, Hendon, London, NW4 4BT",
                "020 8411 5000",
                "hendon@postalms.com",
                "Tube: Hendon Central (Northern Line)\nBus: 113, 143, 183\nParking available on site",
                20, y);

            LocationCard("PostalMS -- Cat Hill Service Centre",
                "Cat Hill, East Barnet, London, EN4 8HT",
                "020 8411 6000",
                "cathill@postalms.com",
                "Bus: 84, 299, 384\nParking available on site\n5 minutes walk from East Barnet",
                500, y);
            y += 260;

            LocationCard("PostalMS -- Archway Drop-Off Point",
                "2 Junction Road, Archway, London, N19 5QU",
                "020 7272 0000",
                "archway@postalms.com",
                "Tube: Archway (Northern Line)\nBus: 43, 134, 263\nNo parking -- public transport recommended",
                20, y);

            LocationCard("PostalMS -- Wembley Service Centre",
                "Engineers Way, Wembley, London, HA9 0ED",
                "020 8900 0000",
                "wembley@postalms.com",
                "Tube: Wembley Park (Metropolitan and Jubilee)\nBus: 83, 182, 224\nParking available nearby",
                500, y);
            y += 260;

            // What to bring section
            SHead("What to Bring When You Visit", y); y += 44;

            Panel bring = new Panel { Location = new Point(20, y), Size = new Size(960, 160), BackColor = Color.White };
            bring.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(960, 5), BackColor = Red });
            int bx = 20;
            string[] items = { "Photo ID (passport or driving licence)", "Your PostalMS account email address", "Parcel tracking ID if you have one", "Payment method (cash or card accepted)", "The item you wish to send (if dropping off)" };
            int by2 = 20;
            foreach (string item in items)
            {
                bring.Controls.Add(new Label { Text = "  -- " + item, Font = new Font("Segoe UI", 10), ForeColor = Dark, Location = new Point(bx, by2), Size = new Size(900, 24) });
                by2 += 26;
            }
            this.Controls.Add(bring);
            y += 175;

            // Contact us section
            SHead("Contact Us Directly", y); y += 44;

            Panel contact = new Panel { Location = new Point(20, y), Size = new Size(960, 130), BackColor = Color.White };
            contact.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(960, 5), BackColor = Red });
            contact.Controls.Add(new Label { Text = "General Enquiries:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(20, 18), Size = new Size(200, 22) });
            contact.Controls.Add(new Label { Text = "info@postalms.com  |  0800 123 4567 (free from UK landlines)", Font = new Font("Segoe UI", 10), ForeColor = Dark, Location = new Point(20, 40), Size = new Size(700, 22) });
            contact.Controls.Add(new Label { Text = "Parcel Issues:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(20, 68), Size = new Size(200, 22) });
            contact.Controls.Add(new Label { Text = "support@postalms.com  |  0800 987 6543", Font = new Font("Segoe UI", 10), ForeColor = Dark, Location = new Point(20, 90), Size = new Size(700, 22) });
            this.Controls.Add(contact);
            y += 145;
        }

        private void SHead(string t, int y)
        {
            this.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(20, y), Size = new Size(700, 28) });
            this.Controls.Add(new Panel { Location = new Point(20, y + 30), Size = new Size(960, 2), BackColor = Color.FromArgb(230, 180, 180) });
        }

        private void LocationCard(string name, string address, string phone, string email, string travel, int x, int y)
        {
            Panel card = new Panel { Location = new Point(x, y), Size = new Size(460, 245), BackColor = Color.White };
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(460, 5), BackColor = Red });

            // Name
            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 14), Size = new Size(428, 22) });

            // Address
            card.Controls.Add(new Label { Text = "Address:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Grey, Location = new Point(16, 42), Size = new Size(80, 18) });
            card.Controls.Add(new Label { Text = address, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(100, 42), Size = new Size(340, 18) });

            // Phone
            card.Controls.Add(new Label { Text = "Phone:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Grey, Location = new Point(16, 66), Size = new Size(80, 18) });
            card.Controls.Add(new Label { Text = phone, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(100, 66), Size = new Size(340, 18) });

            // Email
            card.Controls.Add(new Label { Text = "Email:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Grey, Location = new Point(16, 90), Size = new Size(80, 18) });
            card.Controls.Add(new Label { Text = email, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(100, 90), Size = new Size(340, 18) });

            // Divider
            card.Controls.Add(new Panel { Location = new Point(16, 114), Size = new Size(428, 1), BackColor = Color.FromArgb(220, 220, 220) });

            // Travel
            card.Controls.Add(new Label { Text = "How to get here:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Grey, Location = new Point(16, 122), Size = new Size(428, 18) });
            card.Controls.Add(new Label { Text = travel, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(16, 142), Size = new Size(428, 60) });

            this.Controls.Add(card);
        }
    }
}