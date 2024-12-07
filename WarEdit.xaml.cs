using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.Forms.MessageBox;

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
            this.Loaded += (e, a) => { LoadHosts(List_Hosts); };
            List_Hosts.SelectionChanged += (a, e) => { LoadTextB(); };

        }


        /// <summary>
        ///Загрузка имен бд в лист из реестра
        /// </summary>
        /// <param name="ListHost">Имя ListBox для которого требуется выгрузить список бд</param>
        public void LoadHosts(ListBox ListHost)
        {
            List<string> Hosts_Name = new List<string>();
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts");

            foreach (var item in key?.GetValueNames())
            {
                if (item.Contains("Name_"))
                    Hosts_Name.Add(key.GetValue(item).ToString());

            }


            //Hosts.Add(key.ToString());
            Hosts_Name = Hosts_Name.OrderBy(item => item).ToList();
            ListHost.ItemsSource = Hosts_Name;

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
                    key?.SetValue("Post_" + Text_NameHost.Text, Text_Port.Text);
                    key?.SetValue("DataBase_" + Text_NameHost.Text, Text_DB.Text);
                }
                else MessageBox.Show("Необходимо заполнить все поля!");


            }
            ClearTextB();
            LoadHosts(List_Hosts);
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
                            string founded = host.Replace(" ", "").Replace("Name_", "").Replace("Host_", "").Replace("Post", "").Replace("DataBase_", "");
                            if (List_Hosts.SelectedItem.ToString().Replace(" ", "").Replace("Name_", "").Replace("Host_", "").Replace("Post", "").Replace("DataBase_", "") == founded)
                            {
                                key.DeleteValue(host);

                            }

                        }
                    }

                }
            }
            LoadHosts(List_Hosts);
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
            ClearTextB();
            string ssl = List_Hosts.SelectedItem?.ToString(); // выбраное бд
            if (ssl != null)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts");
                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains(ssl))
                    {
                        string founded = item.Replace(" ", "").Replace("Name_", "").Replace("Host_", "").Replace("Post_", "").Replace("DataBase_", "");
                        if (founded == ssl.Replace(" ", ""))
                        {
                            if (item.Contains("Name_")) Text_NameHost.Text = key?.GetValue(item).ToString();
                            if (item.Contains("Host_")) TextHost.Text = key?.GetValue(item).ToString();
                            if (item.Contains("Post_")) Text_Port.Text = key?.GetValue(item).ToString();
                            if (item.Contains("DataBase_")) Text_DB.Text = key?.GetValue(item).ToString();
                        }

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
            if (Search_Lbd.Text != "")
            {
                foreach (var item in List_Hosts.Items)
                    if (item.ToString().ToUpper().Contains(Search_Lbd.Text.ToUpper()))
                    {
                        List_Hosts.SelectedItem = item;
                        List_Hosts.ScrollIntoView(List_Hosts.Items.GetItemAt(List_Hosts.SelectedIndex));
                    }
            }




        }
    }
}
