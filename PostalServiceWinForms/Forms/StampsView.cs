// StampsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Stamps page - completely separate from parcels.
// Order stamps like ordering food - choose Delivery or Collect,
// fill in details, choose how to pay (online or in store).
// Missed pickup logic with auto reschedule after 48 hours.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PostalServiceWinForms.Forms
{
    public class StampsView : UserControl
    {
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);
        private Color Dark = Color.FromArgb(30, 30, 30);
        private Color Grey = Color.FromArgb(90, 90, 90);
        private Color LightBg = Color.FromArgb(245, 245, 245);
        private Color Green = Color.FromArgb(20, 130, 65);
        private Color Orange = Color.FromArgb(180, 100, 0);

        // Tab panels and buttons
        private Panel pnlBrowse, pnlOrder, pnlMyOrders;
        private Button btnBrowse, btnOrder, btnMyOrders;

        // Delivery vs Collect selection cards
        private Panel pnlDeliveryCard, pnlCollectCard;
        private bool isDelivery = true;

        // Delivery details section
        private Panel pnlDeliveryDetails, pnlCollectDetails;
        private TextBox txtDeliveryName, txtDeliveryAddr, txtDeliveryCity, txtDeliveryPost, txtDeliveryEmail;

        // Collect details section
        private ComboBox cboPickupLocation, cboPickupTime;
        private DateTimePicker dtPickup;
        private TextBox txtCollectEmail;

        // Stamp selection
        private ComboBox cboStampType, cboQty;

        // Delivery speed (only for delivery)
        private ComboBox cboDeliverySpeed;

        // Payment
        private Panel pnlPayOnline, pnlPayStore;
        private bool payOnline = true;

        // Price labels
        private Label lblBreakdown, lblTotal;

        // Order history
        private List<StampOrder> stampOrders = new List<StampOrder>();

        private class StampOrder
        {
            public string Id, Type, Method, Status, Date, Payment, Location, TimeSlot, Email, Address;
            public int Qty;
            public double Total;
            public bool Missed;
            public string RescheduleDate;
        }

        private Dictionary<string, double> stampPrices = new Dictionary<string, double>
        {
            { "First Class (1ST)",        1.10 },
            { "Second Class (2ND)",       0.75 },
            { "Large Letter (LRG)",       1.55 },
            { "International (INTL)",     1.85 },
            { "Special Delivery (SPEC)",  6.85 },
            { "Signed For (SIGN)",        1.95 },
        };

        private string[] pickupLocations =
        {
            "PostalMS Hendon Centre -- The Burroughs, Hendon NW4 4BT",
            "PostalMS Cat Hill Centre -- Cat Hill, East Barnet EN4 8HT",
            "PostalMS Archway Point -- 2 Junction Road, Archway N19 5QU",
            "PostalMS Wembley Centre -- Engineers Way, Wembley HA9 0ED",
            "WH Smith Brent Cross -- Brent Cross Shopping Centre NW4 3FP",
            "Tesco Golders Green -- 186 Golders Green Road NW11 9AA",
        };

        private string[] pickupTimes =
        {
            "09:00 - 10:00", "10:00 - 11:00", "11:00 - 12:00",
            "12:00 - 13:00", "13:00 - 14:00", "14:00 - 15:00",
            "15:00 - 16:00", "16:00 - 17:00", "17:00 - 17:30",
        };

        public StampsView()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = LightBg;
            this.AutoScroll = false;
            Build();
        }

        private void Build()
        {
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.White };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(218, 218, 218)), 0, 109, topBar.Width, 109);
            this.Controls.Add(topBar);

            topBar.Controls.Add(new Label { Text = "Stamps", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 10), Size = new Size(200, 34) });
            topBar.Controls.Add(new Label { Text = "Browse and order stamps for delivery or collection -- like ordering food, your choice.", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, 46), Size = new Size(900, 20) });

            btnBrowse = Tab(topBar, "Browse Stamps", 10, 70, () => SwTab("browse"));
            btnOrder = Tab(topBar, "Order Stamps", 180, 70, () => SwTab("order"));
            btnMyOrders = Tab(topBar, "My Stamp Orders", 350, 70, () => SwTab("orders"));
            SetTab(btnBrowse);

            pnlBrowse = new Panel { Dock = DockStyle.Fill, BackColor = LightBg, AutoScroll = true, Visible = true };
            this.Controls.Add(pnlBrowse);
            BuildBrowse();

            pnlOrder = new Panel { Dock = DockStyle.Fill, BackColor = LightBg, AutoScroll = true, Visible = false };
            this.Controls.Add(pnlOrder);
            BuildOrder();

            pnlMyOrders = new Panel { Dock = DockStyle.Fill, BackColor = LightBg, AutoScroll = true, Visible = false };
            this.Controls.Add(pnlMyOrders);
            BuildMyOrders();
        }

        // ============================================================
        // BROWSE TAB
        // ============================================================
        private void BuildBrowse()
        {
            int y = 16;
            SH(pnlBrowse, "Available Stamp Types", y); y += 40;

            var stamps = new (string sym, string name, string price, string desc, string weight, string time)[]
            {
                ("[1ST]",  "First Class",      "GBP 1.10", "Fastest UK domestic delivery",      "Up to 100g",  "Next working day"),
                ("[2ND]",  "Second Class",     "GBP 0.75", "Standard UK domestic delivery",     "Up to 100g",  "2-3 working days"),
                ("[LRG]",  "Large Letter",     "GBP 1.55", "For larger envelopes and packets",  "Up to 250g",  "2-3 working days"),
                ("[INTL]", "International",    "GBP 1.85", "Send letters to Europe and beyond", "Up to 100g",  "3-7 working days"),
                ("[SPEC]", "Special Delivery", "GBP 6.85", "Tracked, guaranteed, insured",      "Up to 500g",  "Next day by 1pm"),
                ("[SIGN]", "Signed For",       "GBP 1.95", "Requires signature on delivery",    "Up to 100g",  "2-3 working days"),
            };

            int sx = 10; int sy = y;
            foreach (var s in stamps)
            {
                Panel card = new Panel { Location = new Point(sx, sy), Size = new Size(195, 220), BackColor = Color.White };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(195, 5), BackColor = Red });
                Panel badge = new Panel { Location = new Point(10, 14), Size = new Size(175, 52), BackColor = Red };
                badge.Controls.Add(new Label { Text = s.sym, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Location = new Point(0, 10), Size = new Size(175, 32), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
                card.Controls.Add(badge);
                card.Controls.Add(new Label { Text = s.name, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 74), Size = new Size(175, 20) });
                card.Controls.Add(new Label { Text = s.price, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Dark, Location = new Point(10, 96), Size = new Size(175, 28) });
                card.Controls.Add(new Panel { Location = new Point(10, 128), Size = new Size(175, 1), BackColor = Color.FromArgb(220, 220, 220) });
                card.Controls.Add(new Label { Text = s.desc, Font = new Font("Segoe UI", 8), ForeColor = Grey, Location = new Point(10, 134), Size = new Size(175, 32) });
                card.Controls.Add(new Label { Text = s.weight, Font = new Font("Segoe UI", 8), ForeColor = Grey, Location = new Point(10, 168), Size = new Size(175, 18) });
                card.Controls.Add(new Label { Text = s.time, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Green, Location = new Point(10, 186), Size = new Size(175, 18) });
                pnlBrowse.Controls.Add(card);
                sx += 205;
                if (sx > 1200) { sx = 10; sy += 230; }
            }
            y = sy + 230;

            SH(pnlBrowse, "Stamp Books and Sheets", y); y += 40;
            var books = new (string sym, string name, string price, string each, string saving)[]
            {
                ("[x12]", "12 First Class Stamps",  "GBP 12.50", "GBP 0.92 each", "Save GBP 0.70 vs single stamps"),
                ("[x12]", "12 Second Class Stamps", "GBP 8.75",  "GBP 0.73 each", "Save GBP 0.25 vs single stamps"),
                ("[x25]", "25 Mixed Stamp Sheet",   "GBP 22.00", "GBP 0.88 each", "Mixed first and second class"),
                ("[x50]", "50 First Class Bundle",  "GBP 48.00", "GBP 0.96 each", "Best value for high volume senders"),
            };
            int bx = 10;
            foreach (var b in books)
            {
                Panel bc = new Panel { Location = new Point(bx, y), Size = new Size(290, 130), BackColor = Color.White };
                bc.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(290, 5), BackColor = Red });
                Panel bb = new Panel { Location = new Point(10, 14), Size = new Size(60, 60), BackColor = Red };
                bb.Controls.Add(new Label { Text = b.sym, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White, Location = new Point(0, 18), Size = new Size(60, 24), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
                bc.Controls.Add(bb);
                bc.Controls.Add(new Label { Text = b.name, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(80, 14), Size = new Size(200, 20) });
                bc.Controls.Add(new Label { Text = b.price, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Dark, Location = new Point(80, 36), Size = new Size(200, 26) });
                bc.Controls.Add(new Label { Text = b.each, Font = new Font("Segoe UI", 8), ForeColor = Grey, Location = new Point(80, 64), Size = new Size(200, 18) });
                bc.Controls.Add(new Label { Text = b.saving, Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Green, Location = new Point(80, 84), Size = new Size(200, 18) });
                pnlBrowse.Controls.Add(bc);
                bx += 305;
            }
            y += 145;

            Button goOrder = new Button { Text = "Order Stamps Now ->", Location = new Point(10, y), Size = new Size(260, 50), Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            goOrder.FlatAppearance.BorderSize = 0;
            goOrder.Click += (s, e) => SwTab("order");
            pnlBrowse.Controls.Add(goOrder);
        }

        // ============================================================
        // ORDER TAB
        // ============================================================
        private void BuildOrder()
        {
            int y = 16;

            // ---- STEP 1: Choose stamps ----
            SH2(pnlOrder, "Step 1 -- What stamps do you want?", y); y += 36;
            pnlOrder.Controls.Add(new Label { Text = "STAMP TYPE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, y), Size = new Size(300, 16) });
            pnlOrder.Controls.Add(new Label { Text = "QUANTITY", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(480, y), Size = new Size(200, 16) });
            y += 18;

            cboStampType = new ComboBox { Location = new Point(10, y), Size = new Size(460, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var k in stampPrices.Keys) cboStampType.Items.Add(k);
            cboStampType.SelectedIndex = 0;
            cboStampType.SelectedIndexChanged += (s, e) => RecalcOrder();
            pnlOrder.Controls.Add(cboStampType);

            cboQty = new ComboBox { Location = new Point(480, y), Size = new Size(200, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (int q in new[] { 1, 2, 5, 10, 12, 20, 25, 50, 100 }) cboQty.Items.Add(q);
            cboQty.SelectedIndex = 0;
            cboQty.SelectedIndexChanged += (s, e) => RecalcOrder();
            pnlOrder.Controls.Add(cboQty);
            y += 60;

            // ---- STEP 2: Delivery or Collect (like a food app) ----
            SH2(pnlOrder, "Step 2 -- Delivery or Collection?", y); y += 36;
            pnlOrder.Controls.Add(new Label { Text = "Choose how you want to receive your stamps:", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, y), Size = new Size(700, 22) });
            y += 30;

            // Delivery card
            pnlDeliveryCard = new Panel { Location = new Point(10, y), Size = new Size(340, 100), BackColor = Color.White, Cursor = Cursors.Hand };
            pnlDeliveryCard.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(340, 5), BackColor = Red, Name = "bar" });
            pnlDeliveryCard.Controls.Add(new Label { Text = "Deliver to My Address", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 14), Size = new Size(308, 26), BackColor = Color.Transparent });
            pnlDeliveryCard.Controls.Add(new Label { Text = "Stamps delivered straight to your door.", Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(16, 42), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlDeliveryCard.Controls.Add(new Label { Text = "Standard GBP 1.50  |  Express GBP 3.99", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Dark, Location = new Point(16, 62), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlDeliveryCard.Controls.Add(new Label { Text = "2-3 days  |  Next day", Font = new Font("Segoe UI", 9), ForeColor = Green, Location = new Point(16, 80), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlDeliveryCard.Click += (s, e) => SelectMethod(true);
            foreach (Control c in pnlDeliveryCard.Controls) c.Click += (s, e) => SelectMethod(true);
            pnlOrder.Controls.Add(pnlDeliveryCard);

            // Collect card
            pnlCollectCard = new Panel { Location = new Point(370, y), Size = new Size(340, 100), BackColor = Color.White, Cursor = Cursors.Hand };
            pnlCollectCard.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(340, 5), BackColor = Color.FromArgb(200, 180, 180), Name = "bar" });
            pnlCollectCard.Controls.Add(new Label { Text = "Click and Collect -- Free", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 14), Size = new Size(308, 26), BackColor = Color.Transparent });
            pnlCollectCard.Controls.Add(new Label { Text = "Pick up from your nearest PostalMS location.", Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(16, 42), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlCollectCard.Controls.Add(new Label { Text = "FREE -- No delivery charge", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(16, 62), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlCollectCard.Controls.Add(new Label { Text = "Choose your date and time slot", Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(16, 80), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlCollectCard.Click += (s, e) => SelectMethod(false);
            foreach (Control c in pnlCollectCard.Controls) c.Click += (s, e) => SelectMethod(false);
            pnlOrder.Controls.Add(pnlCollectCard);
            y += 120;

            // ---- STEP 3: Your Details ----
            SH2(pnlOrder, "Step 3 -- Your Details", y); y += 36;

            int detailY = y;

            // Delivery details panel
            pnlDeliveryDetails = new Panel { Location = new Point(10, detailY), Size = new Size(1240, 260), BackColor = Color.FromArgb(250, 250, 250), Visible = true };
            pnlOrder.Controls.Add(pnlDeliveryDetails);

            // Delivery speed
            pnlDeliveryDetails.Controls.Add(new Label { Text = "DELIVERY SPEED", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 10), Size = new Size(300, 16) });
            cboDeliverySpeed = new ComboBox { Location = new Point(10, 28), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboDeliverySpeed.Items.Add("Standard Delivery -- GBP 1.50 -- 2-3 working days");
            cboDeliverySpeed.Items.Add("Express Delivery -- GBP 3.99 -- Next working day");
            cboDeliverySpeed.SelectedIndex = 0;
            cboDeliverySpeed.SelectedIndexChanged += (s, e) => RecalcOrder();
            pnlDeliveryDetails.Controls.Add(cboDeliverySpeed);

            // Name and Phone row
            pnlDeliveryDetails.Controls.Add(new Label { Text = "FULL NAME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 72), Size = new Size(300, 16) });
            txtDeliveryName = new TextBox { Location = new Point(10, 90), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlDeliveryDetails.Controls.Add(txtDeliveryName);

            // Address row
            pnlDeliveryDetails.Controls.Add(new Label { Text = "DELIVERY ADDRESS", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 134), Size = new Size(300, 16) });
            txtDeliveryAddr = new TextBox { Location = new Point(10, 152), Size = new Size(900, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlDeliveryDetails.Controls.Add(txtDeliveryAddr);

            // City and Postcode row
            pnlDeliveryDetails.Controls.Add(new Label { Text = "CITY", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 196), Size = new Size(200, 16) });
            pnlDeliveryDetails.Controls.Add(new Label { Text = "POSTCODE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(310, 196), Size = new Size(200, 16) });
            pnlDeliveryDetails.Controls.Add(new Label { Text = "EMAIL (must be @gmail.com)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(580, 196), Size = new Size(300, 16) });
            txtDeliveryCity = new TextBox { Location = new Point(10, 214), Size = new Size(290, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            txtDeliveryPost = new TextBox { Location = new Point(310, 214), Size = new Size(200, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            txtDeliveryEmail = new TextBox { Location = new Point(580, 214), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlDeliveryDetails.Controls.AddRange(new Control[] { txtDeliveryCity, txtDeliveryPost, txtDeliveryEmail });

            // Collect details panel - increased height for extra fields
            pnlCollectDetails = new Panel { Location = new Point(10, detailY), Size = new Size(1240, 420), BackColor = Color.FromArgb(250, 250, 250), Visible = false };
            pnlOrder.Controls.Add(pnlCollectDetails);

            // Row 1 - Full name and phone
            pnlCollectDetails.Controls.Add(new Label { Text = "FULL NAME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 10), Size = new Size(300, 16) });
            pnlCollectDetails.Controls.Add(new Label { Text = "PHONE NUMBER", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(480, 10), Size = new Size(300, 16) });
            var txtCollectName = new TextBox { Location = new Point(10, 28), Size = new Size(460, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Name = "txtCollectName" };
            var txtCollectPhone = new TextBox { Location = new Point(480, 28), Size = new Size(360, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Name = "txtCollectPhone" };
            pnlCollectDetails.Controls.AddRange(new Control[] { txtCollectName, txtCollectPhone });

            // Row 2 - Date of birth
            pnlCollectDetails.Controls.Add(new Label { Text = "DATE OF BIRTH (DD/MM/YYYY)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 72), Size = new Size(300, 16) });
            var txtCollectDOB = new TextBox { Location = new Point(10, 90), Size = new Size(260, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Name = "txtCollectDOB", Text = "DD/MM/YYYY", ForeColor = Color.LightGray };
            txtCollectDOB.Enter += (s, e) => { if (txtCollectDOB.ForeColor == Color.LightGray) { txtCollectDOB.Text = ""; txtCollectDOB.ForeColor = Color.Black; } };
            txtCollectDOB.Leave += (s, e) => { if (txtCollectDOB.Text == "") { txtCollectDOB.Text = "DD/MM/YYYY"; txtCollectDOB.ForeColor = Color.LightGray; } };
            pnlCollectDetails.Controls.Add(txtCollectDOB);

            // Row 3 - Location
            pnlCollectDetails.Controls.Add(new Label { Text = "SELECT COLLECTION LOCATION", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 134), Size = new Size(400, 16) });
            cboPickupLocation = new ComboBox { Location = new Point(10, 152), Size = new Size(900, 34), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (string loc in pickupLocations) cboPickupLocation.Items.Add(loc);
            cboPickupLocation.SelectedIndex = 0;
            pnlCollectDetails.Controls.Add(cboPickupLocation);

            // Row 4 - Date and time
            pnlCollectDetails.Controls.Add(new Label { Text = "COLLECTION DATE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 196), Size = new Size(200, 16) });
            dtPickup = new DateTimePicker { Location = new Point(10, 214), Size = new Size(280, 34), Font = new Font("Segoe UI", 11), MinDate = DateTime.Today.AddDays(1), MaxDate = DateTime.Today.AddDays(14) };
            pnlCollectDetails.Controls.Add(dtPickup);

            pnlCollectDetails.Controls.Add(new Label { Text = "TIME SLOT", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(310, 196), Size = new Size(200, 16) });
            cboPickupTime = new ComboBox { Location = new Point(310, 214), Size = new Size(260, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (string t in pickupTimes) cboPickupTime.Items.Add(t);
            cboPickupTime.SelectedIndex = 0;
            pnlCollectDetails.Controls.Add(cboPickupTime);

            // Missed pickup warning
            Panel missedWarn = new Panel { Location = new Point(10, 258), Size = new Size(900, 50), BackColor = Color.FromArgb(255, 245, 220) };
            missedWarn.Controls.Add(new Label { Text = "If you miss your collection slot:\n-- Your order is held for 48 hours. After 48 hours it is automatically rescheduled to the next available slot at the same location.", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(140, 80, 0), Location = new Point(10, 6), Size = new Size(880, 38), BackColor = Color.Transparent });
            pnlCollectDetails.Controls.Add(missedWarn);

            // Row 5 - Email
            pnlCollectDetails.Controls.Add(new Label { Text = "YOUR EMAIL (must be @gmail.com)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 318), Size = new Size(400, 16) });
            txtCollectEmail = new TextBox { Location = new Point(10, 336), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlCollectDetails.Controls.Add(txtCollectEmail);

            y = detailY + 430;

            // ---- STEP 4: Payment ----
            SH2(pnlOrder, "Step 4 -- How would you like to pay?", y); y += 36;
            pnlOrder.Controls.Add(new Label { Text = "Choose your payment method:", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, y), Size = new Size(700, 22) });
            y += 30;

            // Pay Online card
            pnlPayOnline = new Panel { Location = new Point(10, y), Size = new Size(340, 90), BackColor = Color.White, Cursor = Cursors.Hand };
            pnlPayOnline.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(340, 5), BackColor = Red, Name = "bar" });
            pnlPayOnline.Controls.Add(new Label { Text = "Pay Now Online", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 14), Size = new Size(308, 26), BackColor = Color.Transparent });
            pnlPayOnline.Controls.Add(new Label { Text = "Pay securely by credit or debit card.", Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(16, 42), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlPayOnline.Controls.Add(new Label { Text = "Card charged immediately on order.", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(16, 62), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlPayOnline.Click += (s, e) => SelectPayment(true);
            foreach (Control c in pnlPayOnline.Controls) c.Click += (s, e) => SelectPayment(true);
            pnlOrder.Controls.Add(pnlPayOnline);

            // Pay In Store card
            pnlPayStore = new Panel { Location = new Point(370, y), Size = new Size(340, 90), BackColor = Color.White, Cursor = Cursors.Hand };
            pnlPayStore.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(340, 5), BackColor = Color.FromArgb(200, 180, 180), Name = "bar" });
            pnlPayStore.Controls.Add(new Label { Text = "Pay In Store", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 14), Size = new Size(308, 26), BackColor = Color.Transparent });
            pnlPayStore.Controls.Add(new Label { Text = "Reserve now, pay when you collect.", Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(16, 42), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlPayStore.Controls.Add(new Label { Text = "Only available with Click and Collect.", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Orange, Location = new Point(16, 62), Size = new Size(308, 18), BackColor = Color.Transparent });
            pnlPayStore.Click += (s, e) => SelectPayment(false);
            foreach (Control c in pnlPayStore.Controls) c.Click += (s, e) => SelectPayment(false);
            pnlOrder.Controls.Add(pnlPayStore);
            y += 110;

            // Pay in store info note
            Panel payStoreNote = new Panel { Location = new Point(10, y), Size = new Size(700, 70), BackColor = Color.FromArgb(235, 245, 255), Visible = false, Name = "pnlPayStoreNote" };
            payStoreNote.Controls.Add(new Label { Text = "Pay In Store Instructions:\n1. Place order now to reserve your stamps at the chosen location\n2. Visit on your selected date and show your Order ID at the counter\n3. Pay by cash or card -- if not paid within 48 hours your reservation is cancelled", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(10, 6), Size = new Size(680, 58), BackColor = Color.Transparent });
            pnlOrder.Controls.Add(payStoreNote);
            y += 80;

            // ---- STEP 5: Price Breakdown ----
            SH2(pnlOrder, "Step 5 -- Price Breakdown", y); y += 36;

            Panel priceBox = new Panel { Location = new Point(10, y), Size = new Size(580, 200), BackColor = Color.White };
            priceBox.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(580, 5), BackColor = Red });
            lblBreakdown = new Label { Location = new Point(14, 14), Size = new Size(552, 140), Font = new Font("Segoe UI", 10), ForeColor = Dark, BackColor = Color.Transparent };
            priceBox.Controls.Add(lblBreakdown);
            priceBox.Controls.Add(new Panel { Location = new Point(14, 158), Size = new Size(552, 1), BackColor = Color.FromArgb(220, 220, 220) });
            lblTotal = new Label { Location = new Point(14, 164), Size = new Size(552, 28), Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, BackColor = Color.Transparent };
            priceBox.Controls.Add(lblTotal);
            pnlOrder.Controls.Add(priceBox);
            y += 210;

            // Place order button
            Button btnPlace = new Button { Text = "Place Order ->", Location = new Point(10, y), Size = new Size(260, 52), Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnPlace.FlatAppearance.BorderSize = 0;
            btnPlace.MouseEnter += (s, e) => btnPlace.BackColor = DarkRed;
            btnPlace.MouseLeave += (s, e) => btnPlace.BackColor = Red;
            btnPlace.Click += PlaceOrder_Click;
            pnlOrder.Controls.Add(btnPlace);

            SelectMethod(true);
            SelectPayment(true);
            RecalcOrder();
        }

        // Select delivery or collect
        private void SelectMethod(bool delivery)
        {
            isDelivery = delivery;
            pnlDeliveryCard.Controls["bar"].BackColor = delivery ? Red : Color.FromArgb(200, 180, 180);
            pnlCollectCard.Controls["bar"].BackColor = delivery ? Color.FromArgb(200, 180, 180) : Red;
            pnlDeliveryDetails.Visible = delivery;
            pnlCollectDetails.Visible = !delivery;

            // Pay in store only works with collect
            if (delivery && !payOnline)
                SelectPayment(true);

            RecalcOrder();
        }

        // Select payment method
        private void SelectPayment(bool online)
        {
            // Pay in store only allowed with collect
            if (!online && isDelivery)
            {
                MessageBox.Show("Pay In Store is only available with Click and Collect.\nPlease select Click and Collect above first.", "Payment Option", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            payOnline = online;
            pnlPayOnline.Controls["bar"].BackColor = online ? Red : Color.FromArgb(200, 180, 180);
            pnlPayStore.Controls["bar"].BackColor = online ? Color.FromArgb(200, 180, 180) : Red;

            // Show pay in store note
            foreach (Control c in pnlOrder.Controls)
                if (c.Name == "pnlPayStoreNote") c.Visible = !online;

            RecalcOrder();
        }

        // Place order
        private void PlaceOrder_Click(object sender, EventArgs e)
        {
            string email = isDelivery ? txtDeliveryEmail?.Text.Trim() : txtCollectEmail?.Text.Trim();
            string payment = payOnline ? "Pay Now Online" : "Pay In Store";

            // If paying online show bank details form first
            if (payOnline)
            {
                bool paid = ShowBankDetailsForm();
                if (!paid) return; // User cancelled payment
            }

            // Validate Gmail
            if (string.IsNullOrEmpty(email) || !email.EndsWith("@gmail.com") || email.Length <= "@gmail.com".Length)
            { MessageBox.Show("Please enter a valid Gmail address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (isDelivery)
            {
                if (string.IsNullOrWhiteSpace(txtDeliveryName?.Text))
                { MessageBox.Show("Please enter your full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtDeliveryAddr?.Text))
                { MessageBox.Show("Please enter your delivery address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            }

            string orderId = "STO-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string type = cboStampType.SelectedItem.ToString();
            int qty = Convert.ToInt32(cboQty.SelectedItem);
            string method = isDelivery ? cboDeliverySpeed.SelectedItem.ToString() : "Click and Collect";
            string location = isDelivery ? "--" : cboPickupLocation.SelectedItem?.ToString() ?? "--";
            string timeSlot = isDelivery ? "--" : cboPickupTime.SelectedItem?.ToString() ?? "--";
            string address = isDelivery ? (txtDeliveryAddr?.Text + ", " + txtDeliveryCity?.Text + " " + txtDeliveryPost?.Text) : location;
            double total = CalcTotal();
            string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            stampOrders.Add(new StampOrder
            {
                Id = orderId,
                Type = type,
                Qty = qty,
                Method = method,
                Status = payOnline ? "Processing" : "Reserved -- Pay In Store",
                Date = date,
                Payment = payment,
                Location = location,
                TimeSlot = timeSlot,
                Email = email,
                Address = address,
                Total = total,
                Missed = false,
                RescheduleDate = ""
            });

            string msg = "Order placed!\n\nOrder ID: " + orderId +
                         "\nStamps: " + qty + "x " + type +
                         "\nMethod: " + (isDelivery ? "Delivery" : "Click and Collect") +
                         "\nPayment: " + payment +
                         "\nTotal: GBP " + total.ToString("0.00");

            if (isDelivery)
                msg += "\nDelivery to: " + address;
            else
            {
                msg += "\nLocation: " + location +
                       "\nDate: " + dtPickup.Value.ToString("dd MMM yyyy") +
                       "\nTime: " + timeSlot;
                if (!payOnline)
                    msg += "\n\nBring Order ID to store and pay by cash or card.\nHeld for 48 hours if missed.";
            }

            msg += "\nConfirmation sent to: " + email;
            MessageBox.Show(msg, "Order Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SwTab("orders");
        }

        // Recalculate price breakdown
        private void RecalcOrder()
        {
            try
            {
                string stampType = cboStampType?.SelectedItem?.ToString() ?? "";
                int qty = cboQty?.SelectedItem != null ? Convert.ToInt32(cboQty.SelectedItem) : 1;
                if (!stampPrices.ContainsKey(stampType)) return;

                double unitPrice = stampPrices[stampType];
                double stampTotal = unitPrice * qty;
                double delivery = isDelivery
                    ? (cboDeliverySpeed?.SelectedIndex == 1 ? 3.99 : 1.50)
                    : 0.00;
                double subtotal = stampTotal + delivery;
                double tax = Math.Round(subtotal * 0.20, 2);
                double service = Math.Round(subtotal * 0.02, 2);
                double total = Math.Round(subtotal + tax + service, 2);

                string methodStr = isDelivery
                    ? (cboDeliverySpeed?.SelectedIndex == 1 ? "Express Delivery" : "Standard Delivery")
                    : "Click and Collect (Free)";
                string payStr = payOnline ? "Pay Now Online" : "Pay In Store on Collection";

                if (lblBreakdown != null)
                    lblBreakdown.Text =
                        qty + "x " + stampType + "  --  GBP " + unitPrice.ToString("0.00") + " each\n" +
                        "Stamps subtotal:       GBP " + stampTotal.ToString("0.00") + "\n" +
                        methodStr + ":    GBP " + delivery.ToString("0.00") + "\n" +
                        "VAT (20%):             GBP " + tax.ToString("0.00") + "\n" +
                        "Service fee (2%):      GBP " + service.ToString("0.00") + "\n" +
                        "Payment:               " + payStr;

                if (lblTotal != null)
                    lblTotal.Text = "Total:  GBP " + total.ToString("0.00") +
                                    (payOnline ? "" : "  (pay on collection)");
            }
            catch { }
        }

        // Bank details form shown when user selects Pay Now Online
        private bool ShowBankDetailsForm()
        {
            Form bankForm = new Form
            {
                Text = "PostalMS -- Secure Payment",
                Size = new Size(480, 530),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Color BankRed = Color.FromArgb(180, 30, 30);

            // Header panel
            Panel hdr = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = BankRed };
            hdr.Controls.Add(new Label { Text = "Secure Payment", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(16, 14), Size = new Size(300, 28), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "Your card details are encrypted and secure", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(255, 200, 200), Location = new Point(16, 38), Size = new Size(400, 18), BackColor = Color.Transparent });
            bankForm.Controls.Add(hdr);

            // White card panel
            Panel card = new Panel { Location = new Point(20, 76), Size = new Size(432, 350), BackColor = Color.White };
            bankForm.Controls.Add(card);

            // Cardholder name
            card.Controls.Add(new Label { Text = "CARDHOLDER NAME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 16), Size = new Size(400, 16) });
            var txtCardName = new TextBox { Location = new Point(16, 34), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "Full name as it appears on card", ForeColor = Color.LightGray };
            txtCardName.Enter += (s, e2) => { if (txtCardName.ForeColor == Color.LightGray) { txtCardName.Text = ""; txtCardName.ForeColor = Color.Black; } };
            card.Controls.Add(txtCardName);

            // Card number
            card.Controls.Add(new Label { Text = "CARD NUMBER", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 84), Size = new Size(400, 16) });
            var txtCardNum = new TextBox { Location = new Point(16, 102), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "1234 5678 9012 3456", ForeColor = Color.LightGray };
            txtCardNum.Enter += (s, e2) => { if (txtCardNum.ForeColor == Color.LightGray) { txtCardNum.Text = ""; txtCardNum.ForeColor = Color.Black; } };
            card.Controls.Add(txtCardNum);

            // Expiry date
            card.Controls.Add(new Label { Text = "EXPIRY DATE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 152), Size = new Size(190, 16) });
            var txtExpiry = new TextBox { Location = new Point(16, 170), Size = new Size(190, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "MM/YY", ForeColor = Color.LightGray };
            txtExpiry.Enter += (s, e2) => { if (txtExpiry.ForeColor == Color.LightGray) { txtExpiry.Text = ""; txtExpiry.ForeColor = Color.Black; } };
            card.Controls.Add(txtExpiry);

            // CVV
            card.Controls.Add(new Label { Text = "CVV", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(220, 152), Size = new Size(190, 16) });
            var txtCVV = new TextBox { Location = new Point(220, 170), Size = new Size(190, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "CVV", ForeColor = Color.LightGray, PasswordChar = '*' };
            txtCVV.Enter += (s, e2) => { if (txtCVV.ForeColor == Color.LightGray) { txtCVV.Text = ""; txtCVV.ForeColor = Color.Black; } };
            card.Controls.Add(txtCVV);

            // Billing postcode
            card.Controls.Add(new Label { Text = "BILLING POSTCODE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 220), Size = new Size(300, 16) });
            var txtPostcode = new TextBox { Location = new Point(16, 238), Size = new Size(200, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "e.g. NW4 4BT", ForeColor = Color.LightGray };
            txtPostcode.Enter += (s, e2) => { if (txtPostcode.ForeColor == Color.LightGray) { txtPostcode.Text = ""; txtPostcode.ForeColor = Color.Black; } };
            card.Controls.Add(txtPostcode);

            // Security note
            Panel secNote = new Panel { Location = new Point(16, 288), Size = new Size(400, 36), BackColor = Color.FromArgb(235, 245, 255) };
            secNote.Controls.Add(new Label { Text = "Secure encrypted payment. We do not store your card details.", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(10, 10), Size = new Size(380, 16), BackColor = Color.Transparent });
            card.Controls.Add(secNote);

            // Result flag
            bool paid = false;

            // Pay Now button
            Button btnPay = new Button { Text = "Pay Now  GBP " + CalcTotal().ToString("0.00"), Location = new Point(20, 442), Size = new Size(432, 50), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = BankRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += (s, e2) =>
            {
                string cName = txtCardName.ForeColor == Color.LightGray ? "" : txtCardName.Text.Trim();
                string cNum = txtCardNum.ForeColor == Color.LightGray ? "" : txtCardNum.Text.Trim();
                string cExp = txtExpiry.ForeColor == Color.LightGray ? "" : txtExpiry.Text.Trim();
                string cCVV = txtCVV.ForeColor == Color.LightGray ? "" : txtCVV.Text.Trim();
                string cPost = txtPostcode.ForeColor == Color.LightGray ? "" : txtPostcode.Text.Trim();

                if (string.IsNullOrEmpty(cName)) { MessageBox.Show("Please enter the cardholder name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(cNum)) { MessageBox.Show("Please enter your card number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(cExp)) { MessageBox.Show("Please enter the expiry date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(cCVV)) { MessageBox.Show("Please enter the CVV.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(cPost)) { MessageBox.Show("Please enter your billing postcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                paid = true;
                bankForm.Close();
            };
            bankForm.Controls.Add(btnPay);

            // Cancel button
            Button btnCancel = new Button { Text = "Cancel", Location = new Point(358, 452), Size = new Size(94, 30), Font = new Font("Segoe UI", 9), BackColor = Color.FromArgb(240, 240, 240), ForeColor = BankRed, FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e2) => bankForm.Close();
            bankForm.Controls.Add(btnCancel);

            bankForm.ShowDialog();
            return paid;
        }

        private double CalcTotal()
        {
            try
            {
                string stampType = cboStampType?.SelectedItem?.ToString() ?? "";
                int qty = Convert.ToInt32(cboQty?.SelectedItem ?? 1);
                double unitPrice = stampPrices.ContainsKey(stampType) ? stampPrices[stampType] : 0;
                double stampTotal = unitPrice * qty;
                double delivery = isDelivery ? (cboDeliverySpeed?.SelectedIndex == 1 ? 3.99 : 1.50) : 0.00;
                double subtotal = stampTotal + delivery;
                double tax = Math.Round(subtotal * 0.20, 2);
                double service = Math.Round(subtotal * 0.02, 2);
                return Math.Round(subtotal + tax + service, 2);
            }
            catch { return 0; }
        }

        // ============================================================
        // MY ORDERS TAB
        // ============================================================
        private void BuildMyOrders()
        {
            pnlMyOrders.Controls.Clear();
            pnlMyOrders.Controls.Add(new Label { Text = "My Stamp Orders", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 16), Size = new Size(400, 34) });
            pnlMyOrders.Controls.Add(new Label { Text = "All your stamp orders and their current delivery or collection status.", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, 52), Size = new Size(800, 22) });

            if (stampOrders.Count == 0)
            {
                Panel empty = new Panel { Location = new Point(10, 90), Size = new Size(700, 70), BackColor = Color.White };
                empty.Controls.Add(new Label { Text = "No stamp orders yet. Go to Order Stamps to place your first order.", Font = new Font("Segoe UI", 11), ForeColor = Grey, Location = new Point(20, 22), Size = new Size(660, 26) });
                pnlMyOrders.Controls.Add(empty);
                return;
            }

            int y = 90;
            foreach (var order in stampOrders)
            {
                // Missed pickup logic
                if (!order.Missed && order.Method == "Click and Collect" && order.Status == "Reserved -- Pay In Store")
                {
                    if (DateTime.TryParse(order.Date, out DateTime orderDt) && orderDt.AddDays(2) < DateTime.Now)
                    {
                        order.Missed = true;
                        order.Status = "Missed -- Rescheduled";
                        order.RescheduleDate = DateTime.Now.AddDays(2).ToString("dd MMM yyyy");
                    }
                }

                Color sc = order.Status.Contains("Delivered") || order.Status.Contains("Collected") ? Green :
                           order.Status.Contains("Missed") ? Color.FromArgb(160, 30, 30) :
                           order.Status.Contains("Reserved") ? Orange : Orange;

                bool hasMissed = order.Missed;
                int cardH = hasMissed ? 130 : 110;

                Panel card = new Panel { Location = new Point(10, y), Size = new Size(1200, cardH), BackColor = Color.White };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(1200, 5), BackColor = sc });

                card.Controls.Add(new Label { Text = order.Id, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 12), Size = new Size(230, 20) });
                card.Controls.Add(new Label { Text = order.Qty + "x " + order.Type, Font = new Font("Segoe UI", 10), ForeColor = Dark, Location = new Point(250, 12), Size = new Size(360, 20) });
                card.Controls.Add(new Label { Text = "GBP " + order.Total.ToString("0.00"), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Dark, Location = new Point(620, 12), Size = new Size(140, 20) });
                card.Controls.Add(new Label { Text = order.Status, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = sc, Location = new Point(770, 12), Size = new Size(380, 20) });
                card.Controls.Add(new Label { Text = "Ordered: " + order.Date + "  |  Payment: " + order.Payment, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 40), Size = new Size(700, 18) });
                card.Controls.Add(new Label { Text = "Method: " + order.Method, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 60), Size = new Size(500, 18) });

                if (order.Method == "Click and Collect")
                {
                    card.Controls.Add(new Label { Text = "Location: " + order.Location, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 80), Size = new Size(700, 18) });
                    card.Controls.Add(new Label { Text = "Time: " + order.TimeSlot, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(730, 80), Size = new Size(300, 18) });
                }
                else
                {
                    string expected = order.Method.Contains("Express") ? DateTime.Now.AddDays(1).ToString("dd MMM yyyy") : DateTime.Now.AddDays(3).ToString("dd MMM yyyy");
                    card.Controls.Add(new Label { Text = "Expected delivery: " + expected, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(530, 80), Size = new Size(400, 18) });
                }

                if (hasMissed)
                {
                    Panel missedBanner = new Panel { Location = new Point(0, 96), Size = new Size(1200, 28), BackColor = Color.FromArgb(255, 230, 230) };
                    missedBanner.Controls.Add(new Label { Text = "You missed your collection slot. Your order has been rescheduled to " + order.RescheduleDate + " at the same location and time.", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(160, 30, 30), Location = new Point(10, 6), Size = new Size(1180, 16), BackColor = Color.Transparent });
                    card.Controls.Add(missedBanner);
                }

                pnlMyOrders.Controls.Add(card);
                y += cardH + 10;
            }
        }

        private void SwTab(string which)
        {
            pnlBrowse.Visible = which == "browse";
            pnlOrder.Visible = which == "order";
            pnlMyOrders.Visible = which == "orders";

            if (which == "browse") pnlBrowse.BringToFront();
            if (which == "order") pnlOrder.BringToFront();
            if (which == "orders") { BuildMyOrders(); pnlMyOrders.BringToFront(); }

            SetTab(which == "browse" ? btnBrowse : which == "order" ? btnOrder : btnMyOrders);
        }

        private void SetTab(Button active)
        {
            foreach (Button b in new[] { btnBrowse, btnOrder, btnMyOrders })
            {
                if (b == null) continue;
                b.BackColor = b == active ? Red : Color.White;
                b.ForeColor = b == active ? Color.White : Red;
            }
        }

        private Button Tab(Panel p, string text, int x, int y, Action click)
        {
            Button b = new Button { Text = text, Location = new Point(x, y), Size = new Size(160, 34), Font = new Font("Segoe UI", 9), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Red, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderColor = Color.FromArgb(210, 190, 190);
            b.Click += (s, e) => click();
            p.Controls.Add(b);
            return b;
        }

        private void SH(Panel p, string text, int y)
        {
            p.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(10, y), Size = new Size(700, 28) });
            p.Controls.Add(new Panel { Location = new Point(10, y + 30), Size = new Size(1240, 2), BackColor = Color.FromArgb(230, 180, 180) });
        }

        private void SH2(Panel p, string text, int y)
        {
            p.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(10, y), Size = new Size(700, 24) });
            p.Controls.Add(new Panel { Location = new Point(10, y + 26), Size = new Size(1240, 1), BackColor = Color.FromArgb(230, 180, 180) });
        }
    }
}