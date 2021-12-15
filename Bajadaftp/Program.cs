using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bajadaftp
{
    class Program
    {
        static void Main(string[] args)
        {

            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create("ftp://BRM-ONLINE@mercadoexperts.com/CATALOGO/");
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            NetworkCredential credentials = new NetworkCredential("BRM-ONLINE@mercadoexperts.com", "NpSgFwh40q7M");
            listRequest.Credentials = credentials;

            List<string> lines = new List<string>();

            using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (string line in lines)
            {
                string[] tokens =
                line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[8];
                string permissions = tokens[0];

                string localFilePath = Path.Combine(@"C:\Users\DELL\Desktop\Visual Studio\archivo_brm", name);
                string fileUrl = "ftp://BRM-ONLINE@mercadoexperts.com/CATALOGO/"  + name ;

                if (permissions[0] == 'd')
                {
                    if (!Directory.Exists(localFilePath))
                    {
                        Directory.CreateDirectory(localFilePath);
                    }


                }
                else
                {
                    FtpWebRequest downloadRequest =
                        (FtpWebRequest)WebRequest.Create(fileUrl);
                    downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    downloadRequest.Credentials = credentials;


                    using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                    using (Stream sourceStream = downloadResponse.GetResponseStream())
                    using (Stream targetStream = File.Create(localFilePath))
                    {
                        byte[] buffer = new byte[10240];
                        int read;
                        while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            targetStream.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }
    }
}
