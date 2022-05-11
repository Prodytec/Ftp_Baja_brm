using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Bajadaftp
{
    public class Log
    {
        private string Path = "";
        string fecha = DateTime.Now.ToString("yyyyMMdd_Hmmss");

        public Log(string Path)
        {
            this.Path = Path;
        }

        public void Add(string sLog)
        {
            CreateDirectory();
            string nombre = GetNameFile();
            string cadena = "";
            
            cadena += DateTime.Now + " - " + sLog + Environment.NewLine; //Environment.NewLine es un salto de linea en el archivo

            StreamWriter sw = new StreamWriter(Path + "/" + nombre, true);
            sw.Write(cadena);
            sw.Close();
        }

        #region Helper
        private string GetNameFile()
        {
            string nombre = "";
            nombre = "log_" + fecha + ".txt";

            return nombre;
        }

        private void CreateDirectory()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
            }
            catch(DirectoryNotFoundException ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        #endregion
    }
}
