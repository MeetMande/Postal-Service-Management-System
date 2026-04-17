// ParcelsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// The parcels page with two tabs: dashboard and send form.
// Dashboard shows the user's parcels with status tracking and refund requests.
// Send form allows sending mail and packages domestically and internationally.
// Price formula: (ServicePrice + Weight * 1.2) * SizeMultiplier * InternationalMultiplier

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Collections.Generic;

namespace PostalServiceWinForms.Forms
{
    public class ParcelsView : UserControl
    {
        // Dashboard
        private DataGridView dgvParcels;
        private Label lblTotalVal, lblPendingVal, lblInTransitVal, lblDeliveredVal, lblFailedVal, lblSpentVal;

        // Send form
        private TextBox txtRcvName, txtRcvAddr, txtWeight;
        private ComboBox cboSize, cboCountry;
        private Label lblPrice, lblTrackGen, lblEstDays;
        private Panel pnlIntlSection;
        private RadioButton rbDomestic, rbInternational;

        // Mail vs Package
        private Panel pnlMailCard, pnlPkgCard;
        private bool isMail = false;

        // Service panels
        private Panel pnlMailSvc, pnlPkgSvc;

        // -- Mail services (in one GroupBox so only ONE can be selected) --
        private Panel pnlMailFirst, pnlMailPriority, pnlMailExpress;
        private int mailSvcIndex = 0; // 0=FirstClass 1=Priority 2=Express

        // -- Package services (in one GroupBox so only ONE can be selected) --
        private Panel pnlPkgGround, pnlPkgPriority, pnlPkgExpress, pnlPkgNextDay;
        private int pkgSvcIndex = 0; // 0=Ground 1=Priority 2=Express 3=NextDay

        private Panel pnlDash, pnlSend;
        private Button btnDash, btnSend;

        private DatabaseHelper db;
        private string userID, userName;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);
        private Color Bg = Color.FromArgb(245, 245, 245);

        private Dictionary<string, (double mult, int days)> countries = new Dictionary<string, (double, int)>
        {
            {"France",(1.8,5)},{"Germany",(1.8,5)},{"Spain",(1.8,5)},{"Italy",(1.8,5)},
            {"Ireland",(1.8,4)},{"Netherlands",(1.8,5)},{"Belgium",(1.8,5)},{"Portugal",(1.8,6)},
            {"Poland",(2.2,7)},{"Sweden",(2.2,7)},{"Norway",(2.2,6)},{"United States",(3.0,10)},
            {"Canada",(3.0,10)},{"Australia",(3.8,14)},{"New Zealand",(3.8,14)},
            {"Japan",(3.8,12)},{"China",(3.5,12)},{"India",(3.2,12)},
            {"UAE",(2.8,8)},{"South Africa",(3.5,14)},{"Brazil",(3.8,16)},
        };

        public ParcelsView(string uid, string name, DatabaseHelper dbHelper)
        {
            userID = uid; userName = name; db = dbHelper;
            this.Dock = DockStyle.Fill; this.BackColor = Bg;
            Build();
        }

