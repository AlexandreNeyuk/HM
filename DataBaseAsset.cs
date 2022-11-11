using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HM
{
    internal class DataBaseAsset
    {
        private string con = "Host=172.19.50.4;Username=Nenyuk.A.S;Password=Dec2Tek_125247;Database=shiptor";
        
        public void TestConnect()
        {
            NpgsqlConnection nc = new NpgsqlConnection(con);
            try
            {
                //Открываем соединение.
                nc.Open();
                if (nc.FullState == ConnectionState.Broken || nc.FullState == ConnectionState.Closed)
                {

                    MessageBox.Show("Нет подключения");

                }
                else MessageBox.Show("Есть коннект!!!");

                nc.Close();
            }
            catch (Exception ex)
            {
                nc.Close();
                MessageBox.Show(ex.Message);
                //Код обработки ошибок
            }
        }


    }
}
