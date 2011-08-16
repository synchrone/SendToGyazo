using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.IO;
using WebHelper;
using System.Security;
using System.Security.Cryptography;
using System.Configuration;


namespace SendToGyazo
{
    static class Program
    {
        static NotifyIcon i = new NotifyIcon()
        {
            Text = "SendToGyazo",
            Icon = SendToGyazo.Properties.Resources.icon,
            Visible = true
        };
        static string postURL = "http://gyazo.com/upload.cgi";
        static string userAgent = "Gyazowin/1.0";

        [STAThread]
        static void Main(string[] args)
        {
            Stream imgStream;
            string msg = "";
            if (args.Length != 1 && Clipboard.ContainsImage())
            {
                imgStream = new MemoryStream();
                Clipboard.GetImage().Save(imgStream, System.Drawing.Imaging.ImageFormat.Png);
                imgStream.Seek(0, SeekOrigin.Begin);
                msg = "Got data from clipboard";
            }
            else if (args.Length == 1)
            {
                // Read file data
                imgStream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
                
                msg = "Got data from cmdline arguments";
            }
            else {
                return;
            }

            byte[] data = new byte[imgStream.Length];
            imgStream.Read(data, 0, data.Length);
            imgStream.Close();
            i.ShowBalloonTip(5000, "Uploading image", msg, ToolTipIcon.Info);
            

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("id", new SHA1CryptoServiceProvider().ComputeHash(data));
            postParameters.Add("imagedata", new WebHelpers.FileItem("gyazo.com", data));

            System.Net.ServicePointManager.Expect100Continue = false; //okay .net that's silly
            HttpWebResponse webResponse = WebHelpers.MultipartFormDataPost(postURL, userAgent, postParameters,
                "----BOUNDARYBOUNDARY----"
            );
            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            responseReader.Close();
            webResponse.Close();


            EventHandler hndlr = delegate
            {
                Clipboard.SetText(fullResponse);
                Application.Exit();
            };

            i.DoubleClick += hndlr;
            i.BalloonTipClicked += hndlr;

            i.ShowBalloonTip(5000, "Image sent to gyazo.com", "Click balloon or Doubleclick icon to copy it's url", ToolTipIcon.Info);
            Application.Run();
        }
    }
}
