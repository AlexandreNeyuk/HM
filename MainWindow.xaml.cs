using ICSharpCode.AvalonEdit;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using OfficeOpenXml;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Brush = System.Windows.Media.Brush;
using Clipboard = System.Windows.Clipboard;
using Label = System.Windows.Controls.Label;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;


namespace HM
{
    public partial class MainWindow : Window
    {
        #region Global Переменные

        List<TextBox> KeyTextBoxies; // ///все поля для которых нужно свойство введения HotKeys, через  ",
        List<Canvas> MenuCanvas; // все элементы меню 
        List<Grid> grids; //все гриды окон

        Animations Animations = new Animations();
        DataBaseAsset dataBases;
        string statusConnect;
        bool SetPanel = false; //false - закрытая панель, true - открытая панлеь

        //Дешифрованные данные пользователя
        private string Decrypt_UserName;
        private string Decrypt_Password;

        //Зашифрованные данные пользователя полученные из реестра 
        private string Encrypt_UserName;
        private string Encrypt_Password;


        #endregion


        public MainWindow()
        {
            InitializeComponent();

            #region Начальные накстройки
            TB.Margin = new Thickness(0, 0, 0, 0); //выравниванеи TableControl 
            grids = new List<Grid>() { PartyGrid, HomeGrid, PostomatsGrid, SettingsGrid, StoreGrid, RunnerGrid }; ///включение в список всех гридов-окон
            HM.Title += " " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3); //версия в названии (менять в свовах проекта)
            TitleVersionText.Content = "v. " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            //сразу нажатое окно брони 
            BronbGrid.IsEnabled = true; Bronirovanie_Canvas.Background = (Brush)new BrushConverter().ConvertFrom("#FF808080"); BronbGrid.Visibility = Visibility.Visible; SyncEngyGrid.Visibility = Visibility.Hidden; SyncEngyGrid.IsEnabled = false; Sync_Canvas.Background = new SolidColorBrush(Colors.Transparent);
            #endregion

            #region Regisrty Staff Загрузка Настроек из реестра
            ///Пересоздание корня настроек в реестре + синхрон с реестром настроек--
            using RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
            using RegistryKey registry1 = Registry.CurrentUser.CreateSubKey(@"Software\HM\Hosts");
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Settings"))
            {
                if (key != null)
                {
                    Encrypt_UserName = key.GetValue("Имя пользователя")?.ToString();
                    Encrypt_Password = key.GetValue("Пароль")?.ToString();
                    TokenPost_PM.Text = key.GetValue("X-AUTH-TOKEN_PM")?.ToString();
                    TokenPost_Engy.Text = key.GetValue("X-AUTH-TOKEN_Engy")?.ToString();
                    SP_HotKey.Text = key.GetValue(SP_HotKey.Name)?.ToString();
                    Settings_HotKey.Text = key.GetValue(Settings_HotKey.Name)?.ToString();
                    Visual_ViewMenu.IsChecked = Convert.ToBoolean(key.GetValue("Settings_Visual_ViewMenu")?.ToString());
                }
            }
            #endregion

            #region Дешифровка данных пользователя полученных из реестра с помошью ключа безопасности 
            if ((Encrypt_UserName != null) && (Encrypt_Password != null))
            {//если есть данные о пользователе из реестра, то рабтаем дольше с ключом, если ключа нет то выведет сообщение 


                //получаю ключ из файла
                var decriptionkey = CriptoAES_Container.RetrieveEncryptionKey();
                if (decriptionkey != null) //если ключ существует
                {
                    //Дешифрую данные с помощью ключа
                    Decrypt_UserName = CriptoAES_Container.DecryptString(Encrypt_UserName, decriptionkey);
                    Decrypt_Password = CriptoAES_Container.DecryptString(Encrypt_Password, decriptionkey);

                    UserName.Text = Decrypt_UserName;
                    UserPass.Password = Decrypt_Password;

                }
            }
            else
            {//если нет данных о пользователе из реестра , то удаляем (если есть такая папка) ключ
                CriptoAES_Container.DeleteKeyFolder();
            }
            //если данные подгружены и расшифрованы, то кнопка "Удалить данные" будет доступна
            if ((UserName.Text == "" && UserPass.Password == ""))
                DeleteUserData.IsEnabled = false;
            else DeleteUserData.IsEnabled = true;





            #endregion

            #region Запуск Проверки соединения с БД по таймеру на основе тех данных что были дешифрованы в начале программы (эти данные обновляются при сохранении новых)
            CheckProtectConnect_Timer();

            /* dataBases = new DataBaseAsset(Decrypt_UserName, Decrypt_Password);
             dataBases.ProtectedConnection(this);*/
            #endregion

            #region добавление всех начальных функций обработок в Настройки
            #region Прописывыаю все HotKeys что при ESC очищались 
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

            //Привязываю ссылку на вопрос по токенам постомата какие вставлять в поле
            QstLB_POST.MouseDown += (a, e) => { Process.Start(new ProcessStartInfo("https://wiki.sblogistica.ru/display/LINE2SUPP/NEW+Back-end+ENGY+-+supportapi") { UseShellExecute = true }); };


            #endregion

            VisualMenu_Apply(); //применение сохраненных настроек к всплывающему меню

            #region Добавление обработки всех элементов меню (навердений мыши  и клики)
            MenuCanvas = new List<Canvas>() { SettingCanvas, PostomatsCanvas, PartyCanvas, Home, StoreCanvas, RunnerCanvas };
            foreach (var item in MenuCanvas)
            {
                item.MouseEnter += (a, e) => { item.Background = new SolidColorBrush(Colors.Gray); };
                item.MouseLeave += (a, e) => { item.Background = new SolidColorBrush(Colors.Transparent); };

            }
            ///Анимация вращения кнопки настроек 
            SettingCanvas.MouseEnter += (a, e) => { Animations.Animation_rotate_ON(1, settingImage_AnimatingEl); };
            SettingCanvas.MouseLeave += (a, e) => { Animations.Animation_rotate_OFF(settingImage_AnimatingEl); };

            ///обработка кликов на элементы меню
            PartyCanvas.MouseDown += (a, e) => { OpenGrid(PartyGrid); WarEdit wr = new WarEdit(); wr.LoadHosts(ListWarhouses); wr.Close(); };
            Home.MouseDown += (a, e) => { OpenGrid(HomeGrid); };
            PostomatsCanvas.MouseDown += (a, e) => { OpenGrid(PostomatsGrid); };
            SettingCanvas.MouseDown += (a, e) => { OpenGrid(SettingsGrid); };
            StoreCanvas.MouseDown += (a, e) => { OpenGrid(StoreGrid); };
            RunnerCanvas.MouseDown += (a, e) => { OpenGrid(RunnerGrid); };

            //обработка кликов на пункты меню постаматов
            Bronirovanie_Canvas.MouseDown += (a, e) => { BronbGrid.IsEnabled = true; Bronirovanie_Canvas.Background = (Brush)new BrushConverter().ConvertFrom("#FF808080"); BronbGrid.Visibility = Visibility.Visible; SyncEngyGrid.Visibility = Visibility.Hidden; SyncEngyGrid.IsEnabled = false; Sync_Canvas.Background = new SolidColorBrush(Colors.Transparent); };
            Sync_Canvas.MouseDown += (a, e) => { BronbGrid.IsEnabled = false; Bronirovanie_Canvas.Background = new SolidColorBrush(Colors.Transparent); BronbGrid.Visibility = Visibility.Hidden; SyncEngyGrid.Visibility = Visibility.Visible; SyncEngyGrid.IsEnabled = true; Sync_Canvas.Background = (Brush)new BrushConverter().ConvertFrom("#FF808080"); };
            cleanLog1.Click += (a, e) => { LogPOST1.Text = null; };
            ClearLog_sync.Click += (a, e) => { LogPOST2.Text = null; };
            Synchra_Button1.Click += (a, e) => { SynchronizationPackages(); };

            //Обработка кнопок в Склады-Импорт
            SearchWarh.Click += (a, e) => { SearchStoreinDB(SearchWH.Text); };
            #endregion

