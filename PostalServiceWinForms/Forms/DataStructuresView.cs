// DataStructuresView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Visual demonstration page for the three custom data structures.
// Shows Hash Table contents, BST in-order traversal and Queue state.

using System;
using System.Drawing;
using System.Windows.Forms;
using PostalServiceWinForms.DataStructures;

namespace PostalServiceWinForms.Forms
{
    public class DataStructuresView : UserControl
    {
        private DatabaseHelper db;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color Green = Color.FromArgb(20, 130, 65);
        private Color Gold = Color.FromArgb(160, 100, 0);
        private RichTextBox rtbHash, rtbBST, rtbQueue;
        private TextBox txtHashKey;
        private Label lblHashTime, lblBSTTime, lblQueueTime;

        public DataStructuresView(DatabaseHelper dbHelper)
        {
            db = dbHelper; this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245); this.AutoScroll = true;
            Build();
        }

        private void Build()
        {
            this.Controls.Add(new Label { Text = "Data Structures", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Red, Location = new Point(0, 0), Size = new Size(500, 40) });
            this.Controls.Add(new Label { Text = "Live demonstration of custom Hash Table O(1), BST O(log n) and Queue O(1) -- all built from scratch, no STL", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(0, 42), Size = new Size(1000, 22) });

            int y = 74;

            // -- HASH TABLE
            Panel hp = Sec("  Hash Table -- O(1) Average Lookup", y, Color.FromArgb(255, 240, 240), Red); y += 290; this.Controls.Add(hp);
            hp.Controls.Add(new Label { Text = "Search by Tracking ID:", Font = new Font("Segoe UI", 10), ForeColor = Red, Location = new Point(15, 40), Size = new Size(190, 22), BackColor = Color.Transparent });
            txtHashKey = new TextBox { Location = new Point(210, 38), Size = new Size(280, 30), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, Text = "PS-2026-00001" };
            hp.Controls.Add(txtHashKey);
            Btn("Search", 500, 36, Red, hp).Click += BtnHashSearch;
            Btn("List All", 655, 36, Color.White, hp, Red).Click += BtnHashAll;
            lblHashTime = new Label { Text = "Ready -- " + db.ParcelHashTable.Count + " items loaded", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Red, Location = new Point(15, 76), Size = new Size(600, 20), BackColor = Color.Transparent };
            hp.Controls.Add(lblHashTime);
            rtbHash = RTB(hp, 15, 100, 830, 160, Color.FromArgb(30, 10, 10), Color.FromArgb(255, 160, 160));
            rtbHash.Text = "// Hash Table ready\n// " + db.ParcelHashTable.Count + " parcels loaded at startup\n// Click 'Search' or 'List All' to see it working";

            // -- BST
            Panel bp = Sec("  Binary Search Tree -- O(log n) Sorted Access", y, Color.FromArgb(228, 248, 235), Green); y += 290; this.Controls.Add(bp);
            Btn("In-Order Traversal", 15, 36, Green, bp).Click += BtnBSTTraversal;
            Btn("Find Minimum", 200, 36, Color.White, bp, Green).Click += BtnBSTMin;
            lblBSTTime = new Label { Text = "Ready -- " + db.ParcelBST.Count + " nodes", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Green, Location = new Point(15, 76), Size = new Size(600, 20), BackColor = Color.Transparent };
            bp.Controls.Add(lblBSTTime);
            rtbBST = RTB(bp, 15, 100, 830, 160, Color.FromArgb(16, 36, 24), Color.FromArgb(100, 220, 120));
            rtbBST.Text = "// BST ready -- " + db.ParcelBST.Count + " nodes\n// In-order traversal returns parcels sorted by TrackingID";

            // -- QUEUE
            Panel qp = Sec("  Custom Queue -- O(1) FIFO Delivery Processing", y, Color.FromArgb(255, 248, 220), Gold); y += 290; this.Controls.Add(qp);
            Btn("View Queue", 15, 36, Gold, qp).Click += BtnQueueView;
            Btn("Peek Front", 165, 36, Color.White, qp, Gold).Click += BtnQueuePeek;
            Btn("Dequeue", 310, 36, Color.White, qp, Gold).Click += BtnQueueDeq;
            lblQueueTime = new Label { Text = "Ready -- " + db.DeliveryQueue.Count + " items", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Gold, Location = new Point(15, 76), Size = new Size(600, 20), BackColor = Color.Transparent };
            qp.Controls.Add(lblQueueTime);
            rtbQueue = RTB(qp, 15, 100, 830, 160, Color.FromArgb(38, 28, 10), Color.FromArgb(255, 200, 80));
            rtbQueue.Text = "// Queue ready -- " + db.DeliveryQueue.Count + " pending deliveries\n// FIFO: First In First Out";

            // Complexity table
            this.Controls.Add(CTable(y));
        }

