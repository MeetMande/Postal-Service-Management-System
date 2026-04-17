// StampsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Stamps page - completely separate from parcels.
// Browse stamps, order with delivery or pickup,
// choose to pay now (card) or pay in store,
// missed pickup logic with automatic rescheduling,
// track all stamp orders with live status.

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

        // Order form controls
        private ComboBox cboStampType, cboQty, cboDeliveryType, cboPickupLocation, cboPickupTime;
        private ComboBox cboPayment;
        private TextBox txtOrderEmail, txtOrderName, txtOrderAddress;
        private Label lblOrderTotal, lblOrderBreakdown, lblPaymentNote;
        private Panel pnlDeliverySection, pnlPickupSection, pnlPaymentSection;
        private DateTimePicker dtPickup;

        // Stamp order history stored in memory for this session
        private List<StampOrder> stampOrders = new List<StampOrder>();

        // Stamp order model
        private class StampOrder
        {
            public string Id, Type, Method, Status, Date, PaymentMethod, Location, TimeSlot, Email;
            public int Qty;
            public double Total;
            public bool Missed;
            public string RescheduleDate;
        }

        // Stamp prices per unit
        private Dictionary<string, double> stampPrices = new Dictionary<string, double>
        {
            { "First Class (1ST)",        1.10 },
            { "Second Class (2ND)",       0.75 },
            { "Large Letter (LRG)",       1.55 },
            { "International (INTL)",     1.85 },
            { "Special Delivery (SPEC)",  6.85 },
            { "Signed For (SIGN)",        1.95 },
        };

        // Pickup locations
        private string[] pickupLocations =
        {
            "PostalMS Hendon Centre -- The Burroughs, Hendon NW4 4BT",
            "PostalMS Cat Hill Centre -- Cat Hill, East Barnet EN4 8HT",
            "PostalMS Archway Point -- 2 Junction Road, Archway N19 5QU",
            "PostalMS Wembley Centre -- Engineers Way, Wembley HA9 0ED",
            "WH Smith Brent Cross -- Brent Cross Shopping Centre NW4 3FP",
            "Tesco Golders Green -- 186 Golders Green Road NW11 9AA",
        };

        // Available pickup time slots
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
            // Top tab bar
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.White };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(218, 218, 218)), 0, 109, topBar.Width, 109);
            this.Controls.Add(topBar);

            topBar.Controls.Add(new Label { Text = "Stamps", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 10), Size = new Size(200, 34) });
            topBar.Controls.Add(new Label { Text = "Browse, order and track your stamps -- completely separate from parcel deliveries.", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, 46), Size = new Size(900, 20) });

            btnBrowse = Tab(topBar, "Browse Stamps", 10, 70, () => SwTab("browse"));
            btnOrder = Tab(topBar, "Order Stamps", 180, 70, () => SwTab("order"));
            btnMyOrders = Tab(topBar, "My Stamp Orders", 350, 70, () => SwTab("orders"));
            SetTab(btnBrowse);

            // Browse panel
            pnlBrowse = new Panel { Dock = DockStyle.Fill, BackColor = LightBg, AutoScroll = true, Visible = true };
            this.Controls.Add(pnlBrowse);
            BuildBrowse();

            // Order panel
            pnlOrder = new Panel { Dock = DockStyle.Fill, BackColor = LightBg, AutoScroll = true, Visible = false };
            this.Controls.Add(pnlOrder);
            BuildOrder();

            // My Orders panel
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

            // Books of stamps
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

            // Step 1 - Stamp type and quantity
            SH2(pnlOrder, "Step 1 -- Choose Your Stamps", y); y += 36;
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
            y += 56;

            // Step 2 - Delivery or Pickup
            SH2(pnlOrder, "Step 2 -- Delivery or Pickup", y); y += 36;
            pnlOrder.Controls.Add(new Label { Text = "HOW WOULD YOU LIKE TO RECEIVE YOUR STAMPS?", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, y), Size = new Size(600, 16) });
            y += 18;

            cboDeliveryType = new ComboBox { Location = new Point(10, y), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboDeliveryType.Items.AddRange(new object[]
            {
                "Home Delivery (GBP 1.50)",
                "Express Home Delivery (GBP 3.99)",
                "Click and Collect -- Free Pickup"
            });
            cboDeliveryType.SelectedIndex = 0;
            pnlOrder.Controls.Add(cboDeliveryType);
            y += 54;

            // Fixed detail Y so nothing overlaps
            int detailY = y;

            // Delivery section
            pnlDeliverySection = new Panel { Location = new Point(10, detailY), Size = new Size(1240, 200), BackColor = Color.FromArgb(248, 248, 248), Visible = true };
            pnlOrder.Controls.Add(pnlDeliverySection);

            pnlDeliverySection.Controls.Add(new Label { Text = "YOUR FULL NAME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 10), Size = new Size(300, 16) });
            txtOrderName = new TextBox { Location = new Point(10, 28), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlDeliverySection.Controls.Add(txtOrderName);

            pnlDeliverySection.Controls.Add(new Label { Text = "DELIVERY ADDRESS", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 72), Size = new Size(300, 16) });
            txtOrderAddress = new TextBox { Location = new Point(10, 90), Size = new Size(900, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlDeliverySection.Controls.Add(txtOrderAddress);

            pnlDeliverySection.Controls.Add(new Label { Text = "YOUR EMAIL (must be @gmail.com)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 134), Size = new Size(400, 16) });
            txtOrderEmail = new TextBox { Location = new Point(10, 152), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlDeliverySection.Controls.Add(txtOrderEmail);

            // Pickup section -- same Y, hidden by default
            pnlPickupSection = new Panel { Location = new Point(10, detailY), Size = new Size(1240, 240), BackColor = Color.FromArgb(248, 248, 248), Visible = false };
            pnlOrder.Controls.Add(pnlPickupSection);

            pnlPickupSection.Controls.Add(new Label { Text = "SELECT PICKUP LOCATION", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 10), Size = new Size(400, 16) });
            cboPickupLocation = new ComboBox { Location = new Point(10, 28), Size = new Size(900, 34), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (string loc in pickupLocations) cboPickupLocation.Items.Add(loc);
            cboPickupLocation.SelectedIndex = 0;
            pnlPickupSection.Controls.Add(cboPickupLocation);

            pnlPickupSection.Controls.Add(new Label { Text = "SELECT PICKUP DATE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 72), Size = new Size(200, 16) });
            dtPickup = new DateTimePicker { Location = new Point(10, 90), Size = new Size(260, 34), Font = new Font("Segoe UI", 11), MinDate = DateTime.Today.AddDays(1), MaxDate = DateTime.Today.AddDays(14) };
            pnlPickupSection.Controls.Add(dtPickup);

            pnlPickupSection.Controls.Add(new Label { Text = "SELECT TIME SLOT", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(290, 72), Size = new Size(200, 16) });
            cboPickupTime = new ComboBox { Location = new Point(290, 90), Size = new Size(240, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (string t in pickupTimes) cboPickupTime.Items.Add(t);
            cboPickupTime.SelectedIndex = 0;
            pnlPickupSection.Controls.Add(cboPickupTime);

            // Missed pickup warning info
            Panel missedInfo = new Panel { Location = new Point(10, 134), Size = new Size(900, 46), BackColor = Color.FromArgb(255, 245, 220) };
            missedInfo.Controls.Add(new Label
            {
                Text = "If you miss your pickup slot your order will be held for 48 hours.\nAfter 48 hours it is automatically rescheduled to the next available slot at the same location.",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(140, 80, 0),
                Location = new Point(10, 6),
                Size = new Size(880, 34),
                BackColor = Color.Transparent
            });
            pnlPickupSection.Controls.Add(missedInfo);

            pnlPickupSection.Controls.Add(new Label { Text = "YOUR EMAIL (must be @gmail.com)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 192), Size = new Size(400, 16) });
            var txtPickupEmail = new TextBox { Location = new Point(10, 210), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Name = "txtPickupEmail" };
            pnlPickupSection.Controls.Add(txtPickupEmail);

            // Wire up delivery type toggle
            cboDeliveryType.SelectedIndexChanged += (s, e) =>
            {
                bool isPickup = cboDeliveryType.SelectedIndex == 2;
                pnlDeliverySection.Visible = !isPickup;
                pnlPickupSection.Visible = isPickup;
                RecalcOrder();
                UpdatePaymentNote();
            };

            // Step 3 - Payment method -- fixed position below both sections
            y = detailY + 260;
            SH2(pnlOrder, "Step 3 -- Payment Method", y); y += 36;

            pnlOrder.Controls.Add(new Label { Text = "HOW WOULD YOU LIKE TO PAY?", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, y), Size = new Size(400, 16) });
            y += 18;

            cboPayment = new ComboBox { Location = new Point(10, y), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboPayment.Items.AddRange(new object[]
            {
                "Pay Now -- Credit or Debit Card (Online)",
                "Pay In Store -- Pay When You Collect"
            });
            cboPayment.SelectedIndex = 0;
            cboPayment.SelectedIndexChanged += (s, e) => { RecalcOrder(); UpdatePaymentNote(); };
            pnlOrder.Controls.Add(cboPayment);
            y += 44;

            // Payment note box
            pnlPaymentSection = new Panel { Location = new Point(10, y), Size = new Size(900, 56), BackColor = Color.FromArgb(230, 255, 240), Visible = true };
            lblPaymentNote = new Label { Location = new Point(12, 10), Size = new Size(876, 36), Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(10, 100, 40), BackColor = Color.Transparent };
            pnlPaymentSection.Controls.Add(lblPaymentNote);
            pnlOrder.Controls.Add(pnlPaymentSection);
            y += 66;

            // Pay in store info panel -- shown when pay in store selected
            Panel payInStoreInfo = new Panel { Location = new Point(10, y), Size = new Size(900, 80), BackColor = Color.FromArgb(235, 245, 255), Visible = false, Name = "pnlPayInStoreInfo" };
            payInStoreInfo.Controls.Add(new Label
            {
                Text = "Pay In Store Instructions:\n" +
                             "1. Place your order now to reserve your stamps\n" +
                             "2. Visit your chosen pickup location on your selected date\n" +
                             "3. Show your Order ID at the counter and pay by cash or card\n" +
                             "4. If you do not pay within 48 hours your reservation is cancelled",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(20, 60, 140),
                Location = new Point(12, 8),
                Size = new Size(876, 64),
                BackColor = Color.Transparent
            });
            pnlOrder.Controls.Add(payInStoreInfo);

            cboPayment.SelectedIndexChanged += (s, e2) =>
            {
                bool payInStore = cboPayment.SelectedIndex == 1;
                payInStoreInfo.Visible = payInStore;
            };
            y += 90;

            // Step 4 - Price breakdown
            SH2(pnlOrder, "Step 4 -- Price Breakdown", y); y += 36;

            Panel priceBox = new Panel { Location = new Point(10, y), Size = new Size(580, 200), BackColor = Color.White };
            priceBox.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(580, 5), BackColor = Red });
            lblOrderBreakdown = new Label { Location = new Point(14, 14), Size = new Size(552, 140), Font = new Font("Segoe UI", 10), ForeColor = Dark, BackColor = Color.Transparent };
            priceBox.Controls.Add(lblOrderBreakdown);
            priceBox.Controls.Add(new Panel { Location = new Point(14, 158), Size = new Size(552, 1), BackColor = Color.FromArgb(220, 220, 220) });
            lblOrderTotal = new Label { Location = new Point(14, 164), Size = new Size(552, 28), Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, BackColor = Color.Transparent };
            priceBox.Controls.Add(lblOrderTotal);
            pnlOrder.Controls.Add(priceBox);
            y += 210;

            // Delivery time info
            Panel timeInfo = new Panel { Location = new Point(10, y), Size = new Size(900, 40), BackColor = Color.FromArgb(235, 245, 255) };
            timeInfo.Controls.Add(new Label { Text = "Home Delivery: 2-3 working days  |  Express: Next working day  |  Click and Collect: From your selected date", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(12, 10), Size = new Size(876, 20), BackColor = Color.Transparent });
            pnlOrder.Controls.Add(timeInfo);
            y += 50;

            // Place order button
            Button btnPlaceOrder = new Button { Text = "Place Stamp Order ->", Location = new Point(10, y), Size = new Size(280, 52), Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnPlaceOrder.FlatAppearance.BorderSize = 0;
            btnPlaceOrder.MouseEnter += (s, e) => btnPlaceOrder.BackColor = DarkRed;
            btnPlaceOrder.MouseLeave += (s, e) => btnPlaceOrder.BackColor = Red;
            btnPlaceOrder.Click += PlaceOrder_Click;
            pnlOrder.Controls.Add(btnPlaceOrder);

            UpdatePaymentNote();
            RecalcOrder();
        }

        // Place order button click handler
        private void PlaceOrder_Click(object sender, EventArgs e)
        {
            bool isPickup = cboDeliveryType.SelectedIndex == 2;
            bool payInStore = cboPayment.SelectedIndex == 1;
            string email = "";

            if (isPickup)
            {
                var tb = pnlPickupSection.Controls["txtPickupEmail"] as TextBox;
                email = tb?.Text.Trim() ?? "";
            }
            else
            {
                email = txtOrderEmail?.Text.Trim() ?? "";
            }

            // Validate Gmail
            if (!email.EndsWith("@gmail.com") || email.Length <= "@gmail.com".Length)
            {
                MessageBox.Show("Please enter a valid Gmail address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate name for delivery
            if (!isPickup && string.IsNullOrWhiteSpace(txtOrderName?.Text))
            {
                MessageBox.Show("Please enter your full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate address for delivery
            if (!isPickup && string.IsNullOrWhiteSpace(txtOrderAddress?.Text))
            {
                MessageBox.Show("Please enter your delivery address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderId = "STO-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string type = cboStampType.SelectedItem.ToString();
            int qty = Convert.ToInt32(cboQty.SelectedItem);
            string method = isPickup ? "Click and Collect" : cboDeliveryType.SelectedItem.ToString();
            string payment = cboPayment.SelectedItem.ToString();
            double total = CalcTotal();
            string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            string location = isPickup ? cboPickupLocation.SelectedItem?.ToString() ?? "--" : "--";
            string timeSlot = isPickup ? cboPickupTime.SelectedItem?.ToString() ?? "--" : "--";
            string pickupDate = isPickup ? dtPickup.Value.ToString("dd MMMM yyyy") : "--";

            // Create order
            stampOrders.Add(new StampOrder
            {
                Id = orderId,
                Type = type,
                Qty = qty,
                Method = method,
                Status = payInStore && isPickup ? "Reserved -- Pay In Store" : "Processing",
                Date = date,
                PaymentMethod = payment,
                Location = location,
                TimeSlot = timeSlot,
                Email = email,
                Total = total,
                Missed = false,
                RescheduleDate = ""
            });

            // Build confirmation message
            string msg = "Stamp order placed successfully!\n\n" +
                         "Order ID: " + orderId + "\n" +
                         "Stamps: " + qty + "x " + type + "\n" +
                         "Method: " + method + "\n" +
                         "Payment: " + (payInStore ? "Pay In Store" : "Paid Online") + "\n" +
                         "Total: GBP " + total.ToString("0.00");

            if (isPickup)
            {
                msg += "\n\nPickup Details:\n" +
                       "Location: " + location + "\n" +
                       "Date: " + pickupDate + "\n" +
                       "Time Slot: " + timeSlot;

                if (payInStore)
                    msg += "\n\nIMPORTANT: Bring your Order ID (" + orderId + ") to the store.\nPay by cash or card at the counter.\nIf you miss your slot your order is held for 48 hours then rescheduled.";
            }
            else
            {
                msg += "\n\nDelivery to: " + txtOrderAddress.Text;
            }

            msg += "\n\nConfirmation sent to: " + email;

            MessageBox.Show(msg, "Order Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SwTab("orders");
        }

        // ============================================================
        // MY ORDERS TAB
        // ============================================================
        private void BuildMyOrders()
        {
            pnlMyOrders.Controls.Clear();

            pnlMyOrders.Controls.Add(new Label { Text = "My Stamp Orders", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 16), Size = new Size(400, 34) });
            pnlMyOrders.Controls.Add(new Label { Text = "All your stamp orders and their current status.", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, 52), Size = new Size(800, 22) });

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
                // Check missed pickup logic
                string displayStatus = order.Status;
                string missedNote = "";

                if (order.Method == "Click and Collect" && !order.Missed)
                {
                    // Simulate: if pickup date has passed mark as missed and reschedule
                    DateTime pickupDt;
                    if (DateTime.TryParse(order.Date, out DateTime orderDt))
                    {
                        // For demo purposes if order placed more than 2 days ago and not collected
                        if (orderDt.AddDays(2) < DateTime.Now && order.Status == "Processing")
                        {
                            order.Missed = true;
                            order.Status = "Missed -- Rescheduled";
                            order.RescheduleDate = DateTime.Now.AddDays(2).ToString("dd MMM yyyy");
                            displayStatus = "Missed -- Rescheduled";
                            missedNote = "New pickup: " + order.RescheduleDate + " same time slot";
                        }
                    }
                }

                Color statusColor = displayStatus.Contains("Delivered") || displayStatus.Contains("Collected") ? Green :
                                    displayStatus.Contains("Missed") ? Color.FromArgb(160, 30, 30) :
                                    displayStatus.Contains("Reserved") ? Orange :
                                    displayStatus.Contains("Processing") ? Orange : Grey;

                Panel card = new Panel { Location = new Point(10, y), Size = new Size(1200, 110), BackColor = Color.White };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(1200, 5), BackColor = statusColor });

                card.Controls.Add(new Label { Text = order.Id, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 12), Size = new Size(220, 20) });
                card.Controls.Add(new Label { Text = order.Qty + "x " + order.Type, Font = new Font("Segoe UI", 10), ForeColor = Dark, Location = new Point(240, 12), Size = new Size(360, 20) });
                card.Controls.Add(new Label { Text = "GBP " + order.Total.ToString("0.00"), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Dark, Location = new Point(610, 12), Size = new Size(140, 20) });
                card.Controls.Add(new Label { Text = displayStatus, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = statusColor, Location = new Point(760, 12), Size = new Size(300, 20) });
                card.Controls.Add(new Label { Text = "Ordered: " + order.Date, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 40), Size = new Size(300, 18) });
                card.Controls.Add(new Label { Text = "Method: " + order.Method + "  |  Payment: " + (order.PaymentMethod.Contains("Store") ? "Pay In Store" : "Paid Online"), Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 60), Size = new Size(600, 18) });

                // Show pickup details if applicable
                if (order.Method == "Click and Collect")
                {
                    card.Controls.Add(new Label { Text = "Location: " + order.Location, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 80), Size = new Size(700, 18) });
                    card.Controls.Add(new Label { Text = "Time Slot: " + order.TimeSlot, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(730, 80), Size = new Size(300, 18) });
                }

                // Show missed and rescheduled info
                if (!string.IsNullOrEmpty(missedNote))
                {
                    Panel missedBanner = new Panel { Location = new Point(0, 92), Size = new Size(1200, 24), BackColor = Color.FromArgb(255, 230, 230) };
                    missedBanner.Controls.Add(new Label { Text = "You missed your pickup slot. " + missedNote + ". Please contact us if you need to change this.", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(160, 30, 30), Location = new Point(10, 4), Size = new Size(1180, 16), BackColor = Color.Transparent });
                    card.Controls.Add(missedBanner);
                    card.Size = new Size(1200, 116);
                }

                // Show expected delivery date
                string expected = order.Method.Contains("Express") ? DateTime.Now.AddDays(1).ToString("dd MMM yyyy") :
                                  order.Method.Contains("Collect") ? "See pickup details" :
                                  DateTime.Now.AddDays(3).ToString("dd MMM yyyy");
                card.Controls.Add(new Label { Text = "Expected: " + expected, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(850, 40), Size = new Size(300, 18) });

                pnlMyOrders.Controls.Add(card);
                y += card.Height + 10;
            }
        }

        // Recalculate order total with tax and service fee
        private void RecalcOrder()
        {
            try
            {
                string stampType = cboStampType?.SelectedItem?.ToString() ?? "";
                int qty = cboQty?.SelectedItem != null ? Convert.ToInt32(cboQty.SelectedItem) : 1;
                int delType = cboDeliveryType?.SelectedIndex ?? 0;
                bool payStore = cboPayment?.SelectedIndex == 1;

                if (!stampPrices.ContainsKey(stampType)) return;

                double unitPrice = stampPrices[stampType];
                double stampTotal = unitPrice * qty;
                double delivery = delType == 0 ? 1.50 : delType == 1 ? 3.99 : 0.00;
                double subtotal = stampTotal + delivery;
                double tax = Math.Round(subtotal * 0.20, 2);
                double serviceFee = Math.Round(subtotal * 0.02, 2);
                double total = Math.Round(subtotal + tax + serviceFee, 2);

                string methodStr = delType == 0 ? "Home Delivery" : delType == 1 ? "Express Delivery" : "Click and Collect (Free)";
                string payStr = payStore ? "Pay In Store (reserve now, pay on collection)" : "Pay Now Online";

                if (lblOrderBreakdown != null)
                    lblOrderBreakdown.Text =
                        qty + "x " + stampType + "  --  GBP " + unitPrice.ToString("0.00") + " each\n" +
                        "Stamps subtotal:         GBP " + stampTotal.ToString("0.00") + "\n" +
                        methodStr + ":  GBP " + delivery.ToString("0.00") + "\n" +
                        "VAT (20%):               GBP " + tax.ToString("0.00") + "\n" +
                        "Service fee (2%):        GBP " + serviceFee.ToString("0.00") + "\n" +
                        "Payment method:          " + payStr;

                if (lblOrderTotal != null)
                    lblOrderTotal.Text = "Total:  GBP " + total.ToString("0.00") +
                                         (payStore ? "  (pay on collection)" : "  (pay now online)");
            }
            catch { }
        }

        // Update payment note based on selections
        private void UpdatePaymentNote()
        {
            if (lblPaymentNote == null || cboPayment == null) return;
            bool isPickup = cboDeliveryType?.SelectedIndex == 2;
            bool payStore = cboPayment.SelectedIndex == 1;

            if (payStore && !isPickup)
            {
                // Pay in store only available for pickup
                lblPaymentNote.Text = "Pay In Store is only available with Click and Collect. Please select Click and Collect above or choose Pay Now.";
                pnlPaymentSection.BackColor = Color.FromArgb(255, 235, 235);
                lblPaymentNote.ForeColor = Color.FromArgb(160, 30, 30);
            }
            else if (payStore && isPickup)
            {
                lblPaymentNote.Text = "You will reserve your stamps now and pay by cash or card when you collect at the store. Your slot is held for 48 hours.";
                pnlPaymentSection.BackColor = Color.FromArgb(230, 255, 240);
                lblPaymentNote.ForeColor = Color.FromArgb(10, 100, 40);
            }
            else
            {
                lblPaymentNote.Text = "Your card will be charged immediately when you place the order. A confirmation email will be sent to your Gmail address.";
                pnlPaymentSection.BackColor = Color.FromArgb(230, 255, 240);
                lblPaymentNote.ForeColor = Color.FromArgb(10, 100, 40);
            }
        }

        private double CalcTotal()
        {
            try
            {
                string stampType = cboStampType?.SelectedItem?.ToString() ?? "";
                int qty = Convert.ToInt32(cboQty?.SelectedItem ?? 1);
                int delType = cboDeliveryType?.SelectedIndex ?? 0;
                double unitPrice = stampPrices.ContainsKey(stampType) ? stampPrices[stampType] : 0;
                double stampTotal = unitPrice * qty;
                double delivery = delType == 0 ? 1.50 : delType == 1 ? 3.99 : 0.00;
                double subtotal = stampTotal + delivery;
                double tax = Math.Round(subtotal * 0.20, 2);
                double serviceFee = Math.Round(subtotal * 0.02, 2);
                return Math.Round(subtotal + tax + serviceFee, 2);
            }
            catch { return 0; }
        }

        // Switch between tabs
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