        private void Build()
        {
            // -- TOP BAR (Dock Top -- always visible) --
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(218, 218, 218)), 0, 51, topBar.Width, 51);
            this.Controls.Add(topBar);

            topBar.Controls.Add(new Label { Text = "Parcels", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 12), Size = new Size(115, 28) });

            btnDash = new Button
            {
                Text = "  My Dashboard",
                Location = new Point(132, 8),
                Size = new Size(185, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDash.FlatAppearance.BorderSize = 0;
            btnDash.Click += (s, e) => Switch("dash");
            topBar.Controls.Add(btnDash);

            btnSend = new Button
            {
                Text = "  Send Mail or Package",
                Location = new Point(325, 8),
                Size = new Size(225, 36),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Red,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderColor = Color.FromArgb(210, 190, 190);
            btnSend.FlatAppearance.BorderSize = 1;
            btnSend.Click += (s, e) => Switch("send");
            topBar.Controls.Add(btnSend);

            // -- DASHBOARD --
            pnlDash = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = true };
            this.Controls.Add(pnlDash);
            BuildDashboard();

            // -- SEND FORM (scrollable) --
            pnlSend = new Panel { Dock = DockStyle.Fill, BackColor = Bg, AutoScroll = true, Visible = false };
            this.Controls.Add(pnlSend);
            BuildSendForm();

            RefreshDashboard();
        }

        private void Switch(string which)
        {
            bool isDash = which == "dash";
            if (isDash) pnlDash.BringToFront(); else pnlSend.BringToFront();
            pnlDash.Visible = isDash; pnlSend.Visible = !isDash;
            btnDash.BackColor = isDash ? Red : Color.FromArgb(240, 240, 240);
            btnDash.ForeColor = isDash ? Color.White : Red;
            btnSend.BackColor = isDash ? Color.FromArgb(240, 240, 240) : Red;
            btnSend.ForeColor = isDash ? Red : Color.White;
            if (isDash) RefreshDashboard(); else RefreshTrackID();
        }

        // ============================
        // DASHBOARD
        // ============================
        private void BuildDashboard()
        {
            Panel cards = new Panel { Location = new Point(0, 8), Size = new Size(1280, 118), BackColor = Color.Transparent };
            pnlDash.Controls.Add(cards);
            MakeStat(cards, "  Total Parcels", out lblTotalVal, 0, Red);
            MakeStat(cards, "  Pending", out lblPendingVal, 215, Color.FromArgb(160, 90, 0));
            MakeStat(cards, "  In Transit", out lblInTransitVal, 430, Color.FromArgb(30, 90, 180));
            MakeStat(cards, "  Delivered", out lblDeliveredVal, 645, Color.FromArgb(20, 130, 65));
            MakeStat(cards, "  Failed", out lblFailedVal, 860, DarkRed);
            MakeStat(cards, "  Total Spent", out lblSpentVal, 1075, Color.FromArgb(90, 40, 160));

            pnlDash.Controls.Add(new Label { Text = "All My Parcels & Mail", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(0, 134), Size = new Size(340, 26) });
            pnlDash.Controls.Add(new Label { Text = "(i)  No parcels yet? Click  Send Mail or Package above to get started!", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(355, 138), Size = new Size(700, 20) });

            TextBox srch = new TextBox { Location = new Point(0, 168), Size = new Size(420, 32), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, Text = "  Search tracking ID or receiver...", ForeColor = Color.LightGray };
            srch.Enter += (s, e) => { if (srch.ForeColor == Color.LightGray) { srch.Text = ""; srch.ForeColor = Color.Black; } };
            srch.Leave += (s, e) => { if (srch.Text == "") { srch.Text = "  Search tracking ID or receiver..."; srch.ForeColor = Color.LightGray; } };
            srch.TextChanged += (s, e) => { if (srch.ForeColor != Color.LightGray) Filter(srch.Text); };
            pnlDash.Controls.Add(srch);

            dgvParcels = new DataGridView
            {
                Location = new Point(0, 208),
                Size = new Size(1280, 365),
                ReadOnly = true,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvParcels.ColumnHeadersDefaultCellStyle.BackColor = Red;
            dgvParcels.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvParcels.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvParcels.ColumnHeadersHeight = 44;
            dgvParcels.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvParcels.RowTemplate.Height = 42;
            dgvParcels.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 248, 248);
            dgvParcels.CellFormatting += StatusFmt;
            dgvParcels.SelectionChanged += DgvSelect;
            pnlDash.Controls.Add(dgvParcels);

            Panel det = new Panel { Location = new Point(0, 580), Size = new Size(1280, 0), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Name = "pnlDetail" };
            pnlDash.Controls.Add(det);
        }

        private void MakeStat(Panel p, string title, out Label valLabel, int x, Color color)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(205, 114), BackColor = Color.White };
            c.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(205, 5), BackColor = color });
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = color, Location = new Point(10, 12), Size = new Size(185, 20) });
            valLabel = new Label { Text = "--", Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = color, Location = new Point(10, 36), Size = new Size(185, 50) };
            c.Controls.Add(valLabel);
            p.Controls.Add(c);
        }

        private void RefreshDashboard()
        {
            try
            {
                dgvParcels.DataSource = db.GetParcelsByCustomer(userID);
                HC();
                DataTable s = db.GetUserStats(userID);
                if (s.Rows.Count > 0)
                {
                    DataRow r = s.Rows[0];
                    lblTotalVal.Text = r["TotalParcels"].ToString(); lblPendingVal.Text = r["Pending"].ToString();
                    lblInTransitVal.Text = r["InTransit"].ToString(); lblDeliveredVal.Text = r["Delivered"].ToString();
                    lblFailedVal.Text = r["Failed"].ToString(); lblSpentVal.Text = "GBP" + Convert.ToDouble(r["TotalSpent"]).ToString("0.00");
                }
            }
            catch { }
        }

        private void HC()
        {
            string[] hide = { "IsInternational", "RefundRequested", "RefundReason", "CustomerID", "SenderAddress", "ReceiverAddress" };
            foreach (string c in hide) if (dgvParcels.Columns.Contains(c)) dgvParcels.Columns[c].Visible = false;
            if (dgvParcels.Columns.Contains("DateSent")) dgvParcels.Columns["DateSent"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgvParcels.Columns.Contains("EstimatedDelivery")) dgvParcels.Columns["EstimatedDelivery"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgvParcels.Columns.Contains("Price")) dgvParcels.Columns["Price"].DefaultCellStyle.Format = "GBP0.00";
        }

        private void Filter(string q)
        {
            try
            {
                DataTable full = db.GetParcelsByCustomer(userID);
                if (string.IsNullOrEmpty(q)) { dgvParcels.DataSource = full; HC(); return; }
                DataTable f2 = full.Clone();
                foreach (DataRow row in full.Rows)
                    if (row["TrackingID"].ToString().ToLower().Contains(q.ToLower()) || row["ReceiverName"].ToString().ToLower().Contains(q.ToLower()))
                        f2.ImportRow(row);
                dgvParcels.DataSource = f2; HC();
            }
            catch { }
        }

        private void DgvSelect(object sender, EventArgs e)
        {
            if (dgvParcels.CurrentRow == null) return;
            Panel det = null;
            foreach (Control c in pnlDash.Controls) if (c.Name == "pnlDetail") { det = c as Panel; break; }
            if (det == null) return;
            det.Controls.Clear(); det.Size = new Size(1280, 190);
            var row = dgvParcels.CurrentRow;
            string tid = row.Cells["TrackingID"].Value?.ToString() ?? "--";
            string st = row.Cells["Status"].Value?.ToString() ?? "--";
            det.Controls.Add(new Label { Text = "Details -- " + tid, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 10), Size = new Size(500, 24) });
            void DR(string l, string v, int x, int y2) { det.Controls.Add(new Label { Text = l, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(x, y2), Size = new Size(160, 18) }); det.Controls.Add(new Label { Text = v, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(40, 20, 20), Location = new Point(x, y2 + 18), Size = new Size(210, 22) }); }
            DR("Tracking ID", tid, 14, 42); DR("Receiver", row.Cells["ReceiverName"].Value?.ToString() ?? "--", 240, 42);
            DR("Type", row.Cells["ParcelType"].Value?.ToString() ?? "--", 460, 42); DR("Service", row.Cells["ServiceType"].Value?.ToString() ?? "--", 640, 42);
            DR("Price", "GBP" + row.Cells["Price"].Value?.ToString(), 820, 42); DR("Status", st, 14, 108);
            string[] stages = { "Pending", "In Transit", "Out for Delivery", "Delivered" };
            int cur = Array.IndexOf(stages, st);
            for (int i = 0; i < 4; i++) { bool done = i <= cur; det.Controls.Add(new Label { Text = (done ? " " : " ") + stages[i], Font = new Font("Segoe UI", 9, done ? FontStyle.Bold : FontStyle.Regular), ForeColor = done ? Color.FromArgb(20, 130, 65) : Color.LightGray, Location = new Point(240 + i * 248, 108), Size = new Size(240, 36) }); }
            if (st == "Failed" || st == "Delivered") { Button br = new Button { Text = "Request Refund", Location = new Point(1056, 106), Size = new Size(150, 36), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; br.FlatAppearance.BorderSize = 0; br.Click += (s2, e2) => ShowRefund(tid); det.Controls.Add(br); }
        }

        private void ShowRefund(string tid)
        {
            Form f = new Form { Text = "Refund -- " + tid, Size = new Size(480, 300), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };
            f.Controls.Add(new Label { Text = "Refund Request", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(20, 18), Size = new Size(400, 28) });
            TextBox r2 = new TextBox { Location = new Point(20, 78), Size = new Size(430, 95), Font = new Font("Segoe UI", 10), Multiline = true, BorderStyle = BorderStyle.FixedSingle }; f.Controls.Add(r2);
            f.Controls.Add(new Label { Text = "Describe the reason:", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(20, 52), Size = new Size(430, 22) });
            f.Controls.Add(new Label { Text = "Reviewed within 3-5 working days.", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(20, 183), Size = new Size(430, 20) });
            Button sub = new Button { Text = "Submit", Location = new Point(20, 213), Size = new Size(130, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; sub.FlatAppearance.BorderSize = 0;
            sub.Click += (s2, e2) => { if (string.IsNullOrWhiteSpace(r2.Text)) { MessageBox.Show("Please enter a reason."); return; } MessageBox.Show(db.RequestRefund(tid, r2.Text.Trim())); f.Close(); RefreshDashboard(); };
            Button can = new Button { Text = "Cancel", Location = new Point(162, 213), Size = new Size(100, 40), Font = new Font("Segoe UI", 10), BackColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; can.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200); can.Click += (s2, e2) => f.Close();
            f.Controls.AddRange(new Control[] { sub, can }); f.ShowDialog();
        }

        // ============================
        // SEND FORM
        // ============================
        private void BuildSendForm()
        {
            int y = 8;

            // Tracking ID
            Panel tb = new Panel { Location = new Point(10, y), Size = new Size(1240, 38), BackColor = Color.FromArgb(255, 235, 235) };
            pnlSend.Controls.Add(tb);
            lblTrackGen = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 9), Size = new Size(1010, 22), BackColor = Color.Transparent };
            tb.Controls.Add(lblTrackGen);
            y += 48;

            // -- STEP 1: Mail or Package --
            SH("Step 1 -- What are you sending?", y); y += 36;
            pnlMailCard = MakeTypeCard("", "Send Mail", "Letters, documents, postcards\nMax 2kg     From GBP1.99", 10, y, 590, () => SelectType(true));
            pnlPkgCard = MakeTypeCard("", "Send a Package", "Parcels, goods, gifts of any size\nMax 30kg     From GBP2.99", 620, y, 630, () => SelectType(false));
            pnlSend.Controls.AddRange(new Control[] { pnlMailCard, pnlPkgCard });
            y += 118;

            // -- STEP 2: Services -- FIXED: each set is in its own GroupBox so only ONE selectable --
            SH("Step 2 -- Choose a Service (select one)", y); y += 36;

            // Mail services panel
            pnlMailSvc = new Panel { Location = new Point(10, y), Size = new Size(1240, 90), BackColor = Color.Transparent, Visible = false };
            pnlSend.Controls.Add(pnlMailSvc);

            // Package services panel
            pnlPkgSvc = new Panel { Location = new Point(10, y), Size = new Size(1240, 90), BackColor = Color.Transparent, Visible = true };
            pnlSend.Controls.Add(pnlPkgSvc);

            // Mail service cards -- clicking sets mailSvcIndex and redraws
            pnlMailFirst = SvcCard(pnlMailSvc, "  First Class", "Up to 100g, standard", "GBP1.99", "3-5 days", 0, 0, () => { mailSvcIndex = 0; RefreshMailSvc(); Recalc(); });
            pnlMailPriority = SvcCard(pnlMailSvc, "  Priority", "Up to 2kg, tracked", "GBP3.99", "1-2 days", 310, 1, () => { mailSvcIndex = 1; RefreshMailSvc(); Recalc(); });
            pnlMailExpress = SvcCard(pnlMailSvc, "  Express", "Up to 2kg, guaranteed", "GBP6.99", "Next day", 620, 2, () => { mailSvcIndex = 2; RefreshMailSvc(); Recalc(); });

            // Package service cards
            pnlPkgGround = SvcCard(pnlPkgSvc, "  Ground", "Up to 30kg, fully tracked", "GBP2.99+", "3-5 days", 0, 0, () => { pkgSvcIndex = 0; RefreshPkgSvc(); Recalc(); });
            pnlPkgPriority = SvcCard(pnlPkgSvc, "  Priority", "Faster delivery, tracked", "GBP5.99+", "1-2 days", 310, 1, () => { pkgSvcIndex = 1; RefreshPkgSvc(); Recalc(); });
            pnlPkgExpress = SvcCard(pnlPkgSvc, "  Express", "Guaranteed, tracked", "GBP9.99+", "Next day", 620, 2, () => { pkgSvcIndex = 2; RefreshPkgSvc(); Recalc(); });
            pnlPkgNextDay = SvcCard(pnlPkgSvc, "  Next Day", "By 1pm next working day", "GBP14.99+", "By 1pm", 930, 3, () => { pkgSvcIndex = 3; RefreshPkgSvc(); Recalc(); });

            // Set defaults
            mailSvcIndex = 0; pkgSvcIndex = 0;
            RefreshMailSvc(); RefreshPkgSvc();
            y += 100;

            // -- STEP 3: Destination --
            SH("Step 3 -- Where is it going?", y); y += 36;
            rbDomestic = new RadioButton { Text = "  UK Domestic", Font = new Font("Segoe UI", 11), Location = new Point(10, y), Size = new Size(220, 28), Checked = true };
            rbInternational = new RadioButton { Text = "  International", Font = new Font("Segoe UI", 11), Location = new Point(240, y), Size = new Size(200, 28) };
            pnlSend.Controls.AddRange(new Control[] { rbDomestic, rbInternational });
            y += 36;

            cboCountry = new ComboBox { Location = new Point(10, y), Size = new Size(360, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            foreach (var k in countries.Keys) cboCountry.Items.Add(k);
            cboCountry.SelectedIndex = 0; cboCountry.SelectedIndexChanged += (s, e) => Recalc();
            pnlSend.Controls.Add(cboCountry);

            pnlIntlSection = new Panel { Location = new Point(10, y + 38), Size = new Size(1240, 50), BackColor = Color.FromArgb(242, 255, 242), Visible = false };
            pnlSend.Controls.Add(pnlIntlSection);
            pnlIntlSection.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 50), BackColor = Color.FromArgb(20, 130, 65) });
            pnlIntlSection.Controls.Add(new Label { Text = "Zone 1 Europe x1.8 (5-7d)  |  Zone 2 N.Europe x2.2 (6-8d)  |  Zone 3 USA/UAE x3.0 (8-12d)  |  Zone 4 World x3.5-3.8 (12-16d)\nCustoms declaration required. Import duties may apply.", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(20, 80, 40), Location = new Point(12, 4), Size = new Size(1210, 42), BackColor = Color.Transparent });

            rbDomestic.CheckedChanged += (s, e) => { cboCountry.Visible = rbInternational.Checked; pnlIntlSection.Visible = rbInternational.Checked; Recalc(); };
            rbInternational.CheckedChanged += (s, e) => { cboCountry.Visible = rbInternational.Checked; pnlIntlSection.Visible = rbInternational.Checked; Recalc(); };
            y += 96;

            // -- STEP 4: Receiver --
            SH("Step 4 -- Receiver Details", y); y += 36;
            FL("RECEIVER FULL NAME", 10, y); FL("RECEIVER ADDRESS (include postcode)", 450, y); y += 16;
            txtRcvName = FT("Full name of recipient", 10, y, 425);
            txtRcvAddr = FT("Full delivery address + postcode", 450, y, 800); y += 50;

            // -- STEP 5: Size and Weight --
            SH("Step 5 -- Size and Weight", y); y += 36;
            FL("WEIGHT (KG)", 10, y); FL("SIZE", 270, y); y += 16;
            txtWeight = FT("e.g. 1.5", 10, y, 245); txtWeight.TextChanged += (s, e) => Recalc();
            cboSize = new ComboBox { Location = new Point(270, y), Size = new Size(270, 34), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cboSize.Items.AddRange(new string[] { "Small (up to 1kg)", "Medium (1-5kg)", "Large (5-15kg)", "Extra Large (15kg+)" });
            cboSize.SelectedIndex = 0; cboSize.SelectedIndexChanged += (s, e) => Recalc();
            pnlSend.Controls.Add(cboSize);
            y += 50;

            // Charges
            Panel chg = new Panel { Location = new Point(10, y), Size = new Size(1240, 36), BackColor = Color.FromArgb(255, 248, 220) };
            chg.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 36), BackColor = Color.FromArgb(180, 120, 0) });
            chg.Controls.Add(new Label { Text = "Extra charges: Tracked Mail +0.50  |  Service surcharges shown above  |  Insurance over 100: +2.50  |  International customs: varies", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(110, 70, 0), Location = new Point(12, 10), Size = new Size(1200, 18), BackColor = Color.Transparent });
            pnlSend.Controls.Add(chg);
            y += 44;

            // Price estimate
            Panel priceBox = new Panel { Location = new Point(10, y), Size = new Size(1240, 64), BackColor = Color.FromArgb(255, 235, 235) };
            priceBox.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 64), BackColor = Red });
            priceBox.Controls.Add(new Label { Text = "ESTIMATED PRICE AND DELIVERY TIME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(12, 5), Size = new Size(500, 15), BackColor = Color.Transparent });
            lblPrice = new Label { Text = "2.99", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 20), Size = new Size(200, 38), BackColor = Color.Transparent };
            lblEstDays = new Label { Text = "3-5 working days", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(218, 30), Size = new Size(500, 24), BackColor = Color.Transparent };
            priceBox.Controls.AddRange(new Control[] { lblPrice, lblEstDays });
            pnlSend.Controls.Add(priceBox);
            y += 72;

            // Prohibited
            Panel proh = new Panel { Location = new Point(10, y), Size = new Size(1240, 32), BackColor = Color.FromArgb(255, 240, 240) };
            proh.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 32), BackColor = Red });
            proh.Controls.Add(new Label { Text = "Prohibited: Weapons, illegal drugs, hazardous materials, live animals, counterfeit goods, cash, unapproved perishables", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(140, 30, 30), Location = new Point(12, 8), Size = new Size(1200, 18), BackColor = Color.Transparent });
            pnlSend.Controls.Add(proh);
            y += 40;

            // Submit
            Button bSub = new Button { Text = "Submit and Send", Location = new Point(10, y), Size = new Size(210, 48), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            bSub.FlatAppearance.BorderSize = 0;
            bSub.MouseEnter += (s, e) => bSub.BackColor = DarkRed;
            bSub.MouseLeave += (s, e) => bSub.BackColor = Red;
            bSub.Click += Submit_Click;
            Button bClr = new Button { Text = "Clear Form", Location = new Point(228, y), Size = new Size(120, 48), Font = new Font("Segoe UI", 11), BackColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            bClr.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            bClr.Click += (s, e) => ClearForm();
            pnlSend.Controls.AddRange(new Control[] { bSub, bClr });

            SelectType(false); RefreshTrackID(); Recalc();
        }

        // Makes a Mail/Package type card
        private Panel MakeTypeCard(string icon, string title, string desc, int x, int y, int w, Action onClick)
        {
            Panel c = new Panel { Location = new Point(x, y), Size = new Size(w, 108), BackColor = Color.White, Cursor = Cursors.Hand };
            c.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI", 26), Location = new Point(10, 10), Size = new Size(50, 44), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(66, 10), Size = new Size(w - 76, 24), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(66, 36), Size = new Size(w - 76, 38), BackColor = Color.Transparent });
            foreach (Control ch in c.Controls) ch.Click += (s, e) => onClick();
            c.Click += (s, e) => onClick();
            return c;
        }

        // Makes a service option card -- no RadioButton, uses index-based selection
        private Panel SvcCard(Panel parent, string title, string desc, string price, string time, int x, int idx, Action onSelect)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(295, 84), BackColor = Color.White, Cursor = Cursors.Hand };
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 6), Size = new Size(275, 20), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray, Location = new Point(12, 28), Size = new Size(275, 30), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = price, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 58), Size = new Size(130, 22), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = time, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(155, 60), Size = new Size(130, 18), BackColor = Color.Transparent });
            foreach (Control ch in c.Controls) ch.Click += (s, e) => onSelect();
            c.Click += (s, e) => onSelect();
            parent.Controls.Add(c);
            return c;
        }

        // Redraws mail service cards -- only selected one gets red border
        private void RefreshMailSvc()
        {
            Panel[] cards = { pnlMailFirst, pnlMailPriority, pnlMailExpress };
            for (int i = 0; i < cards.Length; i++)
            {
                int idx = i;
                cards[i].BackColor = mailSvcIndex == i ? Color.FromArgb(255, 245, 245) : Color.White;
                cards[i].Paint -= SvcPaint;
                if (mailSvcIndex == i)
                    cards[i].Paint += SvcPaint;
                cards[i].Invalidate();
            }
        }

        // Redraws package service cards -- only selected one gets red border
        private void RefreshPkgSvc()
        {
            Panel[] cards = { pnlPkgGround, pnlPkgPriority, pnlPkgExpress, pnlPkgNextDay };
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].BackColor = pkgSvcIndex == i ? Color.FromArgb(255, 245, 245) : Color.White;
                cards[i].Paint -= SvcPaint;
                if (pkgSvcIndex == i)
                    cards[i].Paint += SvcPaint;
                cards[i].Invalidate();
            }
        }

        private void SvcPaint(object s, PaintEventArgs e)
        {
            var p = s as Panel;
            if (p != null) e.Graphics.DrawRectangle(new Pen(Red, 3), 1, 1, p.Width - 3, p.Height - 3);
        }

        private void SelectType(bool mail)
        {
            isMail = mail;
            // Redraw type cards
            pnlMailCard.Paint -= MailCardPaint; pnlPkgCard.Paint -= PkgCardPaint;
            pnlMailCard.Paint += MailCardPaint; pnlPkgCard.Paint += PkgCardPaint;
            pnlMailCard.Invalidate(); pnlPkgCard.Invalidate();
            // Show correct service panel
            pnlMailSvc.Visible = mail;
            pnlPkgSvc.Visible = !mail;
            Recalc();
        }

        private void MailCardPaint(object s, PaintEventArgs e)
        {
            if (isMail) e.Graphics.DrawRectangle(new Pen(Red, 3), 1, 1, pnlMailCard.Width - 3, pnlMailCard.Height - 3);
            else e.Graphics.DrawRectangle(new Pen(Color.FromArgb(215, 215, 215), 1), 0, 0, pnlMailCard.Width - 1, pnlMailCard.Height - 1);
        }
        private void PkgCardPaint(object s, PaintEventArgs e)
        {
            if (!isMail) e.Graphics.DrawRectangle(new Pen(Red, 3), 1, 1, pnlPkgCard.Width - 3, pnlPkgCard.Height - 3);
            else e.Graphics.DrawRectangle(new Pen(Color.FromArgb(215, 215, 215), 1), 0, 0, pnlPkgCard.Width - 1, pnlPkgCard.Height - 1);
        }

        private void RefreshTrackID()
        {
            if (lblTrackGen == null) return;
            lblTrackGen.Text = "Tracking ID will be:  PS-" + DateTime.Now.Year + "-" + (db.GetParcelCount() + 1).ToString("D5") + "     (auto-assigned on submit)";
        }

        private string GetServiceName()
        {
            if (isMail) { switch (mailSvcIndex) { case 0: return "First Class"; case 1: return "Priority"; case 2: return "Express"; } }
            else { switch (pkgSvcIndex) { case 0: return "Standard"; case 1: return "Priority"; case 2: return "Express"; case 3: return "Next Day"; } }
            return "Standard";
        }

        private double GetServicePrice()
        {
            if (isMail) { switch (mailSvcIndex) { case 0: return 1.99; case 1: return 3.99; case 2: return 6.99; } }
            else { switch (pkgSvcIndex) { case 0: return 2.99; case 1: return 5.99; case 2: return 9.99; case 3: return 14.99; } }
            return 2.99;
        }

        private int GetServiceDays()
        {
            if (isMail) { switch (mailSvcIndex) { case 0: return 4; case 1: return 2; case 2: return 1; } }
            else { switch (pkgSvcIndex) { case 0: return 5; case 1: return 2; case 2: return 1; case 3: return 1; } }
            return 5;
        }

        private void Recalc()
        {
            try
            {
                double w = double.TryParse(txtWeight?.Text, out double wv) ? wv : 0;
                double sm = cboSize?.SelectedIndex == 0 ? 1.0 : cboSize?.SelectedIndex == 1 ? 1.5 : cboSize?.SelectedIndex == 2 ? 2.5 : 3.5;
                double svcP = GetServicePrice();
                int days = GetServiceDays();
                double intlM = 1.0;
                if (rbInternational?.Checked == true && cboCountry?.SelectedItem != null)
                {
                    string sel = cboCountry.SelectedItem.ToString();
                    if (countries.ContainsKey(sel)) { intlM = countries[sel].mult; days = countries[sel].days; }
                }
                double price = Math.Round((svcP + w * 1.2) * sm * intlM, 2);
                if (lblPrice != null) lblPrice.Text = "GBP" + price.ToString("0.00");
                if (lblEstDays != null) lblEstDays.Text = (rbInternational?.Checked == true ? "International -- " : "UK Domestic -- ") + days + " working day(s)";
            }
            catch { }
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            if (txtRcvName.ForeColor == Color.LightGray || string.IsNullOrWhiteSpace(txtRcvName.Text) ||
                txtRcvAddr.ForeColor == Color.LightGray || string.IsNullOrWhiteSpace(txtRcvAddr.Text))
            { MessageBox.Show("Please fill in receiver name and address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            bool isIntl = rbInternational.Checked;
            string ptype = isMail ? "Mail" : "Package";
            string svcName = GetServiceName();
            int cnt = db.GetParcelCount();
            string tid = "PS-" + DateTime.Now.Year + "-" + (cnt + 1).ToString("D5");
            double w = double.TryParse(txtWeight.Text, out double wv) ? wv : 0;
            double price = double.TryParse(lblPrice.Text.Replace("GBP", ""), out double pv) ? pv : 0;
            string country = isIntl && cboCountry.SelectedItem != null ? cboCountry.SelectedItem.ToString() : null;
            string sz = cboSize.SelectedItem.ToString().Split('(')[0].Trim().Split(' ')[0];

            string res = db.AddParcel(tid, userID, ptype, userName, "My Address",
                txtRcvName.Text.Trim(), txtRcvAddr.Text.Trim(),
                w, sz, svcName, isIntl, country, "Pending", price, DateTime.Now.AddDays(5));

            if (res.Contains("successfully"))
            {
                db.AutoAssignDelivery(tid, "DEL-" + DateTime.Now.Ticks.ToString().Substring(0, 8));
                MessageBox.Show(
                    "Parcel submitted successfully!\n\n" +
                    "Tracking ID: " + tid + "\n" +
                    "Type:        " + ptype + "\n" +
                    "Service:     " + svcName + "\n" +
                    "To:          " + (isIntl ? country + " (International)" : "UK Domestic") + "\n" +
                    "Price:       " + lblPrice.Text,
                    "Submitted!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm(); Switch("dash");
            }
            else MessageBox.Show(res, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ClearForm()
        {
            txtRcvName.Text = "Full name of recipient"; txtRcvName.ForeColor = Color.LightGray;
            txtRcvAddr.Text = "Full delivery address + postcode"; txtRcvAddr.ForeColor = Color.LightGray;
            txtWeight.Text = "e.g. 1.5"; txtWeight.ForeColor = Color.LightGray;
            cboSize.SelectedIndex = 0; rbDomestic.Checked = true;
            mailSvcIndex = 0; pkgSvcIndex = 0;
            RefreshMailSvc(); RefreshPkgSvc();
            SelectType(false); RefreshTrackID(); Recalc();
        }

        private void SH(string t, int y) { pnlSend.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Red, Location = new Point(10, y), Size = new Size(900, 26) }); pnlSend.Controls.Add(new Panel { Location = new Point(10, y + 28), Size = new Size(1240, 1), BackColor = Color.FromArgb(230, 180, 180) }); }
        private void FL(string t, int x, int y) => pnlSend.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(x, y), Size = new Size(430, 15) });
        private TextBox FT(string ph, int x, int y, int w) { TextBox tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = ph, ForeColor = Color.LightGray }; tb.Enter += (s, e) => { if (tb.ForeColor == Color.LightGray) { tb.Text = ""; tb.ForeColor = Color.Black; } }; tb.Leave += (s, e) => { if (tb.Text == "") { tb.Text = ph; tb.ForeColor = Color.LightGray; } }; pnlSend.Controls.Add(tb); return tb; }
        private void StatusFmt(object sender, DataGridViewCellFormattingEventArgs e) { var g = sender as DataGridView; if (g == null || e.ColumnIndex < 0 || e.ColumnIndex >= g.Columns.Count || g.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return; switch (e.Value.ToString()) { case "In Transit": e.CellStyle.ForeColor = Color.FromArgb(140, 80, 0); e.CellStyle.BackColor = Color.FromArgb(255, 243, 220); break; case "Delivered": e.CellStyle.ForeColor = Color.FromArgb(20, 110, 50); e.CellStyle.BackColor = Color.FromArgb(210, 248, 225); break; case "Pending": e.CellStyle.ForeColor = Color.FromArgb(50, 80, 160); e.CellStyle.BackColor = Color.FromArgb(220, 232, 255); break; case "Failed": e.CellStyle.ForeColor = Color.FromArgb(160, 30, 30); e.CellStyle.BackColor = Color.FromArgb(255, 220, 220); break; case "Out for Delivery": e.CellStyle.ForeColor = Color.FromArgb(80, 45, 160); e.CellStyle.BackColor = Color.FromArgb(235, 225, 255); break; } }
    }
}
