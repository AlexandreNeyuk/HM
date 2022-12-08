using ICSharpCode.AvalonEdit;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Clipboard = System.Windows.Clipboard;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.Forms.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;

namespace HM
{
    public partial class MainWindow : Window
    {
        #region Global Переменные

        List<TextBox> KeyTextBoxies; // ///все поля для которых нужно свойство введения HotKeys, через  ",
        List<Canvas> MenuCanvas; // все элементы меню 
        //Animations Animations = new Animations();
        DataBaseAsset dataBases = new DataBaseAsset();

        bool SetPanel = false; //false - закрытая панель, true - открытая панлеь
        #endregion


        public MainWindow()
        {
            InitializeComponent();

            #region Проверка соединения с сервером
            dataBases.ProtectedConnection();
            #endregion

            #region Начальные накстройки
            TB.Margin = new Thickness(0, 0, 0, 0); //выравниванеи TableControl 
            ///отключение панели настроек при иницииализации --
            SettingsGrid.IsEnabled = false;
            SettingsGrid.Visibility = Visibility.Hidden;
            PartyGrid.IsEnabled = false;
            PartyGrid.Visibility = Visibility.Hidden;
            #endregion

            #region Regisrty Staff           
            ///Пересоздание корня настроек в реестре + синхрон с реестром настроек--
            using RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
            using RegistryKey registry1 = Registry.CurrentUser.CreateSubKey(@"Software\HM\Hosts");
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Settings"))
            {
                if (key != null)
                {
                    UserName.Text = key.GetValue("Имя пользователя")?.ToString();
                    UserPass.Password = key.GetValue("Пароль")?.ToString();
                    SP_HotKey.Text = key.GetValue(SP_HotKey.Name)?.ToString();
                    Settings_HotKey.Text = key.GetValue(Settings_HotKey.Name)?.ToString();
                }
            }
            #endregion

            #region Добавление обработки событий для всех полей HotKeys в Настройках
            KeyTextBoxies = new List<TextBox>() { SP_HotKey, Settings_HotKey }; ///все поля для которых нужно свойство HotKeys, через  ","
            foreach (var item in KeyTextBoxies)
            {
                item.KeyDown += (s, a) =>
                {
                    if (a.Key == Key.Escape)
                        item.Text = " ";
                    else
                    {
                        item.IsReadOnly = true;
                        item.Text = a.Key.ToString();
                    }
                };

            }

            #endregion

            #region Добавление обработки всех элементов меню (навердений мыши  и клики)
            MenuCanvas = new List<Canvas>() { SettingCanvas, PostomatsCanvas, PartyCanvas, Home };
            foreach (var item in MenuCanvas)
            {
                item.MouseEnter += (a, e) => { item.Background = new SolidColorBrush(Colors.Gray); };
                item.MouseLeave += (a, e) => { item.Background = new SolidColorBrush(Colors.Transparent); };

            }

            ///обработка кликов на элементы меню
            PartyCanvas.MouseDown += (a, e) => { PartyGrid.IsEnabled = true; PartyGrid.Visibility = Visibility.Visible; TB.IsEnabled = false; TB.Visibility = Visibility.Hidden; WarEdit wr = new WarEdit(); wr.LoadHosts(ListWarhouses); wr.Close(); SwitchPanel(); };
            Home.MouseDown += (a, e) => { PartyGrid.IsEnabled = false; PartyGrid.Visibility = Visibility.Hidden; TB.IsEnabled = true; TB.Visibility = Visibility.Visible; SwitchPanel(); };
            PostomatsCanvas.MouseDown += (a, e) => { MessageBox.Show("Здесь кода-нибудь чтото будет !))"); SwitchPanel(); };
            SettingCanvas.MouseDown += (a, e) => { OpenSettings(); };


            #endregion



        }
        #region Своя кнопка "Закрыть"
        ///// <summary>
        //////кнопка закрытия 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Close_Butt_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{

        //}
        ///// <summary>
        ///// смеа цвета на красный
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Close_Butt_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    Close_Butt.Background =
        //}
        ///// <summary>
        ///// смена цвета на = не цвет
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Close_Butt_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        //{

        //    Close_Butt.Background = null;
        //}
        #endregion Button_close


        #region Боковая панель

