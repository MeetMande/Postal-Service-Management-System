// DeliveriesView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Deliveries page showing detailed tracking information for each parcel.
// Shows driver details, route, status timeline, attempt history and expected delivery.
// Filters by All, Assigned, In Transit and Delivered.

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
            // Detail panel at the bottom
            pnlDetail = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            this.Controls.Add(pnlDetail);

            // Grid in the middle
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
            dgv.RowTemplate.Height = 44;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 248, 248);
            dgv.SelectionChanged += DgvSelect;
            dgv.CellFormatting += StatusFmt;
            this.Controls.Add(dgv);

            // Header at the top
            Panel hdr = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.FromArgb(245, 245, 245) };
            this.Controls.Add(hdr);

            hdr.Controls.Add(new Label
            {
                Text = "Deliveries",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(0, 8),
                Size = new Size(300, 30)
            });
            hdr.Controls.Add(new Label
            {
                Text = "Track your parcels in real time. Click any row to see full delivery details, driver information and timeline.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, 40),
                Size = new Size(900, 20)
            });

            // Filter buttons
            int fx = 0;
            foreach (string f in new[] { "All", "Assigned", "In Transit", "Delivered" })
            {
                string filter = f;
                Button btn = new Button
                {
                    Text = f,
                    Location = new Point(fx, 68),
                    Size = new Size(130, 34),
                    Font = new Font("Segoe UI", 9),
                    BackColor = f == "All" ? Red : Color.White,
                    ForeColor = f == "All" ? Color.White : Red,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Tag = f
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(210, 190, 190);
                if (f == "All") activeFilter = btn;
                btn.Click += (s, e) =>
                {
                    if (activeFilter != null) { activeFilter.BackColor = Color.White; activeFilter.ForeColor = Red; }
                    btn.BackColor = Red; btn.ForeColor = Color.White;
                    activeFilter = btn;
                    LoadDeliveries(filter);
                };
                hdr.Controls.Add(btn);
                fx += 138;
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
            // Hide columns not needed in the grid view
            string[] hide = { "SenderName", "ParcelType" };
            foreach (string col in hide)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            // Format dates
            if (dgv.Columns.Contains("AssignedDate"))
                dgv.Columns["AssignedDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (dgv.Columns.Contains("LastAttemptDate"))
                dgv.Columns["LastAttemptDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        // Show detailed tracking info when a delivery row is clicked
        private void DgvSelect(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;

            var row = dgv.CurrentRow;

            // Get all field values
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

            // Parse dates for display
            DateTime assignedDate;
            bool hasAssigned = DateTime.TryParse(assigned, out assignedDate);
            string assignedStr = hasAssigned ? assignedDate.ToString("dd MMM yyyy, HH:mm") : "--";

            // Calculate expected delivery based on status and assigned date
            string expectedDelivery = "--";
            if (hasAssigned)
            {
                DateTime expected = assignedDate.AddDays(3);
                expectedDelivery = expected.ToString("dd MMM yyyy");
            }

            // Build the detail panel
            pnlDetail.Controls.Clear();
            pnlDetail.Size = new Size(this.Width, 280);
            pnlDetail.Visible = true;

            // Header bar
            Panel headerBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(pnlDetail.Width, 40),
                BackColor = Red
            };
            headerBar.Controls.Add(new Label
            {
                Text = "Delivery Details -- " + trackID + "   |   Status: " + status,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10),
                Size = new Size(800, 22),
                BackColor = Color.Transparent
            });
            pnlDetail.Controls.Add(headerBar);

            // Section: Driver and Route
            AddDetailSection(pnlDetail, "Driver Information", 12, 52);
            AddDetailField(pnlDetail, "Driver Name", driver, 12, 76);
            AddDetailField(pnlDetail, "Route", route, 220, 76);
            AddDetailField(pnlDetail, "Delivery ID", delID, 440, 76);
            AddDetailField(pnlDetail, "Receiver", receiver, 660, 76);

            // Section: Tracking Timeline
            AddDetailSection(pnlDetail, "Tracking Timeline", 12, 120);
            AddDetailField(pnlDetail, "Assigned Date", assignedStr, 12, 144);
            AddDetailField(pnlDetail, "Expected Delivery", expectedDelivery, 220, 144);
            AddDetailField(pnlDetail, "Attempt Count", attempts + " attempt(s)", 440, 144);
            AddDetailField(pnlDetail, "Last Attempt",
                lastAttempt != "--" && DateTime.TryParse(lastAttempt, out DateTime la)
                    ? la.ToString("dd MMM yyyy, HH:mm") : "--",
                660, 144);

            // Section: Current Location / Status
            AddDetailSection(pnlDetail, "Current Status", 12, 188);

            // Status explanation based on current status
            string statusExplanation = GetStatusExplanation(status, assignedStr, expectedDelivery);
            pnlDetail.Controls.Add(new Label
            {
                Text = statusExplanation,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(30, 30, 80),
                Location = new Point(12, 210),
                Size = new Size(900, 40),
                BackColor = Color.White
            });

            // Notes
            if (notes != "No notes" && !string.IsNullOrEmpty(notes))
            {
                pnlDetail.Controls.Add(new Label
                {
                    Text = "Notes: " + notes,
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(12, 252),
                    Size = new Size(900, 20),
                    BackColor = Color.White
                });
            }

            // Status progress tracker
            string[] stages = { "Assigned", "In Transit", "Out for Delivery", "Delivered" };
            int cur = Array.IndexOf(stages, status);
            for (int i = 0; i < stages.Length; i++)
            {
                bool done = i <= cur;
                pnlDetail.Controls.Add(new Label
                {
                    Text = (done ? ">> " : "   ") + stages[i],
                    Font = new Font("Segoe UI", 9, done ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = done ? Green : Color.LightGray,
                    Location = new Point(950 + i * 200, 80),
                    Size = new Size(195, 22),
                    BackColor = Color.White
                });
            }
        }

        // Get a human readable status explanation
        private string GetStatusExplanation(string status, string assignedStr, string expectedDelivery)
        {
            switch (status)
            {
                case "Assigned":
                    return "Your parcel has been assigned to a driver and is being prepared for collection. Assigned: " + assignedStr;
                case "In Transit":
                    return "Your parcel is currently on its way. It is moving through our delivery network towards the destination. Expected delivery: " + expectedDelivery;
                case "Out for Delivery":
                    return "Your parcel is out for delivery today. The driver is on their way to the delivery address. Please ensure someone is available to receive it.";
                case "Delivered":
                    return "Your parcel has been successfully delivered. If you did not receive it please contact us immediately.";
                case "Failed":
                    return "Delivery was attempted but was unsuccessful. Please check the notes for more information. You can request a redelivery or refund.";
                default:
                    return "Status: " + status + ". Expected delivery: " + expectedDelivery;
            }
        }

        // Helper: add a section label
        private void AddDetailSection(Panel p, string title, int x, int y)
        {
            p.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(x, y),
                Size = new Size(300, 18),
                BackColor = Color.White
            });
            p.Controls.Add(new Panel
            {
                Location = new Point(x, y + 18),
                Size = new Size(p.Width - x - 12, 1),
                BackColor = Color.FromArgb(230, 180, 180)
            });
        }

        // Helper: add a label + value pair
        private void AddDetailField(Panel p, string label, string value, int x, int y)
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
            if (g == null || e.ColumnIndex < 0 || e.ColumnIndex >= g.Columns.Count || g.Columns[e.ColumnIndex].Name != "DeliveryStatus" || e.Value == null) return;
            switch (e.Value.ToString())
            {
                case "In Transit": e.CellStyle.ForeColor = Color.FromArgb(140, 80, 0); e.CellStyle.BackColor = Color.FromArgb(255, 243, 220); break;
                case "Delivered": e.CellStyle.ForeColor = Color.FromArgb(20, 110, 50); e.CellStyle.BackColor = Color.FromArgb(210, 248, 225); break;
                case "Assigned": e.CellStyle.ForeColor = Color.FromArgb(50, 80, 160); e.CellStyle.BackColor = Color.FromArgb(220, 232, 255); break;
                case "Failed": e.CellStyle.ForeColor = Color.FromArgb(160, 30, 30); e.CellStyle.BackColor = Color.FromArgb(255, 220, 220); break;
                case "Out for Delivery": e.CellStyle.ForeColor = Color.FromArgb(80, 45, 160); e.CellStyle.BackColor = Color.FromArgb(235, 225, 255); break;
            }
        }
    }
}