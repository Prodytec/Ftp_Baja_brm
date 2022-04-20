using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Bajadaftp.BD.DbConnection;
using static Bajadaftp.Funciones;

namespace Bajadaftp
{
    class Program
    {
        static void Main(string[] args)
        {
            Funciones F = new Funciones();
            string Valor = (string)Instalacion.GetValor();
            switch (Valor.ToLower())
            {
                case "brm":
                    F.Bajada();
                    return;
                case "digitalapps":
                    F.leercsv();
                    return;
                case "american":
                    F.Bajada();
                    return;
                default:
                    MessageBox.Show("No se encontro la instalacion");
                    break;
            }
        }
    }
}
