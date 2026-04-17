// StampsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Stamps page - completely separate from parcels.
// Browse stamp types, order for delivery or pickup,
// see full price breakdown with tax, track stamp orders.

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

        // Tabs
        private Panel pnlBrowse, pnlOrder, pnlMyOrders;
        private Button btnBrowse, btnOrder, btnMyOrders;

        // Order form controls
        private ComboBox cboStampType, cboQty, cboDeliveryType, cboPickupLocation, cboPickupTime;
        private TextBox txtOrderEmail, txtOrderName, txtOrderAddress;
        private Label lblOrderTotal, lblOrderBreakdown;
        private Panel pnlDeliverySection, pnlPickupSection;

        // Stamp order history stored in memory
        private List<(string id, string type, int qty, string method, string status, string date, double total)> stampOrders
            = new List<(string, string, int, string, string, string, double)>();

        // Stamp prices
        private Dictionary<string, double> stampPrices = new Dictionary<string, double>
        {
            { "First Class (1ST)",        1.10 },
            { "Second Class (2ND)",       0.75 },
            { "Large Letter (LRG)",       1.55 },
            { "International (INTL)",     1.85 },
            { "Special Delivery (SPEC)",  6.85 },
            { "Signed For (SIGN)",        1.95 },
        };

        // Pickup locations with addresses
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
            topBar.Controls.Add(new Label { Text = "Browse, order and track your stamp purchases -- completely separate from parcel deliveries", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, 46), Size = new Size(900, 20) });

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

            // Stamp type cards
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

                // Click to order
                Button orderBtn = new Button { Text = "Order This", Location = new Point(10, 196), Size = new Size(175, 0), Visible = false };
                string stampName = s.name + " (" + s.sym.Trim('[', ']') + ")";

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
                ("[x50]", "50 First Class Bundle",  "GBP 48.00", "GBP 0.96 each", "Best value for high-volume senders"),
            };

            int bx = 10;
            foreach (var b in books)
            {
                Panel bcard = new Panel { Location = new Point(bx, y), Size = new Size(290, 130), BackColor = Color.White };
                bcard.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(290, 5), BackColor = Red });
                Panel bbadge = new Panel { Location = new Point(10, 14), Size = new Size(60, 60), BackColor = Red };
                bbadge.Controls.Add(new Label { Text = b.sym, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White, Location = new Point(0, 18), Size = new Size(60, 24), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
                bcard.Controls.Add(bbadge);
                bcard.Controls.Add(new Label { Text = b.name, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(80, 14), Size = new Size(200, 20) });
                bcard.Controls.Add(new Label { Text = b.price, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Dark, Location = new Point(80, 36), Size = new Size(200, 26) });
                bcard.Controls.Add(new Label { Text = b.each, Font = new Font("Segoe UI", 8), ForeColor = Grey, Location = new Point(80, 64), Size = new Size(200, 18) });
                bcard.Controls.Add(new Label { Text = b.saving, Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Green, Location = new Point(80, 84), Size = new Size(200, 18) });
                pnlBrowse.Controls.Add(bcard);
                bx += 305;
            }
            y += 145;

            // Order now button
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
            // Use fixed y positions throughout so nothing overlaps
            int y = 16;

            // Step 1 heading
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

            // Step 2 heading
            SH2(pnlOrder, "Step 2 -- Delivery or Pickup", y); y += 36;

            pnlOrder.Controls.Add(new Label { Text = "HOW WOULD YOU LIKE TO RECEIVE YOUR STAMPS?", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, y), Size = new Size(600, 16) });
            y += 18;

            cboDeliveryType = new ComboBox { Location = new Point(10, y), Size = new Size(380, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboDeliveryType.Items.AddRange(new object[] { "Home Delivery (GBP 1.50)", "Express Home Delivery (GBP 3.99)", "Click and Collect -- Free Pickup" });
            cboDeliveryType.SelectedIndex = 0;
            pnlOrder.Controls.Add(cboDeliveryType);
            y += 54;

            // Delivery address section -- fixed position below the dropdown
            int detailY = y;
            pnlDeliverySection = new Panel
            {
                Location = new Point(10, detailY),
                Size = new Size(1240, 200),
                BackColor = Color.FromArgb(248, 248, 248),
                Visible = true
            };
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

            // Pickup section -- same fixed position, hidden by default
            pnlPickupSection = new Panel
            {
                Location = new Point(10, detailY),
                Size = new Size(1240, 220),
                BackColor = Color.FromArgb(248, 248, 248),
                Visible = false
            };
            pnlOrder.Controls.Add(pnlPickupSection);

            pnlPickupSection.Controls.Add(new Label { Text = "SELECT PICKUP LOCATION", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 10), Size = new Size(400, 16) });
            cboPickupLocation = new ComboBox { Location = new Point(10, 28), Size = new Size(900, 34), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (string loc in pickupLocations) cboPickupLocation.Items.Add(loc);
            cboPickupLocation.SelectedIndex = 0;
            pnlPickupSection.Controls.Add(cboPickupLocation);

            pnlPickupSection.Controls.Add(new Label { Text = "SELECT PICKUP DATE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 72), Size = new Size(200, 16) });
            var dtPickup = new DateTimePicker { Location = new Point(10, 90), Size = new Size(260, 34), Font = new Font("Segoe UI", 11), MinDate = DateTime.Today.AddDays(1), MaxDate = DateTime.Today.AddDays(14) };
            pnlPickupSection.Controls.Add(dtPickup);

            pnlPickupSection.Controls.Add(new Label { Text = "SELECT TIME SLOT", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(290, 72), Size = new Size(200, 16) });
            cboPickupTime = new ComboBox { Location = new Point(290, 90), Size = new Size(240, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (string t in pickupTimes) cboPickupTime.Items.Add(t);
            cboPickupTime.SelectedIndex = 0;
            pnlPickupSection.Controls.Add(cboPickupTime);

            pnlPickupSection.Controls.Add(new Label { Text = "YOUR EMAIL (must be @gmail.com)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(10, 134), Size = new Size(400, 16) });
            var txtPickupEmail = new TextBox { Location = new Point(10, 152), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Name = "txtPickupEmail" };
            pnlPickupSection.Controls.Add(txtPickupEmail);

            // Wire up delivery type toggle
            cboDeliveryType.SelectedIndexChanged += (s, e) =>
            {
                bool isPickup = cboDeliveryType.SelectedIndex == 2;
                pnlDeliverySection.Visible = !isPickup;
                pnlPickupSection.Visible = isPickup;
                RecalcOrder();
            };

            // Step 3 - Price breakdown -- fixed position BELOW both sections
            y = detailY + 230;
            SH2(pnlOrder, "Step 3 -- Price Breakdown", y); y += 36;

            Panel priceBox = new Panel { Location = new Point(10, y), Size = new Size(560, 190), BackColor = Color.White };
            priceBox.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(560, 5), BackColor = Red });
            lblOrderBreakdown = new Label { Location = new Point(14, 14), Size = new Size(532, 130), Font = new Font("Segoe UI", 10), ForeColor = Dark, BackColor = Color.Transparent };
            priceBox.Controls.Add(lblOrderBreakdown);
            priceBox.Controls.Add(new Panel { Location = new Point(14, 148), Size = new Size(532, 1), BackColor = Color.FromArgb(220, 220, 220) });
            lblOrderTotal = new Label { Location = new Point(14, 155), Size = new Size(532, 28), Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, BackColor = Color.Transparent };
            priceBox.Controls.Add(lblOrderTotal);
            pnlOrder.Controls.Add(priceBox);
            y += 200;

            // Info note
            Panel infoNote = new Panel { Location = new Point(10, y), Size = new Size(900, 46), BackColor = Color.FromArgb(235, 245, 255) };
            infoNote.Controls.Add(new Label { Text = "Home Delivery: 2-3 working days  |  Express: Next working day  |  Click and Collect: From selected date", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(12, 14), Size = new Size(876, 18), BackColor = Color.Transparent });
            pnlOrder.Controls.Add(infoNote);
            y += 56;

            // Place order button
            Button btnPlaceOrder = new Button { Text = "Place Stamp Order ->", Location = new Point(10, y), Size = new Size(280, 52), Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnPlaceOrder.FlatAppearance.BorderSize = 0;
            btnPlaceOrder.MouseEnter += (s, e) => btnPlaceOrder.BackColor = DarkRed;
            btnPlaceOrder.MouseLeave += (s, e) => btnPlaceOrder.BackColor = Red;
            btnPlaceOrder.Click += (s, e) =>
            {
                bool isPickup = cboDeliveryType.SelectedIndex == 2;
                string email = isPickup
                    ? (pnlPickupSection.Controls["txtPickupEmail"] as TextBox)?.Text.Trim() ?? ""
                    : txtOrderEmail.Text.Trim();

                if (!email.EndsWith("@gmail.com") || email.Length <= "@gmail.com".Length)
                { MessageBox.Show("Please enter a valid Gmail address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                if (!isPickup && string.IsNullOrWhiteSpace(txtOrderName?.Text))
                { MessageBox.Show("Please enter your full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                if (!isPickup && string.IsNullOrWhiteSpace(txtOrderAddress?.Text))
                { MessageBox.Show("Please enter your delivery address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                string orderId = "STO-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                string type = cboStampType.SelectedItem.ToString();
                int qty = Convert.ToInt32(cboQty.SelectedItem);
                string method = isPickup ? "Click and Collect" : cboDeliveryType.SelectedItem.ToString();
                double total = CalcTotal();
                string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                stampOrders.Add((orderId, type, qty, method, "Processing", date, total));

                string pickupInfo = isPickup
                    ? "\n\nPickup Location: " + (cboPickupLocation.SelectedItem?.ToString() ?? "--") +
                      "\nPickup Date: " + dtPickup.Value.ToString("dd MMMM yyyy") +
                      "\nTime Slot: " + (cboPickupTime.SelectedItem?.ToString() ?? "--")
                    : "\n\nDelivery to: " + txtOrderAddress.Text;

                MessageBox.Show(
                    "Stamp order placed successfully!\n\n" +
                    "Order ID: " + orderId + "\n" +
                    "Stamps: " + qty + "x " + type + "\n" +
                    "Method: " + method + "\n" +
                    "Total Paid: GBP " + total.ToString("0.00") +
                    pickupInfo + "\n\n" +
                    "Confirmation will be sent to: " + email,
                    "Order Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                SwTab("orders");
            };
            pnlOrder.Controls.Add(btnPlaceOrder);

            RecalcOrder();
        }

        // ============================================================
        // MY ORDERS TAB
        // ============================================================
        private void BuildMyOrders()
        {
            pnlMyOrders.Controls.Clear();

            pnlMyOrders.Controls.Add(new Label { Text = "My Stamp Orders", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 16), Size = new Size(400, 34) });
            pnlMyOrders.Controls.Add(new Label { Text = "All your stamp orders and their current delivery or pickup status.", Font = new Font("Segoe UI", 10), ForeColor = Grey, Location = new Point(10, 52), Size = new Size(800, 22) });

            if (stampOrders.Count == 0)
            {
                Panel emptyBox = new Panel { Location = new Point(10, 90), Size = new Size(700, 80), BackColor = Color.White };
                emptyBox.Controls.Add(new Label { Text = "No stamp orders yet. Go to Order Stamps to place your first order.", Font = new Font("Segoe UI", 11), ForeColor = Grey, Location = new Point(20, 26), Size = new Size(660, 28) });
                pnlMyOrders.Controls.Add(emptyBox);
                return;
            }

            int y = 90;
            foreach (var order in stampOrders)
            {
                Panel card = new Panel { Location = new Point(10, y), Size = new Size(1200, 90), BackColor = Color.White };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(1200, 5), BackColor = Red });

                // Status colour
                Color statusColor = order.status == "Delivered" || order.status == "Ready for Collection"
                    ? Green : order.status == "Processing" ? Color.FromArgb(140, 80, 0) : Red;

                card.Controls.Add(new Label { Text = order.id, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 14), Size = new Size(200, 20) });
                card.Controls.Add(new Label { Text = order.type, Font = new Font("Segoe UI", 10), ForeColor = Dark, Location = new Point(220, 14), Size = new Size(300, 20) });
                card.Controls.Add(new Label { Text = "Qty: " + order.qty, Font = new Font("Segoe UI", 10), ForeColor = Dark, Location = new Point(530, 14), Size = new Size(100, 20) });
                card.Controls.Add(new Label { Text = "GBP " + order.total.ToString("0.00"), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Dark, Location = new Point(640, 14), Size = new Size(140, 20) });
                card.Controls.Add(new Label { Text = order.status, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = statusColor, Location = new Point(790, 14), Size = new Size(200, 20) });
                card.Controls.Add(new Label { Text = "Ordered: " + order.date, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 42), Size = new Size(300, 18) });
                card.Controls.Add(new Label { Text = "Method: " + order.method, Font = new Font("Segoe UI", 9), ForeColor = Grey, Location = new Point(14, 62), Size = new Size(500, 18) });

                // Expected date
                string expected = order.method.Contains("Express") ? DateTime.Now.AddDays(1).ToString("dd MMM yyyy") :
                                  order.method.Contains("Collect") ? "See confirmation email" :
                                  DateTime.Now.AddDays(3).ToString("dd MMM yyyy");
                card.Controls.Add(new Label { Text = "Expected: " + expected, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(530, 42), Size = new Size(300, 18) });

                pnlMyOrders.Controls.Add(card);
                y += 100;
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

                if (!stampPrices.ContainsKey(stampType)) return;

                double unitPrice = stampPrices[stampType];
                double stampTotal = unitPrice * qty;
                double delivery = delType == 0 ? 1.50 : delType == 1 ? 3.99 : 0.00;
                double subtotal = stampTotal + delivery;
                double tax = Math.Round(subtotal * 0.20, 2); // 20% VAT
                double serviceFee = Math.Round(subtotal * 0.02, 2); // 2% service fee
                double total = Math.Round(subtotal + tax + serviceFee, 2);

                string method = delType == 0 ? "Home Delivery" : delType == 1 ? "Express Delivery" : "Click and Collect (Free)";

                if (lblOrderBreakdown != null)
                    lblOrderBreakdown.Text =
                        qty + "x " + stampType + "  @ GBP " + unitPrice.ToString("0.00") + " each\n" +
                        "Stamps subtotal:     GBP " + stampTotal.ToString("0.00") + "\n" +
                        method + ":       GBP " + delivery.ToString("0.00") + "\n" +
                        "VAT (20%):           GBP " + tax.ToString("0.00") + "\n" +
                        "Service fee (2%):    GBP " + serviceFee.ToString("0.00");

                if (lblOrderTotal != null)
                    lblOrderTotal.Text = "Total to Pay:  GBP " + total.ToString("0.00");
            }
            catch { }
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