        /// <summary>
        ///Кнопка боковой панели 
        /// </summary>
        private async void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { SwitchPanel(); }
        async public void SwitchPanel()
        {
            const int marginLeftMin = 0;
            const int marginLeftMax = 150;
            double CurrentVarginLeft = TabControGrid.Margin.Left;

            TextBox.IsEnabled = false;
            ListOne.IsEnabled = false;
            ListTwo.IsEnabled = false;
            ListCopyElm.IsEnabled = false;



            ///анимация боковой панели  
            if (SetPanel == true)
            {
                while (TabControGrid.Margin.Left - TabControGrid.Margin.Left / 6 > marginLeftMin)
                {
                    CurrentVarginLeft = TabControGrid.Margin.Left;
                    await Task.Delay(1);
                    TabControGrid.Margin = new Thickness(CurrentVarginLeft - CurrentVarginLeft / 6, 0, 0, 0);
                    if (TabControGrid.Margin.Left < 2)
                    {
                        TabControGrid.Margin = new Thickness(0, 0, 0, 0);
                        break;
                    }
                }
                SetPanel = !SetPanel;

            }
            else
            {

                while (CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 6 < marginLeftMax)
                {
                    CurrentVarginLeft = TabControGrid.Margin.Left;
                    await Task.Delay(1);
                    TabControGrid.Margin = new Thickness(CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 6, 0, 0, 0);
                    if (TabControGrid.Margin.Left > 147)
                    {
                        TabControGrid.Margin = new Thickness(150, 0, 0, 0);
                        break;
                    }
                }
                SetPanel = !SetPanel;
            }

            TextBox.IsEnabled = true;
            ListOne.IsEnabled = true;
            ListTwo.IsEnabled = true;
            ListCopyElm.IsEnabled = true;

        }

        #endregion

        #region Сложные  функции элементов бокоовой панели 

        /// <summary>
        /// Открыть панель настроек 
        /// </summary>
        public void OpenSettings()
        {
            SettingsGrid.Visibility = Visibility.Visible;
            SettingsGrid.IsEnabled = true;
            SwitchPanel();


        }

        #endregion


        #region Table Item 1   

        async private void ListProcess_Click(object sender, RoutedEventArgs e)
        {


            if (ContextRP.IsChecked == true)
            {
                //по сенарию RP (simple_List)
                // TextBox.Text = TextBox.Text.Replace("RP", "");

                //чуть сложнее
                List<string> stringsRP = To_List(TextBox);
                List<string> resilt = new List<string>();  //лист с цифрами от RP
                foreach (var item in stringsRP)
                {
                    if (item.Contains("RP"))
                    {
                        // string h = item.Substring(item.IndexOf("RP"), 12);
                        string[] valuesRP = item.Split("RP"); ///RP - 5656564
                        if (valuesRP.Length >= 2)//в строке несколько RP
                        {
                            foreach (var item1 in valuesRP)
                                if (string.Join("", item1.Where(c => char.IsDigit(c))) != "")
                                    resilt.Add(string.Join("", item1.Where(c => char.IsDigit(c))));
                        }
                        else
                            resilt.Add(string.Join("", item.Where(c => char.IsDigit(c))));

                    }

                }
                resilt = resilt.Distinct().ToList();
                TextBox.Text = string.Join("\r\n", resilt);

            }
            else
            {
                //по сценарию с UPPER`S
                if (!TextBox.Text.Contains("UPPER"))
                {
                    TextBox.Text = TextBox.Text.Replace("\r\n", "')," + "\rUPPER ('") + "')";
                    TextBox.Text = "UPPER ('" + TextBox.Text;
                }
                //ищу RP в БД из UPPER`s
                if (UpperSearch.IsChecked == true)
                {
                    List<string> Upper_RP = dataBases.ConnectDB("Шиптор", $@"select id, external_id, * from public.package p where UPPER(p.external_id) in ({TextBox.Text})").AsEnumerable().Select(x => x[0].ToString()).ToList();
                    TextBox.Text = string.Join("\r\n", Upper_RP);
                    TextBox.Text = TextBox.Text.Replace("\r\n", ",\n");

                }
            }

            //______________работа с запятыми______________
            if (comma.IsEnabled == true && comma.IsChecked == true) TextBox.Text = TextBox.Text.Replace("\r\n", ",\n"); // - работает
            if (comma.IsEnabled == true && comma.IsChecked == false) TextBox.Text = TextBox.Text.Replace(",", "\r");

            ///Работа с родителями и всей семьей
            if (AF_1.IsChecked == true && ContextRP.IsChecked == true) //вся семья
            {
                List<string> ParrentsList = dataBases.ConnectDB("Шиптор", $@"select id, parent_id from package p where id  in ({TextBox.Text}) or parent_id in ({TextBox.Text})").AsEnumerable().Select(x => x[1].ToString()).ToList();
                foreach (var Parrent in ParrentsList)
                    if (Parrent != "") TextBox.Text += ",\n" + Parrent;

                List<string> AllFamily = dataBases.ConnectDB("Шиптор", $@"select id, parent_id from package p where id  in ({TextBox.Text}) or parent_id in ({TextBox.Text})").AsEnumerable().Select(x => x[0].ToString()).ToList();
                TextBox.Text = TextBox.Text.Replace(",", "");
                List<string> text = To_List(TextBox);
                foreach (var el in AllFamily)
                    if (el != "") text.Add(el);
                TextBox.Text = string.Join("\r\n", text.Distinct().ToList());
                TextBox.Text = TextBox.Text.Replace("\r\n", ",\n");

            }
            //поиск родителей 
            if (AF_2.IsChecked == true && ContextRP.IsChecked == true)
            {
                List<string> ParrentsList = dataBases.ConnectDB("Шиптор", $@"select id, parent_id from package p where id  in ({TextBox.Text}) or parent_id in ({TextBox.Text})").AsEnumerable().Select(x => x[1].ToString()).ToList();
                foreach (var Parrent in ParrentsList)
                {
                    if (Parrent != "") TextBox.Text += ",\n" + Parrent;

                }
            }



            if (TextBox.Text != "") Clipboard.SetText(TextBox.Text); //запись в  буфер
            BFcopy.Text = "Результат скопирован в буфер обмена";
            await Task.Delay(1000);
            BFcopy.Text = null;

        }

