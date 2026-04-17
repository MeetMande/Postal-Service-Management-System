// DeliveriesView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Deliveries page showing all delivery records linked to the user's parcels.
// Displays driver name, route, status, attempt count and notes.
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
        private Panel pnlBottom;
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
            // Controls are added in reverse order when using Dock
            // Add Fill control FIRST, then Top/Bottom after

            // -- BOTTOM detail panel --
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            this.Controls.Add(pnlBottom);

            // -- GRID (Fill) --
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
            dgv.CellClick += DgvCellClick;
            this.Controls.Add(dgv);

            // -- HEADER (Top) -- added last so it appears at top --
            Panel hdr = new Panel
            {
                Dock = DockStyle.Top,
                Height = 108,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            this.Controls.Add(hdr);

            hdr.Controls.Add(new Label
            {
                Text = "My Deliveries",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(0, 4),
                Size = new Size(400, 38)
            });
            hdr.Controls.Add(new Label
            {
                Text = "Click   View  on any row to see parcel details     Click any row for delivery status",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, 46),
                Size = new Size(900, 20)
            });

            // Filter buttons row
            hdr.Controls.Add(new Label
            {
                Text = "Filter:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, 74),
                Size = new Size(52, 26)
            });
            Button bAll = FB(hdr, "All", 56, () => LoadDeliveries("All"));
            Button bAss = FB(hdr, "Assigned", 145, () => LoadDeliveries("Assigned"));
            Button bInP = FB(hdr, "In Progress", 245, () => LoadDeliveries("In Progress"));
            Button bCom = FB(hdr, "Completed ", 374, () => LoadDeliveries("Completed"));
            Button bFail = FB(hdr, "Failed ", 512, () => LoadDeliveries("Failed"));
            Button bRet = FB(hdr, "Return to Depot", 616, () => LoadDeliveries("Return to Depot"));
            SetActive(bAll);
        }

        private Button FB(Panel p, string text, int x, Action click)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 72),
                Size = new Size(text.Length > 10 ? 136 : 88, 30),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.Click += (s, e) => { click(); SetActive(btn); };
            p.Controls.Add(btn);
            return btn;
        }

        private void SetActive(Button btn)
        {
            if (activeFilter != null) { activeFilter.BackColor = Color.White; activeFilter.ForeColor = Color.FromArgb(60, 60, 60); }
            if (btn != null) { btn.BackColor = Red; btn.ForeColor = Color.White; }
            activeFilter = btn;
        }

        private void LoadDeliveries(string filter)
        {
            pnlBottom.Visible = false;
            pnlBottom.Height = 0;
            pnlBottom.Controls.Clear();

            try
            {
                DataTable dt = filter == "All"
                    ? db.GetDeliveriesForUser(userID)
                    : db.GetDeliveriesForUserByStatus(userID, filter);

                if (!dt.Columns.Contains(" View"))
                    dt.Columns.Add(" View", typeof(string));
                foreach (DataRow row in dt.Rows)
                    row[" View"] = " View";

                dgv.DataSource = dt;

                if (dgv.Columns.Contains("AssignedDate"))
                    dgv.Columns["AssignedDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                if (dgv.Columns.Contains("LastAttemptDate"))
                    dgv.Columns["LastAttemptDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

                if (dgv.Columns.Contains(" View"))
                {
                    var col = dgv.Columns[" View"];
                    col.Width = 85;
                    col.FillWeight = 5;
                    col.DefaultCellStyle.ForeColor = Color.FromArgb(30, 80, 200);
                    col.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.BackColor = Color.FromArgb(235, 242, 255);
                }
            }
            catch { }
        }

        private void DgvCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name == " View")
            {
                string tid = dgv.Rows[e.RowIndex].Cells["TrackingID"].Value?.ToString() ?? "";
                if (!string.IsNullOrEmpty(tid)) ShowParcelPanel(tid);
            }
        }

        private void ShowParcelPanel(string tid)
        {
            pnlBottom.Controls.Clear();
            pnlBottom.Height = 258;
            pnlBottom.Visible = true;

            pnlBottom.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 5, BackColor = Red });

            Panel titleRow = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.FromArgb(255, 248, 248) };
            titleRow.Controls.Add(new Label { Text = "  Parcel Details -- " + tid, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 7), Size = new Size(700, 26) });
            AddCloseBtn(titleRow, () => { pnlBottom.Visible = false; pnlBottom.Height = 0; });
            pnlBottom.Controls.Add(titleRow);

            Panel body = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(255, 248, 248), Padding = new Padding(14, 8, 14, 8) };
            pnlBottom.Controls.Add(body);

            try
            {
                DataTable dt = db.SearchParcel(tid);
                if (dt.Rows.Count == 0)
                { body.Controls.Add(new Label { Text = "No parcel data found.", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(14, 20), Size = new Size(400, 26) }); return; }

                DataRow r = dt.Rows[0];
                void A(string l, string v, int x, int y)
                {
                    body.Controls.Add(new Label { Text = l, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(x, y), Size = new Size(170, 15) });
                    body.Controls.Add(new Label { Text = v, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(30, 30, 30), Location = new Point(x, y + 16), Size = new Size(195, 22) });
                }

                A("TRACKING ID", r["TrackingID"].ToString(), 0, 4);
                A("TYPE", r["ParcelType"].ToString(), 205, 4);
                A("SERVICE", r["ServiceType"].ToString(), 410, 4);
                A("STATUS", r["Status"].ToString(), 615, 4);
                A("DATE SENT", DateTime.TryParse(r["DateSent"].ToString(), out DateTime ds) ? ds.ToString("dd/MM/yyyy") : "--", 820, 4);
                A("PRICE", "GBP" + r["Price"], 1025, 4);
                A("SENDER", r["SenderName"].ToString(), 0, 58);
                A("SENDER ADDRESS", r["SenderAddress"].ToString(), 205, 58);
                A("RECEIVER", r["ReceiverName"].ToString(), 615, 58);
                A("RECEIVER ADDR", r["ReceiverAddress"].ToString(), 820, 58);
                A("WEIGHT", r["Weight"] + " kg", 0, 112);
                A("SIZE", r["Size"].ToString(), 205, 112);
                A("INTERNATIONAL", r["IsInternational"]?.ToString() == "True" ? "Yes -- " + r["DestinationCountry"] : "No (UK)", 410, 112);
                A("EST. DELIVERY", r["EstimatedDelivery"] == DBNull.Value ? "TBC" : DateTime.TryParse(r["EstimatedDelivery"].ToString(), out DateTime ed) ? ed.ToString("dd/MM/yyyy") : "TBC", 615, 112);

                Color bc = r["Status"].ToString() == "Delivered" ? Green : r["Status"].ToString() == "Failed" ? Color.FromArgb(160, 30, 30) : Red;
                body.Controls.Add(new Label { Text = "  " + r["Status"] + "  ", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White, BackColor = bc, Location = new Point(820, 112), AutoSize = true, Padding = new Padding(6, 3, 6, 3) });
            }
            catch (Exception ex)
            { body.Controls.Add(new Label { Text = "Error: " + ex.Message, Font = new Font("Segoe UI", 9), ForeColor = Color.Red, Location = new Point(14, 20), Size = new Size(800, 22) }); }
        }

        private void DgvSelect(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            if (dgv.CurrentCell != null && dgv.CurrentCell.ColumnIndex >= 0 &&
                dgv.CurrentCell.ColumnIndex < dgv.Columns.Count &&
                dgv.Columns[dgv.CurrentCell.ColumnIndex].Name == " View") return;

            pnlBottom.Controls.Clear();
            pnlBottom.Height = 226;
            pnlBottom.Visible = true;

            var row = dgv.CurrentRow;
            string status = row.Cells["DeliveryStatus"].Value?.ToString() ?? "--";
            string trackID = row.Cells["TrackingID"].Value?.ToString() ?? "--";
            string driver = row.Cells["DriverName"].Value?.ToString() ?? "Auto-Assigned";
            string notes = row.Cells["Notes"].Value?.ToString() ?? "No notes";
            int attempts = Convert.ToInt32(row.Cells["AttemptCount"].Value ?? 0);
            string lastAtt = row.Cells["LastAttemptDate"].Value?.ToString() ?? "";
            string rcv = row.Cells["ReceiverName"].Value?.ToString() ?? "--";
            string ptype = row.Cells["ParcelType"].Value?.ToString() ?? "--";

            pnlBottom.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 5, BackColor = Red });

            Panel titleRow = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.White };
            titleRow.Controls.Add(new Label { Text = $"  Delivery -- {trackID}  ({ptype})", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 7), Size = new Size(500, 26) });
            Button btnP = new Button { Text = "  View Parcel", Location = new Point(530, 6), Size = new Size(155, 28), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(30, 80, 180), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnP.FlatAppearance.BorderSize = 0;
            btnP.Click += (s2, e2) => ShowParcelPanel(trackID);
            titleRow.Controls.Add(btnP);
            AddCloseBtn(titleRow, () => { pnlBottom.Visible = false; pnlBottom.Height = 0; });
            pnlBottom.Controls.Add(titleRow);

            Panel body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14, 6, 14, 6) };
            pnlBottom.Controls.Add(body);

            Color sc = status == "Completed" ? Green : status == "Failed" || status == "Return to Depot" ? Color.FromArgb(170, 35, 35) : status == "In Progress" ? Color.FromArgb(160, 95, 15) : Color.FromArgb(50, 80, 160);
            body.Controls.Add(new Label { Text = "  " + status + "  ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, BackColor = sc, Location = new Point(0, 4), AutoSize = true, Padding = new Padding(8, 4, 8, 4) });

            void DR(string l, string v, int x, int y)
            {
                body.Controls.Add(new Label { Text = l, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(x, y), Size = new Size(175, 15) });
                body.Controls.Add(new Label { Text = v, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(40, 20, 20), Location = new Point(x, y + 16), Size = new Size(210, 22) });
            }

            DR("Tracking ID", trackID, 0, 44);
            DR("Receiver", rcv, 235, 44);
            DR("Driver", driver, 470, 44);
            DR("Attempts", attempts + " / 3", 705, 44);
            DR("Last Attempt", string.IsNullOrEmpty(lastAtt) ? "N/A" : lastAtt, 940, 44);
            DR("Notes", notes, 0, 104);

            string msg; Color mf;
            switch (status)
            {
                case "Completed": msg = "  Delivered successfully!"; mf = Green; break;
                case "In Progress": msg = "  Out for delivery -- expected today or next working day."; mf = Color.FromArgb(140, 80, 0); break;
                case "Failed": msg = attempts >= 3 ? "  Max attempts -- returning to depot. Contact help@postalms.com." : $"  Failed ({attempts} attempt(s)). Re-attempt in 1-2 days."; mf = Color.FromArgb(160, 30, 30); break;
                case "Return to Depot": msg = "  Returned to depot. Contact help@postalms.com."; mf = Color.FromArgb(130, 20, 20); break;
                case "Assigned": msg = "  Assigned -- awaiting collection."; mf = Color.FromArgb(40, 70, 160); break;
                default: msg = "(i)  Being processed."; mf = Color.FromArgb(60, 60, 60); break;
            }
            body.Controls.Add(new Label { Text = msg, Font = new Font("Segoe UI", 10), ForeColor = mf, Location = new Point(0, 148), Size = new Size(1240, 22) });
        }

        private void AddCloseBtn(Panel p, Action onClick)
        {
            Button b = new Button { Text = "", Size = new Size(32, 26), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.White, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Cursor = Cursors.Hand, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            b.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            b.Location = new Point(p.Width - 38, 6);
            b.Click += (s, e) => onClick();
            p.Controls.Add(b);
        }

        private void StatusFmt(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var g = sender as DataGridView;
            if (g == null || e.ColumnIndex < 0 || e.ColumnIndex >= g.Columns.Count || g.Columns[e.ColumnIndex].Name != "DeliveryStatus" || e.Value == null) return;
            switch (e.Value.ToString())
            {
                case "In Progress": e.CellStyle.ForeColor = Color.FromArgb(140, 80, 0); e.CellStyle.BackColor = Color.FromArgb(255, 243, 220); break;
                case "Completed": e.CellStyle.ForeColor = Color.FromArgb(20, 110, 50); e.CellStyle.BackColor = Color.FromArgb(210, 248, 225); break;
                case "Assigned": e.CellStyle.ForeColor = Color.FromArgb(50, 80, 160); e.CellStyle.BackColor = Color.FromArgb(220, 232, 255); break;
                case "Failed": e.CellStyle.ForeColor = Color.FromArgb(160, 30, 30); e.CellStyle.BackColor = Color.FromArgb(255, 220, 220); break;
                case "Return to Depot": e.CellStyle.ForeColor = Color.FromArgb(120, 20, 20); e.CellStyle.BackColor = Color.FromArgb(255, 200, 200); break;
            }
        }
    }
}
