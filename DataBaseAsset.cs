using Microsoft.Win32;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using Label = System.Windows.Controls.Label;

namespace HM
{

    internal class DataBaseAsset
    {

        ///Если будут проблемы со скоростью впоследствии 
        ///то слздать класс с хастами и в фоне выгрузить все данные о бд из реестра в этот лист
        private string UserData_D;
        private string Pass_D;
        bool ConnectBool = false;
        private string stats = "Статус подключения";
        Label label = new Label();
        MainWindow ol;

        public DataBaseAsset(string U, string P)
        {
            UserData_D = U;
            Pass_D = P;
        }



        /// <summary>
        /// проверка подключения к БД (запускается при запуске программы)
        /// </summary>
        async public void ProtectedConnection(MainWindow cl)
        {
            ol = cl;

            await Task.Run(() =>
           {
               string con = "Host={0};Username={1};Password={2};Database={3}";
               string Host = "", DataBase = "", User = UserData_D, Pass = Pass_D;

               using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts"))
               {
                   foreach (var item in key?.GetValueNames())
                   {
                       if (item.Contains("Шиптор") && item.Contains("Host_")) Host = key.GetValue(item).ToString();
                       if (item.Contains("Шиптор") && item.Contains("DataBase_")) DataBase = key.GetValue(item).ToString();
                   }
               }

               if (Host != "" && Pass != "" && DataBase != "" && User != "")
               {
                   con = String.Format(con, Host, User, Pass, DataBase);
                   NpgsqlConnection nc = new NpgsqlConnection(con);
                   try
                   {
                       //Открываем соединение.                         
                       nc.Open();
                       stats = "Подключено";
                       ol.UpdateLabel(stats);
                       stats = "Подключено";
                       ConnectBool = true;
                       nc.Close();
                   }
                   catch (Exception ex)
                   {
                       nc.Close();
                       stats = "Не подключено!";
                       ol.UpdateLabel(stats);
                       MessageBox.Show("Нет соединения с Сервером. Проверьте FortiClient VPN! А после подключения перезапустите меня! \n" + ex.Message);
                       //Код обработки ошибок
                   }
               }
               else MessageBox.Show("Нет данных о пользователе (либо о Шиптор, что врядли, если ты првильно все установил(а))! Перейди в Настройки и внеси их! ЭТО ВАЖНО и это тебе не шутки!");

           });




        }


        /// <summary>
        /// Запорос к БД
        /// </summary>
        /// <param name="db">Имя БД на русском, мы же русские люди!</param>
        /// <param name="query">Запрос, что тебе надо</param>
        public DataTable ConnectDB(string db, string query)
        {
            ///загрузка из реестра данных об складе через поиск по переменной db
            ///формирование конечной строки соединений con
            string con = "Host={0};Username={1};Password={2};Database={3}";
            string Host = "", DataBase = "", User = "", Pass = "";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts"))
            {

                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains(db))
                    {
                        string founded = item.Replace(" ", "").Replace("Name_", "").Replace("Host_", "").Replace("Post", "").Replace("DataBase_", "");
                        if (founded == db.Replace(" ", ""))
                        {
                            if (item.Contains("Host_")) Host = key?.GetValue(item).ToString();
                            if (item.Contains("DataBase_")) DataBase = key?.GetValue(item).ToString();
                        }

                    }

                }

            }



            ///получать данные после дешифровки от главного скрипта (!)
            User = UserData_D;
            Pass = Pass_D;

            con = String.Format(con, Host, User, Pass, DataBase);
            DataTable dt = new DataTable();
            if (ConnectBool == false) MessageBox.Show("Нет коннекта! Проверьте FortiClient VPN!\n А затем перезапустите программу. ");
            else
            {
                if ((Host != "") && (DataBase != ""))
                {
                    NpgsqlConnection nc = new NpgsqlConnection(con);
                    try
                    {
                        //Открываем соединение.
                        nc.Open();
                        if (nc.FullState == ConnectionState.Broken || nc.FullState == ConnectionState.Closed) MessageBox.Show("Нет коннекта. Сломано или Соекдинение закрылось. Ксива коннекта, мб в нем чего: \n" + con);
                        NpgsqlCommand cmd = new NpgsqlCommand(query, nc);
                        dt.Load(cmd.ExecuteReader());
                        nc.Close();
                        // MessageBox.Show(dt.Rows[0][1].ToString());
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        nc.Close();
                        MessageBox.Show("Ошибка подключения: " + ex.Message);
                        //Код обработки ошибок
                    }
                }
                else MessageBox.Show($@"Какоето поле не найдено из базы БД (возможно неверное имя бд, отличающееся от имени из таблички Warehouse): Host: {Host}; DB: {DataBase}");


            }
            return dt;

        }


    }
}
