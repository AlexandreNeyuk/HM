﻿using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;

namespace HM
{
    /// <summary>
    ///Экземпляр Хоста
    /// </summary>
    public class Host
    {
        string Host_name { get; set; }
        string Host_ { get; set; }
        string Port { get; set; }
        string DataBase_ { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="host_name">Имя хоста</param>
        /// <param name="host_">Хост</param>
        /// <param name="port">Порт</param>
        /// <param name="dataBase_">База даных</param>
        public Host(string host_name, string host_, string port, string dataBase_)
        {
            Host_name = host_name;
            Host_ = host_;
            Port = port;
            DataBase_ = dataBase_;
        }
    }

    /// <summary>
    /// Логика взаимодействия для WarEdit.xaml
    /// </summary>
    public partial class WarEdit : Window
    {
        public WarEdit()
        {
            InitializeComponent();
            this.Loaded += (e, a) => { LoadHosts(); };
            List_Hosts.SelectionChanged += (a, e) => { LoadTextB(); };

        }


        /// <summary>
        ///Загрузка имен бд в лист из реестра
        /// </summary>
        public void LoadHosts()
        {
            List<string> Hosts_Name = new List<string>();
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts");

            foreach (var item in key?.GetValueNames())
            {
                if (item.Contains("Name_"))
                    Hosts_Name.Add(key.GetValue(item).ToString());

            }

            //Hosts.Add(key.ToString());

            List_Hosts.ItemsSource = Hosts_Name;
        }
        /// <summary>
        /// Сейв в реестр
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Hosts"))
            {
                if (Text_DB.Text != "" && Text_NameHost.Text != "" && Text_Port.Text != "" && TextHost.Text != "")
                {
                    key?.SetValue("Name_" + Text_NameHost.Text, Text_NameHost.Text);
                    key?.SetValue("Host_" + Text_NameHost.Text, TextHost.Text);
                    key?.SetValue("Post" + Text_NameHost.Text, Text_Port.Text);
                    key?.SetValue("DataBase_" + Text_NameHost.Text, Text_DB.Text);
                }
                else MessageBox.Show("А нука ввел все сука поля, иначе сломаюсь к хуям, ты же меня знаешь -_-!!!");


            }
            ClearTextB();
            LoadHosts();
        }
        /// <summary>
        ///Удалаение Хоста
        /// </summary>
        private void RemoveReg_Click(object sender, RoutedEventArgs e)
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts", true);
            if (List_Hosts.SelectedItem != null)
            {
                foreach (var item in List_Hosts.Items) //шарапово из лсита на форме 
                {
                    //ищу выбранный элемент в реестре
                    foreach (var host in key?.GetValueNames()) //Name_... 
                    {
                        if (host.Contains(List_Hosts.SelectedItem.ToString()))
                        {
                            key.DeleteValue(host);
                        }
                    }

                }
            }
            LoadHosts();
        }
        /// <summary>
        /// Очистка полей
        /// </summary>
        public void ClearTextB()
        {
            Text_NameHost.Text = null;
            TextHost.Text = null;
            Text_Port.Text = null;
            Text_DB.Text = null;

        }
        /// <summary>
        ///Заполнение полей из реестра
        /// </summary>
        public void LoadTextB()
        {
            string ssl = List_Hosts.SelectedItem?.ToString(); // выбраное бд
            if (ssl != null)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts");
                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains(ssl))
                    {
                        if (item.Contains("Name_")) Text_NameHost.Text = key?.GetValue(item).ToString();
                        if (item.Contains("Host_")) TextHost.Text = key?.GetValue(item).ToString();
                        if (item.Contains("Post")) Text_Port.Text = key?.GetValue(item).ToString();
                        if (item.Contains("DataBase_")) Text_DB.Text = key?.GetValue(item).ToString();

                    }
                    //Hosts_Name.Add(key.GetValue(item).ToString());

                }
            }

        }

        /// <summary>
        ///  Поиск БД в Листе
        /// </summary>
        private void Search_Lbd_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            foreach (var item in List_Hosts.Items)
                if (item.ToString().Contains(Search_Lbd.Text))
                {
                    List_Hosts.SelectedItem = item;

                }

        }
    }
}