        /// <summary>
        ///очистка пвсего поля 
        /// </summary>
        private void Clear_all_textB_Click(object sender, RoutedEventArgs e)
        {
            TextBox.Text = null;
        }

        /// <summary>
        /// Переключатель родителей - логика 1
        /// </summary>
        private void AF_1_Checked(object sender, RoutedEventArgs e) { AF_2.IsChecked = false; }

        /// <summary>
        /// Переключатель родителей - логика 2
        /// </summary>
        private void AF_2_Checked(object sender, RoutedEventArgs e) { AF_1.IsChecked = false; }
        /// <summary>
        /// радио ботон 1 -RP
        /// </summary>
        private void ConrextUpper_Checked(object sender, RoutedEventArgs e)
        {
            comma.IsEnabled = false;
            AF_1.IsChecked = false;
            AF_1.IsEnabled = false;
            AF_2.IsChecked = false;
            AF_2.IsEnabled = false;
            UpperSearch.IsEnabled = true;

        }

        /// <summary>
        ///Радио батон 2 - UPPER
        /// </summary>
        private void ContextRP_Checked(object sender, RoutedEventArgs e)
        {
            comma.IsEnabled = true;
            AF_1.IsEnabled = true;
            AF_2.IsEnabled = true;
            UpperSearch.IsChecked = false;
            UpperSearch.IsEnabled = false;

        }
        #endregion

        #region Tab_Item 2    
        /// <summary>
        /// кнопка листов - выитание листов
        /// </summary>
        private void FidCopy_Click(object sender, RoutedEventArgs e)
        {
            List<string> list1 = To_List(ListOne);
            ListTwo.Text += "\r\n";
            List<string> list2 = To_List(ListTwo);
            list2 = list2.Distinct().ToList();
            ListTwo.Text = string.Join(Environment.NewLine, list2);

            List<string> result = list1.Except(list2).ToList();
            list1 = list1.Distinct().ToList();
            ListOne.Text = string.Join(Environment.NewLine, result);



        }

        /// <summary>
        ///Очистка полей листов
        /// </summary>
        private void FidCopy_Copy_Click(object sender, RoutedEventArgs e)
        {
            ListOne.Text = null;
            ListTwo.Text = null;
            ListCopyElm.Text = null;
        }

        #endregion

        #region Party

