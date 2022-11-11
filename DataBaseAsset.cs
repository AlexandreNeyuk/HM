using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HM
{
    internal class DataBaseAsset
    {

        private string con;


         public void TestConnect( TextBox Host, TextBox Username, PasswordBox Password, TextBox Database)
        {  
            string con = "Host={0};Username={1};Password={2};Database={3}";

        con = String.Format(con, Host.Text, Username.Text, Password.Password, Database.Text); 

            MessageBox.Show(con);
            //NpgsqlConnection nc = new NpgsqlConnection(con);
            //try
            //{
            //    //Открываем соединение.
            //    nc.Open();
            //    if (nc.FullState == ConnectionState.Broken || nc.FullState == ConnectionState.Closed)
            //    {

            //        MessageBox.Show("Нет подключения");

            //    }
            //    else MessageBox.Show("Есть коннект!!!");

            //    nc.Close();
            //}
            //catch (Exception ex)
            //{
            //    nc.Close();
            //    MessageBox.Show(ex.Message);
            //    //Код обработки ошибок
            //}
        }





    }
}