        private void BtnHashSearch(object sender, EventArgs e)
        {
            string k = txtHashKey.Text.Trim().ToUpper();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            object r = db.ParcelHashTable.Search(k); sw.Stop();
            lblHashTime.Text = $"Elapsed: {sw.ElapsedTicks} ticks  |  O(1) average  |  Table: {db.ParcelHashTable.Count} items";
            rtbHash.Clear();
            rtbHash.AppendText("// HASH TABLE SEARCH -- O(1) average\n// ---------------------------------\n");
            rtbHash.AppendText($"Key:    \"{k}\"\nResult: {(r != null ? $"FOUND   ->  Status: {r}" : "NOT FOUND ")}\n\n");
            rtbHash.AppendText("// Hash function: h = (h * 31 + char) % tableSize\n// Collision: chaining (linked list)\n// Auto-resize at load factor > 75%");
        }

        private void BtnHashAll(object sender, EventArgs e)
        {
            var sorted = db.ParcelBST.InOrder();
            lblHashTime.Text = $"All {db.ParcelHashTable.Count} items  |  O(n) to display";
            rtbHash.Clear(); rtbHash.AppendText($"// ALL ITEMS IN HASH TABLE ({db.ParcelHashTable.Count} total)\n// -------------------------\n");
            foreach (var kv in sorted) rtbHash.AppendText($"  {kv.Key,-24}  ->  {kv.Value}\n");
            rtbHash.AppendText("\n// Each individual lookup is O(1)");
        }

