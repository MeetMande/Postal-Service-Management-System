// StampsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Stamps page showing available stamp types and nearby post offices
// where customers can purchase stamps in person.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PostalServiceWinForms.Forms
{
    public class StampsView : UserControl
    {
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color Dark = Color.FromArgb(30, 30, 30);
        private Color Grey = Color.FromArgb(90, 90, 90);
        private Color LightBg = Color.FromArgb(245, 245, 245);

        public StampsView()
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
            header.Controls.Add(new Label { Text = "Buy Stamps", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 18), Size = new Size(400, 42), BackColor = Color.Transparent });
            this.Controls.Add(header);
            y += 90;

            // Intro
            this.Controls.Add(new Label { Text = "Browse our stamp options below and find your nearest location to purchase them.", Font = new Font("Segoe UI", 12), ForeColor = Grey, Location = new Point(20, y), Size = new Size(900, 28) });
            y += 44;

            // Stamp types section
            SHead("Available Stamp Types", y); y += 44;

            // Stamp cards
            StampCard("First Class Stamp", "1.10", "Delivery next working day within the UK.\nMax weight 100g. Standard letter size.", 20, y);
            StampCard("Second Class Stamp", "0.75", "Delivery within 2-3 working days.\nMax weight 100g. Standard letter size.", 320, y);
            StampCard("Large Letter Stamp", "1.55", "For larger envelopes up to 250g.\nDelivery within 2-3 working days.", 620, y);
            StampCard("International Stamp", "1.85", "Send letters to Europe and worldwide.\nDelivery within 3-7 working days.", 920, y);
            y += 200;

            // Book of stamps
            SHead("Stamp Books and Sheets", y); y += 44;

            BookCard("Book of 12 First Class", "12.50", "Save money with a book of 12 first class stamps.\nPerfect for regular senders.", 20, y);
            BookCard("Book of 12 Second Class", "8.75", "12 second class stamps at a discounted rate.\nIdeal for non-urgent letters.", 420, y);
            BookCard("Sheet of 25 Mixed", "22.00", "25 stamps in a mixed sheet of first and second class.\nGreat value for businesses.", 820, y);
            y += 175;

            // Where to buy section
            SHead("Where to Buy Stamps Near You", y); y += 44;

            // Info note
            Panel note = new Panel { Location = new Point(20, y), Size = new Size(960, 46), BackColor = Color.FromArgb(235, 245, 255) };
            note.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 46), BackColor = Color.FromArgb(30, 90, 180) });
            note.Controls.Add(new Label { Text = "All locations below sell PostalMS stamps. Simply walk in during opening hours and ask at the counter.", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(14, 12), Size = new Size(930, 22), BackColor = Color.Transparent });
            this.Controls.Add(note);
            y += 60;

            // Location cards
            BuyLocation("PostalMS Hendon Campus Centre", "The Burroughs, Hendon, London, NW4 4BT", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", 20, y);
            BuyLocation("PostalMS Cat Hill Centre", "Cat Hill, East Barnet, London, EN4 8HT", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", 500, y);
            y += 155;
            BuyLocation("PostalMS Archway Drop-Off", "2 Junction Road, Archway, London, N19 5QU", "Mon-Fri 9am-6pm, Sat 10am-2pm", 20, y);
            BuyLocation("PostalMS Wembley Centre", "Engineers Way, Wembley, London, HA9 0ED", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", 500, y);
            y += 155;
            BuyLocation("WH Smith -- Brent Cross", "Brent Cross Shopping Centre, London, NW4 3FP", "Mon-Sat 9am-8pm, Sun 11am-5pm", 20, y);
            BuyLocation("Tesco Express -- Golders Green", "186 Golders Green Road, London, NW11 9AA", "Open daily 7am-11pm", 500, y);
            y += 155;

            // Tip section
            Panel tip = new Panel { Location = new Point(20, y), Size = new Size(960, 70), BackColor = Color.FromArgb(240, 255, 240) };
            tip.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 70), BackColor = Color.FromArgb(20, 130, 65) });
            tip.Controls.Add(new Label { Text = "Top Tip: You can also attach stamps directly at any PostalMS service centre counter.", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(20, 100, 50), Location = new Point(14, 10), Size = new Size(930, 22), BackColor = Color.Transparent });
            tip.Controls.Add(new Label { Text = "Staff will weigh your item and help you choose the correct stamp. No appointment needed.", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(20, 100, 50), Location = new Point(14, 36), Size = new Size(930, 22), BackColor = Color.Transparent });
            this.Controls.Add(tip);
            y += 85;
        }

        private void SHead(string t, int y)
        {
            this.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(20, y), Size = new Size(700, 28) });
            this.Controls.Add(new Panel { Location = new Point(20, y + 30), Size = new Size(960, 2), BackColor = Color.FromArgb(230, 180, 180) });
        }

        private void StampCard(string name, string price, string desc, int x, int y)
        {
            Panel card = new Panel { Location = new Point(x, y), Size = new Size(280, 185), BackColor = Color.White };
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(280, 5), BackColor = Red });
            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 14), Size = new Size(252, 22) });
            card.Controls.Add(new Label { Text = "Price: GBP " + price, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Dark, Location = new Point(14, 42), Size = new Size(252, 26) });
            card.Controls.Add(new Panel { Location = new Point(14, 74), Size = new Size(252, 1), BackColor = Color.FromArgb(220, 220, 220) });
            card.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 82), Size = new Size(252, 60) });
            this.Controls.Add(card);
        }

        private void BookCard(string name, string price, string desc, int x, int y)
        {
            Panel card = new Panel { Location = new Point(x, y), Size = new Size(360, 160), BackColor = Color.White };
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(360, 5), BackColor = Red });
            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 14), Size = new Size(332, 22) });
            card.Controls.Add(new Label { Text = "GBP " + price + " per book", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Dark, Location = new Point(14, 42), Size = new Size(332, 26) });
            card.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 76), Size = new Size(332, 60) });
            this.Controls.Add(card);
        }

        private void BuyLocation(string name, string address, string hours, int x, int y)
        {
            Panel card = new Panel { Location = new Point(x, y), Size = new Size(460, 140), BackColor = Color.White };
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(460, 5), BackColor = Red });
            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 12), Size = new Size(432, 20) });
            card.Controls.Add(new Label { Text = "Address:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Grey, Location = new Point(14, 36), Size = new Size(80, 18) });
            card.Controls.Add(new Label { Text = address, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(96, 36), Size = new Size(350, 18) });
            card.Controls.Add(new Label { Text = "Hours:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Grey, Location = new Point(14, 60), Size = new Size(80, 18) });
            card.Controls.Add(new Label { Text = hours, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(96, 60), Size = new Size(350, 18) });
            card.Controls.Add(new Label { Text = "Stamps sold at counter -- no appointment needed", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(20, 130, 65), Location = new Point(14, 88), Size = new Size(432, 18) });
            this.Controls.Add(card);
        }
    }
}