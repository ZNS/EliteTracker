using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ZNS.EliteTracker.Controllers
{
    public class ExeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Esent()
        {
            // Get the full file path
            string strFilePath = Server.MapPath("/app_data/db/fix.bat");

            // Create the ProcessInfo object
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.WorkingDirectory = Server.MapPath("/app_data/db/");

            // Start the process
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);


            // Open the batch file for reading
            System.IO.StreamReader strm = System.IO.File.OpenText(strFilePath);

            // Attach the output for reading
            System.IO.StreamReader sOut = proc.StandardOutput;

            // Attach the in for writing
            System.IO.StreamWriter sIn = proc.StandardInput;


            // Write each line of the batch file to standard input
            while(strm.Peek() != -1)
            {
              sIn.WriteLine(strm.ReadLine());
            }

            strm.Close();

            // Exit CMD.EXE
            string stEchoFmt = "# {0} run successfully. Exiting";

            sIn.WriteLine(String.Format(stEchoFmt, strFilePath));
            sIn.WriteLine("EXIT");

            // Close the process
            proc.Close();

            // Read the sOut to a string.
            string results = sOut.ReadToEnd().Trim();


            // Close the io Streams;
            sIn.Close();
            sOut.Close();


            // Write out the results.
            string fmtStdOut = "<font face=courier size=0>{0}</font>";
            return Content(String.Format(fmtStdOut,results.Replace(System.Environment.NewLine, "<br>")), "text/html");
        }
    }
}