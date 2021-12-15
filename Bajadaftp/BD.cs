using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Bajadaftp
{
    public class BD
    {
        public sealed class DbConnection
        {
            private static volatile DbConnection instance;

            private static SqlConnection cnn;

            private DbConnection() { }
            static DbConnection()
            {
                if (cnn == null)
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\InTime\\ItPyme"); //entradas de registro
                    if (rk != null)
                    {
                        Object database = rk.GetValue("BDatos");
                        Object server = rk.GetValue("Servidor");
                        Object userName = rk.GetValue("UserName");
                        Object password = rk.GetValue("PSW");
                        cnn = new SqlConnection("Server=" + server.ToString() + ";Database=" + database.ToString() + ";User Id=" + userName.ToString() + ";Password=" + password.ToString() + ";MultipleActiveResultSets=true");
                    }
                }
            }
            public static DbConnection Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (cnn)
                        {
                            if (instance == null)
                                instance = new DbConnection();
                        }
                    }

                    return instance;
                }
            }
            static public SqlConnection getDBConnection()
            {
                if (cnn != null && cnn.State == ConnectionState.Closed) cnn.Open();
                return cnn;
            }
        }
    }
}
