using Confluent.Kafka;
using Confluent.Kafka;
using Confluent.Kafka;
using Confluent.Kafka;
using ICSharpCode.AvalonEdit;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Slicer.Style;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
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

#if DEBUG
            Label_COnnectDB_onSettings.Visibility = Visibility.Visible;
            Warehouses.Visibility = Visibility.Visible;

#elif RELEASE
            Label_COnnectDB_onSettings.Visibility = Visibility.Collapsed;
            Warehouses.Visibility = Visibility.Collapsed;
#endif

            #region начальная настройка веток реестра для работы !!!! Regisrty Staff Загрузка Настроек из реестра
            ///Пересоздание корня настроек в реестре + синхрон с реестром настроек--
            using RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
            using RegistryKey registry1 = Registry.CurrentUser.CreateSubKey(@"Software\HM\Hosts");
            using RegistryKey registry2 = Registry.CurrentUser.CreateSubKey(@"Software\HM");
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Posts_requests")) ;

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
            MenuCanvas = new List<Canvas>() { SettingCanvas, PostomatsCanvas, PartyCanvas, Home, StoreCanvas, RunnerCanvas, RemoveCanvas_Postman, AddCanvas_Postman, EditCanvas_Postman, SaveEdit_Postman };
            foreach (var item in MenuCanvas)
            {
                item.MouseEnter += (a, e) => { item.Background = new SolidColorBrush(Colors.Gray); };
                item.MouseLeave += (a, e) => { item.Background = new SolidColorBrush(Colors.Transparent); };

            }
            ///Анимация вращения кнопки настроек 
            SettingCanvas.MouseEnter += (a, e) => { Animations.Animation_rotate_ON(1, settingImage_AnimatingEl); };
            SettingCanvas.MouseLeave += (a, e) => { Animations.Animation_rotate_OFF(settingImage_AnimatingEl); };

            ///обработка кликов на элементы меню
            PartyCanvas.MouseDown += (a, e) => { OpenGrid(PartyGrid); };
            Home.MouseDown += (a, e) => { OpenGrid(HomeGrid); };
            PostomatsCanvas.MouseDown += (a, e) => { OpenGrid(PostomatsGrid); };
            SettingCanvas.MouseDown += (a, e) => { OpenGrid(SettingsGrid); };
            StoreCanvas.MouseDown += (a, e) => { OpenGrid(StoreGrid); };
            RunnerCanvas.MouseDown += (a, e) => { OpenGrid(RunnerGrid); LoadPosts(); };

            //обработка кликов на пункты меню постаматов
            Bronirovanie_Canvas.MouseDown += (a, e) => { BronbGrid.IsEnabled = true; Bronirovanie_Canvas.Background = (Brush)new BrushConverter().ConvertFrom("#FF808080"); BronbGrid.Visibility = Visibility.Visible; SyncEngyGrid.Visibility = Visibility.Hidden; SyncEngyGrid.IsEnabled = false; Sync_Canvas.Background = new SolidColorBrush(Colors.Transparent); };
            Sync_Canvas.MouseDown += (a, e) => { BronbGrid.IsEnabled = false; Bronirovanie_Canvas.Background = new SolidColorBrush(Colors.Transparent); BronbGrid.Visibility = Visibility.Hidden; SyncEngyGrid.Visibility = Visibility.Visible; SyncEngyGrid.IsEnabled = true; Sync_Canvas.Background = (Brush)new BrushConverter().ConvertFrom("#FF808080"); };
            cleanLog1.Click += (a, e) => { LogPOST1.Text = null; };
            ClearLog_sync.Click += (a, e) => { LogPOST2.Text = null; };
            Synchra_Button1.Click += (a, e) => { SynchronizationPackages(); };

            //Обработка кнопок в Склады-Импорт
            SearchWarh.Click += (a, e) => { SearchStoreinDB(SearchWH.Text); };


            //Для csm 
            TextBox_Raspologenie_Otchet_CSM.Text = Put_CSM;


            //Кнопки раннера
            AddCanvas_Postman.MouseDown += (a, e) => { Addrequest_inRegistry(); };
            RemoveCanvas_Postman.MouseDown += (a, e) => { DeletePost(); };
            List_JSONS.SelectionChanged += (a, e) => { LoadTextB(); };
            SaveEdit_Postman.MouseDown += (a, e) => { EditPost_SaveButton(); };
            EditCanvas_Postman.MouseDown += (a, e) => { EditPost_buttonSaveON(); };

            //обрабаотка функций при открытии на TB в TB_SelectionChanged()

            #endregion

            OpenGrid(HomeGrid); //открытие начальной страницы
            //лог
            WriteLogsToFile("-------------------------------------------\n\r--------------------ВХОД В СИСТЕМУ--------------------", "");
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

        #region Logging - логирование

        /// <summary>
        ///Функция записи в логи на сетевой диск
        /// </summary>
        /// <param name="Action">Действие</param>
        /// <param name="content">Номера посылок или другие подробности для лога</param>
        public async void WriteLogsToFile(string Action, string content)
        {
            await Task.Run(() =>
            {
                //путь к сетевому диску
                string networkPath = @"\\int.sblogistica.ru\sbl\Блок ИT и технологии\Департамент инфраструктуры и поддержки\Направление Service Desk\HM_Logs";
                //Имя файла
                string fileName = Decrypt_UserName + ".log";
                //Редактирование контента
                content = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")).ToString() + " - " + Decrypt_UserName + " - " + Action + " - [" + content + "]";

                // Проверка доступности сетевого диска
                if (!Directory.Exists(networkPath))
                {
                    Console.WriteLine("Сетевой диск недоступен."); //возможный вариант на смену msBox.show

                    //MessageBox.Show("Сетевой диск недоступен! Функции программы ограничены! СРОЧНО ЗАПРОСИ ДОСТУП!!!!!!!!!!");

                    return;
                }

                // Полный путь к файлу
                string fullPath = Path.Combine(networkPath, fileName);
                try
                {
                    // Открытие файла для записи (создание, если не существует)
                    using (StreamWriter writer = new StreamWriter(fullPath, true))
                    {
                        writer.WriteLine(content);
                    }

                    Console.WriteLine("Запись в файл успешно завершена.");
                    //MessageBox.Show("Запись в файл успешно завершена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при записи в файл: {ex.Message}");
                    //MessageBox.Show("Сетевой диск недоступен! Функции программы ограничены! СРОЧНО ЗАПРОСИ ДОСТУП!!!!!!!!!!");
                }
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
                    //дополнение к переключению клавиши Home: открытие первой вкладки автоматом
                    if (item.Name == "HomeGrid")
                    {
                        if (HomeGrid.IsEnabled && HomeGrid.Visibility == Visibility.Visible)
                        {
                            TB.SelectedIndex = 0;
                            ContextRP.IsChecked = true;
                        }
                    }
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

        /// <summary>
        /// Анимация боковой панели
        /// </summary>
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
                if ((TextBox.Text.Contains("SBC")) || (TextBox.Text.Contains("SMM")))
                {
                    List<string> SBCs = To_List(TextBox); // лист разделенный на строки из всего TextBox
                    foreach (var item in SBCs)
                    {
                        if (item.Contains("SBC"))
                        {
                            SBC_strings.Add("'" + item + "'"); //добавляю в основной лист с SBC
                        }
                        if (item.Contains("SMM"))
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

            //лог
            WriteLogsToFile("Обработка RP", TextBox.Text.Replace("\n", ""));

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

        /// <summary>
        /// Убрать запятые
        /// </summary>
        private void Ubrat_zapyatie_RP_spiski_Click(object sender, RoutedEventArgs e)
        {
            ListOne.Text = ListOne.Text.Replace(",", "");
            ListTwo.Text = ListTwo.Text.Replace(",", "");
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

        string avtootvet_dlya_party = "";

        /// <summary>
        /// Кнопка обработки Партий
        /// </summary>
        private void PartyEx_Click(object sender, RoutedEventArgs e)
        {
            // todo  [Партии]: При добавлении предупреждать о дубликатах если такие посылки уже добавлены и обрабатывать так, чтобы тех которых нет - прокидывать в партию, а дубли - убирать из запроса


            if (Party.Text != "")
            {
                string paty = Party.Text; //партия 
                if (paty.Contains("R_RET") || paty.Contains("RET")) paty = paty.Replace("R_RET", "").Replace("RET", "");
                List<string> StockID = dataBases.ConnectDB("Шиптор", $@"select * from package_return pr where id in ({paty})").AsEnumerable().Select(x => x["stock_id"].ToString()).ToList();
                //проверка существования в шипторе И ИМЕНИ СКЛАДА в шипе и меняем в заголовке имени склада 
                List<string> StockName = new List<string>();
                if (StockID.Count == 1)
                {
                    //ищем имя склада в Шипторе
                    StockName = dataBases.ConnectDB("Шиптор", $@"SELECT id, ""name""  FROM public.warehouse where id = {StockID[0]}").AsEnumerable().Select(x => x[1].ToString()).ToList();
                    if (Name_War.Content != StockName[0]) if (StockName[0] != null) Name_War.Content = StockName[0]; else MessageBox.Show($@"Склада с id {StockID[0]} не найдено в Шиптор!");//меняем имя склада на то что нашлось

                    // если поле посылок заполнено  - работаем и с посылкми и с партией, если нет то только с партией
                    if ((RP_Party.Text != ""))
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

                                            //если выбраны корректировки статусов, то выполняю и их 
                                            if (CheckBox_CorrectStatus_Party.IsChecked == true)
                                            {
                                                dataBases.ConnectDB("Шиптор", $@"update public.package set current_status = 'to_return', returned_at = now() WHERE id in ({RPid})");
                                                dataBases.ConnectDB(StockName[0], $@"update public.package set status = 'in_package_return' where package_fid in ({RPid})");

                                            }
                                            //• Звук уведомление о финале 
                                            using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                                            using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                                                new SoundPlayer(gzOut).Play();
                                        }



                                        else
                                        {
                                            MessageBox.Show("Поле для отправлений пусто! Добавьте отправления");
                                            //прогрывать звук Windows Ошибка error
                                            string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";
                                            // Создание экземпляра SoundPlayer и проигрывание звука
                                            using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                                            {
                                                errorSoundPlayer.Play();
                                            };

                                        }

                                    }
                                }
                                else
                                {
                                    MessageBox.Show($@"Партия не найдена на складе {StockName[0]}!");
                                    //прогрывать звук Windows Ошибка error
                                    string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";
                                    // Создание экземпляра SoundPlayer и проигрывание звука
                                    using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                                    {
                                        errorSoundPlayer.Play();
                                    };
                                }

                            }

                            //Удаление
                            if (SelectAction.SelectedIndex == 1)
                            {

                                DataTable CurrentStock = dataBases?.ConnectDB(StockName[0], $@"select id, status from package_return pr where return_fid = {paty}");//ERROR::Место вылета изза имени БД (точнее его отсутствии в твоих данных)
                                if (CurrentStock.Rows.Count > 0)
                                {
                                    List<string> ParyStockID = CurrentStock.AsEnumerable().Select(x => x["id"].ToString()).ToList();
                                    List<string> PartyStockStatus = CurrentStock.AsEnumerable().Select(x => x["status"].ToString()).ToList();

                                    if (ParyStockID[0] != null) //если партия существует на складе
                                    {
                                        //Удаление из паллеты

                                        if (RP_Party.Text != "")
                                        {
                                            string RPid = RP_Party.Text.Replace("\r\n", "");

                                            //удаляю из партии в шипторе и корректировка статусов посылок на "УПАКОВАНА"
                                            dataBases.ConnectDB("Шиптор", $@"update package set current_status = 'packed', sent_at = NULL, returned_at = null, reported_at = null, returning_to_warehouse_at = null, delivery_point_accepted_at = null, delivered_at = null, removed_at = null, lost_at = null, in_store_since = now(), measured_at = now(), packed_since = now(), prepared_to_send_since = now(), return_id=null   WHERE id in ({RPid})");
                                            dataBases.ConnectDB("Шиптор", $@"UPDATE package_departure SET package_action = NULL  WHERE package_id in ({RPid})");

                                            //удаление из партии склада 
                                            dataBases.ConnectDB(StockName[0], $@"Delete from package_return_item where package_id in (select id from package p where package_fid in({RPid}))");

                                            //смена статусов в Заппстор после удаления из партии
                                            dataBases.ConnectDB(StockName[0], $@"UPDATE package SET status = 'in_store' where package_fid in ({RPid})");

                                            //• Звук уведомление о финале 
                                            using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                                            using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                                                new SoundPlayer(gzOut).Play();

                                            //лог
                                            WriteLogsToFile($@"Удаление из партии R_RET{Party.Text}. Посылки: ", RP_Party.Text.Replace("\n", ""));



                                        }

                                        else
                                        {
                                            string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                                            // Создание экземпляра SoundPlayer и проигрывание звука
                                            using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                                            {
                                                errorSoundPlayer.Play();
                                            }
                                            MessageBox.Show("Поле для отправлений пусто! Добавьте отправления");

                                        }



                                    }
                                    else MessageBox.Show($@"Партия не найдена на складе {StockName[0]}!");
                                    //прогрывать звук Windows Ошибка error
                                }
                                else MessageBox.Show("Не найдено подключение к Базе данных в реестре");



                            }


                        }
                        ActionsForParty(StockName, paty);


                    }
                    else
                    {
                        if (SelectAction_Party.SelectedIndex != 0)
                        {
                            ActionsForParty(StockName, paty);
                        }
                        else
                        {
                            MessageBox.Show("Поле посылок не заполнено!");
                            //прогрывать звук Windows Ошибка error
                            string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                            // Создание экземпляра SoundPlayer и проигрывание звука
                            using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                            {
                                errorSoundPlayer.Play();
                            }
                        }


                    }//поле посылок устое - работаю только с партией



                }
                else
                {
                    MessageBox.Show("Такая партия не одна (!) или ее не существует в Шипторе!");
                    //прогрывать звук Windows Ошибка error
                    string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                    // Создание экземпляра SoundPlayer и проигрывание звука
                    using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                    {
                        errorSoundPlayer.Play();
                    }
                }

            }
            else
            {
                MessageBox.Show("Поле партии не заполнено!");
                //прогрывать звук Windows Ошибка error
                string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                // Создание экземпляра SoundPlayer и проигрывание звука
                using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                {
                    errorSoundPlayer.Play();
                }
            }
            avtootvet_dlya_party = SelectAction_Party.Text;
            SelectAction_Party.SelectedIndex = 0;

            //лог
            Kopirovat_otvet_party_Click(sender, e);
            WriteLogsToFile("Взаимодействие с партиями", $@"{avtootvet_dlya_party} : Партия R_RET{Party.Text}, Посылки: {RP_Party.Text.Replace("\n\r", "")}");
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

                        break;
                    case 2: //Выбрано "собрана"
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'gathered' where return_fid in ({party})");
                        // dataBases.ConnectDB("Шиптор", $@"UPDATE package_return SET sent_at = NULL, SET delivered_at = NULL, SET cancelled_at = NULL, SET closed_at = NULL where id in ({party})");

                        break;
                    case 3: //Выбрано "Упакована"
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'packed'  where return_fid in ({party})");
                        //dataBases.ConnectDB("Шиптор", $@"UPDATE package_return SET sent_at = NULL, SET delivered_at = NULL, SET cancelled_at = NULL, SET closed_at = NULL where id in ({party})");

                        break;
                    case 4: //Выдана
                        dataBases.ConnectDB(Stock_name[0], $@"UPDATE package_return SET status = 'delivered'  where return_fid in ({party})");
                        dataBases.ConnectDB("Шиптор", $@"UPDATE package_return SET delivered_at = now() where id in ({party})");

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
                        dataBases.ConnectDB("Шиптор", $@"UPDATE public.package SET return_id = NULL, current_status = 'packed', sent_at = NULL, returned_at = NULL, returning_to_warehouse_at = NULL, packed_since = now()   WHERE return_id in ({party})");
                        dataBases.ConnectDB("Шиптор", $@"UPDATE package_departure SET package_action = NULL  WHERE package_id in ({string.Join(",", RPfromParty.AsEnumerable().Select(x => x["id"].ToString()).ToList())})");

                        break;

                    default:
                        break;
                }
                //• Звук уведомление о финале 
                using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                    new SoundPlayer(gzOut).Play();
            }
            else
            {
                MessageBox.Show("Такая партия не одна (!) или ее не существует в Шипторе!");
                //прогрывать звук Windows Ошибка error
                string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                // Создание экземпляра SoundPlayer и проигрывание звука
                using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                {
                    errorSoundPlayer.Play();
                }
            }
        }

        /// <summary>
        ///  Делаем доступным ввод только цифр в поле Партий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Party_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                // Удаляем нецифровые символы из текста
                textBox.Text = new string(textBox.Text.Where(char.IsDigit).ToArray());

                // Устанавливаем курсор в конец текста
                textBox.CaretIndex = textBox.Text.Length;
            }
        }

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

        /// <summary>
        ///ВЫбор добавления или удаления из партии (для того чтобы выбрать чек бокс со статусами)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectAction.SelectedIndex == 0)
            {
                //если выбрано только добавление в партию 
                CheckBox_CorrectStatus_Party.IsEnabled = true;

            }
            else
            {
                //если выбрано удаление 
                CheckBox_CorrectStatus_Party.IsEnabled = false;
                CheckBox_CorrectStatus_Party.IsChecked = false;

            }

        }

        /// <summary>
        /// кнопка очистки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Text_Party_Click(object sender, RoutedEventArgs e)
        {
            RP_Party.Text = null;
            Party.Text = null;
        }

        private void Kopirovat_otvet_party_Click(object sender, RoutedEventArgs e)
        {

            switch (avtootvet_dlya_party)
            {
                case "Ничего":
                    try { Clipboard.SetText("Здравствуйте! Посылки были удалены из партии возврата."); } catch { };
                    break;
                case "Собирается":
                    try { Clipboard.SetText("Здравствуйте! Статус партии был скорректирован на 'Собирается'."); } catch { };
                    break;
                case "Собрана":
                    try { Clipboard.SetText("Здравствуйте! Статус партии был скорректирован на 'Собрана'."); } catch { };
                    break;
                case "Упакована":
                    try { Clipboard.SetText("Здравствуйте! Статус партии был скорректирован на 'Упакована'."); } catch { };
                    break;
                case "Выдана":
                    try { Clipboard.SetText("Здравствуйте! Статус партии был скорректирован на 'Выдана'."); } catch { };
                    break;
                case "Расформирована":
                    try { Clipboard.SetText("Здравствуйте! Статус партии был скорректирован на 'Расформирована'."); } catch { };
                    break;
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
        bool active_PalM = false;

        /// <summary>
        /// Действия при переключепнии вкладок на Складе: керпка вывода предупреждения при входе во вкладку CSM + подгрузка заппов на вкладке с Паллетами и Мешками 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Store_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            System.Windows.Controls.TabItem selectedTab = tabControl.SelectedItem as System.Windows.Controls.TabItem;

            if (selectedTab != null)
            {
                switch (selectedTab.Name)
                {
                    case "CSM":
                        // Код для обработки выбора первой вкладки
                        Teni_CSM.Visibility = Visibility.Visible;
                        CSM_show_okno_teney.Visibility = Visibility.Visible;
                        break;
                    case "Pallet_and_meshok":
                        //Вкладка паллет и мешка
                        loaderL_P_M();
                        active_PalM = true;
                        break;
                }
            }


        }



        #region Import
        DataTable data; //таблица со складами из поиска
        int selected_id_warh; //выбранный ID выбранного склада 
        string tekushiy_sklad;




        /// <summary>
        ///Поиск склада и отображение его в таблице складов с адресом и slug
        /// </summary>
        /// <param name="TextSearchWarh">Тескт из поля поиска</param>
        public void SearchStoreinDB(string TextSearchWarh)
        {
            data = dataBases.ConnectDB("Шиптор", $@"select id, slug, name, address from warehouse where name  ilike ('%{TextSearchWarh}%') or slug ilike ('%{TextSearchWarh}%') or address ilike ('%{TextSearchWarh}%') order by id");
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
                tekushiy_sklad = cellValue.ToString();
            }
        }

        /// <summary>
        /// Кнопка копирующая автоответ в буфер обмена.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stone_import_otvet_Click(object sender, RoutedEventArgs e)
        {
            try { Clipboard.SetText("Здравствуйте! Посылки импортированы на склад '" + tekushiy_sklad + "'"); } catch { }
            tekushiy_sklad = "";
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
        public void UpdatesShiptor_InportStore(TextEditor txBx)
        {
            if (selected_id_warh != 0)
            {
                /// если выбран скалд то пишу в его шиптор посылкам
                dataBases.ConnectDB("Шиптор", $@"update package set current_warehouse_id = {selected_id_warh},next_warehouse_id = {selected_id_warh} where id in ({txBx.Text})");
                dataBases.ConnectDB("Шиптор", $@"update package set ems_execution_mode = false where id in ({txBx.Text}) and ems_execution_mode = true");
                selected_id_warh = 0;

            }
            else
            {

                MessageBox.Show("Не выбран склад!!!");

            }

        }

        /// <summary>
        /// Кнопка "Импорт"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Import_button_Click(object sender, RoutedEventArgs e)
        {

            if (TextBox1_importText.Text != "")
            {
                //Обновляю в шипторе корректные склады
                UpdatesShiptor_InportStore(TextBox1_importText);
                perepodgotovka_posilok(TextBox1_importText);
                //• Звук уведомление о финале 
                if (Selected_NameWarh.Content != null)
                {
                    using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                    using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                        new SoundPlayer(gzOut).Play();
                    Selected_NameWarh.Content = null;
                    WarhausesTable.SelectedItem = null;
                }


                //лог
                WriteLogsToFile("Импорт посылок на склад", $@"Склад: {tekushiy_sklad}, Посылки: {TextBox1_importText.Text.Replace("\n", "")}");

            }
            else
            {

                MessageBox.Show("Список посылок пуст!");

            }


        }

        /// <summary>
        /// Кнопка Очистки полей  Импорта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClear_ImportStrore_Click(object sender, RoutedEventArgs e)
        {
            TextBox1_importText.Text = null;
            Selected_NameWarh.Content = null;
            SearchWH.Text = null;
            WarhausesTable.ItemsSource = null;
        }

        #endregion

        #region CSM 

        //Общие переменные
        /// <summary>
        /// Путь для отчёта CSM
        /// </summary>
        /// <param name="Put_CSM">Путь для отчёта CSM</param>   
        string Put_CSM = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string Name_Otchet_CSM = "Выгрузка";


        /// <summary>
        /// Кнопка скрытия предупреждения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Knopka_ubrat_teni_Click(object sender, RoutedEventArgs e)
        {
            CSM_show_okno_teney.Visibility = Visibility.Collapsed;
            Teni_CSM.Visibility = Visibility.Collapsed;
        }

        //1. делаем апдейт в бидэ "апдейт сделать красиво"
        //2. переподготавливаем список посылок
        //3. создаём отчёт
        /// <summary>
        /// Тут мы кидаем апдейт на склады в шиптор (CSM)
        /// </summary>
        /// <param name="nomera">nomera это ID РПшек через запятые</param>
        void update_shiptor_for_treck_number(TextEditor nomera)
        {
            //апдейт сделать красиво
            dataBases.ConnectDB("Шиптор", $@"update package set current_warehouse_id = destination_warehouse_id, next_warehouse_id = destination_warehouse_id where id in ({nomera.Text})");
        }

        int VsegoThreads_perepodgotovka;
        int ThreadComplited_perepodgotovka = 0; //переменные для метода переподготовки 1. Всего потоков. 2. всего выполненных потоков.

        /// <summary>
        ///Кнопка Старта CMS 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_CMS_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Nomera_for_CSM.Text != "")
            {
                TextBox_Raspologenie_Otchet_CSM.Text = Put_CSM;
                if (TextBox_Name_Otchet_CSM.Text == "")
                {
                    TextBox_Name_Otchet_CSM.Text = Name_Otchet_CSM;
                }

                //1. делаем апдейт в бидэ "апдейт сделать красиво"? если выбьрана галка для трек-номеров
                if (CheckBox_Prisv_trackCSM.IsChecked == true)
                {
                    update_shiptor_for_treck_number(TextBox_Nomera_for_CSM); // апдейт для подготовки посылок в шиптор для присвоения трек-номеров
                    csm_createOrder_api(TextBox_Nomera_for_CSM); //прокидываем метод по присвоению трек-номеров
                    perepodgotovka_posilok(TextBox_Nomera_for_CSM);

                    //лог
                    WriteLogsToFile("CSM - Присвоение Трек-номера посылкам: ", TextBox_Nomera_for_CSM.Text.Replace("\n", ""));
                }
                if (CheckBox_Snyat_Stop_CSM.IsChecked == true)
                {
                    csm_zz_ne_sozdan(TextBox_Nomera_for_CSM);
                    //лог
                    WriteLogsToFile("CSM - Снятие стопа 'Заказ не создан у партнера' для посылок: ", TextBox_Nomera_for_CSM.Text.Replace("\n", ""));
                }

                //если не выьбрано то вывод отчета 
                if (CheckBox_Otchet_CSM.IsChecked == true)
                {
                    if (CheckBox_Otchet_dlya_sebya_CSM.IsChecked == true)
                    {
                        //если отчет чекнут для себя 
                        otchet_CSM_(dataBases.ConnectDB("Шиптор", $@"select *
from (
select 'RP' || p2.id as ""Родительское"", 'RP' || p.id as ""Дочернее"", p.external_id as ""ШК"", p.current_status as ""Статус"", case when t.tracking is null then t2.tracking else t.tracking end as ""Трек-номер""
, case
    when pd.carrier_method_slug in ('return_courier', 'pvz', 'return_fulfilment', 'postamat_sbl', 'return_terminal') then 'Не партнёрская доставка'
    when pd.delivery_point_id is null then 'Курьерская доставка'
    when (p.current_status in ('removed') or p.removed_at is not null) then 'Удалена'
    when (EXISTS (SELECT 1 FROM package p_inner where t.tracking is null and (p_inner.parent_id = p.parent_id or p_inner.id = p.id) and p_inner.current_status in ('to_return', 'returned', 'return_to_sender', 'returned_to_warehouse', 'returning_to_warehouse') LIMIT 1)) then 'На возврате'
    when (EXISTS (SELECT 1 FROM package p_inner WHERE t.tracking is null and ((p_inner.parent_id = p.parent_id and p_inner.current_status in ('new')) or (p_inner.id = p.id and p_inner.current_status in ('new'))) LIMIT 1)) then 'Не обработана складом'
    when (EXISTS (SELECT 1 FROM package p_inner WHERE t.tracking is null and (p_inner.parent_id = p.parent_id or p_inner.id = p.id) AND p_inner.current_warehouse_id != p_inner.next_warehouse_id LIMIT 1)) then 'Находится в пути на склад'
    when p.current_status in ('delivered') then 'Уже доставлена'
    when (EXISTS (SELECT 1 FROM package p_inner WHERE t.tracking is null and (p_inner.parent_id = p.parent_id or p_inner.id = p.id) AND p_inner.current_warehouse_id != p_inner.destination_warehouse_id LIMIT 1)) then 'Не на конечном складе'
    when p2.extra_data #>> '{{is_completed}}' in ('false') then 'Посылка не скомплектована'
    when (a.marked_as_trash_at is not null) then 'Некорректный адрес'
    when (pp2.type in ('package-is-lost')) then 'Утеряна'
    when (dp.is_active in (false)) then 'Неактивный пункт выдачи'
    when (((pp.description is not null) and (co.rejection_reason is null)) and (t.tracking is null and t2.tracking is null)) then pp.description
    when co.rejection_reason is not null then co.rejection_reason
    when ((t.tracking is not null) or (t2.tracking is not null)) then ''
end as ""Ошибка""
,pd.carrier_method_slug as ""Партнёр""
from package p
left join warehouse w on w.id = p.current_warehouse_id
left join package p2 on p.parent_id = p2.id
join package_departure pd on p.id = pd.package_id
left join tracking.tracking t on pd.tracking_id = t.id
left join package_departure pd2 on p.parent_id = pd2.package_id
left join tracking.tracking t2 on pd2.tracking_id = t2.id
left join package_problem pp on (p2.id = pp.package_id and pp.description not in ('Посылка задерживается', 'Смена метода доставки', 'Клиент отказался от заказа', 'Техническая проблема', 'Нет свободных ячеек подходящего размера', 'Срок пребывания в очереди на бронирование истек'))
left join package_problem pp2 on p.id = pp2.package_id
left join shipping.delivery_point dp on pd.delivery_point_id = dp.id
left join address a on pd.address_id = a.id
left join csm_order co on p2.id = co.package_id
where 1 = 1
and p.id in ({TextBox_Nomera_for_CSM.Text})--Дочерние посылки RP
--and upper(p.external_id) in ()--Дочерние посылки external в верхнем регистре
and p.previous_id is null
) as table_name
where 1 = 1
group by ""Родительское"", ""Дочернее"", ""ШК"", ""Статус"", ""Трек-номер"", ""Ошибка"", ""Партнёр""
order by ""Ошибка"" desc"));

                    }
                    else
                    {
                        otchet_CSM_(dataBases.ConnectDB("Шиптор", $@"select *
from (
select  p.external_id as ""ШК"", case when t.tracking is null then t2.tracking else t.tracking end as ""Трек-номер"",
case
    when pd.carrier_method_slug in ('return_courier', 'pvz', 'return_fulfilment', 'postamat_sbl', 'return_terminal') then 'Не партнёрская доставка'
    when pd.delivery_point_id is null then 'Курьерская доставка'
    when (p.current_status in ('removed') or p.removed_at is not null) then 'Удалена'
    when (EXISTS (SELECT 1 FROM package p_inner where t.tracking is null and (p_inner.parent_id = p.parent_id or p_inner.id = p.id) and p_inner.current_status in ('to_return', 'returned', 'return_to_sender', 'returned_to_warehouse', 'returning_to_warehouse') LIMIT 1)) then 'На возврате'
    when (EXISTS (SELECT 1 FROM package p_inner WHERE t.tracking is null and ((p_inner.parent_id = p.parent_id and p_inner.current_status in ('new')) or (p_inner.id = p.id and p_inner.current_status in ('new'))) LIMIT 1)) then 'Не обработана складом'
    when (EXISTS (SELECT 1 FROM package p_inner WHERE t.tracking is null and (p_inner.parent_id = p.parent_id or p_inner.id = p.id) AND p_inner.current_warehouse_id != p_inner.next_warehouse_id LIMIT 1)) then 'Находится в пути на склад'
    when p.current_status in ('delivered') then 'Уже доставлена'
    when (EXISTS (SELECT 1 FROM package p_inner WHERE t.tracking is null and (p_inner.parent_id = p.parent_id or p_inner.id = p.id) AND p_inner.current_warehouse_id != p_inner.destination_warehouse_id LIMIT 1)) then 'Не на конечном складе'
    when p2.extra_data #>> '{{is_completed}}' in ('false') then 'Посылка не скомплектована'
    when (a.marked_as_trash_at is not null) then 'Некорректный адрес'
    when (pp2.type in ('package-is-lost')) then 'Утеряна'
    when (dp.is_active in (false)) then 'Неактивный пункт выдачи'
    when (((pp.description is not null) and (co.rejection_reason is null)) and (t.tracking is null and t2.tracking is null)) then pp.description
    when co.rejection_reason is not null then co.rejection_reason
    when ((t.tracking is not null) or (t2.tracking is not null)) then ''
end as ""Ошибка""
from package p
left join warehouse w on w.id = p.current_warehouse_id
left join package p2 on p.parent_id = p2.id
join package_departure pd on p.id = pd.package_id
left join tracking.tracking t on pd.tracking_id = t.id
left join package_departure pd2 on p.parent_id = pd2.package_id
left join tracking.tracking t2 on pd2.tracking_id = t2.id
left join package_problem pp on (p2.id = pp.package_id and pp.description not in ('Посылка задерживается', 'Смена метода доставки', 'Клиент отказался от заказа', 'Техническая проблема', 'Нет свободных ячеек подходящего размера', 'Срок пребывания в очереди на бронирование истек'))
left join package_problem pp2 on p.id = pp2.package_id
left join shipping.delivery_point dp on pd.delivery_point_id = dp.id
left join address a on pd.address_id = a.id
left join csm_order co on p2.id = co.package_id
where 1 = 1
and p.id in ({TextBox_Nomera_for_CSM.Text})--Дочерние посылки RP
--and upper(p.external_id) in ()--Дочерние посылки external в верхнем регистре
and p.previous_id is null
) as table_name
where 1 = 1
group by ""ШК"", ""Трек-номер"", ""Ошибка"" order by ""Ошибка"" desc"));
                    }


                }
                //MessageBox.Show("Готово!");
                //Звук уведомление о финале файла
                using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                    new SoundPlayer(gzOut).Play();
            }
            else
            {
                //прогрывать звук Windows Ошибка error
                string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                // Создание экземпляра SoundPlayer и проигрывание звука
                using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                {
                    errorSoundPlayer.Play();
                }
                MessageBox.Show("Не введены номера посылок!");
            }
        }

        /// <summary>
        ///    Запуск переподготовки посылок
        /// </summary>
        /// <param name="spisok_RP">spisok_RP это ID РПшек через запятые</param>
        void perepodgotovka_posilok(TextEditor spisok_RP)
        {
            //локальная переменная для удаления запятых
            string forzapya = spisok_RP.Text;
            //Убираем все пустые строки
            // ListRP_postman.Text.Split(",\n").ToList();
            string[] lines = forzapya.Split(new[] { "\r\n", "\r", "\n", "," }, StringSplitOptions.None);
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            forzapya = string.Join(Environment.NewLine, lines);
            int kol_vo_potokov = 6;
            if (spisok_RP.LineCount <= 10) //если в поле менее 10 строк (отправленмий), то автоматически ставится 1 поток
                kol_vo_potokov = 1;
            List<string> listRP = new List<string>(lines); //лист со всеми rp

            #region RP_Stats
            /////////////--------------------------------
            ////////////-----------запись кол-ва в реестр для статы--------------
            int statsRP = 0;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM"))
            {

                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains("RP_post_stats_"))
                    {
                        statsRP = (int)key?.GetValue(item);

                    }
                }
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM"))
            {
                if (listRP.Count != 0)
                {
                    key?.SetValue("RP_post_stats_", listRP.Count + statsRP);

                }
            }
            //////////------------------------------------------
            #endregion




            //•Делим списки RP для нашего числа потоков 
            int chunkSize_perepodgotovka = listRP.Count / kol_vo_potokov;
            VsegoThreads_perepodgotovka = kol_vo_potokov;
            ThreadComplited_perepodgotovka = 0;
            List<List<string>> chunks = new List<List<string>>();
            for (int i = 0; i < listRP.Count; i += chunkSize_perepodgotovka)
            {
                if ((listRP.Count - (chunkSize_perepodgotovka * (chunks.Count + 1))) < listRP.Count / 10)
                {

                    //берем все до конца с текущего I 
                    chunks.Add(listRP.GetRange(i, listRP.Count - i));
                    break;
                }
                else
                {
                    chunks.Add(listRP.GetRange(i, Math.Min(chunkSize_perepodgotovka, listRP.Count - i)));
                }

            }

            //• получаю данне об post-запросе (url и body)
            string urlP = "https://api.shiptor.ru/system/v1?key=SemEd5DexEk7Ub2YrVuyNavNutEh4Uh8TerSuwEnMev";
            string bodyP = $@"{{
  ""id"": ""JsonRpcClient.js"",
  ""jsonrpc"": ""2.0"",
  ""method"": ""delivery.importPackage"",
  ""params"": [{{{{R}}}}]
}}";

            //• Запуск раннеров по колву потоков /Распределитель потоков
            for (int i = 0; i < kol_vo_potokov; i++)
            {

                //выбираем ЛисВью в который будем писать, нужный url, body, номер потока, и колво посылок для этого потока и запускаем раннер                     

                RuunerPost_Postman(urlP, bodyP, i, chunks);

            }
            ThreadComplited_perepodgotovka = 0;
        }

        /// <summary>
        ///    Запуск присвоения трек-номеров 
        /// </summary>
        /// <param name="spisok_RP">spisok_RP это ID РПшек через запятые</param>
        void csm_createOrder_api(TextEditor spisok_RP)
        {
            string forzapya1 = spisok_RP.Text;
            //Убираем все пустые строки
            // ListRP_postman.Text.Split(",\n").ToList();
            string[] lines = forzapya1.Split(new[] { "\r\n", "\r", "\n", "," }, StringSplitOptions.None);
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            forzapya1 = string.Join(Environment.NewLine, lines);
            int kol_vo_potokov = 6;
            if (spisok_RP.LineCount <= 10) //если в поле менее 10 строк (отправленмий), то автоматически ставится 1 поток
                kol_vo_potokov = 1;
            List<string> listRP = new List<string>(To_List(spisok_RP)); //лист со всеми rp

            #region RP_Stats
            /////////////--------------------------------
            ////////////-----------запись кол-ва в реестр для статы--------------
            int statsRP = 0;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM"))
            {

                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains("RP_post_stats_"))
                    {
                        statsRP = (int)key?.GetValue(item);

                    }
                }
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM"))
            {
                if (listRP.Count != 0)
                {
                    key?.SetValue("RP_post_stats_", listRP.Count + statsRP);

                }
            }
            //////////------------------------------------------
            #endregion


            //•Делим списки RP для нашего числа потоков 
            int chunkSize_perepodgotovka = listRP.Count / kol_vo_potokov;
            VsegoThreads_perepodgotovka = kol_vo_potokov;
            ThreadComplited_perepodgotovka = 0;
            List<List<string>> chunks = new List<List<string>>();
            for (int i = 0; i < listRP.Count; i += chunkSize_perepodgotovka)
            {
                if ((listRP.Count - (chunkSize_perepodgotovka * (chunks.Count + 1))) < listRP.Count / 10)
                {

                    //берем все до конца с текущего I 
                    chunks.Add(listRP.GetRange(i, listRP.Count - i));
                    break;
                }
                else
                {
                    chunks.Add(listRP.GetRange(i, Math.Min(chunkSize_perepodgotovka, listRP.Count - i)));
                }

            }

            //• получаю данне об post-запросе (url и body)
            string urlP = "https://api.shiptor.ru/system/v1?key=SemEd5DexEk7Ub2YrVuyNavNutEh4Uh8TerSuwEnMev";
            string bodyP = $@"{{
    ""id"": ""JsonRpcClient.js"",
    ""jsonrpc"": ""2.0"",
    ""method"": ""csm.createOrder"",
    ""params"": [
        {{
            ""package_id"": {{{{R}}}}
        }}
    ]
}}";

            //• Запуск раннеров по колву потоков /Распределитель потоков
            for (int i = 0; i < kol_vo_potokov; i++)
            {

                //выбираем ЛисВью в который будем писать, нужный url, body, номер потока, и колво посылок для этого потока и запускаем раннер                     

                RuunerPost_Postman(urlP, bodyP, i, chunks);

            }
        }

        /// <summary>
        /// Снимает стоп "Заказ не создан у партнёра"
        /// </summary>
        /// <param name="spisok_RP"></param>
        void csm_zz_ne_sozdan(TextEditor spisok_RP)
        {
            string forzapya1 = spisok_RP.Text;
            //Убираем все пустые строки
            // ListRP_postman.Text.Split(",\n").ToList();
            string[] lines = forzapya1.Split(new[] { "\r\n", "\r", "\n", "," }, StringSplitOptions.None);
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            forzapya1 = string.Join(Environment.NewLine, lines);
            int kol_vo_potokov = 1;
            if (spisok_RP.LineCount <= 10) //если в поле менее 10 строк (отправленмий), то автоматически ставится 1 поток
                kol_vo_potokov = 1;
            List<string> listRP = new List<string>(lines); //лист со всеми rp

            #region RP_Stats
            /////////////--------------------------------
            ////////////-----------запись кол-ва в реестр для статы--------------
            int statsRP = 0;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM"))
            {

                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains("RP_post_stats_"))
                    {
                        statsRP = (int)key?.GetValue(item);

                    }
                }
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM"))
            {
                if (listRP.Count != 0)
                {
                    key?.SetValue("RP_post_stats_", listRP.Count + statsRP);

                }
            }
            //////////------------------------------------------
            #endregion


            //•Делим списки RP для нашего числа потоков 
            int chunkSize_perepodgotovka = listRP.Count / kol_vo_potokov;
            VsegoThreads_perepodgotovka = kol_vo_potokov;
            ThreadComplited_perepodgotovka = 0;
            List<List<string>> chunks = new List<List<string>>();
            for (int i = 0; i < listRP.Count; i += chunkSize_perepodgotovka)
            {
                if ((listRP.Count - (chunkSize_perepodgotovka * (chunks.Count + 1))) < listRP.Count / 10)
                {

                    //берем все до конца с текущего I 
                    chunks.Add(listRP.GetRange(i, listRP.Count - i));
                    break;
                }
                else
                {
                    chunks.Add(listRP.GetRange(i, Math.Min(chunkSize_perepodgotovka, listRP.Count - i)));
                }

            }

            //• получаю данне об post-запросе (url и body)
            string urlP = $@"https://{TextBox_imya_sklada_CSM.Text}.zappstore.pro/api/jsonrpc?key=8f79c9ef35f955feb7c758735bb6bac1767a07a1";
            string bodyP = $@"{{
    ""id"": ""JsonRpcClient.js"",
    ""jsonrpc"": ""2.0"",
    ""method"": ""package.removeStop"",
    ""params"" : [{{""package"":""{{{{R}}}}"",""type"":""creating_partner_order""}}]
}}";

            //• Запуск раннеров по колву потоков /Распределитель потоков
            for (int i = 0; i < kol_vo_potokov; i++)
            {

                //выбираем ЛисВью в который будем писать, нужный url, body, номер потока, и колво посылок для этого потока и запускаем раннер                     

                RuunerPost_Postman(urlP, bodyP, i, chunks);

            }
        }

        /// <summary>
        /// Создание отчёта CSM, отображая всю таблицу из селекта (otchet_CSM - сюда селект)
        /// </summary>
        /// <param name="otchet_CSM">Готовый селект из БД для отчета CSM</param>
        void otchet_CSM_(DataTable otchet_CSM)
        {
            //•Создаем файлик Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var file = new FileInfo(Put_CSM + @"\" + TextBox_Name_Otchet_CSM.Text + ".xlsx");
            using (var package = new ExcelPackage(file))
            {
                // Добавление нового листа
                var worksheet = package.Workbook.Worksheets["Выгрузка"];

                if (worksheet != null)
                {
                    package.Workbook.Worksheets.Delete(worksheet);
                }
                worksheet = package.Workbook.Worksheets.Add("Выгрузка");


                // Заполняем первую строку названиями столбцов
                for (int i = 0; i < otchet_CSM.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = otchet_CSM.Columns[i].ColumnName;
                }

                // Заполняем остальные строки данными из DataTable
                for (int i = 0; i < otchet_CSM.Rows.Count; i++)
                {
                    DataRow row = otchet_CSM.Rows[i];
                    for (int j = 0; j < row.ItemArray.Length; j++)
                    {
                        worksheet.Cells[i + 2, j + 1].Value = row[j];
                    }
                }

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


            //• открываем проводник и выделяем наш файл в нем
            string filePath = Path.Combine(Put_CSM, TextBox_Name_Otchet_CSM.Text + ".xlsx");
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {// Передаем команду открытия и выделения файла
                FileName = "explorer.exe",
                Arguments = $"/select, /order, \"date\", \"{filePath}\""
            };
            // Запускаем процесс
            process.Start();
        }

        /// <summary>
        /// Кнопка расположения отчётов CSM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Raspologenie_Otchet_CSM_Click(object sender, RoutedEventArgs e)
        {

            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();

            if (dialog.ShowDialog() == true)
                Put_CSM = dialog.SelectedPath;
            SelectedFolderHashes.Content = Path.GetFileName(Put_CSM);
            TextBox_Raspologenie_Otchet_CSM.Text = Put_CSM;
        }

        /// <summary>
        /// Метод переключения видимости текстбоксов и лабелок в зависимости от чекбокса CheckBox_Otchet_CSM В меню склады в подменю CSM
        /// </summary>
        private void CheckBox_Otchet_CSM_Checked_1(object sender, RoutedEventArgs e)
        {
            Label_Name_Otchet_CSM.IsEnabled = true;
            TextBox_Name_Otchet_CSM.IsEnabled = true;
            Label_Raspologenie_Otchet_CSM.IsEnabled = true;
            TextBox_Raspologenie_Otchet_CSM.IsEnabled = true;
            button_Raspologenie_Otchet_CSM.IsEnabled = true;
            TextBox_Raspologenie_Otchet_CSM.Text = Put_CSM;
            CheckBox_Otchet_dlya_sebya_CSM.IsEnabled = true;
        }

        /// <summary>
        /// Метод переключения видимости текстбоксов и лабелок в зависимости от чекбокса CheckBox_Otchet_CSM В меню склады в подменю CSM
        /// </summary>
        private void CheckBox_Otchet_CSM_Unchecked(object sender, RoutedEventArgs e)
        {
            Label_Name_Otchet_CSM.IsEnabled = false;
            TextBox_Name_Otchet_CSM.IsEnabled = false;
            Label_Raspologenie_Otchet_CSM.IsEnabled = false;
            TextBox_Raspologenie_Otchet_CSM.IsEnabled = false;
            button_Raspologenie_Otchet_CSM.IsEnabled = false;
            TextBox_Raspologenie_Otchet_CSM.Text = Put_CSM;
            CheckBox_Otchet_dlya_sebya_CSM.IsEnabled = false;
        }

        /// <summary>
        /// Очистить CSM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClear_CSM_Click(object sender, RoutedEventArgs e)
        {
            TextBox_imya_sklada_CSM.Text = null;
            TextBox_Nomera_for_CSM.Text = null;
            CheckBox_Prisv_trackCSM.IsChecked = false;
            CheckBox_Snyat_Stop_CSM.IsChecked = false;
            CheckBox_Otchet_dlya_sebya_CSM.IsChecked = false;
            CheckBox_Otchet_CSM.IsChecked = false;
        }

        /// <summary>
        /// Кнопка автоответа в вкладке CSM
        /// </summary>
        private void kopirovat_otvet_csm_Click(object sender, RoutedEventArgs e)
        {
            if ((CheckBox_Prisv_trackCSM.IsChecked == true) && (CheckBox_Snyat_Stop_CSM.IsChecked == false))
            {
                try { Clipboard.SetText("Здравствуйте! Трек-номера присвоены."); } catch { };
            }

            if (CheckBox_Snyat_Stop_CSM.IsChecked == true)
            {
                try { Clipboard.SetText("Здравствуйте! Стоп 'Заказ не создан у партнёра' был снят."); } catch { };
            }

            if ((CheckBox_Otchet_CSM.IsChecked == true) && (CheckBox_Prisv_trackCSM.IsChecked == false) && (CheckBox_Snyat_Stop_CSM.IsChecked == false))
            {
                try { Clipboard.SetText("Здравствуйте! Требуемый отчёт приложен в файле 'Выгрузка'."); } catch { };
            }
            if ((CheckBox_Otchet_CSM.IsChecked == false) && (CheckBox_Prisv_trackCSM.IsChecked == false) && (CheckBox_Snyat_Stop_CSM.IsChecked == false))
            {
                try { Clipboard.SetText("Готово!"); } catch { };
            }
        }
        #endregion

        #region Pallets && Bags

        string DB_ZS_Palmet_Name_IS;
        string palmet_avtootvet = "";

        /// <summary>
        /// Загрузчик списка складов в лист list_palmet
        /// </summary>
        public void loaderL_P_M()
        {
            WarEdit warEdit = new WarEdit();
            warEdit.LoadHosts(list_palmet);
            warEdit.Close();
        }

        /// <summary>
        /// Поле Поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_palmet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Search_palmet.Text.Length >= 3)
                if (Search_palmet.Text != "")
                    foreach (var item in list_palmet.Items)
                        if (item.ToString().ToUpper().Contains(Search_palmet.Text.ToUpper()))
                        {
                            list_palmet.SelectedItem = item;
                            list_palmet.ScrollIntoView(list_palmet.Items.GetItemAt(list_palmet.SelectedIndex));
                        }

        }

        /// <summary>
        ///выбор склада - вывод вывббранного в лейбл 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void list_palmet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selected_NameWarh_palmet.Content = list_palmet.SelectedItem.ToString();
        }

        /// <summary>
        /// Загружает информцию о БД заппа из реестра в переменную "DB_ZS_Palmet_Name_IS"
        /// </summary>
        public void LoadData_FromReg_palmet()
        {
            string ssl = list_palmet.SelectedItem?.ToString(); // выбраное бд
            if (ssl != null)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts");
                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains(ssl))
                    {
                        string founded = item.Replace(" ", "").Replace("Name_", "").Replace("Host_", "").Replace("Post", "").Replace("DataBase_", "");
                        if (founded == ssl.Replace(" ", ""))
                        {
                            if (item.Contains("Name_")) DB_ZS_Palmet_Name_IS = key?.GetValue(item).ToString();
                        }

                    }
                    //Hosts_Name.Add(key.GetValue(item).ToString());

                }
            }

        }

        /// <summary>
        /// Смена возможных статусов взависимости от  мешка или паллета 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combobox_palmet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            if (active_PalM == true)
            {
                switch (Combobox_palmet.SelectedIndex)
                {
                    case 0:// паллет
                        for (int i = 0; i < Combobox_Actions_palmet.Items.Count; i++)
                        {
                            ComboBoxItem itemPall = (ComboBoxItem)Combobox_Actions_palmet.ItemContainerGenerator.ContainerFromIndex(i);
                            if (itemPall != null)
                            {
                                itemPall.IsEnabled = true;
                            }
                        }
                        ComboBoxItem itemBagNew = (ComboBoxItem)Combobox_Actions_palmet.Items[1];// ContainerGenerator.ContainerFromIndex(1);
                        itemBagNew.IsEnabled = false;
                        ComboBoxItem itemBagTransit = (ComboBoxItem)Combobox_Actions_palmet.Items[2]; // ContainerGenerator.ContainerFromIndex(2);
                        itemBagTransit.IsEnabled = true;
                        ComboBoxItem itemBag12 = (ComboBoxItem)Combobox_Actions_palmet.Items[7]; // ContainerGenerator.ContainerFromIndex(7); // блок отправлен и доставлен
                        itemBag12.IsEnabled = false;
                        ComboBoxItem itemBag22 = (ComboBoxItem)Combobox_Actions_palmet.Items[8]; // ContainerGenerator.ContainerFromIndex(8);
                        itemBag22.IsEnabled = false;
                        Combobox_Actions_palmet.SelectedIndex = 0;
                        check_x_dox_palmet.IsEnabled = true;
                        check_x_dox_palmet.IsChecked = false;
                        break;
                    case 1: // мешок

                        ComboBoxItem itemBag1 = (ComboBoxItem)Combobox_Actions_palmet.Items[1]; //ContainerGenerator.ContainerFromIndex(1);
                        itemBag1.IsEnabled = true;
                        ComboBoxItem itemBag11 = (ComboBoxItem)Combobox_Actions_palmet.Items[2]; //ContainerGenerator.ContainerFromIndex(2);
                        itemBag11.IsEnabled = false;
                        ComboBoxItem itemBag111 = (ComboBoxItem)Combobox_Actions_palmet.Items[7];   //ContainerGenerator.ContainerFromIndex(7);
                        itemBag111.IsEnabled = false;
                        ComboBoxItem itemBag2 = (ComboBoxItem)Combobox_Actions_palmet.Items[8];   //ContainerGenerator.ContainerFromIndex(8);
                        itemBag2.IsEnabled = false;
                        Combobox_Actions_palmet.SelectedIndex = 0;
                        check_x_dox_palmet.IsEnabled = false;
                        check_x_dox_palmet.IsChecked = false;
                        break;
                    default:
                        break;

                }
            }
        }

        /// <summary>
        /// Кнопка "Сделать" в паллетах и мешках. //DB_ZS_Palmet_Name_IS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_palmet_Click(object sender, RoutedEventArgs e)
        {
            /// Создание автоответа
            palmet_avtootvet = "";
            switch (Combobox_palmet.SelectionBoxItem)
            {
                case "Паллета":
                    palmet_avtootvet = "паллеты";
                    break;
                case "Мешок":
                    palmet_avtootvet = "мешка";
                    break;
            }
            switch (Combobox_Actions_palmet.SelectionBoxItem)
            {
                case "Собирается":
                    palmet_avtootvet = "Здравствуйте! Статус " + palmet_avtootvet + " скорректирован на 'Собирается'.";
                    break;
                case "Собрана":
                    palmet_avtootvet = "Здравствуйте! Статус " + palmet_avtootvet + " скорректирован на 'Собрана'.";
                    try { Clipboard.SetText(palmet_avtootvet); } catch { };
                    break;
                case "Упакована":
                    palmet_avtootvet = "Здравствуйте! Статус " + palmet_avtootvet + " скорректирован на 'Упакована'.";
                    try { Clipboard.SetText(palmet_avtootvet); } catch { };
                    break;
                case "Отправлен":
                    palmet_avtootvet = "Здравствуйте! Статус " + palmet_avtootvet + " скорректирован на 'Отправлено'.";
                    try { Clipboard.SetText(palmet_avtootvet); } catch { };
                    break;
                case "Доставлен":
                    palmet_avtootvet = "Здравствуйте! Статус " + palmet_avtootvet + " скорректирован на 'Выдан'.";
                    try { Clipboard.SetText(palmet_avtootvet); } catch { };
                    break;
                case "Расформирована":
                    palmet_avtootvet = "Здравствуйте! Статус " + palmet_avtootvet + " скорректирован на 'Расформирована'.";
                    try { Clipboard.SetText(palmet_avtootvet); } catch { };
                    break;
            }
            //Основной кусок кнопки
            try
            {
                if (text_editor_palmet.Text != "")
                {
                    switch (Combobox_palmet.SelectedIndex)
                    {
                        #region SelectPallet

                        case 0: //Ввыбрана паллета
                            LoadData_FromReg_palmet();
                            if (Selected_NameWarh_palmet != null) //
                            {//если найдена инфа по хосту и бд в реестре то идем дальше
                                if (Combobox_Actions_palmet.SelectedIndex == 0)
                                {
                                    //прогрывать звук Windows Ошибка error
                                    string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";
                                    // Создание экземпляра SoundPlayer и проигрывание звука
                                    using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                                    {
                                        errorSoundPlayer.Play();
                                    };
                                    MessageBox.Show("Не выбрано действие!");
                                }
                                else
                                {
                                    if (check_x_dox_palmet.IsChecked == false)
                                    {
                                        switch (Combobox_Actions_palmet.SelectionBoxItem)
                                        {
                                            case "Собирается":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'gathering' where id in ({text_editor_palmet.Text});");
                                                break;
                                            case "Собрана":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'gathered' where id in ({text_editor_palmet.Text});");
                                                break;
                                            case "Упакована":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'packed' where id in ({text_editor_palmet.Text});");
                                                break;
                                            case "Расформирована": //обновлено 1.11.2024 - убраны state_id
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'disbanded', last_pallet_packages = null where id in ({text_editor_palmet.Text});");
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update package set pallet_id = null where pallet_id in ({text_editor_palmet.Text});");
                                                break;
                                            case "Ждет транзита":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'wait_transit' where id in ({text_editor_palmet.Text});");
                                                break;
                                            case "Ожидает сборки":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'wait_gather' where id in ({text_editor_palmet.Text});");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (Combobox_Actions_palmet.SelectionBoxItem)
                                        {
                                            case "Собирается":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'gathering' where last_pallet_code in ({text_editor_palmet.Text});");
                                                break;
                                            case "Собрана":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'gathered' where last_pallet_code in ({text_editor_palmet.Text});");
                                                break;
                                            case "Упакована":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'packed' where last_pallet_code in ({text_editor_palmet.Text});");
                                                break;
                                            case "Расформирована":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'disbanded', last_pallet_packages = null where last_pallet_code in ({text_editor_palmet.Text});");
                                                //dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update package set pallet_id = null where pallet_id in ({text_editor_palmet.Text});");
                                                break;
                                            case "Ждет транзита":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'wait_transit' where last_pallet_code in ({text_editor_palmet.Text});");
                                                break;
                                            case "Ожидает сборки":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update pallet set status = 'wait_gather' where last_pallet_code in ({text_editor_palmet.Text});");
                                                break;
                                        }
                                    }

                                    DB_ZS_Palmet_Name_IS = null;
                                    Combobox_Actions_palmet.SelectedIndex = 0;
                                    //• Звук уведомление о финале 
                                    using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                                    using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                                        new SoundPlayer(gzOut).Play();
                                }

                            }
                            else
                            {
                                //прогрывать звук Windows Ошибка error
                                string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";
                                // Создание экземпляра SoundPlayer и проигрывание звука
                                using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                                {
                                    errorSoundPlayer.Play();
                                };
                                MessageBox.Show("Не выбран склад!");
                            }
                            break;
                        #endregion

                        #region SelectBag

                        case 1: // выбран мешок

                            LoadData_FromReg_palmet();
                            if (DB_ZS_Palmet_Name_IS != null)
                            {//если найдена инфа по хосту и бд в реестре то идем дальше
                                if (Combobox_Actions_palmet.SelectedIndex == 0)
                                { //если не выбрано действие над мешком
                                    //прогрывать звук Windows Ошибка error
                                    string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";
                                    // Создание экземпляра SoundPlayer и проигрывание звука
                                    using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                                    {
                                        errorSoundPlayer.Play();
                                    };
                                    MessageBox.Show("Не выбрано действие!");
                                }
                                else
                                {
                                    //переводим список мешков в лист 
                                    //проверяем каждую строку на наличие MK, по дефолту считаем что там bag_id 
                                    //MK собираем в один болльшой лист и находим от них Bag ID 
                                    //соединяем оба листа 
                                    List<string> MK_bags = new List<string>();
                                    List<string> ID_bags = new List<string>();
                                    List<string> result_ID_bags = new List<string>();

                                    // Разбиваем текст на строки по разделителю-запятой
                                    string[] arrStr = text_editor_palmet.Text.Split(new char[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                                    // Преобразуем массив строк в список
                                    List<string> all_bags_list = new List<string>(arrStr);

                                    foreach (string number in all_bags_list)
                                    {
                                        if (!string.IsNullOrEmpty(number))
                                        {
                                            if (number.Contains("MK"))
                                                MK_bags.Add("'" + number + "'");
                                            else
                                                ID_bags.Add(number);

                                            //all_bags_list.Add("'" + number + "'");
                                        }
                                    }
                                    //ищу id от mk в мешках 
                                    List<string> Bags_ID_of_MK = new List<string>();
                                    if (MK_bags.Count > 0)
                                        Bags_ID_of_MK = dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"select id from bag b where seal_id in ({string.Join(",", MK_bags)})").AsEnumerable().Select(x => x[0].ToString()).ToList();

                                    //складываю оба свписка
                                    if (Bags_ID_of_MK.Count > 0)
                                        ID_bags.AddRange(Bags_ID_of_MK);

                                    string id_bags_complited = "";
                                    if (ID_bags.Count > 0)
                                    {
                                        id_bags_complited = string.Join(",", ID_bags); //пишем в переменную все элементы листа с id мешков через запятую для запросов
                                    }
                                    if (id_bags_complited != "")
                                    { ///менять статусы и производить какие либо действия будем ТОЛЬКО по bag_id 
                                        switch (Combobox_Actions_palmet.SelectionBoxItem)
                                        {
                                            case "Новая":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update bag set status = 'created' where id in ({id_bags_complited});");
                                                break;
                                            case "Собирается":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update bag set status = 'gathering' where id in ({id_bags_complited});");
                                                break;
                                            case "Собрана":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update bag set status = 'gathered' where id in ({id_bags_complited});");
                                                break;
                                            case "Упакована":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update bag set status = 'packed' where id in ({id_bags_complited});");
                                                break;
                                            case "Расформирована":
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update bag set status = 'disbanded' where id in ({id_bags_complited});");
                                                dataBases.ConnectDB(DB_ZS_Palmet_Name_IS, $@"update package set bag_id = null where bag_id in ({id_bags_complited});");
                                                break;
                                        }
                                        DB_ZS_Palmet_Name_IS = null;
                                        Combobox_Actions_palmet.SelectedIndex = 0;
                                        //• Звук уведомление о финале 
                                        using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                                        using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                                            new SoundPlayer(gzOut).Play();

                                    }

                                }

                            }
                            else
                            {
                                //прогрывать звук Windows Ошибка error
                                string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                                // Создание экземпляра SoundPlayer и проигрывание звука
                                using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                                {
                                    errorSoundPlayer.Play();
                                };
                                MessageBox.Show("Не выбран склад!");
                            }

                            break;
                            #endregion

                    }

                    //лог
                    WriteLogsToFile("Взаимодействие с партиями", $@"{palmet_avtootvet} : Партия R_RET{Party.Text}, Посылки: {RP_Party.Text.Replace("\n", "")}");


                }
                else
                {
                    MessageBox.Show("Не заполнен список паллет/мешков!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Что-то пошло не так. Скорее всего список паллет/мешков содержит буквы или коннект к выбранному складу устарел. Ошибка: " + ex.ToString());
            }


        }

        /// <summary>
        /// Очистка полей в паллетах/мешках
        /// </summary>
        private void ButtonClear_ImportStrore1_Click(object sender, RoutedEventArgs e)
        {
            Selected_NameWarh_palmet.Content = "";
            text_editor_palmet.Text = "";
            //list_palmet.ItemsSource = null;
            Search_palmet.Text = "";
        }
        /// <summary>
        /// Кнопка автоответа для паллет и мешков
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Kopirovat_orvet_palmet_Click(object sender, RoutedEventArgs e)
        {
            try { Clipboard.SetText(palmet_avtootvet); } catch { };
        }

        #endregion


        #endregion

        #region RunnerTool

        //• Местные Общие Переменные
        int ThreadComplited = 0; // текущее количество выполненных потоков (++ после выполнения каждого из потоков, пока не достигнет числа)
        int VsegoThreads; //Общее количество запущенных потоков (присваивается до начала распределения потоков)
        List<string> RPList_postman;//лист с цифрами от RP (с удаленныеми запятыми (если вообще они будут))
        bool isStopped_Runner = false; // если будет остановлен раннен станет True, если нет то False

        /// <summary>
        ///Действия при выборе вкладок Раннера 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPostman_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            System.Windows.Controls.TabItem selectedTab = tabControl.SelectedItem as System.Windows.Controls.TabItem;

            if (selectedTab != null)
            {
                switch (selectedTab.Name)
                {
                    case "ListRP_Postman_Tab1":
                        // Код для обработки выбора первой вкладки
                        EditCanvas_Postman.Visibility = Visibility.Hidden; EditCanvas_Postman.IsEnabled = false;
                        break;
                    case "bodyTabItem":
                        // Код для обработки выбора второй вкладки
                        EditCanvas_Postman.Visibility = Visibility.Visible; EditCanvas_Postman.IsEnabled = true;
                        break;
                }
            }

        }

        /// <summary>
        /// Поиск запросов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchText_PostmanPost_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (SearchText_PostmanPost.Text != "")
            {
                foreach (var item in List_JSONS.Items)
                    if (item.ToString().ToUpper().Contains(SearchText_PostmanPost.Text.ToUpper()))
                    {
                        List_JSONS.SelectedItem = item;
                        List_JSONS.ScrollIntoView(List_JSONS.Items.GetItemAt(List_JSONS.SelectedIndex));
                    }
            }
        }

        #region Кнопки ADD, Delete, Edit в окне раннера

        /// <summary>
        /// Метод для добавления в реестр запроса
        /// </summary>
        public void Addrequest_inRegistry()
        {
            AddPost_request AddPostRq = new AddPost_request(this);
            AddPostRq.Show();


        }

        /// <summary>
        /// Удаление выбранного Запроса из реестра и обновление списка запросов
        /// </summary>
        public void DeletePost()
        {
            Body_post_text.Text = "";
            url_post_text.Text = "";

            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Posts_requests", true);
            if (List_JSONS.SelectedItem != null)
            {
                foreach (var item in List_JSONS.Items) //шарапово из лсита на форме 
                {
                    //ищу выбранный элемент в реестре
                    foreach (var post in key?.GetValueNames()) //Name_... 
                    {
                        if (post.Contains(List_JSONS.SelectedItem.ToString()))
                        {
                            string founded = post.Replace(" ", "").Replace("Name_", "").Replace("Url_", "").Replace("Body_", "");
                            if (List_JSONS.SelectedItem.ToString().Replace(" ", "").Replace("Name_", "").Replace("Url_", "").Replace("Body_", "") == founded)
                            {
                                key.DeleteValue(post);

                            }

                        }
                    }

                }
            }
            LoadPosts();
        }

        /// <summary>
        /// Метод реактинрования запросов апи для Postman - делает лоступными поля для редактирования
        /// </summary>
        public void EditPost_buttonSaveON()
        {
            string ssl = List_JSONS.SelectedItem?.ToString();
            if (ssl != null)
            {
                //делаю поля не isReadOnly + делаю активной кнопку сохранения
                Body_post_text.IsReadOnly = false;
                url_post_text.IsReadOnly = false;
                SaveEdit_Postman.IsEnabled = true;
                SaveEdit_Postman.Visibility = Visibility.Visible;
            }
            else
            {

                MessageBox.Show("Выберите сначала запрос из списка для редактирования!");

            }


        }

        /// <summary>
        ///Сохранияет отредактированные данные в реестр и обновляет список запросов 
        /// </summary>
        public void EditPost_SaveButton()
        {
            //делаю поля  isReadOnly + делаю неактивной кнопку сохранения
            Body_post_text.IsReadOnly = true;
            url_post_text.IsReadOnly = true;
            SaveEdit_Postman.IsEnabled = false;
            SaveEdit_Postman.Visibility = Visibility.Hidden;

            //сохраняю изменения в реестр 

            string ssl = List_JSONS.SelectedItem?.ToString(); // выбраный запрос (там уже имя запроса)

            if (ssl != null)
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Posts_requests"))
                {
                    if (ssl != "" && url_post_text.Text != "" && Body_post_text.Text != "")
                    {
                        key?.SetValue("Name_" + ssl, ssl);
                        key?.SetValue("Url_" + ssl, url_post_text.Text);
                        key?.SetValue("Body_" + ssl, Body_post_text.Text);

                    }
                    else MessageBox.Show("Необходимо заполнить все поля!");


                }


                SaveText_animPost.Visibility = Visibility.Visible;

                // Устанавливаем таймер для автоматического закрытия надписи "Сохранено" через 2 секунды
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (sender, e) =>
                {
                    SaveText_animPost.Visibility = Visibility.Hidden;
                    timer.Stop();
                };
                timer.Start();
            }

        }
        #endregion

        /// <summary>
        /// Загрузка данных в поля из реестра
        /// </summary>
        public void LoadTextB()
        {


            string ssl = List_JSONS.SelectedItem?.ToString(); // выбраный запрос (там уже имя запроса)
            if (ssl != null)
            {

                //если чтото выбрано, деалем активной кнопку Удалить
                RemoveCanvas_Postman.Opacity = 100;
                RemoveCanvas_Postman.IsEnabled = true;

                string url = url_post_text.Text;
                string body = Body_post_text.Text;
                Post_pull_Up_Registry(ssl, ref url, ref body);

                url_post_text.Text = url;
                Body_post_text.Text = body;
            }

        }


        /// <summary>
        /// Метод, подтягивающий данные о Post-запросе из реестра (url и Body запроса) для того чтобы записать их в поля или выполнить
        /// </summary>
        /// <param name="url">Сюда будет записан url запроса</param>
        /// <param name="bodyPost">Сюда будет записан Body запроса</param>
        /// /// <param name="namePost">Сюда нужно вписать имя запроса</param>
        public void Post_pull_Up_Registry(string namePost, ref string url, ref string bodyPost)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Posts_requests"))
            {

                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains(namePost))
                    {
                        string founded = item.Replace(" ", "").Replace("Url_", "").Replace("Body_", "");
                        if (founded == namePost.Replace(" ", ""))
                        {
                            //получаем значения url и body переменных из реестра в соответствующие переменные
                            if (item.Contains("Url_")) url = key?.GetValue(item).ToString();
                            if (item.Contains("Body_")) bodyPost = key?.GetValue(item).ToString();
                        }

                    }

                }

            }
        }

        /// <summary>
        ///Загрузка имен Запросов в лист из реестра сразу в конкретный лист List_JSONS
        /// </summary>
        public void LoadPosts()
        {
            List<string> Posts_Name = new List<string>();
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Posts_requests");

            foreach (var item in key?.GetValueNames())
            {
                if (item.Contains("Name_"))
                    Posts_Name.Add(key.GetValue(item).ToString());

            }


            //Hosts.Add(key.ToString());
            Posts_Name = Posts_Name.OrderBy(item => item).ToList();
            List_JSONS.ItemsSource = Posts_Name;

        }

        /// <summary>
        ///Кнопка запуска раннера"Пуск"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunnerStart_Click(object sender, RoutedEventArgs e)
        {
            if (ListRP_postman.Text != "")
            {//если у нас там точно чтото есть в поле с посылками 
                //очищать старые раны (потоки в таблице и записи)
                TabPostman_Responses.Items.Clear();
                //если выбран метод и если поля метода не пустые!!
                string ssl = List_JSONS.SelectedItem?.ToString();
                if (ssl != null && url_post_text.Text != null && Body_post_text.Text != null && ListRP_postman.Text != null)
                {

                    //•Распределяем по потокам на отдельные листы
                    //• запускаем раннеры в фоне по каждому элементу каждого листа вместо спецсимвола "{{R}}" = колву потоков и проверяем каждый раз не была ли нажата кнопка "стопа" а также не завершил ли каждый поток проход

                    RunnerStart.IsEnabled = false; //Отключение кнопки "пуск"
                    RunnerStop.IsEnabled = true; //Включение кнопки "Стоп"
                    RunnerStop.Background = (Brush)new BrushConverter().ConvertFrom("#FFC51308");
                    RunnerStop.Foreground = new SolidColorBrush(Colors.White);

                    //• Формируем корректный список из RP

                    //Убираем все пустые строки
                    // ListRP_postman.Text.Split(",\n").ToList();
                    string[] lines = ListRP_postman.Text.Split(new[] { "\r\n", "\r", "\n", "," }, StringSplitOptions.None);
                    lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

                    if (ListRP_postman.LineCount <= 10) //если в поле менее 10 строк (отправленмий), то автоматически ставится 1 поток
                        SelectorThreads_postman.SelectedIndex = 0;
                    List<string> listRP = new List<string>(lines); //лист со всеми rp

                    #region RP_Stats
                    /////////////--------------------------------
                    ////////////-----------запись кол-ва в реестр для статы--------------
                    int statsRP = 0;
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM"))
                    {

                        foreach (var item in key?.GetValueNames())
                        {
                            if (item.Contains("RP_post_stats_"))
                            {
                                statsRP = (int)key?.GetValue(item);

                            }
                        }
                    }

                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM"))
                    {
                        if (listRP.Count != 0)
                        {
                            key?.SetValue("RP_post_stats_", listRP.Count + statsRP);

                        }
                    }
                    //////////------------------------------------------
                    #endregion


                    //•Делим списки RP для нашего числа потоков 
                    int chunkSize_post = listRP.Count / (SelectorThreads_postman.SelectedIndex + 1);
                    VsegoThreads = (SelectorThreads_postman.SelectedIndex + 1);
                    ThreadComplited = 0;
                    List<List<string>> chunks = new List<List<string>>();
                    for (int i = 0; i < listRP.Count; i += chunkSize_post)
                    {
                        if ((listRP.Count - (chunkSize_post * (chunks.Count + 1))) < listRP.Count / 10)
                        {

                            //берем все до конца с текущего I 
                            chunks.Add(listRP.GetRange(i, listRP.Count - i));
                            break;
                        }
                        else
                        {
                            chunks.Add(listRP.GetRange(i, Math.Min(chunkSize_post, listRP.Count - i)));
                        }

                    }

                    //• получаю данне об post-запросе (url и body)
                    string selectPost = List_JSONS.SelectedItem?.ToString(); // выбраный запрос (там уже имя запроса)
                    string urlP = "";
                    string bodyP = "";
                    if (selectPost != null)
                    {

                        //если чтото выбрано, деалем активной кнопку Удалить
                        RemoveCanvas_Postman.Opacity = 100;
                        RemoveCanvas_Postman.IsEnabled = true;


                        Post_pull_Up_Registry(selectPost, ref urlP, ref bodyP);

                    }



                    //• Запуск раннеров по колву потоков /Распределитель потоков
                    for (int i = 0; i < (SelectorThreads_postman.SelectedIndex + 1); i++)
                    {

                        //• Переключение TabControl`ов на таб с динамичными вкладками по потокам 
                        TabPostman.Visibility = Visibility.Hidden;
                        TabPostman_Responses.Visibility = Visibility.Visible;
                        //создаем listView`s по количеству потоков в tabControl

                        // создаем новую вкладку
                        System.Windows.Controls.TabItem tabItem = new System.Windows.Controls.TabItem();
                        tabItem.Header = $"Поток {i + 1}"; // задаем заголовок вкладки

                        // создаем новый Grid для вкладки
                        Grid grid = new Grid();
                        tabItem.Content = grid; // добавляем Grid в содержимое вкладки

                        // создаем новый ListBox для Grid
                        ListBox listBox = new ListBox();
                        listBox.Name = $"listBox{i + 1}"; // задаем имя ListBox на основе индекса вкладки
                        grid.Children.Add(listBox); // добавляем ListBox в Grid

                        // добавляем вкладку в TabControl
                        TabPostman_Responses.Items.Add(tabItem);


                        //выбираем ЛисВью в который будем писать, нужный url, body, номер потока, и колво посылок для этого потока и запускаем раннер                     

                        RuunerPost_Postman(listBox, urlP, bodyP, i, chunks);
                    }
                    //лог
                    WriteLogsToFile("Запущен раннер для метода:", $@"{bodyP} Посылки:{ListRP_postman.Text.Replace("\n\r", "")}");
                }
                else
                {
                    MessageBox.Show("Проверьте : либо не выбран метод, либо поля некорректно заполнены (пустые)!");
                }

            }

            else MessageBox.Show("Заполните поле с посылками!");
        }

        /// <summary>
        /// Раннер
        /// </summary>
        /// <param name="Thread">Поток</param>
        /// <param name="chunks">Чанк посылок</param>
        /// <param name="url">Url нужного запроса</param>
        ///<param name="body">Body нужного запроса</param>
        ///<param name="MyList">Лист для вывода конкретного потока</param>
        async void RuunerPost_Postman(ListBox MyList, string url, string body, int Thread, List<List<string>> chunks)
        {

            for (int i = 0; i < chunks[Thread].Count; i++)
            {
                if (isStopped_Runner == false)
                {// раннен не был остановлен

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (var httpClient = new HttpClient())
                    {
                        string bodyZapros = body.Replace("{{R}}", chunks[Thread][i]);
                        var httpContent = new StringContent(bodyZapros);

                        /*        $@"{{
                                                                    ""id"": ""JsonRpcClient.js"",
                                                                    ""jsonrpc"": ""2.0"",
                                                                    ""method"": ""sapCreatePackage"",
                                                                    ""params"": [{chunks[Thread][i]}]
                                                                }}");*/
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                        using (var response = await httpClient.PostAsync(url, httpContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {

                                ///получать сам ответ от запроса для логирования

                                await response.Content.ReadAsStringAsync();
                                stopwatch.Stop();
                                Label label = new Label();
                                label.Content = $"Iteration {i + 1} success. Response time: {stopwatch.ElapsedMilliseconds} ms";
                                MyList.Items.Add(label);


                            }
                            else
                            {

                                var responseContent = await response.Content.ReadAsStringAsync();
                                // $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";
                                stopwatch.Stop();
                                Label label = new Label();
                                label.Content = $"Iteration {i + 1}. Error:{response.StatusCode}; Error message: {responseContent}.  Response time: {stopwatch.ElapsedMilliseconds} ms";
                                MyList.Items.Add(label);


                            }
                            //return await response.Content.ReadAsStringAsync();
                        }
                    }
                    MyList.ScrollIntoView(MyList.Items.GetItemAt(i));
                }
                else
                {
                    //раннер будет остановлен

                    break;

                }
            }
            checkFinallyThreads();

        }



        /// <summary>
        /// Чек финала потоков Postman
        /// </summary>
        void checkFinallyThreads()
        {
            ThreadComplited++;
            if (ThreadComplited == VsegoThreads)
            {

                if (isStopped_Runner == false)
                {     //если потоки завершились и кнопка остановки не прожималась то выдваем звук конца и возвращаем кнопки запуска и стопа на место для след запусков           

                    //• Звук уведомление о финале файла
                    using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                    using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                        new SoundPlayer(gzOut).Play();

                    RunnerStart.IsEnabled = true;
                    RunnerStop.IsEnabled = false;
                    RunnerStop.Foreground = new SolidColorBrush(Colors.Black);
                    TabPostman.Visibility = Visibility.Visible;
                    TabPostman_Responses.Visibility = Visibility.Hidden;


                }
                else
                {
                    //кнопка остановки прожималась и остановились все потоки 
                    MessageBox.Show("Раннер остановлен");
                    isStopped_Runner = false;
                    RunnerStart.IsEnabled = true;
                    RunnerStop.IsEnabled = false;
                    RunnerStop.Foreground = new SolidColorBrush(Colors.Black);
                    TabPostman.Visibility = Visibility.Visible;
                    TabPostman_Responses.Visibility = Visibility.Hidden;

                }

            }
        }

        /// <summary>
        /// Кнопка "Стоп"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunnerStop_Click(object sender, RoutedEventArgs e)
        {
            isStopped_Runner = true;
            RunnerStart.IsEnabled = true;
            RunnerStop.IsEnabled = false;
            RunnerStop.Foreground = new SolidColorBrush(Colors.Black);
            TabPostman.Visibility = Visibility.Visible;
            TabPostman_Responses.Visibility = Visibility.Hidden;
            //лог
            WriteLogsToFile("Раннер остановлен!", "");
        }

        /// <summary>
        /// ранер для переподготовки (фоновый)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="Thread"></param>
        /// <param name="chunks"></param>
        async void RuunerPost_Postman(string url, string body, int Thread, List<List<string>> chunks)
        {

            for (int i = 0; i < chunks[Thread].Count; i++)
            {
                using (var httpClient = new HttpClient())
                {
                    string bodyZapros = body.Replace("{{R}}", chunks[Thread][i]);
                    var httpContent = new StringContent(bodyZapros);

                    /*        $@"{{
                                                                ""id"": ""JsonRpcClient.js"",
                                                                ""jsonrpc"": ""2.0"",
                                                                ""method"": ""sapCreatePackage"",
                                                                ""params"": [{chunks[Thread][i]}]
                                                            }}");*/
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    using (var response = await httpClient.PostAsync(url, httpContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {

                            ///получать сам ответ от запроса для логирования

                            await response.Content.ReadAsStringAsync();
                        }
                        else
                        {

                            var responseContent = await response.Content.ReadAsStringAsync();
                            // $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";
                        }
                        //return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            checkFinallyThreads();

        }

        /// <summary>
        /// Кнопка очистки
        /// </summary>
        private void ButtonClear_runner_Click(object sender, RoutedEventArgs e)
        {
            ListRP_postman.Text = null;
        }
        #endregion

        #region izmenenie statusov

        /// <summary>
        /// переменные с данными о бд из реестра 
        /// </summary>
        string DB_Name_IS;


        /// <summary>
        /// Подгрузчик списка складов на страницу | обрабаотка функций при открытии на TB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl TB = sender as System.Windows.Controls.TabControl;
            System.Windows.Controls.TabItem selectedTab = TB.SelectedItem as System.Windows.Controls.TabItem;

            if (selectedTab != null)
            {

                switch (selectedTab.Name)
                {
                    case "Change_status":
                        // Код для обработки выбора первой вкладки
                        WarEdit warEdit = new WarEdit();
                        warEdit.LoadHosts(vibor_sklada);
                        warEdit.Close();
                        break;
                }
            }


        }

        /// <summary>
        /// Загрузчик списка складов в лист vibor_sklada
        /// </summary>
        public void zagruzka_skladov_IS()
        {
            WarEdit warEdit = new WarEdit();
            warEdit.LoadHosts(vibor_sklada);
            warEdit.Close();
        }

        /// <summary>
        /// Поле Поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void poisk_skladov_IS_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (text_box_poisk_sklada_IS.Text.Length >= 3)
                if (text_box_poisk_sklada_IS.Text != "")
                    foreach (var item in vibor_sklada.Items)
                        if (item.ToString().ToUpper().Contains(text_box_poisk_sklada_IS.Text.ToUpper()))
                        {
                            vibor_sklada.SelectedItem = item;
                            vibor_sklada.ScrollIntoView(vibor_sklada.Items.GetItemAt(vibor_sklada.SelectedIndex));
                        }

        }



        /// <summary>
        /// Кнопка "Запуск"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Smeni_Status_Click(object sender, RoutedEventArgs e)
        {
            string statusChanget = "";
            string systemChanget = "";
            if (RP_list_Status.Text != "")
            {
                if (ComboBox_Sh_Status.SelectedIndex != 0)
                { //если у нас вообще чтото выбранов поле статусов Шиптора 
                    switch (ComboBox_Sh_Status.SelectedIndex)
                    {
                        case 1:
                            dataBases.ConnectDB("Шиптор", $@"update public.package set measured_at = null, packed_since = null, prepared_to_send_since = null, in_store_since = null, current_status = 'new' where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "new";
                            break;
                        case 2:
                            dataBases.ConnectDB("Шиптор", $@"update package set current_status = 'packed', sent_at = NULL, returned_at = null, reported_at = null, returning_to_warehouse_at = null, delivery_point_accepted_at = null, delivered_at = null, removed_at = null, lost_at = null, in_store_since = now(), measured_at = now(), packed_since = now(), prepared_to_send_since = now() where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "packed";
                            break;
                        case 3:
                            dataBases.ConnectDB("Шиптор", $@"update package p set current_status = 'sent', sent_at = now(), returned_at = null, returning_to_warehouse_at = null, delivery_point_accepted_at = null, delivered_at = null, removed_at = null, lost_at = null where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "sent";
                            break;
                        case 4:
                            dataBases.ConnectDB("Шиптор", $@"update package p set current_status = 'waiting_on_delivery_point', sent_at = now(), returned_at = null, returning_to_warehouse_at = null, delivery_point_accepted_at = now(), delivered_at = null where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "waiting_on_delivery_point";
                            break;
                        case 5:
                            dataBases.ConnectDB("Шиптор", $@"update package set current_status = 'delivered', delivered_at = now(), lost_at = null, reported_at = null, returned_at = null, returning_to_warehouse_at = null, sent_at = now(), removed_at = null where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "delivered";
                            break;
                        case 6:
                            dataBases.ConnectDB("Шиптор", $@"update package p set current_status = 'returned_to_warehouse', returned_at = now(), lost_at = null, removed_at = null, reported_at = null, in_store_since = now(), measured_at = now(), packed_since = now() where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "returned_to_warehouse";
                            break;
                        case 7:
                            dataBases.ConnectDB("Шиптор", $@"update package p set current_status = 'to_return', returned_at = now(), lost_at = null, removed_at = null, reported_at = null, in_store_since = now(), measured_at = now(), packed_since = now() where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "to_return";
                            break;
                        case 8:
                            dataBases.ConnectDB("Шиптор", $@"update package p set current_status = 'returned', returned_at = now(), lost_at = null, removed_at = null, in_store_since = now(), measured_at = now(), packed_since = now(), reported_at = now() where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "returned";
                            break;
                        case 9:
                            dataBases.ConnectDB("Шиптор", $@"update package set current_status = 'removed', removed_at = now() where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "removed";
                            break;
                        case 10:
                            dataBases.ConnectDB("Шиптор", $@"update package p set current_status = 'return_to_sender', returned_at = now(), lost_at = null, removed_at = null, reported_at = null where id in ({RP_list_Status.Text})"); systemChanget = "Шиптор"; statusChanget = "return_to_sender";
                            break;
                        /*case "":
                            dataBases.ConnectDB("Шиптор", $@"where id in ({RP_list_Status.Text})");
                            break;*/
                        default:
                            break;
                    }
                    ComboBox_Sh_Status.SelectedIndex = 0;
                    //• Звук уведомление о финале 
                    using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                    using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                        new SoundPlayer(gzOut).Play();

                    //лог
                    WriteLogsToFile("Изменение статуса", $@" Система {systemChanget}, на какой статус изменен: {statusChanget}, Посылки: {RP_list_Status.Text.Replace("\n", "")}");

                }
                if (ComboBox_ZS_Status.SelectedIndex != 0)
                { //Если чтото выбрано в поле заппа то дальше уже ищем инфо о бд
                    LoadData_FromReg_izmenenie_status();
                    if (DB_Name_IS != null)
                    {//если найдена инфа по хосту и бд в реестре то идем дальше

                        switch (ComboBox_ZS_Status.SelectionBoxItem)
                        {

                            case "На складе":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'in_store' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "in_store";
                                break;
                            case "Отправлена":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'sent' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "sent";
                                break;
                            case "Ждет возврата отправителю":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'wait_return_to_sender' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "wait_return_to_sender";
                                break;
                            case "Расформирована":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'disbanded' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "disbanded";
                                break;
                            case "Ожидает сортировки":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'wait_sorting' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "wait_sorting";
                                break;
                            case "Возвращена на склад":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'returned' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "returned";
                                break;
                            case "Возвращена отправителю":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'returned_to_sender' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "returned_to_sender";
                                break;
                            case "В паллете":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status = 'in_pallet' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "in_pallet";
                                break;
                            case "Ожидает поступления": //
                                dataBases.ConnectDB(DB_Name_IS, $@"update package set status ='waiting_arrival' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "waiting_arrival";
                                break;
                            case "Ждет продуктов":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set set status ='wait_products' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "wait_products";
                                break;
                            case "Ждет повторной подачи":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status ='waiting_for_resubmission' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "waiting_for_resubmission";
                                break;
                            case "В партии":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status ='in_package_return' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "in_package_return";
                                break;
                            case "Ждет сборки":
                                dataBases.ConnectDB(DB_Name_IS, $@"update package a set status ='wait_gather' where package_fid in ({RP_list_Status.Text});"); systemChanget = DB_Name_IS; statusChanget = "wait_gather";
                                break;
                            default:
                                break;
                        }
                        DB_Name_IS = null;
                        ComboBox_ZS_Status.SelectedIndex = 0;

                        //• Звук уведомление о финале 
                        using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                        using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                            new SoundPlayer(gzOut).Play();
                        //лог
                        WriteLogsToFile("Изменение статуса", $@" Система {systemChanget}, на какой статус изменен: {statusChanget}, Посылки: {RP_list_Status.Text.Replace("\n", "")}");
                    }
                    else
                    {
                        //прогрывать звук Windows Ошибка error
                        string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                        // Создание экземпляра SoundPlayer и проигрывание звука
                        using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                        {
                            errorSoundPlayer.Play();
                        };
                        MessageBox.Show("Не выбрад склад!");

                    }
                    //Галка снятия стопа "Разрыв ММ"
                }
                if ((snyat_stop_smena_statusov.IsChecked == true) && (label_vibran_sklad_dinamik.Content != ""))
                {
                    LoadData_FromReg_izmenenie_status();
                    dataBases.ConnectDB(DB_Name_IS, $@"delete from package_stop where package_id in (select id from package where package_fid in  ({RP_list_Status.Text})) and code = 'break_multiplace';");
                    dataBases.ConnectDB(DB_Name_IS, $@"update parent_package pp set is_full = true where pp.fid in ({RP_list_Status.Text});");
                    snyat_stop_smena_statusov.IsChecked = false;
                    //• Звук уведомление о финале 
                    using (MemoryStream fileOut = new MemoryStream(Properties.Resources.untitled))
                    using (GZipStream gzOut = new GZipStream(fileOut, CompressionMode.Decompress))
                        new SoundPlayer(gzOut).Play();
                }
            }
            else
            {
                //прогрывать звук Windows Ошибка error
                string errorSoundPath = @"C:\Windows\Media\Windows Error.wav";

                // Создание экземпляра SoundPlayer и проигрывание звука
                using (SoundPlayer errorSoundPlayer = new SoundPlayer(errorSoundPath))
                {
                    errorSoundPlayer.Play();
                }
                MessageBox.Show("Не введены номера посылок!");
            }
        }

        /// <summary>
        /// Получаем из реестра данные о выбранной бд 
        /// </summary>
        public void LoadData_FromReg_izmenenie_status()
        {
            string ssl = vibor_sklada.SelectedItem?.ToString(); // выбраное бд
            if (ssl != null)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Hosts");
                foreach (var item in key?.GetValueNames())
                {
                    if (item.Contains(ssl))
                    {
                        string founded = item.Replace(" ", "").Replace("Name_", "").Replace("Host_", "").Replace("Post", "").Replace("DataBase_", "");
                        if (founded == ssl.Replace(" ", ""))
                        {
                            if (item.Contains("Name_")) DB_Name_IS = key?.GetValue(item).ToString();
                        }

                    }
                    //Hosts_Name.Add(key.GetValue(item).ToString());

                }
            }


        }


        private void Button_Clear_Smena_Statusa_Click(object sender, RoutedEventArgs e)
        {
            RP_list_Status.Text = null;
            vibor_sklada.SelectedIndex = -1;
            text_box_poisk_sklada_IS.Text = null;
        }

        private void vibor_sklada_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            label_vibran_sklad_dinamik.Content = vibor_sklada.SelectedValue;
        }

        private void Kopirovat_otver_statusi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText("Здравствуйте! Статус посылок был скорректирован.");
            }
            catch
            {

            }
        }




        #endregion





        /*        #region Kafka_and_Import_Sap_ZApp
        // запрос к кафке по номеру sap_id, поиск сообщений из топика 

        /// <summary>
        /// Подключение к Кафка и чтение данных из топика 
        /// </summary>
        /// <param name="host">Хост сервера кафки</param>
        /// <param name="port"> Порт сервера кафки</param>
        /// <param name="GroupId">Группа доступа пользователей (возможно брокер)</param>
        /// <param name="topic">Имя нужного топика для чтения</param>
        /// <param name="TE">Редактор вывода ответа (в будущем можно заменить на что-либо другое)</param>
        public void _kafka_ConnectAndRead(string host, string port, string GroupId, string topic, TextEditor TE)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = $"{host}:{port}",
                GroupId = GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topic);
            while (!TE.Text.Contains("message"))
            {
                var consumeResult = consumer.Consume();
                TE.Text += $"\n\rReceived message from topic {consumeResult.Topic}: {consumeResult.Message.Value}";
            }
        }

        /// <summary>
        ///Кнопка для теста кафки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Kafka_Click(object sender, RoutedEventArgs e)
        {
            _kafka_ConnectAndRead("pd10-kafka-n5.int.sblogistica.ru", "19092", "8", "ems.integration.wms.zappstore", TextEditor_Response_kafka);
        }









        #endregion
        */

    }
}