        private void BtnBSTTraversal(object sender, EventArgs e)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew(); var sorted = db.ParcelBST.InOrder(); sw.Stop();
            lblBSTTime.Text = $"Traversal: {sw.ElapsedTicks} ticks  |  O(n) -- {sorted.Count} nodes visited";
            rtbBST.Clear(); rtbBST.AppendText($"// BST IN-ORDER TRAVERSAL -- O(n)\n// {sorted.Count} parcels sorted by TrackingID\n// -------------------------------------\n");
            int i = 1; foreach (var kv in sorted) rtbBST.AppendText($"{i++,3}. {kv.Key,-24}  [{kv.Value}]\n");
            rtbBST.AppendText("\n// Left -> Root -> Right -> always ascending");
        }

        private void BtnBSTMin(object sender, EventArgs e)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew(); var min = db.ParcelBST.FindMin(); sw.Stop();
            lblBSTTime.Text = $"FindMin: {sw.ElapsedTicks} ticks  |  O(log n)";
            rtbBST.Clear(); rtbBST.AppendText("// BST FIND MINIMUM -- O(log n)\n// Follow left child pointers\n// ---------------------------------\n\n");
            if (min != null) { rtbBST.AppendText($"Minimum key: {min.Key}\nValue:       {min.Value}\n"); rtbBST.AppendText("\n// Traversed left pointers to reach smallest key"); }
            else rtbBST.AppendText("// Tree is empty");
        }

        private void BtnQueueView(object sender, EventArgs e)
        {
            var items = db.DeliveryQueue.ToList();
            lblQueueTime.Text = $"Queue: {items.Count} items  |  O(n) display  |  Enqueue/Dequeue O(1)";
            rtbQueue.Clear(); rtbQueue.AppendText($"// QUEUE CONTENTS -- FIFO ({items.Count} items)\n// -------------------------------------\n");
            if (items.Count == 0) { rtbQueue.AppendText("// Queue empty -- send a parcel to add to queue"); return; }
            for (int i = 0; i < items.Count; i++) rtbQueue.AppendText($"  {i + 1,3}   {items[i],-25}{(i == 0 ? "<- FRONT" : i == items.Count - 1 ? "<- REAR" : "")}\n");
            rtbQueue.AppendText("\n// Enqueue adds to REAR O(1)  |  Dequeue removes from FRONT O(1)");
        }

        private void BtnQueuePeek(object sender, EventArgs e)
        {
            rtbQueue.Clear(); rtbQueue.AppendText("// QUEUE PEEK -- O(1)\n// -----------------------------\n\n");
            try { var sw = System.Diagnostics.Stopwatch.StartNew(); object f = db.DeliveryQueue.Peek(); sw.Stop(); lblQueueTime.Text = $"Peek: {sw.ElapsedTicks} ticks  |  O(1)"; rtbQueue.AppendText($"Next to process: {f}\nQueue size unchanged: {db.DeliveryQueue.Count}"); }
            catch { rtbQueue.AppendText("// Queue is empty"); lblQueueTime.Text = "Queue empty"; }
        }

        private void BtnQueueDeq(object sender, EventArgs e)
        {
            rtbQueue.Clear(); rtbQueue.AppendText("// DEQUEUE -- O(1)\n// -----------------------------\n\n");
            try { var sw = System.Diagnostics.Stopwatch.StartNew(); object r = db.DeliveryQueue.Dequeue(); sw.Stop(); lblQueueTime.Text = $"Dequeue: {sw.ElapsedTicks} ticks  |  O(1)  |  Remaining: {db.DeliveryQueue.Count}"; rtbQueue.AppendText($"Removed: {r}\nRemaining: {db.DeliveryQueue.Count}\n\n// Front pointer moved to next node"); }
            catch { rtbQueue.AppendText("// Queue is empty"); lblQueueTime.Text = "Queue empty"; }
        }

        private Panel Sec(string title, int y, Color bg, Color accent)
        {
            Panel p = new Panel { Location = new Point(0, y), Size = new Size(862, 282), BackColor = bg };
            p.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 282), BackColor = accent });
            p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = accent, Location = new Point(14, 9), Size = new Size(840, 24), BackColor = Color.Transparent });
            return p;
        }

        private Button Btn(string t, int x, int y, Color bg, Panel p, Color? border = null)
        {
            Button b = new Button { Text = t, Location = new Point(x, y), Size = new Size(145, 30), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = bg, ForeColor = bg == Color.White ? (border ?? Red) : Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = bg == Color.White ? 1 : 0;
            if (bg == Color.White && border.HasValue) b.FlatAppearance.BorderColor = border.Value;
            p.Controls.Add(b); return b;
        }

        private RichTextBox RTB(Panel p, int x, int y, int w, int h, Color bg, Color fg)
        {
            RichTextBox r = new RichTextBox { Location = new Point(x, y), Size = new Size(w, h), Font = new Font("Courier New", 9), BackColor = bg, ForeColor = fg, ReadOnly = true, BorderStyle = BorderStyle.None };
            p.Controls.Add(r); return r;
        }

        private Panel CTable(int y)
        {
            Panel p = new Panel { Location = new Point(0, y), Size = new Size(862, 220), BackColor = Color.White };
            p.Controls.Add(new Label { Text = "  Time Complexity Summary", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 12), Size = new Size(500, 26) });
            Panel hdr = new Panel { Location = new Point(0, 44), Size = new Size(862, 30), BackColor = Red };
            string[] heads = { "Structure", "Operation", "Time Complexity", "Notes" };
            int[] xs = { 14, 140, 320, 490 };
            for (int i = 0; i < heads.Length; i++) hdr.Controls.Add(new Label { Text = heads[i], Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White, Location = new Point(xs[i], 8), Size = new Size(200, 16), BackColor = Color.Transparent });
            p.Controls.Add(hdr);
            string[,] rows = {
                {"Hash Table","Add",       "O(1) avg",     "O(n) worst -- all keys collide"},
                {"Hash Table","Search",    "O(1) avg",     "O(n) worst"},
                {"Hash Table","Delete",    "O(1) avg",     "O(n) worst"},
                {"Hash Table","Resize",    "O(n)",         "Triggered at load factor > 75%"},
                {"BST",       "Insert",    "O(log n) avg", "O(n) worst -- unbalanced tree"},
                {"BST",       "Search",    "O(log n) avg", "O(n) worst"},
                {"BST",       "In-Order",  "O(n)",         "Visits all nodes -- always sorted"},
                {"Queue",     "Enqueue",   "O(1)",         "Add to rear -- always constant"},
                {"Queue",     "Dequeue",   "O(1)",         "Remove from front -- always constant"},
                {"Queue",     "Peek",      "O(1)",         "Read front without removing"},
            };
            for (int i = 0; i < rows.GetLength(0); i++)
            {
                Panel row = new Panel { Location = new Point(0, 74 + i * 22), Size = new Size(862, 22), BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(255, 248, 248) };
                for (int j = 0; j < 4; j++)
                {
                    Color fc = j == 2 ? (rows[i, 2].Contains("O(1)") ? Green : rows[i, 2].Contains("O(log") ? Color.FromArgb(30, 90, 180) : Red) : Color.FromArgb(30, 40, 60);
                    row.Controls.Add(new Label { Text = rows[i, j], Font = new Font("Segoe UI", 8, j == 2 ? FontStyle.Bold : FontStyle.Regular), ForeColor = fc, Location = new Point(xs[j], 3), Size = new Size(200, 16) });
                }
                p.Controls.Add(row);
            }
            return p;
        }
    }
}
