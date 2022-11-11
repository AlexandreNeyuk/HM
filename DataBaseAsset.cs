namespace HM
{

    internal class DataBaseAsset
    {

        private string con;


        public void ConnectDB(string db, string query)
        {
            ///загрузка из реестра данных об складе через поиск по переменной db
            ///формирование конечной строрки соединений con


            string con = "Host={0};Username={1};Password={2};Database={3}";

            switch (db)
            {
                case "shiptor":
                    // con = 
                    // default:
                    break;
            }
            // con = String.Format(con, Host.Text, Username.Text, Password.Password, Database.Text); 




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
