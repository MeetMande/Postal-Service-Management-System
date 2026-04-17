// AIAssistantPanel.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Offline AI assistant for parcel support queries.
// Handles tracking, pricing, delivery estimates and FAQs.
// Works entirely offline using keyword matching and rule-based responses.
// No internet connection or external API required.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace PostalServiceWinForms.Forms
{
    public class AIAssistantPanel : Form
    {
        private Panel pnlMessages;
        private TextBox txtInput;
        private Button btnSend;
        private Label lblTyping;
        private DatabaseHelper db;
        private string userID, userName;
        private int _msgY = 10;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);
        private Color Bg = Color.FromArgb(255, 248, 248);

        public AIAssistantPanel(string uid, string name, DatabaseHelper dbHelper)
        {
            userID = uid; userName = name; db = dbHelper;
            this.Text = "PostalMS AI Assistant";
            this.Size = new Size(520, 700);
            this.MinimumSize = new Size(480, 500);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Bg;
            Rectangle scr = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(scr.Right - 540, scr.Bottom - 720);
            BuildUI();
            AddMsg("ai",
                $" Hi {userName.Split(' ')[0]}! I'm your PostalMS AI.\n\n" +
                " Track: 'track PS-2026-00001'\n" +
                " Price: 'how much for 2kg express?'\n" +
                " Predict: 'when will PS-2026-00001 arrive?'\n" +
                " My parcels: 'show my parcels'\n\n" +
                "Type below and press Enter or ");
        }

        private void BuildUI()
        {
            // IMPORTANT: With Dock layout, controls added LAST appear at TOP
            // So we add in this order: Input (Bottom) -> Typing (Bottom) -> Messages (Fill) -> QuickBtns (Top) -> Header (Top)
            // The last Top added will be the topmost

            // 1. INPUT BAR -- Dock Bottom (always visible at very bottom)
            Panel inputBar = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            inputBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(215, 215, 215)), 0, 0, inputBar.Width, 0);
            this.Controls.Add(inputBar);

            txtInput = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(10, 12),
                Size = new Size(this.ClientSize.Width - 82, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.Gray,
                Text = "Ask me anything..."
            };
            txtInput.Enter += (s, e) => { if (txtInput.ForeColor == Color.Gray) { txtInput.Text = ""; txtInput.ForeColor = Color.Black; } };
            txtInput.Leave += (s, e) => { if (txtInput.Text == "") { txtInput.Text = "Ask me anything..."; txtInput.ForeColor = Color.Gray; } };
            txtInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Send(txtInput.Text.Trim()); } };

            btnSend = new Button
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Size = new Size(58, 36),
                Location = new Point(this.ClientSize.Width - 70, 10),
                Text = "",
                Font = new Font("Segoe UI", 14),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => Send(txtInput.Text.Trim());
            inputBar.Controls.AddRange(new Control[] { txtInput, btnSend });

            this.Resize += (s, e) =>
            {
                txtInput.Size = new Size(inputBar.Width - 82, 32);
                btnSend.Location = new Point(inputBar.Width - 70, 10);
            };

            // 2. TYPING INDICATOR -- Dock Bottom
            lblTyping = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "   AI is thinking...",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                BackColor = Bg,
                Visible = false
            };
            this.Controls.Add(lblTyping);

            // 3. MESSAGES -- Dock Fill
            pnlMessages = new Panel { Dock = DockStyle.Fill, BackColor = Bg, AutoScroll = true };
            this.Controls.Add(pnlMessages);

            // 4. QUICK BUTTONS -- Dock Top (added after Fill so it sits above Fill)
            Panel qb = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(255, 235, 235) };
            this.Controls.Add(qb);
            string[] hints = { "My parcels", "Track parcel", "Price estimate", "Help" };
            int sx = 8;
            foreach (string hint in hints)
            {
                Button hb = new Button
                {
                    Text = hint,
                    Location = new Point(sx, 8),
                    Size = new Size(112, 28),
                    Font = new Font("Segoe UI", 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Red,
                    Cursor = Cursors.Hand
                };
                hb.FlatAppearance.BorderColor = Color.FromArgb(230, 160, 160);
                string h = hint;
                hb.Click += (s, e) => Send(h);
                qb.Controls.Add(hb);
                sx += 120;
            }

            // 5. HEADER -- Dock Top (added LAST = appears at very top)
            Panel hdr = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Red };
            hdr.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(
                    hdr.ClientRectangle, Color.FromArgb(155, 18, 18), Color.FromArgb(210, 60, 40), 0f))
                    e.Graphics.FillRectangle(b, hdr.ClientRectangle);
            };
            hdr.Controls.Add(new Label { Text = "", Font = new Font("Segoe UI", 22), ForeColor = Color.White, Location = new Point(14, 14), Size = new Size(44, 40), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "AI Parcel Assistant", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, Location = new Point(60, 12), Size = new Size(300, 24), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "Offline AI     No internet needed", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(255, 200, 200), Location = new Point(60, 36), Size = new Size(300, 18), BackColor = Color.Transparent });
            this.Controls.Add(hdr);
        }

        private void AddMsg(string role, string text)
        {
            if (pnlMessages.InvokeRequired) { pnlMessages.Invoke(new Action(() => AddMsg(role, text))); return; }
            bool isUser = role == "user";
            int maxW = Math.Min(370, pnlMessages.ClientSize.Width - 40);

            Label roleLbl = new Label
            {
                Text = isUser ? userName.Split(' ')[0] : " AI",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(isUser ? pnlMessages.Width - maxW - 18 : 12, _msgY),
                Size = new Size(150, 14)
            };
            _msgY += 16;

            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = isUser ? Color.White : Color.FromArgb(40, 10, 10),
                MaximumSize = new Size(maxW - 24, 0),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Panel bubble = new Panel { BackColor = isUser ? Red : Color.White };
            bubble.Controls.Add(lbl);
            lbl.Location = new Point(12, 10);
            lbl.CreateControl();
            bubble.Size = new Size(Math.Min(lbl.Width + 28, maxW), lbl.Height + 22);
            bubble.Location = new Point(isUser ? pnlMessages.Width - bubble.Width - 14 : 12, _msgY);

            pnlMessages.Controls.AddRange(new Control[] { roleLbl, bubble });
            _msgY += bubble.Height + 20;
            pnlMessages.AutoScrollPosition = new Point(0, _msgY);
        }

        private void Send(string input)
        {
            if (string.IsNullOrEmpty(input) || txtInput.ForeColor == Color.Gray) return;
            txtInput.Text = ""; txtInput.ForeColor = Color.Black;
            AddMsg("user", input);
            lblTyping.Visible = true; btnSend.Enabled = false;
            var timer = new System.Windows.Forms.Timer { Interval = 380 };
            timer.Tick += (s, e) => { timer.Stop(); AddMsg("ai", AI(input.ToLower().Trim())); lblTyping.Visible = false; btnSend.Enabled = true; };
            timer.Start();
        }

        private string AI(string input)
        {
            if (input.Contains("track") || input.Contains("where is") || input.Contains("ps-") || (input.Contains("status") && input.Contains("parcel")))
            { string tid = TID(input); return string.IsNullOrEmpty(tid) ? "Please provide a tracking ID.\nExample: 'track PS-2026-00001'" : Track(tid); }
            if (input.Contains("price") || input.Contains("cost") || input.Contains("how much") || input.Contains("charge")) return Price(input);
            if (input.Contains("when") || input.Contains("arrive") || input.Contains("delivery date"))
            { string tid = TID(input); return string.IsNullOrEmpty(tid) ? "Please provide a tracking ID.\nExample: 'when will PS-2026-00001 arrive?'" : Predict(tid); }
            if (input.Contains("my parcel") || input == "my parcels" || input.Contains("show my")) return MyParcels();
            if (input.Contains("refund")) return "To request a refund:\n1. Parcels -> My Dashboard\n2. Click the parcel row\n3. Click 'Request Refund'\n4. Describe the issue\n\nRefunds reviewed in 3-5 working days.";
            if (input.Contains("international") || input.Contains("abroad")) return "International Shipping:\nParcels -> Send Mail or Package\nSelect 'International' then country.\n\nZone 1 (Europe): x1.8  |  5-7 days\nZone 2 (N.Europe): x2.2  |  6-8 days\nZone 3 (USA/UAE): x3.0  |  8-12 days\nZone 4 (World): x3.5+  |  12-16 days";
            if (input.Contains("help") || input == "help") return "I can help with:\n\n 'track PS-2026-00001'\n 'how much for 2kg express?'\n 'when will PS-2026-00001 arrive?'\n 'show my parcels'\n 'how do I refund?'\n 'international info'";
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey")) return $"Hello {userName.Split(' ')[0]}!  How can I help?";
            if (input.Contains("thank")) return "You're welcome! ";
            if (input.Contains("bye")) return "Goodbye! ";
            return "I'm not sure about that.\n\nTry:\n  'track PS-2026-00001'\n  'how much for 2kg express?'\n  'show my parcels'\n  'help'";
        }

        private string Track(string tid)
        {
            try
            {
                object cached = db.ParcelHashTable.Search(tid);
                string note = cached != null ? $"\n[ Hash Table O(1): {cached}]" : "\n[Queried DB]";
                DataTable dt = db.SearchParcel(tid);
                if (dt.Rows.Count == 0) return $" No parcel found: {tid}";
                DataRow r = dt.Rows[0]; string s = r["Status"].ToString();
                string em = s == "Delivered" ? "" : s == "Failed" ? "" : s == "In Transit" ? "" : s == "Out for Delivery" ? "" : "";
                return $"{em} Found!\n\nTracking: {r["TrackingID"]}\nType:     {r["ParcelType"]}\nSender:   {r["SenderName"]}\nReceiver: {r["ReceiverName"]}\nStatus:   {s}\nService:  {r["ServiceType"]}\nPrice:    GBP{r["Price"]}\nSent:     {DateTime.Parse(r["DateSent"].ToString()):dd/MM/yyyy}{note}";
            }
            catch (Exception ex) { return " Error: " + ex.Message; }
        }

        private string Price(string input)
        {
            double w = 1.0;
            foreach (var word in input.Split(' ')) if (double.TryParse(word.Replace("kg", ""), out double wv)) { w = wv; break; }
            string sz = input.Contains("large") || w > 5 ? "Large" : input.Contains("medium") || w > 1 ? "Medium" : "Small";
            string tp = input.Contains("next day") ? "Next Day" : input.Contains("express") ? "Express" : "Standard";
            double sm = sz == "Small" ? 1.0 : sz == "Medium" ? 1.5 : 2.5;
            double extra = tp == "Next Day" ? 5.0 : tp == "Express" ? 2.0 : 0;
            double im = 1.0; string dest = "UK";
            if (input.Contains("france") || input.Contains("germany") || input.Contains("europe")) { im = 1.8; dest = "Europe"; }
            else if (input.Contains("usa") || input.Contains("america")) { im = 3.0; dest = "N.America"; }
            else if (input.Contains("australia") || input.Contains("japan")) { im = 3.8; dest = "Asia/Pacific"; }
            double price = Math.Round((2.99 + w * 1.2) * sm * im + extra, 2);
            int days = tp == "Standard" ? 5 : tp == "Express" ? 2 : 1;
            if (im > 1) days = im < 2 ? 7 : im < 3 ? 10 : 14;
            return $" Price Estimate\n\nWeight:  {w}kg | Size: {sz}\nService: {tp} | To: {dest}\n-------------------\nPrice:   GBP{price}\nEst:     {days} working day(s)\n\nGo to Parcels -> Send to book!";
        }

        private string Predict(string tid)
        {
            try
            {
                DataTable dt = db.SearchParcel(tid);
                if (dt.Rows.Count == 0) return $" No parcel found: {tid}";
                DataRow r = dt.Rows[0]; string s = r["Status"].ToString(); string tp = r["ServiceType"].ToString();
                string pred; int d;
                switch (s)
                {
                    case "Pending": d = tp == "Next Day" ? 2 : tp == "Express" ? 3 : 6; pred = $"Not dispatched yet. ~{d} days."; break;
                    case "In Transit": d = tp == "Express" ? 1 : 2; pred = $"On its way! ~{d} day(s)."; break;
                    case "Out for Delivery": d = 0; pred = "Out for delivery TODAY! "; break;
                    case "Delivered": d = 0; pred = "Already delivered! "; break;
                    case "Failed": d = 2; pred = "Delivery failed. Re-attempt in ~1-2 days."; break;
                    default: d = -1; pred = "Contact support."; break;
                }
                string eta = d > 0 ? DateTime.Now.AddDays(d).ToString("ddd, dd MMM") : d == 0 ? "Today" : "N/A";
                return $" Prediction\n\nParcel:  {tid}\nStatus:  {s}\nService: {tp}\n-------------------\n{pred}\n\nETA: {eta}";
            }
            catch (Exception ex) { return " Error: " + ex.Message; }
        }

        private string MyParcels()
        {
            try
            {
                DataTable dt = db.GetParcelsByCustomer(userID);
                if (dt.Rows.Count == 0) return $"No parcels yet, {userName.Split(' ')[0]}!\nGo to Parcels -> Send Mail or Package.";
                string res = $" Your Parcels ({dt.Rows.Count} total)\n\n"; int shown = 0;
                foreach (DataRow r in dt.Rows)
                {
                    if (shown >= 6) { res += $"...and {dt.Rows.Count - 6} more.\nSee Parcels -> My Dashboard."; break; }
                    string s2 = r["Status"].ToString();
                    string em = s2 == "Delivered" ? "" : s2 == "Failed" ? "" : s2 == "In Transit" ? "" : "";
                    res += $"{em} {r["TrackingID"]}  ->  {r["ReceiverName"]}  [{s2}]\n"; shown++;
                }
                return res;
            }
            catch { return "Could not load parcels."; }
        }

        private string TID(string input)
        {
            string up = input.ToUpper(); int idx = up.IndexOf("PS-");
            if (idx < 0) return "";
            string sub = up.Substring(idx); int end = sub.IndexOf(' ');
            return end > 0 ? sub.Substring(0, end).Trim() : sub.Trim();
        }
    }
}