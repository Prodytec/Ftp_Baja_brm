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
    public class Funciones
    {
        SqlConnection cnn = BD.DbConnection.getDBConnection();
        public void leercsv()
        {
            DataTable ConfigData = new DataTable();
            string sql = "Select * From ecomm_expert_config";
            SqlCommand cmd = new SqlCommand(sql, cnn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ConfigData);
            string archivo = ConfigData.Rows[0]["CARPETA_ORDENES"].ToString();
            string directorio = archivo + "Procesado\\";
            string[] ruta = Directory.GetFiles(archivo);
            Log L = new Log(directorio);


            foreach (var linea in ruta)
            {
                string[] csv = File.ReadAllLines(linea);
                string filename = Path.GetFileName(linea);

                L.Add("--");
                L.Add("Procesando..");
                L.Add(filename);

                foreach (var line in csv)
                {
                    
                    var valores = line.Split('|');
                    int valorI = 4;
                    if (Convert.ToInt16(valores[1].ToString()) == valorI)
                    {
                        SqlCommand cmdp = new SqlCommand("sp_proyectocolor_stock", cnn);
                        cmdp.CommandType = CommandType.StoredProcedure;
                        cmdp.Parameters.AddWithValue("@idorden", valores[0]);
                        cmdp.Parameters.AddWithValue("@codigo", valores[3].Trim());
                        cmdp.Parameters.AddWithValue("@cantidad", valores[5]);
                        cmdp.ExecuteNonQuery();
                    }
                }
                foreach (var line in csv)
                {
                    var valores = line.Split('|');
                    int valorI = 4;
                    int valor = 3;
                    if (Convert.ToInt16(valores[1].ToString()) == valor && valores[2] == "cancelled")
                    {

                        SqlCommand cmdu = new SqlCommand("sp_proyectocolor_ecomm_update", cnn);
                        cmdu.Parameters.AddWithValue("@idorden", valores[0]);
                        cmdu.Parameters.AddWithValue("@seleccion", 2);
                        cmdu.CommandType = CommandType.StoredProcedure;
                        cmdu.ExecuteNonQuery();

                        SqlCommand listo = new SqlCommand(" update Ecommstock set procesado = 1 where idorden =" + valores[0], cnn);
                        listo.ExecuteNonQuery();
                    }
                    else if (Convert.ToInt16(valores[1].ToString()) == valorI)
                    {
                        SqlCommand cmdu = new SqlCommand("sp_proyectocolor_ecomm_update", cnn);
                        cmdu.Parameters.AddWithValue("@idorden", valores[0]);
                        cmdu.Parameters.AddWithValue("@seleccion", 1);
                        cmdu.CommandType = CommandType.StoredProcedure;
                        cmdu.ExecuteNonQuery();

                        SqlCommand update = new SqlCommand(" update Ecommstock set procesado = 1 where idorden =" + valores[0], cnn);
                        update.ExecuteNonQuery();
                    }
                }
                L.Add("Procesado"+ " "+ filename);
            }

            
            foreach(string s in ruta)
            {
                string filename = Path.GetFileName(s);
                string destfile = Path.Combine(directorio, filename);
                File.Move(s, destfile);
            }

            L.Add("----------------Terminado---------------" + "\n");


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

                    Uri serverFile = new Uri(fileUrl);
                    FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(serverFile);
                    reqFTP.Method = WebRequestMethods.Ftp.Rename;
                    reqFTP.Credentials = new NetworkCredential(usuario, pass);
                    reqFTP.RenameTo = "Procesado/" + name;
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                }
            }
        }
    }
}
