using Microsoft.Win32;
using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;

namespace HM
{

    internal class DataBaseAsset
    {
        ///Если будут проблемы со скоростью впоследствии 
        ///то слздать класс с хастами и в фоне выгрузить все данные о бд из реестра в этот лист

        /// <summary>
        /// Запорос к БД
        /// </summary>
        /// <param name="db">Имя БД на русском, мы же русские люди!</param>
        /// <param name="query">Запрос, что тебе надо</param>
        public void ConnectDB(string db, string query)
        {
            ///загрузка из реестра данных об складе через поиск по переменной db
            ///формирование конечной строрки соединений con
            string con = "Host={0};Username={1};Password={2};Database={3}";
            string Host = "", DataBase = "", User = "", Pass = "";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts"))
            {
                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains(db) && item.Contains("Host_")) Host = key.GetValue(item).ToString();
                    if (item.Contains(db) && item.Contains("DataBase_")) DataBase = key.GetValue(item).ToString();

                }
            }
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Settings"))
            {
                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains("Имя пользователя")) User = key.GetValue(item).ToString();
                    if (item.Contains("Пароль")) Pass = key.GetValue(item).ToString();

                }
            }
            con = String.Format(con, Host, User, Pass, DataBase);


            using NpgsqlConnection nc = new NpgsqlConnection(con);
            try
            {
                //Открываем соединение.
                nc.Open();
                if (nc.FullState == ConnectionState.Broken || nc.FullState == ConnectionState.Closed)
                {
                    Exception ex;
                    MessageBox.Show("Нет подключения. Ошибка: ");
                    return;

                }
                else MessageBox.Show("Есть коннект!!!");

                query = @"select * ...";
                NpgsqlCommand cmd = new NpgsqlCommand(query, nc);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
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
