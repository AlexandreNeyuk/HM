using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;

namespace HM
{
    /// <summary>
    ///Экземпляр Хоста
    /// </summary>
    public class Host
    {
        public string Host_name { get; set; }
        string Host_ { get; set; }
        string Port { get; set; }
        string DataBase_ { get; set; }
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

        }


        /// <summary>
        ///Загрузка бд в лист из реестра
        /// </summary>
        public void LoadHosts()
        {

            List<Host> hsts = new List<Host>();

            List<string> Hosts = new List<string>();
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts");
            if (key != null)
            {

                MessageBox.Show(key.GetSubKeyNames().ToString());

                //Hosts.Add(key.ToString());
            }
            // List_Hosts.Items.Add(Hosts);
        }


    }
}
