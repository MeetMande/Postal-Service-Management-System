// DeliveriesView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Deliveries page showing detailed tracking for each parcel.
// Shows driver name, route, status timeline, attempt history,
// expected delivery date and full status explanation.
// Colour coded status badges in grid and visual progress tracker.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace PostalServiceWinForms.Forms
{
    public class DeliveriesView : UserControl
    {
        private DataGridView dgv;
        private Panel pnlDetail;
        private DatabaseHelper db;
        private string userID;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color Green = Color.FromArgb(20, 130, 65);
        private Color Blue = Color.FromArgb(30, 80, 180);
        private Color Orange = Color.FromArgb(180, 100, 0);
        private Button activeFilter;

        public DeliveriesView(string uid, DatabaseHelper dbHelper)
        {
            userID = uid; db = dbHelper;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            Build();
            LoadDeliveries("All");
        }

        private void Build()
        {
            // Detail panel docked to bottom -- expands when row is clicked
            pnlDetail = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                AutoScroll = true
            };
            this.Controls.Add(pnlDetail);

            // Main delivery grid
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Red;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 44;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.RowTemplate.Height = 46;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 248, 248);
            dgv.SelectionChanged += DgvSelect;
            dgv.CellFormatting += StatusFmt;
            this.Controls.Add(dgv);

            // Page header panel
            Panel hdr = new Panel { Dock = DockStyle.Top, Height = 115, BackColor = Color.FromArgb(245, 245, 245) };
            this.Controls.Add(hdr);

            hdr.Controls.Add(new Label
            {
                Text = "My Deliveries",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(0, 8),
                Size = new Size(400, 32)
            });
            hdr.Controls.Add(new Label
            {
                Text = "Click any row to see full delivery details, driver information, timeline and current parcel location.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, 44),
                Size = new Size(1000, 20)
            });

            // Filter buttons
            int fx = 0;
            foreach (string f in new[] { "All", "Assigned", "In Progress", "Completed", "Failed" })
            {
                string filter = f;
                Button btn = new Button
                {
                    Text = f,
                    Location = new Point(fx, 72),
                    Size = new Size(120, 34),
                    Font = new Font("Segoe UI", 9),
                    BackColor = f == "All" ? Red : Color.White,
                    ForeColor = f == "All" ? Color.White : Red,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(210, 190, 190);
                if (f == "All") activeFilter = btn;
                btn.Click += (s, e) =>
                {
                    if (activeFilter != null) { activeFilter.BackColor = Color.White; activeFilter.ForeColor = Red; }
                    btn.BackColor = Red; btn.ForeColor = Color.White;
                    activeFilter = btn;
                    LoadDeliveries(filter == "All" ? "All" : filter);
                };
                hdr.Controls.Add(btn);
                fx += 128;
            }
        }

        private void LoadDeliveries(string filter)
        {
            try
            {
                DataTable dt = filter == "All"
                    ? db.GetDeliveriesForUser(userID)
                    : db.GetDeliveriesForUserByStatus(userID, filter);
                dgv.DataSource = dt;
                HideColumns();
            }
            catch { }
        }

        private void HideColumns()
        {
            // Hide columns not needed in the main grid
            string[] hide = { "SenderName", "ParcelType" };
            foreach (string col in hide)
                if (dgv.Columns.Contains(col))
                    dgv.Columns[col].Visible = false;

            // Format date columns
            if (dgv.Columns.Contains("AssignedDate"))
                dgv.Columns["AssignedDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (dgv.Columns.Contains("LastAttemptDate"))
                dgv.Columns["LastAttemptDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        // Show detailed delivery info when a row is clicked
        private void DgvSelect(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;

            var row = dgv.CurrentRow;

            // Extract all values from selected row
            string delID = row.Cells["DeliveryID"].Value?.ToString() ?? "--";
            string trackID = row.Cells["TrackingID"].Value?.ToString() ?? "--";
            string driver = row.Cells["DriverName"].Value?.ToString() ?? "Auto-Assigned";
            string route = row.Cells["Route"].Value?.ToString() ?? "Standard Route";
            string status = row.Cells["DeliveryStatus"].Value?.ToString() ?? "--";
            string attempts = row.Cells["AttemptCount"].Value?.ToString() ?? "0";
            string assigned = row.Cells["AssignedDate"].Value?.ToString() ?? "--";
            string lastAttempt = row.Cells["LastAttemptDate"].Value?.ToString() ?? "--";
            string notes = row.Cells["Notes"].Value?.ToString() ?? "No notes";
            string receiver = row.Cells["ReceiverName"].Value?.ToString() ?? "--";

            // Parse dates
            DateTime.TryParse(assigned, out DateTime assignedDt);
            DateTime.TryParse(lastAttempt, out DateTime lastDt);

            string assignedStr = assignedDt != DateTime.MinValue ? assignedDt.ToString("dd MMM yyyy, HH:mm") : "--";
            string lastAttemptStr = lastDt != DateTime.MinValue ? lastDt.ToString("dd MMM yyyy, HH:mm") : "No attempts yet";

            // Calculate expected delivery date based on status
            int daysToAdd = status == "Assigned" ? 3 : status == "In Progress" ? 2 : status == "Failed" ? 4 : 0;
            string expectedDelivery = assignedDt != DateTime.MinValue && daysToAdd > 0
                ? assignedDt.AddDays(daysToAdd).ToString("dd MMM yyyy")
                : status == "Completed" ? "Delivered" : "--";

            // Build the detail panel
            pnlDetail.Controls.Clear();
            pnlDetail.Height = 320;
            pnlDetail.Visible = true;

            int pw = pnlDetail.ClientSize.Width;

            // Red header bar
            Panel headerBar = new Panel { Location = new Point(0, 0), Size = new Size(pw, 44), BackColor = Red };
            headerBar.Controls.Add(new Label
            {
                Text = "Delivery: " + trackID + "   |   Status: " + status + "   |   Driver: " + driver,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 11),
                Size = new Size(pw - 24, 22),
                BackColor = Color.Transparent
            });
            pnlDetail.Controls.Add(headerBar);

            // ---- SECTION 1: Driver and Route ----
            DS(pnlDetail, "Driver and Route Information", 12, 54);
            DF(pnlDetail, "Driver Name", driver, 12, 78);
            DF(pnlDetail, "Route", route, 220, 78);
            DF(pnlDetail, "Delivery ID", delID, 440, 78);
            DF(pnlDetail, "Receiver Name", receiver, 660, 78);

            // ---- SECTION 2: Timeline ----
            DS(pnlDetail, "Delivery Timeline", 12, 132);
            DF(pnlDetail, "Assigned Date", assignedStr, 12, 156);
            DF(pnlDetail, "Expected Delivery", expectedDelivery, 220, 156);
            DF(pnlDetail, "Delivery Attempts", attempts + " attempt(s)", 440, 156);
            DF(pnlDetail, "Last Attempt Date", lastAttemptStr, 660, 156);

            // ---- SECTION 3: Status explanation ----
            DS(pnlDetail, "Current Status Explanation", 12, 210);

            // Colour coded status explanation box
            Color boxBg = status == "Completed" ? Color.FromArgb(230, 255, 240) :
                             status == "Failed" ? Color.FromArgb(255, 230, 230) :
                             status == "In Progress" ? Color.FromArgb(230, 240, 255) :
                             Color.FromArgb(255, 248, 220);
            Color boxFg = status == "Completed" ? Color.FromArgb(10, 100, 40) :
                             status == "Failed" ? Color.FromArgb(160, 20, 20) :
                             status == "In Progress" ? Color.FromArgb(20, 60, 160) :
                             Color.FromArgb(140, 80, 0);

            Panel statusBox = new Panel { Location = new Point(12, 232), Size = new Size(pw - 400, 60), BackColor = boxBg };
            statusBox.Controls.Add(new Label
            {
                Text = GetStatusExplanation(status, assignedStr, expectedDelivery),
                Font = new Font("Segoe UI", 10),
                ForeColor = boxFg,
                Location = new Point(10, 10),
                Size = new Size(pw - 424, 42),
                BackColor = Color.Transparent
            });
            pnlDetail.Controls.Add(statusBox);

            // Notes box
            if (!string.IsNullOrEmpty(notes) && notes != "No notes")
            {
                Panel noteBox = new Panel { Location = new Point(12, 296), Size = new Size(pw - 400, 16), BackColor = Color.White };
                noteBox.Controls.Add(new Label
                {
                    Text = "Driver notes: " + notes,
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(0, 0),
                    Size = new Size(pw - 424, 16),
                    BackColor = Color.Transparent
                });
                pnlDetail.Controls.Add(noteBox);
            }

            // ---- Visual progress tracker on the right ----
            Panel progress = new Panel { Location = new Point(pw - 360, 54), Size = new Size(340, 260), BackColor = Color.White };
            progress.Controls.Add(new Label { Text = "Delivery Progress", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 8), Size = new Size(320, 20), BackColor = Color.White });

            string[] stages = { "Assigned", "In Progress", "Out for Delivery", "Completed" };
            string[] labels = { "Order assigned to driver", "Parcel collected and in transit", "Driver is on the way to you", "Successfully delivered" };

            // Map current status to stage index
            int cur = status == "Assigned" ? 0 :
                      status == "In Progress" ? 1 :
                      status == "Out for Delivery" ? 2 :
                      status == "Completed" ? 3 :
                      status == "Failed" ? 1 : -1;

            for (int i = 0; i < stages.Length; i++)
            {
                bool done = i <= cur && cur >= 0;
                bool current = i == cur;

                // Circle indicator
                Panel circle = new Panel { Location = new Point(10, 38 + i * 54), Size = new Size(24, 24) };
                circle.Paint += (s2, ev) =>
                {
                    ev.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Color fillCol = done ? (current ? Red : Green) : Color.FromArgb(210, 210, 210);
                    ev.Graphics.FillEllipse(new SolidBrush(fillCol), 0, 0, 23, 23);
                    if (done)
                        ev.Graphics.DrawString(current ? ">" : "OK", new Font("Segoe UI", 7, FontStyle.Bold), Brushes.White, current ? 5 : 2, 7);
                };
                progress.Controls.Add(circle);

                // Connecting line between circles
                if (i < stages.Length - 1)
                {
                    bool lineD = i < cur && cur >= 0;
                    progress.Controls.Add(new Panel
                    {
                        Location = new Point(21, 62 + i * 54),
                        Size = new Size(2, 30),
                        BackColor = lineD ? Green : Color.FromArgb(210, 210, 210)
                    });
                }

                // Stage label and description
                progress.Controls.Add(new Label
                {
                    Text = stages[i],
                    Font = new Font("Segoe UI", 9, current ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = done ? (current ? Red : Green) : Color.LightGray,
                    Location = new Point(42, 36 + i * 54),
                    Size = new Size(290, 18),
                    BackColor = Color.White
                });
                progress.Controls.Add(new Label
                {
                    Text = labels[i],
                    Font = new Font("Segoe UI", 8),
                    ForeColor = done ? Color.FromArgb(80, 80, 80) : Color.LightGray,
                    Location = new Point(42, 54 + i * 54),
                    Size = new Size(290, 16),
                    BackColor = Color.White
                });
            }

            // Failed indicator
            if (status == "Failed")
            {
                progress.Controls.Add(new Label
                {
                    Text = "Delivery Failed -- " + attempts + " attempt(s) made",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(160, 20, 20),
                    Location = new Point(10, 256),
                    Size = new Size(320, 20),
                    BackColor = Color.FromArgb(255, 230, 230)
                });
            }

            pnlDetail.Controls.Add(progress);
        }

        // Get a plain English explanation of the delivery status
        private string GetStatusExplanation(string status, string assignedStr, string expectedDelivery)
        {
            switch (status)
            {
                case "Assigned":
                    return "Your parcel has been assigned to a driver and is awaiting collection from the PostalMS depot.\nAssigned: " + assignedStr + ". Expected delivery: " + expectedDelivery;
                case "In Progress":
                    return "Your parcel has been collected and is currently moving through our delivery network towards the destination.\nExpected delivery: " + expectedDelivery;
                case "Out for Delivery":
                    return "Your parcel is out for delivery today. The driver is on their way to the delivery address.\nPlease make sure someone is available to receive it.";
                case "Completed":
                    return "Your parcel has been successfully delivered. Thank you for using PostalMS.\nIf you did not receive it please contact us immediately at support@postalms.com.";
                case "Failed":
                    return "Delivery was attempted but was unsuccessful. Our driver was unable to complete the delivery.\nA re-attempt will be made within 1-2 working days. You may also request a refund.";
                case "Return to Depot":
                    return "After 3 failed delivery attempts your parcel has been returned to the depot.\nPlease contact us to arrange collection or redelivery.";
                default:
                    return "Current status: " + status + ". Expected delivery: " + expectedDelivery + ".";
            }
        }

        // Add a section heading with red underline
        private void DS(Panel p, string title, int x, int y)
        {
            int pw = p.ClientSize.Width;
            p.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(x, y),
                Size = new Size(400, 18),
                BackColor = Color.White
            });
            p.Controls.Add(new Panel
            {
                Location = new Point(x, y + 18),
                Size = new Size(pw - x - 380, 1),
                BackColor = Color.FromArgb(230, 180, 180)
            });
        }

        // Add a label and value pair
        private void DF(Panel p, string label, string value, int x, int y)
        {
            p.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(x, y),
                Size = new Size(200, 16),
                BackColor = Color.White
            });
            p.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 80),
                Location = new Point(x, y + 16),
                Size = new Size(200, 22),
                BackColor = Color.White
            });
        }

        // Colour code the status column in the grid
        private void StatusFmt(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var g = sender as DataGridView;
            if (g == null || e.ColumnIndex < 0 || e.ColumnIndex >= g.Columns.Count) return;
            if (g.Columns[e.ColumnIndex].Name != "DeliveryStatus" || e.Value == null) return;

            switch (e.Value.ToString())
            {
                case "In Progress": e.CellStyle.ForeColor = Color.FromArgb(140, 80, 0); e.CellStyle.BackColor = Color.FromArgb(255, 243, 220); break;
                case "Completed": e.CellStyle.ForeColor = Color.FromArgb(20, 110, 50); e.CellStyle.BackColor = Color.FromArgb(210, 248, 225); break;
                case "Assigned": e.CellStyle.ForeColor = Color.FromArgb(50, 80, 160); e.CellStyle.BackColor = Color.FromArgb(220, 232, 255); break;
                case "Failed": e.CellStyle.ForeColor = Color.FromArgb(160, 30, 30); e.CellStyle.BackColor = Color.FromArgb(255, 220, 220); break;
                case "Out for Delivery": e.CellStyle.ForeColor = Color.FromArgb(80, 45, 160); e.CellStyle.BackColor = Color.FromArgb(235, 225, 255); break;
                case "Return to Depot": e.CellStyle.ForeColor = Color.FromArgb(100, 40, 0); e.CellStyle.BackColor = Color.FromArgb(255, 235, 200); break;
            }
            e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }
    }
}