        /// <summary>
        /// Кнопка обработки Партий
        /// </summary>
        private void PartyEx_Click(object sender, RoutedEventArgs e)
        {
            // todo  [Партии]: При добавлении предупреждать о дубликатах если такие посылки уже добавлены и обрабатывать так, чтобы тех которых нет - прокидывать в партию, а дубли - убирать из запроса
            //Добавление 
            if (SelectAction.SelectedIndex == 0)
            {
                string paty = Party.Text; //партия 
                if (paty.Contains("R_RET")) paty = paty.Replace("R_RET", "");
                List<string> StockID = dataBases.ConnectDB("Шиптор", $@"select * from package_return pr where id in ({paty})").AsEnumerable().Select(x => x["stock_id"].ToString()).ToList();
                if (StockID.Count == 1)//проверка существования в шипторе
                {
                    //ищем имя склада в Шипторе
                    List<string> StockName = dataBases.ConnectDB("Шиптор", $@"SELECT id, ""name""  FROM public.warehouse where id = {StockID[0]}").AsEnumerable().Select(x => x[1].ToString()).ToList();
                    if (Name_War.Content != StockName[0]) if (StockName[0] != null) Name_War.Content = StockName[0]; else MessageBox.Show($@"Склада с id {StockID[0]} не найдено в Шиптор!");//меняем имя склада на то что нашлось

                    DataTable CurrentStock = dataBases.ConnectDB(StockName[0], $@"select id, status from package_return pr where return_fid = {paty}");
                    List<string> ParyStockID = CurrentStock.AsEnumerable().Select(x => x["id"].ToString()).ToList();
                    List<string> PartyStockStatus = CurrentStock.AsEnumerable().Select(x => x["status"].ToString()).ToList();
                    if (ParyStockID[0] != null)
                    {
                        bool d = true;
                        if (PartyStockStatus[0] == "sent" || PartyStockStatus[0] == "disbanded") if (MessageBox.Show($@"Паллета на складе имеет статус {PartyStockStatus[0].ToUpper()}. Все равно добавить в паллету?", "Добавить в паллету?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) d = false;
                        if (d)
                        {   //Добавляю в паллету


                            if (RP_Party.Text != null)
                            {
                                string RPid = RP_Party.Text.Replace("\r\n", "");
                                //добавляю в партию в шипторе, если она есть 
                                dataBases.ConnectDB("Шиптор", $@"UPDATE public.package SET return_id = {paty} WHERE id in ({RPid})");

                                //формирование списка 
                                List<string> ListRPID = dataBases.ConnectDB(StockName[0], $@"select id from package p where package_fid in({RPid})").AsEnumerable().Select(x => x[0].ToString()).ToList();
                                List<string> values = new List<string>();
                                foreach (string RPID in ListRPID)
                                {
                                    values.Add($@"({RPID}, {ParyStockID[0]})");
                                }
                                string Cvalues = string.Join(",", values);
                                //сама запись в партию в бд:
                                dataBases.ConnectDB(StockName[0], $@"INSERT into public.package_return_item(package_id, package_return_id) values {Cvalues}");

                                MessageBox.Show($@"В партию добалено!");
                                RP_Party.Text = null;
                                Party.Text = null;


                            }
                            else MessageBox.Show("Поле для отправлений пусто! Добавьте отправления");
                        }
                    }
                    else MessageBox.Show($@"Партия не найдена на складе {StockName[0]}!");
                }
                else MessageBox.Show("Такая партия не одна (!) или ее не существует в Шипторе!");
            }

            //Удаление
            if (SelectAction.SelectedIndex == 1)
            {
                string paty = Party.Text; //партия 
                if (paty.Contains("R_RET")) paty = paty.Replace("R_RET", "");
                List<string> StockID = dataBases.ConnectDB("Шиптор", $@"select * from package_return pr where id in ({paty})").AsEnumerable().Select(x => x["stock_id"].ToString()).ToList();
                if (StockID.Count == 1)//проверка существования в шипторе
                {
                    //ищем имя склада в Шипторе
                    List<string> StockName = dataBases.ConnectDB("Шиптор", $@"SELECT id, ""name""  FROM public.warehouse where id = {StockID[0]}").AsEnumerable().Select(x => x[1].ToString()).ToList();
                    if (Name_War.Content != StockName[0]) if (StockName[0] != null) Name_War.Content = StockName[0]; else MessageBox.Show($@"Склада с id {StockID[0]} не найдено в Шиптор!");//меняем имя склада на то что нашлось

                    DataTable CurrentStock = dataBases.ConnectDB(StockName[0], $@"select id, status from package_return pr where return_fid = {paty}");
                    List<string> ParyStockID = CurrentStock.AsEnumerable().Select(x => x["id"].ToString()).ToList();
                    List<string> PartyStockStatus = CurrentStock.AsEnumerable().Select(x => x["status"].ToString()).ToList();
                    if (ParyStockID[0] != null) //если партия существует на складе
                    {
                        //Удаление из паллеты

                        if (RP_Party.Text != null)
                        {
                            string RPid = RP_Party.Text.Replace("\r\n", "");
                            //удаляю из партии в шипторе
                            dataBases.ConnectDB("Шиптор", $@"UPDATE public.package SET return_id = NULL WHERE id in ({RPid})");

                            //формирование списка 

                            dataBases.ConnectDB(StockName[0], $@"Delete from package_return_item where package_id in (select id from package p where package_fid in({RPid}))");

                            MessageBox.Show($@"Из партии удалено!");
                            RP_Party.Text = null;
                            Party.Text = null;


                        }
                        else MessageBox.Show("Поле для отправлений пусто! Добавьте отправления");

                    }
                    else MessageBox.Show($@"Партия не найдена на складе {StockName[0]}!");
                }
                else MessageBox.Show("Такая партия не одна (!) или ее не существует в Шипторе!");
            }
        }

        /// <summary>
        /// Посик в листе Склада
        /// </summary>
        private void Search_Warh_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var item in ListWarhouses.Items)
                if (item.ToString().ToUpper().Contains(Search_Warh.Text.ToUpper()))
                {
                    ListWarhouses.SelectedItem = item;

                }
            Name_War.Content = ListWarhouses.SelectedItem;
        }

