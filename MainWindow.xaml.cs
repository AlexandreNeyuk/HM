using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.Forms.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;

namespace HM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            #region Начальные накстройки
            TB.Margin = new Thickness(0, 0, 0, 0); //выравниванеи TableControl 
            ///отключение панели настроек при иницииализации --
            SettingsGrid.IsEnabled = false;
            SettingsGrid.Visibility = Visibility.Hidden;
            #endregion

            #region Regisrty Staff           
            ///Пересоздание корня настроек в реестре + синхрон с реестром настроек--
            using RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Settings"))
            {
                if (key != null)
                {
                    UserName.Text = key.GetValue("Имя пользователя")?.ToString();
                    UserPass.Password = key.GetValue("Пароль")?.ToString();
                    ShiptorHost.Text = key.GetValue(ShiptorHost.Name)?.ToString();
                    ShiptorDB.Text = key.GetValue(ShiptorDB.Name)?.ToString();
                    PostHost.Text = key.GetValue(PostHost.Name)?.ToString();
                    PostDB.Text = key.GetValue(PostDB.Name)?.ToString();
                    PVZHost.Text = key.GetValue(PVZHost.Name)?.ToString();
                    PVZDB.Text = key.GetValue(PVZDB.Name)?.ToString();
                    SP_HotKey.Text = key.GetValue(SP_HotKey.Name)?.ToString();
                }
            }
            #endregion

            #region Добавление обработки событий для всех полей HotKeys в Настройках
                KeyTextBoxies = new List<TextBox>() { SP_HotKey }; ///все поля для которых нужно свойство HotKeys, через  ","
                foreach (var item in KeyTextBoxies)
                {
                    item.KeyDown += (s, a) => { item.IsReadOnly = true; item.Text = a.Key.ToString(); };

                }

            #endregion



        }
        //Обьявление классов / глобальных переменных
        List<TextBox> KeyTextBoxies; // ///все поля для которых нужно свойство введения HotKeys, через  ","
        //Animations Animations = new Animations();
        bool SetPanel = false; //false - закрытая панель, true - открытая панлеь

#if Button_close
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
        //    Close_Butt.Background = new SolidColorBrush(Colors.Red);
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
#endif

        /// <summary>
        ///Кнопка боковой панели 
        /// </summary>
        private async void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            const int marginLeftMin = 0;
            const int marginLeftMax = 150;
            double CurrentVarginLeft = TB.Margin.Left;
            string BF = TextBox.Text;
            TextBox.Text = null;


            ///анимация боковой панели  
            if (SetPanel == true)
            {


                while (TB.Margin.Left - TB.Margin.Left / 6 > marginLeftMin)
                {
                    CurrentVarginLeft = TB.Margin.Left;
                    await Task.Delay(1);
                    TB.Margin = new Thickness(CurrentVarginLeft - CurrentVarginLeft / 6, 0, 0, 0);
                    if (TB.Margin.Left < 2)
                    {
                        TB.Margin = new Thickness(0, 0, 0, 0);
                        break;
                    }
                }
                SetPanel = !SetPanel;

            }
            else
            {

                while (CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 6 < marginLeftMax)
                {
                    CurrentVarginLeft = TB.Margin.Left;
                    await Task.Delay(1);
                    TB.Margin = new Thickness(CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 6, 0, 0, 0);
                    if (TB.Margin.Left > 147)
                    {
                        TB.Margin = new Thickness(150, 0, 0, 0);
                        break;
                    }
                }
                SetPanel = !SetPanel;
            }
            TextBox.Text = BF;




        }


        private void TB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TB.SelectedIndex = 1; MessageBox.Show(TB.SelectedIndex.ToString());      

        }

        /// <summary>
        /// Открыть панель настроек 
        /// </summary>
        private void SettingCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            SettingsGrid.Visibility = Visibility.Visible;
            SettingsGrid.IsEnabled = true;
            Image_MouseLeftButtonDown(sender, e);


        }

        #region Table Item 1       
        /// <summary>
        /// Кнопка RP/UPPER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        private void ListProcess_Click(object sender, RoutedEventArgs e)
        {


            if (ContextRP.IsChecked == true)
            {
                //по сенарию RP (simple_List)
                TextBox.Text = TextBox.Text.Replace("RP", "");


                //###_refer_###
                //TextBox.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries)[TextBox.LineCount-1] + "," ; //отсавляю все что до символа? с указанием номера строки  - по сути сама RP
                ////TextBox.GetLineText(3); // получаю саму строку по номеру 
                //for (int i = 0; i < TextBox.LineCount - 1; i++) str.Add(TextBox.GetLineText(i) + ",");
                //TextBox.Text = null;
                //for (int i = 0; i < str.Count; i++) TextBox.Text += str[i];
            }
            else
            {
                //по сценарию с UPPER`S

                TextBox.Text = TextBox.Text.Replace("\r\n", "')," + "\rUPPER ('") + "')";
                TextBox.Text = "UPPER ('" + TextBox.Text;

            }

            //______________работа с запятыми______________
            if (comma.IsEnabled == true && comma.IsChecked == true)
            {
                TextBox.Text = TextBox.Text.Replace("\r\n", ",\n"); // - работает
            }
            if (comma.IsEnabled == true && comma.IsChecked == false)
            {
                TextBox.Text = TextBox.Text.Replace(",", "\r");
            }




        }

        /// <summary>
        /// радио ботон 1 -RP
        /// </summary>
        private void ConrextUpper_Checked(object sender, RoutedEventArgs e)
        {
            comma.IsEnabled = false;
        }

        /// <summary>
        ///Радио батон 2 - UPPER
        /// </summary>
        private void ContextRP_Checked(object sender, RoutedEventArgs e)
        {
            comma.IsEnabled = true;
        }
        #endregion



        /// <summary>
        /// Быстрые клавиши
        /// </summary>
        /// <param name="e">Введенная клавиша</param>
        private void HM_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ///Отключение ПАНЕЛИ НАСТРОЕК бысчрой клавишей 
            if (e.Key == Key.Escape && SettingsGrid.IsEnabled == true)
            {
                SettingsGrid.IsEnabled = false;
                SettingsGrid.Visibility = Visibility.Hidden;

            }
        }

        #region Settings      

            /// <summary>
            /// Постоматы
            /// </summary>
            private void PostomatsCanvas_MouseDown(object sender, MouseButtonEventArgs e)
            {

                MessageBox.Show("Здесь кода-нибудь чтото будет !))");

            }

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
            ///Сохранение Хостов/БД
            /// </summary>
            private void Button_Click_1(object sender, RoutedEventArgs e)
            {
                ///Шиптор 

                SaveProtected(ShiptorHost, ShiptorDB, SussessHostLabel);
                //PVZ
                SaveProtected(PVZHost, PVZDB, SussessHostLabel);
                ///Post
                SaveProtected(PostHost, PostDB, SussessHostLabel);

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

        #endregion


    }
}