            OpenGrid(HomeGrid); //открытие начальной страницы
        }
        #region Своя панель окна

        /// <summary>
        ///Перемещение своей панели окна 
        /// </summary>
        private void TopPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        //Кнопка закрытия приложения
        private void ButtonClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void ButtonClose_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ButtonClose.Background = new SolidColorBrush(Colors.Red);
        }
        private void ButtonClose_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ButtonClose.Background = null;
        }

        //Кнопка сворачивания окна
        private void MinimalizeWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimalizeWindow.Background = new SolidColorBrush(Colors.Silver);
        }
        private void MinimalizeWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimalizeWindow.Background = null;
        }
        private void MinimalizeWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window window = Window.GetWindow(this);
            SystemCommands.MinimizeWindow(window);

        }


        //Установка свойсва TOPMOST для окна
        private void StatusBar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HM.Topmost = !HM.Topmost;
            if (!HM.Topmost) TitleAppText.Foreground = new SolidColorBrush(Colors.White);
            else TitleAppText.Foreground = BottomBar.Background;
        }

        #endregion Button_close

        #region Применеие  Фиксированного или плавующиего меню из настроек
        public async void VisualMenu_Apply()
        {

            if (Visual_ViewMenu.IsChecked == true)
            {
                //Фиксированное меню
                MajorGrid.Margin = new Thickness(40, MajorGrid.Margin.Top, MajorGrid.Margin.Right, MajorGrid.Margin.Bottom);
                for (double i = 1; i > 0; i = i - 0.1) { await Task.Delay(1); TitleMenu.Opacity = i; }

                Button_Open_SwitchMenu.IsEnabled = false;
                Button_Open_SwitchMenu.Opacity = 0.5;

            }
            else
            {
                //Плавующее меню
                MajorGrid.Margin = new Thickness(0, MajorGrid.Margin.Top, MajorGrid.Margin.Right, MajorGrid.Margin.Bottom);
                for (double i = 0; i < 1; i = i + 0.1) { await Task.Delay(1); TitleMenu.Opacity = i; }
                Button_Open_SwitchMenu.IsEnabled = true;
                Button_Open_SwitchMenu.Opacity = 1;

            }

        }
        #endregion


        #region CheckConnect

        /// <summary>
        ///Проверка соединения с БД по таймеру 
        /// </summary>
        public void CheckProtectConnect_Timer()
        {
            dataBases = new DataBaseAsset(Decrypt_UserName, Decrypt_Password);
            dataBases.ProtectedConnection(this);
        }

        /// <summary>
        ///обновление статуса подключения из другого потока 
        /// </summary>
        /// <param name="newText"></param>
        public void UpdateLabel(string newText)
        {
            // Получаем доступ к диспетчеру WPF UI
            Dispatcher.Invoke(() =>
            {
                // Обновляем текст элемента управления WPF Label
                Status_Con.Content = newText;
            });
        }


        #endregion


        #region Переключение пунктов меню метод - Переключение(открытие) Гридов 

        /// <summary>
        /// Выключение всех других гридов, кроме выбранного (Включение выбранного)
        /// </summary>
        /// <param name="CurrentChange">Выбранный грид</param>
        public void OpenGrid(Grid CurrentChange)
        {
            foreach (var item in grids)
            {
                if (item == CurrentChange)
                {
                    item.Visibility = Visibility.Visible;
                    item.IsEnabled = true;
                }
                else { item.Visibility = Visibility.Hidden; item.IsEnabled = false; }

            }
            if (Visual_ViewMenu.IsChecked == false) SwitchPanel();


        }

        #endregion

        #region Боковая панель

        /// <summary>
        ///Кнопка боковой панели 
        /// </summary>
        private async void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { SwitchPanel(); }
        async public void SwitchPanel()
        {
            const int marginLeftMin = 0;
            const int marginLeftMax = 150;
            double CurrentVarginLeft = MajorGrid.Margin.Left;

            TextBox.IsEnabled = false;
            ListOne.IsEnabled = false;
            ListTwo.IsEnabled = false;
            ListCopyElm.IsEnabled = false;



            ///анимация боковой панели  
            if (SetPanel == true)
            {
                while (MajorGrid.Margin.Left - MajorGrid.Margin.Left / 6 > marginLeftMin)
                {
                    CurrentVarginLeft = MajorGrid.Margin.Left;
                    await Task.Delay(1);
                    MajorGrid.Margin = new Thickness(CurrentVarginLeft - CurrentVarginLeft / 6, 0, 0, 0);
                    if (MajorGrid.Margin.Left < 2)
                    {
                        MajorGrid.Margin = new Thickness(0, 0, 0, 0);
                        break;
                    }
                }
                SetPanel = !SetPanel;

            }
            else
            {

                while (CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 6 < marginLeftMax)
                {
                    CurrentVarginLeft = MajorGrid.Margin.Left;
                    await Task.Delay(1);
                    MajorGrid.Margin = new Thickness(CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 6, 0, 0, 0);
                    if (MajorGrid.Margin.Left > 147)
                    {
                        MajorGrid.Margin = new Thickness(150, 0, 0, 0);
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



        #region Table Item 1   
        /// <summary>
        /// Саммая главная кнопка 
        /// </summary>
        async private void ListProcess_Click(object sender, RoutedEventArgs e)
        {

            if (ContextRP.IsChecked == true)
            {

                //по сенарию RP (simple_List)
                // TextBox.Text = TextBox.Text.Replace("RP", "");

                //чуть сложнее
                //List<string> stringsRP = To_List(TextBox);
                List<string> resilt = new List<string>();  //лист с цифрами от RP

                var pattern = @"RP\d+";
                var matches = Regex.Matches(TextBox.Text, pattern);
                foreach (Match match in matches)
                {
                    resilt.Add(match.Value.Replace("RP", ""));
                }


                /*  foreach (var item in stringsRP)
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

                  }*/
                resilt = resilt.Distinct().ToList();
                TextBox.Text = string.Join("\r\n", resilt);

            }
            else
            {
                string[] lines = TextBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                TextBox.Text = string.Join(Environment.NewLine, lines);
                List<string> SBC_strings = new List<string>();  //лист с SBC
                if (TextBox.Text.Contains("SBC"))
                {
                    List<string> SBCs = To_List(TextBox); // лист разделенный на строки из всего TextBox
                    foreach (var item in SBCs)
                    {
                        if (item.Contains("SBC"))
                        {
                            SBC_strings.Add("'" + item + "'"); //добавляю в основной лист с SBC
                        }
                    }
                    /*var pattern = @"SBC\d+";
                    var matches = Regex.Matches(TextBox.Text, pattern);
                    foreach (Match match in matches)
                    {
                        SBC_strings.Add("'" + match.Value + "'");
                    }
                    SBC_strings = SBC_strings.Distinct().ToList();*/

                }

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
                //Bce все SBC 
                if ((SBC_strings.Count > 0) && (SBC_strings[0] != ""))
                {
                    List<string> SBC_RP = dataBases.ConnectDB("Шиптор", $@"select package_id from package_barcode pb where main in ({string.Join(",\n", SBC_strings).ToUpper()})").AsEnumerable().Select(x => x[0].ToString()).ToList();
                    List<string> SBC_RP_surrogate = dataBases.ConnectDB("Шиптор", $@"select package_id from package_barcode pb where surrogate in ({string.Join(",\n", SBC_strings).ToUpper()})").AsEnumerable().Select(x => x[0].ToString()).ToList();
                    SBC_RP.AddRange(SBC_RP_surrogate);
                    SBC_RP = SBC_RP.Distinct().ToList();
                    if (TextBox.Text != "")
                        TextBox.Text = TextBox.Text + ",\n" + string.Join(",\n", SBC_RP);

                    else TextBox.Text = TextBox.Text + string.Join(",\n", SBC_RP);


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
                TextBox.Text = String.Join(",\n", AllFamily.Distinct());


            }
            //поиск родителей 
            if (AF_2.IsChecked == true && ContextRP.IsChecked == true)
            {
                List<string> ParrentsList = dataBases.ConnectDB("Шиптор", $@"select id, parent_id from package p where id  in ({TextBox.Text}) or parent_id in ({TextBox.Text})").AsEnumerable().Select(x => x[1].ToString()).ToList();
                foreach (var Parrent in ParrentsList)
                    if (Parrent != "") TextBox.Text += "\r\n" + Parrent;
                var list = To_List(TextBox);
                TextBox.Text = string.Join(",\n", list.Distinct());

            }

            //Удаление последней запятой из текста
            if (!string.IsNullOrEmpty(TextBox.Text))
            {
                TextBox.Text = TextBox.Text.TrimEnd(',', ' ', '\n');
            }

            //копирование в Буфер обмена
            try { if (TextBox.Text != "") Clipboard.SetText(TextBox.Text); } catch { }
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
            ConrextUpper.IsChecked = true;
            UpperSearch.IsEnabled = true;
            UpperSearch.IsChecked = true;

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

        /// <summary>
        ///Добавление RP к списку цыфр посылок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void AddRP_Click(object sender, RoutedEventArgs e)
        {
            List<string> result = new List<string>();

            // Разбиваем текст на строки по разделителю-запятой
            string[] arrStr = TextBox.Text.Split(new char[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Преобразуем массив строк в список
            List<string> numbers = new List<string>(arrStr);

            foreach (string number in numbers)
            {
                if (!string.IsNullOrEmpty(number))
                {
                    result.Add("RP" + number);
                }
            }
            TextBox.Text = string.Join(",\r\n", result);

            //копирование в Буфер обмена
            try { if (TextBox.Text != "") Clipboard.SetText(TextBox.Text); } catch { }
            BFcopy.Text = "Результат скопирован в буфер обмена";
            await Task.Delay(1000);
            BFcopy.Text = null;

        }
        /// <summary>
        ///Добавление апострафоф дл ПВЗ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void AddRPforPVZ_Click(object sender, RoutedEventArgs e)
        {

            List<string> result = new List<string>();

            // Разбиваем текст на строки по разделителю-запятой
            string[] arrStr = TextBox.Text.Split(new char[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Преобразуем массив строк в список
            List<string> numbers = new List<string>(arrStr);

            foreach (string number in numbers)
            {
                if (!string.IsNullOrEmpty(number))
                {
                    result.Add("'" + number + "'");
                }
            }
            TextBox.Text = string.Join(",\r\n", result);

            //копирование в Буфер обмена
            try { if (TextBox.Text != "") Clipboard.SetText(TextBox.Text); } catch { }
            BFcopy.Text = "Результат скопирован в буфер обмена";
            await Task.Delay(1000);
            BFcopy.Text = null;
        }

        #endregion

        #region Tab_Item 2    
        /// <summary>
        /// кнопка листов - выитание листов
        /// </summary>
        private void FidCopy_Click(object sender, RoutedEventArgs e)
        {

            FidCopyVoid();

        }

        //метод поиска разницы
        public void FidCopyVoid()
        {
            ListOne.Text = ListOne.Text.ToUpper();
            ListTwo.Text = ListTwo.Text.ToUpper();
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
            FidCopy_CopyVoid();
        }
        public void FidCopy_CopyVoid()
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

            if (Party.Text != "")
            {
                string paty = Party.Text; //партия 
                if (paty.Contains("R_RET")) paty = paty.Replace("R_RET", "");
                List<string> StockID = dataBases.ConnectDB("Шиптор", $@"select * from package_return pr where id in ({paty})").AsEnumerable().Select(x => x["stock_id"].ToString()).ToList();
                //проверка существования в шипторе И ИМЕНИ СКЛАДА в шипе и меняем в заголовке имени склада 
                List<string> StockName = new List<string>();
                if (StockID.Count == 1)
                {
                    //ищем имя склада в Шипторе
                    StockName = dataBases.ConnectDB("Шиптор", $@"SELECT id, ""name""  FROM public.warehouse where id = {StockID[0]}").AsEnumerable().Select(x => x[1].ToString()).ToList();
                    if (Name_War.Content != StockName[0]) if (StockName[0] != null) Name_War.Content = StockName[0]; else MessageBox.Show($@"Склада с id {StockID[0]} не найдено в Шиптор!");//меняем имя склада на то что нашлось

                    // если поле посылок заполнено  - работаем и с посылкми и с партией, если нет то только с партией
                    if (RP_Party.Text != "")
                    {
                        if (SelectAction_Party.SelectedIndex != 5)
                        {
                            //Добавление 
                            if (SelectAction.SelectedIndex == 0)
                            {

                                DataTable CurrentStock = dataBases.ConnectDB(StockName[0], $@"select id, status from package_return pr where return_fid = {paty}");
                                List<string> ParyStockID = CurrentStock.AsEnumerable().Select(x => x["id"].ToString()).ToList();
                                List<string> PartyStockStatus = CurrentStock.AsEnumerable().Select(x => x["status"].ToString()).ToList();
                                if (ParyStockID[0] != null)
                                {
                                    bool d = true;
                                    if (PartyStockStatus[0] == "sent" || PartyStockStatus[0] == "disbanded") if (MessageBox.Show($@"Партия на складе имеет статус {PartyStockStatus[0].ToUpper()}. Все равно добавить в партию?", "Добавить в паллету?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) d = false;
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

                            //Удаление
                            if (SelectAction.SelectedIndex == 1)
                            {

                                DataTable CurrentStock = dataBases.ConnectDB(StockName[0], $@"select id, status from package_return pr where return_fid = {paty}");//ERROR::Место вылета изза имени БД (точнее его отсутствии в твоих данных)
                                List<string> ParyStockID = CurrentStock.AsEnumerable().Select(x => x["id"].ToString()).ToList();
                                List<string> PartyStockStatus = CurrentStock.AsEnumerable().Select(x => x["status"].ToString()).ToList();
                                if (ParyStockID[0] != null) //если партия существует на складе
                                {
                                    //Удаление из паллеты

                                    if (RP_Party.Text != null)
                                    {
                                        string RPid = RP_Party.Text.Replace("\r\n", "");

                                        //удаляю из партии в шипторе и корректировка статусов посылок на "УПАКОВАНА"
                                        dataBases.ConnectDB("Шиптор", $@"UPDATE public.package SET return_id = NULL, current_status='packed', sent_at = NULL,  returned_at = NULL, returning_to_warehouse_at = NULL, packed_since = now()   WHERE id in ({RPid})");
                                        dataBases.ConnectDB("Шиптор", $@"UPDATE package_departure SET package_action = NULL  WHERE package_id in ({RPid})");

                                        //удаление из партии склада 
                                        dataBases.ConnectDB(StockName[0], $@"Delete from package_return_item where package_id in (select id from package p where package_fid in({RPid}))");

                                        //смена статусов в Заппстор после удаления из партии
                                        dataBases.ConnectDB(StockName[0], $@"UPDATE package SET status = 'in_store' where package_fid in ({RPid})");

                                        MessageBox.Show($@"Из партии удалено!");
                                        RP_Party.Text = null;
                                        Party.Text = null;




                                    }
                                    else MessageBox.Show("Поле для отправлений пусто! Добавьте отправления");

                                }
                                else MessageBox.Show($@"Партия не найдена на складе {StockName[0]}!");
                            }
                        }
                        ActionsForParty(StockName, paty);


                    }
                    else { ActionsForParty(StockName, paty); }//поле посылок устое - работаю только с партией



                }
                else MessageBox.Show("Такая партия не одна (!) или ее не существует в Шипторе!");


            }
            else MessageBox.Show("Поле партии не заполнено!");

        }

        /// <summary>
        /// Работа с партией в зависимости от выбора в комбобоксе по статусу партии
        /// </summary>
        /// <param name="Stock_name">Имя склада</param>
        /// <param name="party">Партия</param>
        public void ActionsForParty(List<string> Stock_name, string party)
        {
            //Корректрировка статуса партии
            if (Stock_name.Count == 1)//проверка существования в шипторе
            {
                switch (SelectAction_Party.SelectedIndex)
                {
                    case 0:
                        //Партию в "расформирована" - никакие другие действия недоступны, кроме расформирования партии + корректировка статусов посылок
                        // dataBases.ConnectDB();
                        break;
                    case 1: //выбрано "Собирается"
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'gathering' where return_fid in ({party})");
                        //dataBases.ConnectDB("Шиптор", $@"UPDATE package_return SET sent_at = NULL, SET delivered_at = NULL, SET cancelled_at = NULL, SET closed_at = NULL where id in ({party})");
                        MessageBox.Show("Статус партии скорректирован!");
                        break;
                    case 2: //Выбрано "собрана"
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'gathered' where return_fid in ({party})");
                        // dataBases.ConnectDB("Шиптор", $@"UPDATE package_return SET sent_at = NULL, SET delivered_at = NULL, SET cancelled_at = NULL, SET closed_at = NULL where id in ({party})");
                        MessageBox.Show("Статус партии скорректирован!");
                        break;
                    case 3: //Выбрано "Упакована"
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'packed'  where return_fid in ({party})");
                        //dataBases.ConnectDB("Шиптор", $@"UPDATE package_return SET sent_at = NULL, SET delivered_at = NULL, SET cancelled_at = NULL, SET closed_at = NULL where id in ({party})");
                        MessageBox.Show("Статус партии скорректирован!");
                        break;
                    case 4: //Выдана
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'delivered'  where return_fid in ({party})");
                        dataBases.ConnectDB("Шиптор", $@"UPDATE package_return SET delivered_at = now() where id in ({party})");
                        MessageBox.Show("Статус партии скорректирован!");
                        break;
                    case 5://расформирована
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'disbanded'  where return_fid in ({party})");
                        dataBases.ConnectDB("Шиптор", $@"UPDATE package_return  SET cancelled_at = now(), closed_at = now() where id in ({party})");

                        //найти и удалить все посылки из партии как в шипторе так и в запе и сделать им норм статусы
                        var Ret_Paty = dataBases.ConnectDB(Stock_name[0], $@"select * from package_return_item where package_return_id in (select id from package_return pr where return_fid in ({party}))");
                        List<string> package_id_fromRet_Paty = Ret_Paty.AsEnumerable().Select(x => x["package_id"].ToString()).ToList();
                        List<string> id_package_in_paty = Ret_Paty.AsEnumerable().Select(x => x["id"].ToString()).ToList();
                        //удаляю из партии отправления 
                        dataBases.ConnectDB(Stock_name[0], $@"DELETE FROM public.package_return_item WHERE id in ({string.Join(",", id_package_in_paty)});");
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package SET status = 'in_store' where id in ({string.Join(",", package_id_fromRet_Paty)})");

                        // поиск и удаление из партии и корректировка статусов в шиптор 
                        var RPfromParty = dataBases.ConnectDB("Шиптор", $@"select id, return_id from package p where return_id in ({party})");
                        dataBases.ConnectDB("Шиптор", $@"UPDATE public.package SET return_id = NULL, current_status = 'packed', sent_at = NULL  returned_at = NULL, returning_to_warehouse_at = NULL, packed_since = now()   WHERE return_id in ({party})");
                        dataBases.ConnectDB("Шиптор", $@"UPDATE package_departure SET package_action = NULL  WHERE package_id in ({string.Join(",", RPfromParty.AsEnumerable().Select(x => x["id"].ToString()).ToList())})");
                        MessageBox.Show("Статус партии скорректирован!");
                        break;

                    default:
                        break;
                }
            }
            else MessageBox.Show("Такая партия не одна (!) или ее не существует в Шипторе!");
        }

        /// <summary>
        /// Посик в листе Склада
        /// </summary>
        private void Search_Warh_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Search_Warh.Text != "")
            {
                foreach (var item in ListWarhouses.Items)
                    if (item.ToString().ToUpper().Contains(Search_Warh.Text.ToUpper()))
                    {
                        ListWarhouses.SelectedItem = item;
                        Name_War.Content = ListWarhouses.SelectedItem;
                        ListWarhouses.ScrollIntoView(ListWarhouses.Items.GetItemAt(ListWarhouses.SelectedIndex));


                    }
            }
        }

        /// <summary>
        /// Вывод выбранного склада 
        /// </summary>
        private void ListWarhouses_SelectionChanged(object sender, SelectionChangedEventArgs e) { Name_War.Content = ListWarhouses.SelectedItem; }

        /// <summary>
        /// Выбор "Расформировать"
        /// </summary>
        private void SelectAction_Party_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectAction_Party.SelectedIndex == 5)
            {
                //выбрано "Расформировать партию", значит дейсвия над посылками и список посылок становятся недоступны, т.к все посылки будут удалены из партии
                SelectAction.SelectedIndex = 1;
                SelectAction.IsEnabled = false;
                RP_Party.Text = $@"Все посылки будут удалены из партии,
Партия будет расформирована, как в Шиптор, так и на складе,
Посылкам будет присвоен статус 'Упакована' в Шипторе, 'На складе' - в Zappstore";
                RP_Party.IsEnabled = false;
            }
            else
            {
                //иначе включаем доступность всех этих элементов 
                SelectAction.IsEnabled = true;
                RP_Party.IsEnabled = true;
                if (RP_Party.Text.Contains("Все посылки будут удалены из партии")) RP_Party.Text = "";
            }
        }

        #endregion

        #region Tools

        /// <summary>
        /// Конвертер в лист строк из содержимого TextEditor
        /// </summary>
        /// <param name="tx">Поле для конвертации</param>
        /// <returns></returns>
        public List<string> To_List(TextEditor tx)
        {
            List<string> str = tx.Text.Split(Environment.NewLine).ToList();
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
            if (e.Key.ToString() == SP_HotKey.Text)
            { // открытие боковой панели по кнопке 
                if (Visual_ViewMenu.IsChecked == false) SwitchPanel();

            }
            if (e.Key.ToString() == Settings_HotKey.Text) OpenGrid(SettingsGrid);

        }

        #region Settings      


        #region UserData_crypto
        /// <summary>
        /// Сохранение данных пользователя
        /// </summary>
        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UserName.Text != "" && UserPass.Password != "")
            {
                EncryptSaveUserData();

                using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
                {
                    key?.SetValue("Имя пользователя", Encrypt_UserName);
                    key?.SetValue("Пароль", Encrypt_Password);
                }
                SussessLabel.Content = "Успешно";
                DeleteUserData.IsEnabled = true;

            }
            else SussessLabel.Content = "Не заполнено одно из полей!";

            await Task.Delay(1000);
            SussessLabel.Content = "";

        }

        /// <summary>
        /// Удаление старого файла ключа безопастности и создание зашифрованных данных с помоью ключа безопастности
        /// </summary>
        private void EncryptSaveUserData()
        {

            //генерация нового ключа 
            byte[] encryptionKey = CriptoAES_Container.GenerateRandomKey();

            //сохранение нового ключа безопастности в новый файл (или старый если он уже существет )
            CriptoAES_Container.SaveEncryptionKey(encryptionKey);

            //шифрую данные с помощью нового ключа и сохраняю в переменные
            Encrypt_UserName = CriptoAES_Container.EncryptString(UserName.Text, encryptionKey);
            Encrypt_Password = CriptoAES_Container.EncryptString(UserPass.Password, encryptionKey);

        }

        /// <summary>
        /// Удаление данных пользователя из системы
        /// </summary>
        private void DeleteKeyFolder_method(object sender, RoutedEventArgs e)
        {
            //удаленрие данных пользователя из реестра 
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Settings", true))
            {
                if (key != null)
                {
                    key.DeleteValue("Имя пользователя", false);
                    key.DeleteValue("Пароль", false);
                }
            }
            //удаление папки с ключом
            CriptoAES_Container.DeleteKeyFolder();
            //ОЧИСТКА ПОЛЕЙ 
            UserName.Text = "";
            UserPass.Password = "";

            MessageBox.Show("Данные пользователя успешно удалены из системы");

        }
        #endregion

        /// <summary>
        /// функйция проверки полей и записи в реестр / не используется 
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
            OpenGrid(HomeGrid);

        }

        /// <summary>
        ///Сохранение данных об Токене для POST
        /// </summary>
        async private void SavePostToken(object sender, RoutedEventArgs e)
        {
            if (TokenPost_PM.Text != "" && TokenPost_Engy.Text != "")
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
                {
                    key?.SetValue("X-AUTH-TOKEN_PM", TokenPost_PM.Text);
                    key?.SetValue("X-AUTH-TOKEN_Engy", TokenPost_Engy.Text);
                }
                Save_indicatorPost.Content = "Успешно";

            }
            else Save_indicatorPost.Content = "Не заполнено поле";

            await Task.Delay(1000);
            Save_indicatorPost.Content = "";

        }

        /// <summary>
        ///Сохранение визуальных настроек и приминение сразу же
        /// </summary>
        private void Visual_ViewMenu_Click(object sender, RoutedEventArgs e)
        {
            VisualMenu_Apply();
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
            {
                key?.SetValue("Settings_Visual_ViewMenu", Visual_ViewMenu.IsChecked);
            }
        }




        #endregion

        #region Postomats
        Post post = new Post();
        #endregion

        #region knopka_body
        /// <summary>
        /// Кнопка изменения BODY и ссылки для постмана
        /// </summary>
        private void izmenit_body(object sender, RoutedEventArgs e)
        { 

        }


        #endregion

        #region Bronirovanie
        /// <summary>
        /// Кнопка "Забронировать" + обновить вгх !!!пока не синхронная!!!
        /// </summary> 
        private void Bronirovat_Button_Click(object sender, RoutedEventArgs e)
        { ///~~~ добавить в логику условие что возвращаемое значение Тасков POST не равно null и действовать далее !!!
            LogPOST1.Text = "";

            //проверка наличия вгх в строках формы
            if (Whide_rp.Text != "" && leagh_RP.Text != "" && Ves_RP.Text != "" && hiegh_rp.Text != "")
            { //обновляем вгх и пишем в лог
                dataBases.ConnectDB("Шиптор", $@"update package_departure set postamat_queued_at = now(),postamat_sync_completed_at = now(), linked_with_postamat_at = now() where package_id in ({RP_child.Text})");
                LogPOST1.Text = "Привязано к постамату.\n";
                LogPOST1.Text += "Обновление размеров:\n"; post.UpdateVGH(RP_child.Text, LogPOST1); LogPOST1.Text += "\n ----Конец запроса----";
                LogPOST1.Text += "\nОтвязать от ПМ:\n"; post.unlinkPackage(RP_child.Text, LogPOST1); LogPOST1.Text += "\n ----Конец запроса----";
            }

            LogPOST1.Text += "\nОтвязать от ПМ: \n"; post.unlinkPackage(RP_child.Text, LogPOST1); LogPOST1.Text += " \n ----Конец запроса----";
            //бронировать и писать в лог
            LogPOST1.Text += "\nБронирование 1:\n"; post.enqueue(RP_child.Text, LogPOST1); LogPOST1.Text += "\n ----Конец запроса----";
            LogPOST1.Text += "\nБронирование 2:\n"; post.bookDestinationCell(RP_child.Text, LogPOST1); LogPOST1.Text += " \n ----Конец запроса----";

        }


        #endregion

        #region SyncPackages


        /// <summary>
        ///Кнопка синхронизации посылки с ПМ из engy
        /// </summary>
        public async void SynchronizationPackages()
        {
            ///1. Есть ли он в ПМ
            ///Есть:
            ///1.1 Делаю статус такой же как в Engy:
            /// Заложено в Engy: 
            ///1. В "готова к загрузке" через БД
            ///2. Добавляю ID в ячейку постамата, выкинув оттуда "мусор"
            ///3. Методом в Заложено
            ///4.Галочка отправки смс,если да то отправить https://pm.shiptor.ru/admin/parcel/delivery-sms/resend/--id_parcel и открываю страницу с смс сразу с поиском по номеру из бд
            ///Нет: ``Отдельный void`` {
            ///2.1 Прохожу методами:
            ///1 В новую из ожидает +
            ///2.Смена яч у заброненной на 0, потом на нужную из поля 
            ///3.В заложено постоматы 2.1}
            ///Дальше все тоже самое что и в п.1

            LogPOST2.Text = null;
            var foundID = dataBases.ConnectDB("ПМ", $@"select id from parcel p where external_id in ('{RP_child_sync.Text}')");
            if (foundID.AsEnumerable().Any() == true)
            {
                LogPOST2.Text += "\nПосылка есть в ПМ, синхронизирую с ENGY:\n";
                //посылка есть в пм - вызываю метод синхры с энджи по статусу выбраному в проге
                switch (StatusPackagePM.SelectedIndex)
                {

                    case 0:
                        //готова к загрузке
                        dataBases.ConnectDB("ПМ", $@"update parcel set state = 'ready_for_load' where external_id in ('{RP_child_sync.Text}')");
                        LogPOST2.Text += "Cтатус скорректирован на 'ready_for_load'\n";

                        //!!остается добавить только в яч!!
                        break;
                    case 1:
                        //Заложено
                        Z_SMS_PM();
                        break;
                }
            }
            else
            {//Посылки нет в ПМ
                LogPOST2.Text += "\nПосылки нет в ПМ, прогоняю группу методов Engy:\n";
                //проверить свободна ли ячейка, очистить, сохранив посылку
                var cells_ISfree = dataBases.ConnectDB("ПМ", $@"select current_parcel_id, waiting_parcel_id,  c.id from cell c	join postamat p on p.id = c.postamat_id		where p.serial_number = '{PostomatPS.Text}'	and hardware_number in ({PS_cell.Text})").AsEnumerable().Select(x => x[0].ToString()).ToList();
                string cur_cell = cells_ISfree[0];
                string waiting_cell = cells_ISfree[1];
                string id_post = cells_ISfree[2];

                if ((cur_cell != null) || (waiting_cell != null))
                { //ТЕК Яч или ожидающая чем то заняты, надо сохранить и чистить
                    LogPOST2.Text += $@"В нужной ячейке лежала посылка подкидыш с id для ПМ: {cur_cell},{waiting_cell}";
                    dataBases.ConnectDB("ПМ", $@"UPDATE public.cell	SET current_parcel_id=NULL,waiting_parcel_id = NULL	WHERE id={id_post};");
                }
                post.MethodsSynchr_Engy(RP_child_sync.Text, int.Parse(PS_cell.Text), LogPOST2);
                Z_SMS_PM();
            }

        }
        /// <summary>
        ///Метод закладки в пм и отправки смсc
        /// </summary>
        public void Z_SMS_PM()
        { //1 проверяю есть ли посылка в пм снова до внесения изменений
            var foundID = dataBases.ConnectDB("ПМ", $@"select id from parcel p where external_id in ('{RP_child_sync.Text}')");
            if (foundID.AsEnumerable().Any() == true)

            {
                string RP_id_PM = dataBases.ConnectDB("ПМ", $@"select id from parcel p where external_id in ('{RP_child_sync.Text}')").AsEnumerable().Select(x => x[0].ToString()).ToList()[0];
                LogPOST2.Text += "\n Посылка появилась в ПМ! \n";
                dataBases.ConnectDB("ПМ", $@"update parcel set state = 'ready_for_load' where external_id in ('{RP_child_sync.Text}')");

                string id_postamata = dataBases.ConnectDB("ПМ", $@"select current_parcel_id, waiting_parcel_id,  c.id from cell c	join postamat p on p.id = c.postamat_id		where p.serial_number = '{PostomatPS.Text}'	and hardware_number in ({PS_cell.Text})").AsEnumerable().Select(x => x[0].ToString()).ToList()[2];
                dataBases.ConnectDB("ПМ", $@"UPDATE public.cell	SET waiting_parcel_id = {RP_id_PM}	WHERE id={id_postamata};");

                string Last_eventId = dataBases.ConnectDB("ПМ", $@"select current_parcel_id, waiting_parcel_id,  c.id, last_event_id from cell c	join postamat p on p.id = c.postamat_id		where p.serial_number = '{PostomatPS.Text}'	and hardware_number in ({PS_cell.Text})").AsEnumerable().Select(x => x[0].ToString()).ToList()[3];


                post.Load_PM_CELL(PostomatPS.Text, int.Parse(PS_cell.Text), int.Parse(Last_eventId) + 1, LogPOST2);

                string phone_search = dataBases.ConnectDB("ПМ", $@"select recipient_phone from parcel p where external_id in ('{RP_child_sync.Text}')").AsEnumerable().Select(x => x[0].ToString()).ToList()[0];

                if (SMS_check.IsChecked == true)
                {


                    try
                    {
                        Process.Start("https://pm.shiptor.ru/admin/parcel/delivery-sms/resend/" + RP_id_PM);
                        Process.Start("https://shiptor.ru/control/search/sms?q=%2B" + phone_search.Replace("+", ""));

                    }
                    catch
                    {

                        MessageBox.Show("не удалось открыть браузер");

                    }
                }


            }
            else
            {
                LogPOST2.Text += "\nПосылка не появилась в ПМ. Проверьте лог выше, возможно один из методов вернул отрицательный результат";
            }


        }

        #endregion

        

        #region MGK

        /// <summary>
        /// Clear Button
        /// </summary>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //очищать все потоки и поля на MGK
            ListT1.Items.Clear();
            ListT2.Items.Clear();
            ListT3.Items.Clear();
            ListT4.Items.Clear();
            ListT5.Items.Clear();
            ListT6.Items.Clear();
            ExcelNameFile.Text = null;
            ALL_GM.Text = null;
            FoundedLimeted.Content = "Найдено: ";
        }

        /// <summary>
        ///Выбор папки расположения файла (если не выбиралась, то по умолчанию Desktop)
        /// </summary>
        private void SelectFolderHashes_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();

            if (dialog.ShowDialog() == true)
                pathHashes = dialog.SelectedPath;
            SelectedFolderHashes.Content = Path.GetFileName(pathHashes);

        }

        /// <summary>
        ///Кнопка "Запуск" на странице MGK (сортировка ГМ, поиск по бд, формирование списков для потоков и запуск потоков) (все, до запуска потоков!!!)
        /// </summary>
        private void GO_GK_Click(object sender, RoutedEventArgs e)
        {
            ListT1.Items.Clear();
            ListT2.Items.Clear();
            ListT3.Items.Clear();
            ListT4.Items.Clear();
            ListT5.Items.Clear();
            ListT6.Items.Clear();
            FoundedLimeted.Content = "Найдено: ";

            GO_GK.IsEnabled = false; //Отключение кнопки "Запуск"
            StopRunnerGK_Button.IsEnabled = true; //Включение кнопки "Стоп"
            StopRunnerGK_Button.Background = (Brush)new BrushConverter().ConvertFrom("#FFC51308");
            StopRunnerGK_Button.Foreground = new SolidColorBrush(Colors.White);

            if (ExcelNameFile.Text == "") ExcelNameFile.Text = "Hashes" + "_" + DateTime.Now.ToString("yyyy_MM_dd HH_mm");
            List<string> listRP_GK = new List<string>(To_List(ALL_GM)); //лист со всем "добром" (и шк и RP)
            List<string> SHK;//Лист с ШК
            List<string> RP_fromSHK;//Лист с найденными RP из ШК
            List<string> SHK_fromDB; // Лист с ШК из БД

            //Для файла
            NoFound = new List<string>(); //Список ненайденных

            //Убираем все пустые строки
            string[] lines = ALL_GM.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            ALL_GM.Text = string.Join(Environment.NewLine, lines);

            // • отделение RP от ШК
            curRP = new List<string>();
            var pattern = @"RP\d+";
            var matches = Regex.Matches(ALL_GM.Text, pattern);
            //вычитаем из всего списка RP
            ListOne.Text = string.Join("\r\n", listRP_GK);
            ListTwo.Text = string.Join("\r\n", matches);
            FidCopyVoid();
            SHK = new List<string>(To_List(ListOne));
            FidCopy_CopyVoid();
            foreach (Match match in matches) { curRP.Add(match.Value.Replace("RP", "")); }
            curRP = curRP.Distinct().ToList();

            //•Из ШК находим RP 
            TextBox.Text = string.Join(Environment.NewLine, SHK);
            if ((SHK.Count > 0) && (SHK[0] != ""))
            {
                ConrextUpper_Checked(sender, e);
                ListProcess_Click(sender, e);

            }
            RP_fromSHK = new List<string>(TextBox.Text.Split(",\n").ToList());
            Clear_all_textB_Click(sender, e);
            if (ALL_GM.LineCount <= 10) //если в поле менее 10 строк (отправленмий), то автоматически ставится 1 поток
                ThreadsSelect.SelectedIndex = 0;





            //• Найти ненайденные по ШК
            if ((RP_fromSHK.Count > 0) && (RP_fromSHK[0] != ""))
            {

                SHK_fromDB = dataBases.ConnectDB("Шиптор", $@"select id, external_id  from package p where id  in ({string.Join(",", RP_fromSHK)})").AsEnumerable().Select(x => x[1].ToString()).ToList();
                ListOne.Text = string.Join("\r\n", SHK);
                ListTwo.Text = string.Join("\r\n", SHK_fromDB);
                FidCopyVoid();
                NoFound = new List<string>(To_List(ListOne));//Не найденные по ШК
                FidCopy_CopyVoid();
            }
            //Также находим ненайденные по RP из БД 
            if (curRP.Count > 0)
            {
                ListOne.Text = string.Join(Environment.NewLine, curRP);
                ListTwo.Text = string.Join(Environment.NewLine, dataBases.ConnectDB("Шиптор", $@"select id, external_id  from package p where id  in ({string.Join(",", curRP)})").AsEnumerable().Select(x => x[0].ToString()).ToList());
                FidCopyVoid();
                if (ListOne.Text != "") { NoFound.AddRange(To_List(ListOne)); }
                FidCopy_CopyVoid();
            }

            //• Складываем списки RP (если не пустые!) и чистим по ним хеши
            if ((RP_fromSHK.Count > 0) && (RP_fromSHK[0] != ""))
                curRP.AddRange(RP_fromSHK); // обьединяем списки RP
            if ((curRP.Count > 0) && (curRP[0] != "")) //Если общий список RP не пустой!
            {
                dataBases.ConnectDB("Шиптор", $@"UPDATE public.package_sorter_data SET package_create_hash=NULL, package_merge_hash=NULL WHERE package_id in ({string.Join(",", curRP)})").AsEnumerable().Select(x => x[1].ToString()).ToList();
                FoundedLimeted.Content = "Найдено: " + curRP.Count.ToString();

                if (fromSAP_radioButton.IsChecked == true)
                {//использую 7-ми дневное ограничение 

                    curRP = dataBases.ConnectDB("Шиптор", $@"select id, created_at from package where id in ({string.Join(",", curRP)}) and created_at >= current_date - interval '7 days'").AsEnumerable().Select(x => x[0].ToString()).ToList();

                    FoundedLimeted.Content = "Найдено: " + curRP.Count.ToString();
                }

                //•Делим списки RP для нашего числа потоков 
                int chunkSize = curRP.Count / (ThreadsSelect.SelectedIndex + 1);
                GrThreads = (ThreadsSelect.SelectedIndex + 1);
                curThread = 0;
                List<List<string>> chunks = new List<List<string>>();
                for (int i = 0; i < curRP.Count; i += chunkSize)
                {
                    if ((curRP.Count - (chunkSize * (chunks.Count + 1))) < curRP.Count / 10)
                    {

                        //берем все до конца с текущего I 
                        chunks.Add(curRP.GetRange(i, curRP.Count - i));
                        break;
                    }
                    else
                    {
                        chunks.Add(curRP.GetRange(i, Math.Min(chunkSize, curRP.Count - i)));
                    }

                }

                //• Запуск раннеров
                for (int i = 0; i < (ThreadsSelect.SelectedIndex + 1); i++)
                {
                    RuunerPosts(i, chunks);

                }

            }
            else
            {
                MessageBox.Show("По списку ничего не найдено в системе! Раннер не будет запущен! ");
                GO_GK.IsEnabled = true; //Bключение кнопки "Запуск"
                StopRunnerGK_Button.IsEnabled = false; //Отключение кнопки "Стоп"
                StopRunnerGK_Button.Background = (Brush)new BrushConverter().ConvertFrom("#FFDDDDDD");
                StopRunnerGK_Button.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF696969");
            }


        }

        //• Местные Общие Переменные
        int curThread = 0; // текущее количество выполненных потоков (++ после выполнения каждого из потоков, пока не достигнет числа)
        int GrThreads; //Общее количество запущенных потоков (присваивается до начала распределения потоков)
        List<string> NoFound;
        List<string> curRP;//лист с цифрами от RP отделенный от ШК, в дальнейшем полный список найденных RP 
        string pathHashes = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        bool isStopped = false; // если будет остановлен раннен станет True, если нет то False

        /// <summary>
        /// Чек финала потоков и создание файла по шаблону
        /// </summary>
        void checkFinallyThreadsAndCreateFile()
        {
            curThread++;
            if (curThread == GrThreads)
            {
                if (isStopped == false)
                {
                    //кнопка остановки не прожималась, работаем в штатном режиме
                    //идем дальше проверять хеши и создавать файлик

                    List<string> NoLoadHash = dataBases.ConnectDB("Шиптор", $@"select package_id,package_create_hash from public.package_sorter_data where package_id in ({string.Join(",", curRP)}) and package_create_hash is null").AsEnumerable().Select(x => x[0].ToString()).ToList();
                    List<string> LoadedHash = dataBases.ConnectDB("Шиптор", $@"select package_id,package_create_hash from public.package_sorter_data where package_id in ({string.Join(",", curRP)}) and package_create_hash is not null").AsEnumerable().Select(x => x[0].ToString()).ToList();

                    var NoLoadRP_Status = new DataTable();
                    if ((NoLoadHash.Count > 0) && (NoLoadHash[0] != ""))
                        NoLoadRP_Status = dataBases.ConnectDB("Шиптор", $@"select  id, current_status from package p where id in ({string.Join(",", NoLoadHash)}) ");

                    List<string> NoLoadRP = NoLoadRP_Status.AsEnumerable().Select(x => x[0].ToString()).ToList();
                    List<string> NoLoadStatus = NoLoadRP_Status.AsEnumerable().Select(x => x[1].ToString()).ToList();

                    //•находим также все имеющиеся в Шиптор, но не созданных в таблице в хешами и добавлячем к общим спискам с непрогруженными
                    List<string> ShipHaveButNotInHashesTable;
                    List<string> Buf = LoadedHash.Concat(NoLoadRP).ToList(); //это все что есть в таблице с хешами вместе

                    ListOne.Text = string.Join("\r\n", curRP);
                    ListTwo.Text = string.Join("\r\n", Buf);
                    FidCopyVoid();
                    ShipHaveButNotInHashesTable = new List<string>(To_List(ListOne));
                    FidCopy_CopyVoid();

                    var DopNoLoaded = new DataTable();
                    if ((ShipHaveButNotInHashesTable.Count > 0) && (ShipHaveButNotInHashesTable[0] != ""))
                        DopNoLoaded = dataBases.ConnectDB("Шиптор", $@"select  id, current_status from package p where id in ({string.Join(",", ShipHaveButNotInHashesTable)}) ");
                    List<string> DopNoLoadRP = DopNoLoaded.AsEnumerable().Select(x => x[0].ToString()).ToList();
                    List<string> DopNoLoadStatus = DopNoLoaded.AsEnumerable().Select(x => x[1].ToString()).ToList();

                    NoLoadRP.AddRange(DopNoLoadRP);
                    NoLoadStatus.AddRange(DopNoLoadStatus);

                    //•Создаем файлик Excel
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var file = new FileInfo(pathHashes + @"\" + ExcelNameFile.Text + ".xlsx");
                    using (var package = new ExcelPackage(file))
                    {
                        // Добавление нового листа
                        var worksheet = package.Workbook.Worksheets["L_L"];

                        if (worksheet != null)
                        {
                            package.Workbook.Worksheets.Delete(worksheet);
                        }
                        worksheet = package.Workbook.Worksheets.Add("L_L");


                        //Создаем шаблон
                        worksheet.Cells["A1"].Value = "Прогруженные";
                        worksheet.Cells["C1"].Value = "Не прогруженные";
                        worksheet.Cells["D1"].Value = "Статус";
                        worksheet.Cells["F1"].Value = "Не найденные в Шиптор";

                        if (LoadedHash.Count <= 2) LoadedHash.AddRange(new[] { "", "", "", "" });
                        if (NoLoadRP.Count <= 2) NoLoadRP.AddRange(new[] { "", "", "", "" });
                        if (NoLoadStatus.Count <= 2) NoLoadStatus.AddRange(new[] { "", "", "", "" });
                        if (NoFound.Count <= 2) NoFound.AddRange(new[] { "", "", "", "" });

                        // Запись значений в колонку "Прогруженных"
                        var A_Col = worksheet.Cells["A2:A" + LoadedHash.Count]; ///ERROR::необходимо сделать проверку по колву элементов, т.к когда они 0 то вылетает программа (А0 - не существует в ядре экселя)!!!!! 
                        A_Col.LoadFromCollection(LoadedHash);
                        // Запись значений в колонку "Не прогруженных"
                        var C_Col = worksheet.Cells["C2:C" + NoLoadRP.Count];//ERROR::если этот count 0 kb 1 или 2 - то что ниже или равно 2 то по ядру excel идет ошибка, т.к нумерацию необходимо корректировать 
                        C_Col.LoadFromCollection(NoLoadRP);
                        // Запись значений в колонку "Статус"
                        var D_Col = worksheet.Cells["D2:D" + NoLoadStatus.Count];
                        D_Col.LoadFromCollection(NoLoadStatus);
                        // Запись значений в колонку "Не найденные в Шиптор"
                        var F_Col = worksheet.Cells["F2:F" + NoFound.Count]; //ERROR:: если не найденное будет равно 1 то будет все ломаться из-за ядра excel
                        F_Col.LoadFromCollection(NoFound);

                        // Сохранение файла
                        try
                        {
                            package.Save();
                        }
                        catch (Exception)
                        {

                            MessageBox.Show("Файл открыт в другом процессе! Файл сохранен не будет, его придется собирать самим! ВАЖНО: это сообщение касается ТОЛЬКО файла!", "Предупреждение");

                        }

                    }

                    //• Звук уведомление о финале файла
                    using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                    using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                        new SoundPlayer(gzOut).Play();
                    GO_GK.IsEnabled = true;
                    StopRunnerGK_Button.IsEnabled = false;

                    //• открываем проводник и выделяем наш файл в нем
                    string filePath = Path.Combine(pathHashes, ExcelNameFile.Text + ".xlsx");
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo()
                    {// Передаем команду открытия и выделения файла
                        FileName = "explorer.exe",
                        Arguments = $"/select, /order, \"date\", \"{filePath}\""
                    };
                    // Запускаем процесс
                    process.Start();
                }
                else
                {
                    //кнопка остановки прожималась и остановились все потоки 
                    MessageBox.Show("Раннер остановлен");
                    GO_GK.IsEnabled = true;
                    StopRunnerGK_Button.IsEnabled = false;
                    isStopped = false;

                }

            }
        }

        /// <summary>
        /// Раннер
        /// </summary>
        /// <param name="Thread">Поток</param>
        /// <param name="chunks">Чанк посылок</param>
        async void RuunerPosts(int Thread, List<List<string>> chunks)
        {
            //выбираем ЛисВью в который будем писать 
            ListBox MylistThread = ListT1;
            switch (Thread)
            {
                case 0: MylistThread = ListT1; break;
                case 1: MylistThread = ListT2; break;
                case 2: MylistThread = ListT3; break;
                case 3: MylistThread = ListT4; break;
                case 4: MylistThread = ListT5; break;
                case 5: MylistThread = ListT6; break;

            }

            for (int i = 0; i < chunks[Thread].Count; i++)
            {
                if (isStopped == false)
                {// раннен не был остановлен

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (var httpClient = new HttpClient())
                    {
                        var httpContent = new StringContent($@"{{
                                                                ""id"": ""JsonRpcClient.js"",
                                                                ""jsonrpc"": ""2.0"",
                                                                ""method"": ""sapCreatePackage"",
                                                                ""params"": [{chunks[Thread][i]}]
                                                            }}");
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                        using (var response = await httpClient.PostAsync("https://api.shiptor.ru/system/v1?key=SemEd5DexEk7Ub2YrVuyNavNutEh4Uh8TerSuwEnMev", httpContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {

                                ///получать сам ответ от запроса для логирования

                                await response.Content.ReadAsStringAsync();
                                stopwatch.Stop();
                                Label label = new Label();
                                label.Content = $"Iteration {i + 1} success. Response time: {stopwatch.ElapsedMilliseconds} ms";
                                MylistThread.Items.Add(label);


                            }
                            else
                            {

                                var responseContent = await response.Content.ReadAsStringAsync();
                                // $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";
                                stopwatch.Stop();
                                Label label = new Label();
                                label.Content = $"Iteration {i + 1}. Error:{response.StatusCode}; Error message: {responseContent}.  Response time: {stopwatch.ElapsedMilliseconds} ms";
                                MylistThread.Items.Add(label);


                            }
                            //return await response.Content.ReadAsStringAsync();
                        }
                    }
                    MylistThread.ScrollIntoView(MylistThread.Items.GetItemAt(i));
                }
                else
                {
                    //раннер будет остановлен

                    break;

                }
            }
            checkFinallyThreadsAndCreateFile();

        }

        /// <summary>
        /// Кнопочка "Стоп", останавливающая раннер ГК
        /// </summary>
        private void StopRunnerGK_Button_Click(object sender, RoutedEventArgs e)
        {
            isStopped = true;

        }





        #endregion

        #region Store

        #region Import
        DataTable data; //таблица со складами из поиска
        int selected_id_warh; //выбранный ID выбранного склада 

        /// <summary>
        ///Поиск склада и отображение его в таблице складов с адресом и slug
        /// </summary>
        /// <param name="TextSearchWarh">Тескт из поля поиска</param>
        public void SearchStoreinDB(string TextSearchWarh)
        {
            data = dataBases.ConnectDB("Шиптор", $@"select id, slug, name, address from warehouse where name  ilike ('%{TextSearchWarh}%') or slug ilike ('%{TextSearchWarh}%') or address ilike ('%{TextSearchWarh}%') order by name");
            WarhausesTable.ItemsSource = data.DefaultView;

        }
        /// <summary>
        /// Отобрадение выборанного имени и ID нужной строки склада
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WarhausesTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WarhausesTable.SelectedItem != null)
            {
                DataRowView selectedRow = WarhausesTable.SelectedItem as DataRowView;
                object cellValue = selectedRow["name"];
                selected_id_warh = (int)selectedRow["id"]; //полученпие ID выбранного склада 
                Selected_NameWarh.Content = cellValue.ToString(); //вывод имени склада в Лабел

            }
        }
        /// <summary>
        /// обработка нажатия на Enter в поле поиска склада (чтобы не жать кнопку каждый раз)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchWH_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchStoreinDB(SearchWH.Text); // Вызываем событие Click для yourButton
                e.Handled = true; // Отменяем стандартное действие клавиши Enter
            }
        }

        /// <summary>
        /// 1.Функции обновления посылыок в шипторе (подготовка посылок в Шиптор для импорта)
        /// </summary>
        /// <param name="txBx">TextBox - ID посылок для импорта, через запятую</param>
        public void UpdatesShiptor_InportStore(TextBox txBx)
        {
            dataBases.ConnectDB("Шиптор", $@"update package set current_warehouse_id = {selected_id_warh},next_warehouse_id = {selected_id_warh} where id in ({txBx.Text})");
            dataBases.ConnectDB("Шиптор", $@"update package set ems_execution_mode = false where id in ({txBx.Text}) and ems_execution_mode = true");

        }





        #endregion

        #region CSM

        /// <summary>
        /// Тут мы кидаем апдейт на склады в шиптор (CSM)
        /// </summary>
        /// <param name="nomera">nomera это ID РПшек через запятые</param>
        void update_shiptor_for_treck_number(TextBox nomera)
        {
            dataBases.ConnectDB("Шиптор", $@"update package set current_warehouse_id = destination_warehouse_id, next_warehouse_id = destination_warehouse_id, current_status = 'packed', sent_at = NULL, returned_at = null, returning_to_warehouse_at = null, delivery_point_accepted_at = null, delivered_at = null, removed_at = null, lost_at = null, in_store_since = now(), measured_at = now(), packed_since = now(), prepared_to_send_since = now() where id in ({nomera.Text})");

        }


        #endregion

        #region Runner_Perepodgotovka_Import

        /* /// <summary>
         ///Запуск раннера с импортом и переподготовкой на склад
         /// </summary>
         /// <param name="txBx">TextBox - ID посылок для импорта, через запятую</param>
         public void PostRun_Import(TextBox txBx)
         {
             //удалить запятые и начать импорт
             List<string> list_RP = new List<string>(); // список номеров посылок без запятых

             // Разбиваем текст на строки по разделителю-запятой
             string[] arrStr = TextBox.Text.Split(new char[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

             // Преобразуем массив строк в список
             List<string> numbers = new List<string>(arrStr);

             foreach (string number in numbers)
             {
                 if (!string.IsNullOrEmpty(number))
                 {
                     list_RP.Add(number.Replace(",", ""));
                 }
             }

             //есть список для импорта в апи




             for (int i = 0; i < list_RP.Count; i++)
             {
                 if (isStopped == false)
                 {// раннен не был остановлен

                     Stopwatch stopwatch = new Stopwatch();
                     stopwatch.Start();
                     using (var httpClient = new HttpClient())
                     {
                         var httpContent = new StringContent($@"{{
                                                                   ""id"": ""JsonRpcClient.js"",
                                                                   ""jsonrpc"": ""2.0"",
                                                                   ""method"": ""sapCreatePackage"",
                                                                   ""params"": [{chunks[Thread][i]}]
                                                               }}");
                         httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                         using (var response = await httpClient.PostAsync("https://api.shiptor.ru/system/v1?key=SemEd5DexEk7Ub2YrVuyNavNutEh4Uh8TerSuwEnMev", httpContent))
                         {
                             if (response.IsSuccessStatusCode)
                             {

                                 ///получать сам ответ от запроса для логирования

                                 await response.Content.ReadAsStringAsync();
                                 stopwatch.Stop();
                                 Label label = new Label();
                                 label.Content = $"Iteration {i + 1} success. Response time: {stopwatch.ElapsedMilliseconds} ms";
                                 MylistThread.Items.Add(label);


                             }
                             else
                             {

                                 var responseContent = await response.Content.ReadAsStringAsync();
                                 // $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";
                                 stopwatch.Stop();
                                 Label label = new Label();
                                 label.Content = $"Iteration {i + 1}. Error:{response.StatusCode}; Error message: {responseContent}.  Response time: {stopwatch.ElapsedMilliseconds} ms";
                                 MylistThread.Items.Add(label);


                             }
                             //return await response.Content.ReadAsStringAsync();
                         }
                     }

                 }
                 else
                 {
                     //раннер будет остановлен

                     break;

                 }
             }

         }*/

        #endregion

        #endregion



    }
}
