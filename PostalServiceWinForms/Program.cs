// Program.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
// Entry point for the WinForms application

using System;
using System.Windows.Forms;

namespace PostalServiceWinForms
{
    static class Program
    {
        // Application entry point
        // Initialises Windows Forms and launches the login screen
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start at the login form
            Application.Run(new LoginForm());
        }
    }
}
