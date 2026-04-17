// ParcelsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Parcels page - shows user's parcels and send form.
// Dashboard removed - parcels are shown directly in a clean grid.
// Drop-off location and navigation instructions added.
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
        // Parcel grid
        private DataGridView dgvParcels;

        // Send form controls
        private TextBox txtRcvName, txtRcvAddr, txtWeight, txtSenderEmail;
        private ComboBox cboSize, cboCountry;
        private Label lblPrice, lblTrackGen, lblEstDays, lblDropOff;
        private Panel pnlIntlSection, pnlDropOff;
        private RadioButton rbDomestic, rbInternational;

        // Mail vs Package selection
        private Panel pnlMailCard, pnlPkgCard;
        private bool isMail = false;

        // Service selection panels
        private Panel pnlMailSvc, pnlPkgSvc;
        private Panel pnlMailFirst, pnlMailPriority, pnlMailExpress;
        private int mailSvcIndex = 0;
        private Panel pnlPkgGround, pnlPkgPriority, pnlPkgExpress, pnlPkgNextDay;
        private int pkgSvcIndex = 0;

        // Main panels
        private Panel pnlList, pnlSend;
        private Button btnList, btnSend;

        private DatabaseHelper db;
        private string userID, userName;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);
        private Color Bg = Color.FromArgb(245, 245, 245);

        // Drop off locations for domestic parcels
        private string[] dropOffLocations = {
            "PostalMS Hub - 1 Station Road, London N1 9AA  (Mon-Sat 8am-8pm)",
            "PostalMS Express - 45 High Street, London EC1A 1AA  (Mon-Fri 9am-6pm)",
            "PostalMS North - 12 Market Square, London N7 6JN  (Mon-Sat 8am-7pm)",
            "PostalMS South - 88 Brixton Road, London SW9 8PQ  (Mon-Sat 9am-8pm)",
            "PostalMS West - 22 Shepherd's Bush Road, London W6 7PH  (Mon-Fri 8am-6pm)",
        };

        // International countries with zone multiplier and estimated days
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
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            Build();
        }

        private void Build()
        {
            // Top bar with navigation buttons
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(218, 218, 218)), 0, 51, topBar.Width, 51);
            this.Controls.Add(topBar);

            topBar.Controls.Add(new Label
            {
                Text = "Parcels",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(10, 12),
                Size = new Size(115, 28)
            });

            // My Parcels button
            btnList = new Button
            {
                Text = "  My Parcels",
                Location = new Point(132, 8),
                Size = new Size(160, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnList.FlatAppearance.BorderSize = 0;
            btnList.Click += (s, e) => Switch("list");
            topBar.Controls.Add(btnList);

            // Send Mail or Package button
            btnSend = new Button
            {
                Text = "  Send Mail or Package",
                Location = new Point(300, 8),
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

            // Parcel list panel
            pnlList = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = true };
            this.Controls.Add(pnlList);
            BuildParcelList();

            // Send form panel
            pnlSend = new Panel { Dock = DockStyle.Fill, BackColor = Bg, AutoScroll = true, Visible = false };
            this.Controls.Add(pnlSend);
            BuildSendForm();

            RefreshParcels();
        }

        // Switch between My Parcels and Send form
        private void Switch(string which)
        {
            bool isList = which == "list";
            if (isList) pnlList.BringToFront(); else pnlSend.BringToFront();
            pnlList.Visible = isList; pnlSend.Visible = !isList;
            btnList.BackColor = isList ? Red : Color.FromArgb(240, 240, 240);
            btnList.ForeColor = isList ? Color.White : Red;
            btnSend.BackColor = isList ? Color.FromArgb(240, 240, 240) : Red;
            btnSend.ForeColor = isList ? Red : Color.White;
            if (isList) RefreshParcels(); else RefreshTrackID();
        }

        // Build the parcel list section
        private void BuildParcelList()
        {
            // Page heading
            pnlList.Controls.Add(new Label
            {
                Text = "All My Parcels",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(0, 10),
                Size = new Size(400, 28)
            });

            pnlList.Controls.Add(new Label
            {
                Text = "All your submitted parcels and their current status are shown below.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, 42),
                Size = new Size(800, 20)
            });

            // Search box
            TextBox srch = new TextBox
            {
                Location = new Point(0, 72),
                Size = new Size(420, 32),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Search tracking ID or receiver...",
                ForeColor = Color.LightGray
            };
            srch.Enter += (s, e) => { if (srch.ForeColor == Color.LightGray) { srch.Text = ""; srch.ForeColor = Color.Black; } };
            srch.Leave += (s, e) => { if (srch.Text == "") { srch.Text = "Search tracking ID or receiver..."; srch.ForeColor = Color.LightGray; } };
            srch.TextChanged += (s, e) => { if (srch.ForeColor != Color.LightGray) FilterParcels(srch.Text); };
            pnlList.Controls.Add(srch);

            // Parcel grid
            dgvParcels = new DataGridView
            {
                Location = new Point(0, 112),
                Size = new Size(1280, 460),
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
            pnlList.Controls.Add(dgvParcels);

            // Detail panel shown when row selected
            Panel det = new Panel
            {
                Location = new Point(0, 580),
                Size = new Size(1280, 0),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Name = "pnlDetail"
            };
            pnlList.Controls.Add(det);
        }

        private void RefreshParcels()
        {
            try
            {
                dgvParcels.DataSource = db.GetParcelsByCustomer(userID);
                HideColumns();
            }
            catch { }
        }

        private void HideColumns()
        {
            // Hide internal columns not needed by the user
            string[] hide = { "IsInternational", "RefundRequested", "RefundReason", "CustomerID", "SenderAddress", "ReceiverAddress" };
            foreach (string col in hide)
                if (dgvParcels.Columns.Contains(col))
                    dgvParcels.Columns[col].Visible = false;

            // Format date and price columns
            if (dgvParcels.Columns.Contains("DateSent"))
                dgvParcels.Columns["DateSent"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgvParcels.Columns.Contains("EstimatedDelivery"))
                dgvParcels.Columns["EstimatedDelivery"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgvParcels.Columns.Contains("Price"))
                dgvParcels.Columns["Price"].DefaultCellStyle.Format = "GBP 0.00";
        }

        private void FilterParcels(string query)
        {
            try
            {
                DataTable full = db.GetParcelsByCustomer(userID);
                if (string.IsNullOrEmpty(query)) { dgvParcels.DataSource = full; HideColumns(); return; }
                DataTable filtered = full.Clone();
                foreach (DataRow row in full.Rows)
                    if (row["TrackingID"].ToString().ToLower().Contains(query.ToLower()) ||
                        row["ReceiverName"].ToString().ToLower().Contains(query.ToLower()))
                        filtered.ImportRow(row);
                dgvParcels.DataSource = filtered;
                HideColumns();
            }
            catch { }
        }

        // Show parcel detail when row is selected
        private void DgvSelect(object sender, EventArgs e)
        {
            if (dgvParcels.CurrentRow == null) return;
            Panel det = null;
            foreach (Control c in pnlList.Controls)
                if (c.Name == "pnlDetail") { det = c as Panel; break; }
            if (det == null) return;

            det.Controls.Clear();
            det.Size = new Size(1280, 200);

            var row = dgvParcels.CurrentRow;
            string tid = row.Cells["TrackingID"].Value?.ToString() ?? "--";
            string st = row.Cells["Status"].Value?.ToString() ?? "--";

            // Detail heading
            det.Controls.Add(new Label
            {
                Text = "Details -- " + tid,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(14, 10),
                Size = new Size(500, 24)
            });

            // Detail rows
            void DR(string l, string v, int x, int y2)
            {
                det.Controls.Add(new Label { Text = l, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(x, y2), Size = new Size(160, 18) });
                det.Controls.Add(new Label { Text = v, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(40, 20, 20), Location = new Point(x, y2 + 18), Size = new Size(210, 22) });
            }

            DR("Tracking ID", tid, 14, 42);
            DR("Receiver", row.Cells["ReceiverName"].Value?.ToString() ?? "--", 240, 42);
            DR("Type", row.Cells["ParcelType"].Value?.ToString() ?? "--", 460, 42);
            DR("Service", row.Cells["ServiceType"].Value?.ToString() ?? "--", 640, 42);
            DR("Price", "GBP " + row.Cells["Price"].Value?.ToString(), 820, 42);
            DR("Status", st, 14, 108);

            // Status progress bar
            string[] stages = { "Pending", "In Transit", "Out for Delivery", "Delivered" };
            int cur = Array.IndexOf(stages, st);
            for (int i = 0; i < 4; i++)
            {
                bool done = i <= cur;
                det.Controls.Add(new Label
                {
                    Text = (done ? "OK " : "- ") + stages[i],
                    Font = new Font("Segoe UI", 9, done ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = done ? Color.FromArgb(20, 130, 65) : Color.LightGray,
                    Location = new Point(240 + i * 248, 108),
                    Size = new Size(240, 36)
                });
            }

            // Refund button
            if (st == "Failed" || st == "Delivered")
            {
                Button br = new Button
                {
                    Text = "Request Refund",
                    Location = new Point(1056, 106),
                    Size = new Size(150, 36),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = Red,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                br.FlatAppearance.BorderSize = 0;
                br.Click += (s2, e2) => ShowRefund(tid);
                det.Controls.Add(br);
            }
        }

        private void ShowRefund(string tid)
        {
            Form f = new Form
            {
                Text = "Refund -- " + tid,
                Size = new Size(480, 300),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };
            f.Controls.Add(new Label { Text = "Refund Request", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(20, 18), Size = new Size(400, 28) });
            TextBox r2 = new TextBox { Location = new Point(20, 78), Size = new Size(430, 95), Font = new Font("Segoe UI", 10), Multiline = true, BorderStyle = BorderStyle.FixedSingle };
            f.Controls.Add(r2);
            f.Controls.Add(new Label { Text = "Describe the reason:", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(20, 52), Size = new Size(430, 22) });
            Button sub = new Button { Text = "Submit", Location = new Point(20, 213), Size = new Size(130, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            sub.FlatAppearance.BorderSize = 0;
            sub.Click += (s2, e2) =>
            {
                if (string.IsNullOrWhiteSpace(r2.Text)) { MessageBox.Show("Please enter a reason."); return; }
                MessageBox.Show(db.RequestRefund(tid, r2.Text.Trim()));
                f.Close(); RefreshParcels();
            };
            f.Controls.Add(sub);
            f.ShowDialog();
        }

        // ============================================================
        // SEND FORM
        // ============================================================
        private void BuildSendForm()
        {
            int y = 8;

            // Auto-generated tracking ID display
            Panel tb = new Panel { Location = new Point(10, y), Size = new Size(1240, 38), BackColor = Color.FromArgb(235, 245, 255) };
            pnlSend.Controls.Add(tb);
            lblTrackGen = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 9), Size = new Size(1010, 22), BackColor = Color.Transparent };
            tb.Controls.Add(lblTrackGen);
            y += 48;

            // Step 1 - What are you sending
            SH("Step 1 -- What are you sending?", y); y += 36;
            pnlMailCard = MakeTypeCard("Mail", "Send Mail", "Letters, documents, postcards. Max 2kg. From GBP 1.99", 10, y, 590, () => SelectType(true));
            pnlPkgCard = MakeTypeCard("Package", "Send a Package", "Parcels, gifts, goods. Max 30kg. From GBP 2.99", 620, y, 630, () => SelectType(false));
            pnlSend.Controls.AddRange(new Control[] { pnlMailCard, pnlPkgCard });
            y += 118;

            // Step 2 - Choose a service
            SH("Step 2 -- Choose a Service (select one)", y); y += 36;

            pnlMailSvc = new Panel { Location = new Point(10, y), Size = new Size(1240, 90), BackColor = Color.Transparent, Visible = false };
            pnlSend.Controls.Add(pnlMailSvc);
            pnlPkgSvc = new Panel { Location = new Point(10, y), Size = new Size(1240, 90), BackColor = Color.Transparent, Visible = true };
            pnlSend.Controls.Add(pnlPkgSvc);

            // Mail service cards
            pnlMailFirst = SvcCard(pnlMailSvc, "First Class", "Up to 100g, standard", "GBP 1.99", "3-5 days", 0, () => { mailSvcIndex = 0; RefreshMailSvc(); Recalc(); });
            pnlMailPriority = SvcCard(pnlMailSvc, "Priority", "Up to 2kg, tracked", "GBP 3.99", "1-2 days", 310, () => { mailSvcIndex = 1; RefreshMailSvc(); Recalc(); });
            pnlMailExpress = SvcCard(pnlMailSvc, "Express", "Up to 2kg, guaranteed", "GBP 6.99", "Next day", 620, () => { mailSvcIndex = 2; RefreshMailSvc(); Recalc(); });

            // Package service cards
            pnlPkgGround = SvcCard(pnlPkgSvc, "Ground", "Up to 30kg, tracked", "GBP 2.99+", "3-5 days", 0, () => { pkgSvcIndex = 0; RefreshPkgSvc(); Recalc(); });
            pnlPkgPriority = SvcCard(pnlPkgSvc, "Priority", "Faster delivery, tracked", "GBP 5.99+", "1-2 days", 310, () => { pkgSvcIndex = 1; RefreshPkgSvc(); Recalc(); });
            pnlPkgExpress = SvcCard(pnlPkgSvc, "Express", "Guaranteed, tracked", "GBP 9.99+", "Next day", 620, () => { pkgSvcIndex = 2; RefreshPkgSvc(); Recalc(); });
            pnlPkgNextDay = SvcCard(pnlPkgSvc, "Next Day", "By 1pm next working day", "GBP 14.99+", "By 1pm", 930, () => { pkgSvcIndex = 3; RefreshPkgSvc(); Recalc(); });

            mailSvcIndex = 0; pkgSvcIndex = 0;
            RefreshMailSvc(); RefreshPkgSvc();
            y += 100;

            // Step 3 - Where is it going
            SH("Step 3 -- Where is it going?", y); y += 36;
            rbDomestic = new RadioButton { Text = "UK Domestic", Font = new Font("Segoe UI", 11), Location = new Point(10, y), Size = new Size(220, 28), Checked = true };
            rbInternational = new RadioButton { Text = "International", Font = new Font("Segoe UI", 11), Location = new Point(240, y), Size = new Size(200, 28) };
            pnlSend.Controls.AddRange(new Control[] { rbDomestic, rbInternational });
            y += 36;

            cboCountry = new ComboBox { Location = new Point(10, y), Size = new Size(360, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            foreach (var k in countries.Keys) cboCountry.Items.Add(k);
            cboCountry.SelectedIndex = 0;
            cboCountry.SelectedIndexChanged += (s, e) => Recalc();
            pnlSend.Controls.Add(cboCountry);

            pnlIntlSection = new Panel { Location = new Point(10, y + 38), Size = new Size(1240, 50), BackColor = Color.FromArgb(242, 255, 242), Visible = false };
            pnlSend.Controls.Add(pnlIntlSection);
            pnlIntlSection.Controls.Add(new Label
            {
                Text = "Zone 1 Europe x1.8  |  Zone 2 N.Europe x2.2  |  Zone 3 USA/UAE x3.0  |  Zone 4 World x3.5-3.8",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(20, 80, 40),
                Location = new Point(12, 4),
                Size = new Size(1210, 42),
                BackColor = Color.Transparent
            });

            rbDomestic.CheckedChanged += (s, e) => { cboCountry.Visible = rbInternational.Checked; pnlIntlSection.Visible = rbInternational.Checked; UpdateDropOff(); Recalc(); };
            rbInternational.CheckedChanged += (s, e) => { cboCountry.Visible = rbInternational.Checked; pnlIntlSection.Visible = rbInternational.Checked; UpdateDropOff(); Recalc(); };
            y += 96;

            // Step 4 - Your email (for notifications)
            SH("Step 4 -- Your Contact Email", y); y += 36;
            FL("YOUR EMAIL ADDRESS (for delivery notifications)", 10, y); y += 16;
            txtSenderEmail = FT("your@gmail.com", 10, y, 560);
            txtSenderEmail.TextChanged += (s, e) => ValidateSenderEmail();
            y += 50;

            // Step 5 - Receiver details
            SH("Step 5 -- Receiver Details", y); y += 36;
            FL("RECEIVER FULL NAME", 10, y); FL("RECEIVER ADDRESS (include postcode)", 450, y); y += 16;
            txtRcvName = FT("Full name of recipient", 10, y, 425);
            txtRcvAddr = FT("Full delivery address + postcode", 450, y, 800);
            y += 50;

            // Step 6 - Size and weight
            SH("Step 6 -- Size and Weight", y); y += 36;
            FL("WEIGHT (KG)", 10, y); FL("SIZE", 270, y); y += 16;
            txtWeight = FT("e.g. 1.5", 10, y, 245);
            txtWeight.TextChanged += (s, e) => Recalc();
            cboSize = new ComboBox
            {
                Location = new Point(270, y),
                Size = new Size(270, 34),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboSize.Items.AddRange(new string[] { "Small (up to 1kg)", "Medium (1-5kg)", "Large (5-15kg)", "Extra Large (15kg+)" });
            cboSize.SelectedIndex = 0;
            cboSize.SelectedIndexChanged += (s, e) => Recalc();
            pnlSend.Controls.Add(cboSize);
            y += 50;

            // Drop-off location section
            SH("Step 7 -- Drop Off Your Parcel", y); y += 36;
            pnlDropOff = new Panel { Location = new Point(10, y), Size = new Size(1240, 180), BackColor = Color.FromArgb(240, 248, 255) };
            pnlSend.Controls.Add(pnlDropOff);
            pnlDropOff.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 180), BackColor = Red });
            pnlDropOff.Controls.Add(new Label
            {
                Text = "Nearest PostalMS Drop-Off Locations",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(16, 10),
                Size = new Size(900, 22),
                BackColor = Color.Transparent
            });
            lblDropOff = new Label
            {
                Text = GetDropOffText(),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(30, 30, 60),
                Location = new Point(16, 40),
                Size = new Size(1210, 130),
                BackColor = Color.Transparent
            };
            pnlDropOff.Controls.Add(lblDropOff);
            y += 190;

            // Charges info
            Panel chg = new Panel { Location = new Point(10, y), Size = new Size(1240, 36), BackColor = Color.FromArgb(255, 248, 220) };
            chg.Controls.Add(new Label
            {
                Text = "Extra charges: Tracked Mail +0.50  |  Service surcharges shown above  |  Insurance over 100: +2.50  |  International customs: varies",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(110, 70, 0),
                Location = new Point(12, 10),
                Size = new Size(1200, 18),
                BackColor = Color.Transparent
            });
            pnlSend.Controls.Add(chg);
            y += 44;

            // Price estimate panel
            Panel priceBox = new Panel { Location = new Point(10, y), Size = new Size(1240, 64), BackColor = Color.FromArgb(255, 235, 235) };
            priceBox.Controls.Add(new Label { Text = "ESTIMATED PRICE AND DELIVERY", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(12, 5), Size = new Size(500, 15), BackColor = Color.Transparent });
            lblPrice = new Label { Text = "GBP 2.99", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 20), Size = new Size(220, 38), BackColor = Color.Transparent };
            lblEstDays = new Label { Text = "3-5 working days", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(240, 30), Size = new Size(500, 24), BackColor = Color.Transparent };
            priceBox.Controls.AddRange(new Control[] { lblPrice, lblEstDays });
            pnlSend.Controls.Add(priceBox);
            y += 72;

            // Prohibited items notice
            Panel proh = new Panel { Location = new Point(10, y), Size = new Size(1240, 32), BackColor = Color.FromArgb(255, 240, 240) };
            proh.Controls.Add(new Label
            {
                Text = "Prohibited: Weapons, illegal drugs, hazardous materials, live animals, counterfeit goods, cash",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 30, 30),
                Location = new Point(12, 8),
                Size = new Size(1200, 18),
                BackColor = Color.Transparent
            });
            pnlSend.Controls.Add(proh);
            y += 40;

            // Submit button
            Button bSub = new Button
            {
                Text = "Submit and Send",
                Location = new Point(10, y),
                Size = new Size(210, 48),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            bSub.FlatAppearance.BorderSize = 0;
            bSub.MouseEnter += (s, e) => bSub.BackColor = DarkRed;
            bSub.MouseLeave += (s, e) => bSub.BackColor = Red;
            bSub.Click += Submit_Click;

            Button bClr = new Button
            {
                Text = "Clear Form",
                Location = new Point(228, y),
                Size = new Size(120, 48),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            bClr.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            bClr.Click += (s, e) => ClearForm();
            pnlSend.Controls.AddRange(new Control[] { bSub, bClr });

            SelectType(false); RefreshTrackID(); Recalc();
        }

        // Get drop off text based on domestic or international
        private string GetDropOffText()
        {
            if (rbInternational != null && rbInternational.Checked)
            {
                return "For international parcels please visit any PostalMS Hub below.\n" +
                       "Customs forms will be provided at the counter.\n\n" +
                       "PostalMS International Hub - 1 Station Road, London N1 9AA  (Mon-Fri 8am-6pm)\n" +
                       "PostalMS International Hub - 45 High Street, London EC1A 1AA  (Mon-Fri 9am-5pm)";
            }

            return "Drop your parcel at any of these locations -- no appointment needed:\n\n" +
                   "1. PostalMS Hub - 1 Station Road, London N1 9AA  (Mon-Sat 8am-8pm)\n" +
                   "2. PostalMS Express - 45 High Street, London EC1A 1AA  (Mon-Fri 9am-6pm)\n" +
                   "3. PostalMS North - 12 Market Square, London N7 6JN  (Mon-Sat 8am-7pm)\n" +
                   "4. PostalMS South - 88 Brixton Road, London SW9 8PQ  (Mon-Sat 9am-8pm)\n" +
                   "5. PostalMS West - 22 Shepherd's Bush Road, London W6 7PH  (Mon-Fri 8am-6pm)";
        }

        // Update drop off text when domestic/international changes
        private void UpdateDropOff()
        {
            if (lblDropOff != null)
                lblDropOff.Text = GetDropOffText();
        }

        // Validate sender email must be gmail
        private void ValidateSenderEmail()
        {
            if (txtSenderEmail == null) return;
            string email = txtSenderEmail.Text.Trim();
            if (string.IsNullOrEmpty(email) || txtSenderEmail.ForeColor == Color.LightGray) return;
            bool valid = email.EndsWith("@gmail.com") && email.Length > "@gmail.com".Length;
            txtSenderEmail.BackColor = valid ? Color.FromArgb(240, 255, 240) : Color.FromArgb(255, 240, 240);
        }

        // Make a mail or package type card
        private Panel MakeTypeCard(string tag, string title, string desc, int x, int y, int w, Action onClick)
        {
            Panel c = new Panel { Location = new Point(x, y), Size = new Size(w, 108), BackColor = Color.White, Cursor = Cursors.Hand };
            c.Controls.Add(new Label { Text = "[" + tag + "]", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 10), Size = new Size(80, 26), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(96, 10), Size = new Size(w - 106, 24), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(96, 36), Size = new Size(w - 106, 38), BackColor = Color.Transparent });
            foreach (Control ch in c.Controls) ch.Click += (s, e) => onClick();
            c.Click += (s, e) => onClick();
            return c;
        }

        // Make a service selection card
        private Panel SvcCard(Panel parent, string title, string desc, string price, string time, int x, Action onSelect)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(295, 84), BackColor = Color.White, Cursor = Cursors.Hand };
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 6), Size = new Size(275, 20), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(12, 28), Size = new Size(275, 30), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = price, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 58), Size = new Size(130, 22), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = time, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(155, 60), Size = new Size(130, 18), BackColor = Color.Transparent });
            foreach (Control ch in c.Controls) ch.Click += (s, e) => onSelect();
            c.Click += (s, e) => onSelect();
            parent.Controls.Add(c);
            return c;
        }

        // Highlight selected mail service card
        private void RefreshMailSvc()
        {
            Panel[] cards = { pnlMailFirst, pnlMailPriority, pnlMailExpress };
            for (int i = 0; i < cards.Length; i++)
            {
                int idx = i;
                cards[i].BackColor = mailSvcIndex == i ? Color.FromArgb(255, 245, 245) : Color.White;
                cards[i].Paint -= SvcPaint;
                if (mailSvcIndex == i) cards[i].Paint += SvcPaint;
                cards[i].Invalidate();
            }
        }

        // Highlight selected package service card
        private void RefreshPkgSvc()
        {
            Panel[] cards = { pnlPkgGround, pnlPkgPriority, pnlPkgExpress, pnlPkgNextDay };
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].BackColor = pkgSvcIndex == i ? Color.FromArgb(255, 245, 245) : Color.White;
                cards[i].Paint -= SvcPaint;
                if (pkgSvcIndex == i) cards[i].Paint += SvcPaint;
                cards[i].Invalidate();
            }
        }

        // Draw red border on selected service card
        private void SvcPaint(object s, PaintEventArgs e)
        {
            var p = s as Panel;
            if (p != null) e.Graphics.DrawRectangle(new Pen(Red, 3), 1, 1, p.Width - 3, p.Height - 3);
        }

        // Switch between Mail and Package type
        private void SelectType(bool mail)
        {
            isMail = mail;
            pnlMailCard.Paint -= MailCardPaint; pnlPkgCard.Paint -= PkgCardPaint;
            pnlMailCard.Paint += MailCardPaint; pnlPkgCard.Paint += PkgCardPaint;
            pnlMailCard.Invalidate(); pnlPkgCard.Invalidate();
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

        // Recalculate price based on current selections
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
                if (lblPrice != null) lblPrice.Text = "GBP " + price.ToString("0.00");
                if (lblEstDays != null) lblEstDays.Text = (rbInternational?.Checked == true ? "International -- " : "UK Domestic -- ") + days + " working day(s)";
            }
            catch { }
        }

        // Submit a new parcel
        private void Submit_Click(object sender, EventArgs e)
        {
            // Validate sender email
            string senderEmail = txtSenderEmail?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(senderEmail) || txtSenderEmail.ForeColor == Color.LightGray || !senderEmail.EndsWith("@gmail.com"))
            {
                MessageBox.Show("Please enter a valid Gmail address (e.g. yourname@gmail.com) for delivery notifications.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate receiver fields
            if (txtRcvName.ForeColor == Color.LightGray || string.IsNullOrWhiteSpace(txtRcvName.Text) ||
                txtRcvAddr.ForeColor == Color.LightGray || string.IsNullOrWhiteSpace(txtRcvAddr.Text))
            {
                MessageBox.Show("Please fill in receiver name and address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isIntl = rbInternational.Checked;
            string ptype = isMail ? "Mail" : "Package";
            string svcName = GetServiceName();
            int cnt = db.GetParcelCount();
            string tid = "PS-" + DateTime.Now.Year + "-" + (cnt + 1).ToString("D5");
            double w = double.TryParse(txtWeight.Text, out double wv) ? wv : 0;
            double price = double.TryParse(lblPrice.Text.Replace("GBP ", ""), out double pv) ? pv : 0;
            string country = isIntl && cboCountry.SelectedItem != null ? cboCountry.SelectedItem.ToString() : null;
            string sz = cboSize.SelectedItem.ToString().Split('(')[0].Trim().Split(' ')[0];

            string res = db.AddParcel(tid, userID, ptype, userName, "My Address",
                txtRcvName.Text.Trim(), txtRcvAddr.Text.Trim(),
                w, sz, svcName, isIntl, country, "Pending", price, DateTime.Now.AddDays(5));

            if (res.Contains("successfully"))
            {
                db.AutoAssignDelivery(tid, "DEL-" + DateTime.Now.Ticks.ToString().Substring(0, 8));

                // Get nearest drop off location
                string dropOff = dropOffLocations[0];

                MessageBox.Show(
                    "Parcel submitted successfully!\n\n" +
                    "Tracking ID:    " + tid + "\n" +
                    "Type:           " + ptype + "\n" +
                    "Service:        " + svcName + "\n" +
                    "Destination:    " + (isIntl ? country + " (International)" : "UK Domestic") + "\n" +
                    "Price:          " + lblPrice.Text + "\n\n" +
                    "Confirmation sent to: " + senderEmail + "\n\n" +
                    "Drop off your parcel at:\n" + dropOff,
                    "Submitted!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearForm(); Switch("list");
            }
            else
            {
                MessageBox.Show(res, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear all send form fields
        private void ClearForm()
        {
            txtRcvName.Text = "Full name of recipient"; txtRcvName.ForeColor = Color.LightGray;
            txtRcvAddr.Text = "Full delivery address + postcode"; txtRcvAddr.ForeColor = Color.LightGray;
            txtWeight.Text = "e.g. 1.5"; txtWeight.ForeColor = Color.LightGray;
            txtSenderEmail.Text = "your@gmail.com"; txtSenderEmail.ForeColor = Color.LightGray; txtSenderEmail.BackColor = Color.White;
            cboSize.SelectedIndex = 0; rbDomestic.Checked = true;
            mailSvcIndex = 0; pkgSvcIndex = 0;
            RefreshMailSvc(); RefreshPkgSvc();
            SelectType(false); RefreshTrackID(); Recalc();
        }

        // Helper: section heading
        private void SH(string t, int y)
        {
            pnlSend.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Red, Location = new Point(10, y), Size = new Size(900, 26) });
            pnlSend.Controls.Add(new Panel { Location = new Point(10, y + 28), Size = new Size(1240, 1), BackColor = Color.FromArgb(230, 180, 180) });
        }

        // Helper: field label
        private void FL(string t, int x, int y) =>
            pnlSend.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(x, y), Size = new Size(430, 15) });

        // Helper: text box with placeholder
        private TextBox FT(string ph, int x, int y, int w)
        {
            TextBox tb = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 34),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Text = ph,
                ForeColor = Color.LightGray
            };
            tb.Enter += (s, e) => { if (tb.ForeColor == Color.LightGray) { tb.Text = ""; tb.ForeColor = Color.Black; } };
            tb.Leave += (s, e) => { if (tb.Text == "") { tb.Text = ph; tb.ForeColor = Color.LightGray; } };
            pnlSend.Controls.Add(tb);
            return tb;
        }

        // Colour code status column in the grid
        private void StatusFmt(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var g = sender as DataGridView;
            if (g == null || e.ColumnIndex < 0 || e.ColumnIndex >= g.Columns.Count || g.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return;
            switch (e.Value.ToString())
            {
                case "In Transit": e.CellStyle.ForeColor = Color.FromArgb(140, 80, 0); e.CellStyle.BackColor = Color.FromArgb(255, 243, 220); break;
                case "Delivered": e.CellStyle.ForeColor = Color.FromArgb(20, 110, 50); e.CellStyle.BackColor = Color.FromArgb(210, 248, 225); break;
                case "Pending": e.CellStyle.ForeColor = Color.FromArgb(50, 80, 160); e.CellStyle.BackColor = Color.FromArgb(220, 232, 255); break;
                case "Failed": e.CellStyle.ForeColor = Color.FromArgb(160, 30, 30); e.CellStyle.BackColor = Color.FromArgb(255, 220, 220); break;
                case "Out for Delivery": e.CellStyle.ForeColor = Color.FromArgb(80, 45, 160); e.CellStyle.BackColor = Color.FromArgb(235, 225, 255); break;
            }
        }
    }
}