        /// <summary>
        /// Вывод выбранного склада 
        /// </summary>
        private void ListWarhouses_SelectionChanged(object sender, SelectionChangedEventArgs e) { Name_War.Content = ListWarhouses.SelectedItem; }


        #endregion

        #region Tools

        /// <summary>
        /// Конвертер в лист строк из содержимого TextEditor
        /// </summary>
        /// <param name="tx">Поле для конвертации</param>
        /// <returns></returns>
        public List<string> To_List(TextEditor tx)
        {
            List<string> str = tx.Text.Split("\r\n").ToList();
            return str;

        }

        #endregion


        /// <summary>
        /// Быстрые клавиши
        /// </summary>
        /// <param name="e">Введенная клавиша</param>
        private void HM_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ///Отключение ПАНЕЛИ НАСТРОЕК бысчрой клавишей 
            if (e.Key.ToString() == SP_HotKey.Text) SwitchPanel(); // открытие боковой панели по кнопке 
            if (e.Key.ToString() == Settings_HotKey.Text) OpenSettings();

        }

        #region Settings      

        /// <summary>
        /// Сохранение данных пользователя
        /// </summary>
        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UserName.Text != "" && UserPass.Password != "")
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
                {
                    key?.SetValue("Имя пользователя", UserName.Text);
                    key?.SetValue("Пароль", UserPass.Password);
                }
                SussessLabel.Content = "Успешно";

            }
            else SussessLabel.Content = "Не заполнено поле";

            await Task.Delay(1000);
            SussessLabel.Content = "";

        }

        /// <summary>
        /// функйция проверки полей и записи в реестр
        /// </summary>
        /// <param name="L1">Поле для хостов</param>
        /// <param name="L2">Поле для имени БД </param>
        /// <param name="suslabel">Лейбл статусов</param>
        async public void SaveProtected(TextBox L1, TextBox L2, Label suslabel)
        {
            if (L1.Text != "" && L2.Text != "")
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
                {
                    key?.SetValue(L1.Name, L1.Text);
                    key?.SetValue(L2.Name, L2.Text);

                }
                suslabel.Content = "Успешно";
            }
            else suslabel.Content = "Не запполено одно из полей";
            await Task.Delay(1000);
            suslabel.Content = "";
        }

        /// <summary>
        /// СМохранение HotKeys из листа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var item in KeyTextBoxies)
            {
                if (item.Text != "")
                {
                    using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
                    {
                        key?.SetValue(item.Name, item.Text);

                    }
                    SussessHotKeysLabel.Content = "Успешно";
                    await Task.Delay(1000);
                    SussessHotKeysLabel.Content = "";

                }

            }
        }

        /// <summary>
        /// Кнопка редактирования складов 
        /// </summary>
        private void WarhReg_Click(object sender, RoutedEventArgs e)
        {
            WarEdit wr = new WarEdit();
            wr.Show();

        }

        /// <summary>
        /// эта дичь закрывает окно настроек при нажатии на мышъ
        /// </summary>
        private void Close_settings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsGrid.IsEnabled = false; SettingsGrid.Visibility = Visibility.Hidden;
        }





        #endregion


    }
}