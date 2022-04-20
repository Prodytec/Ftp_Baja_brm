﻿using System;
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
    public class Funciones
    {
        SqlConnection cnn = BD.DbConnection.getDBConnection();
        public void leercsv()
        {
            string[] lineas = File.ReadAllLines("C:/Users/DELL/Desktop/Visual Studio/archivo_brm/Productos_19-04-2022_10-40-35.csv");

            foreach (var linea in lineas)
            {
                var valores = linea.Split(',');
                SqlCommand cmd = new SqlCommand("sp_proyectocolor_stock", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigo", valores[0]);
                cmd.Parameters.AddWithValue("@stock", valores[5]);
                cmd.ExecuteNonQuery();
            }
            //SqlCommand delete = new SqlCommand("delete from stock_pcolor", cnn);
            //delete.ExecuteNonQuery();
        }
        public void Bajada()
        {
            
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
                string fileUrl = Hostbaja + name;

                if (permissions[0] == 'd')
                {
                    if (!Directory.Exists(localFilePath))
                    {
                        Directory.CreateDirectory(localFilePath);
                    }
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
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Hostbaja + name);
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                    request.Credentials = new NetworkCredential(usuario, pass);

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {

                    }
                }
            }
        }
    }
}
