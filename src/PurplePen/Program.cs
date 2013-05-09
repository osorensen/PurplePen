/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using CrashReporterDotNET;

namespace PurplePen
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Enable crash reporting.
            Application.ThreadException += (sender, e) => SendCrashReport(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                    SendCrashReport((Exception)e.ExceptionObject);
                    Environment.Exit(0);
                };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitUILanguage();

            if (args.Length > 0 && LoadCommandLineFile(args[0])) {
                // We successfully loaded a file from the command line.
                // Nothing more to do here.
            }
            else {
                // No command line args. Show initial screen to load/create an event.
                new InitialScreen().Show();
            }

            Application.Run();
        }

        // Initialize the UI language. If there is no language set, keep with the default language.
        static void InitUILanguage()
        {
            string uiLanguage = Settings.Default.UILanguage;

            if (!string.IsNullOrEmpty(uiLanguage)) {
                try {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(uiLanguage);
                }
                catch (Exception) { }        // Ignore problem -- e.g. this culture name isn't supported.
            }
        }

        // Attempt to load file from a command line file. Return true on success.
        static bool LoadCommandLineFile(string filename)
        {
            MainFrame mainFrame = new MainFrame();
            Controller controller = new Controller(mainFrame);

            if (!controller.LoadInitialFile(filename, true)) {
                // File didn't load. 
                // Go back and show the initial screen again.
                mainFrame.Dispose();
                return false;
            }

            // Start the UI
            mainFrame.Show();
            return true;
        }

        // Put up dialog to send crash report.
        private static void SendCrashReport(Exception exception)
        {
            var reportCrash = new ReportCrash {
                FromEmail = "crashreporting@purple-pen.org",
                ToEmail = "crashreport@purple-pen.org",
                SmtpHost = "mail.purple-pen.org",
                Port = 587,
                UserName = "crashreporting@purple-pen.org",
                Password = "PurplePen",
                EnableSSL = false,
            };

            reportCrash.Send(exception);
        }

    }
}