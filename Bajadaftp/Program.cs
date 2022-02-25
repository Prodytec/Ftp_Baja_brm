using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Bajadaftp.BD.DbConnection;

namespace Bajadaftp
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection cnn = BD.DbConnection.getDBConnection();
            DataTable ConfigData = new DataTable();
            string sql = "Select * From ecomm_expert_config";
            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ConfigData);

            string Hostbaja = ConfigData.Rows[0]["hostbaja"].ToString();
            string usuario = ConfigData.Rows[0]["usuario"].ToString();
            string pass = ConfigData.Rows[0]["contraseña"].ToString();
            string archivo = ConfigData.Rows[0]["CARPETA_ORDENES"].ToString();


            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(Hostbaja);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            NetworkCredential credentials = new NetworkCredential(usuario, pass);
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

                string localFilePath = Path.Combine(archivo, name);
                string fileUrl = Hostbaja  + name ;

                if (permissions[0] == 'd')
                {
                    if (!Directory.Exists(localFilePath))
                    {
                        Directory.CreateDirectory(localFilePath);
                    }
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Hostbaja);
                    request.Method = WebRequestMethods.Ftp.DeleteFile;

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Console.WriteLine("Delete status: {0}", response.StatusDescription);
                    response.Close();

                }
                else
                {
                    FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                    downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    downloadRequest.Credentials = credentials;


                    using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                    using (Stream sourceStream = downloadResponse.GetResponseStream())
                    using (Stream targetStream = File.Create(localFilePath))
                    {
                        byte[] buffer = new byte[204800];
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
