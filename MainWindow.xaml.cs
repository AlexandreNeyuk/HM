using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MessageBox = System.Windows.Forms.MessageBox;
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
            TB.Margin = new Thickness(0, 0, 0, 0); //выравниванеи TableControl 
            ///отключение панели настроек при иницииализации --
            SettingsGrid.IsEnabled = false;
            SettingsGrid.Visibility = Visibility.Hidden;
            ///--

            ///Пересоздание корня настроек в реестре + синхрон с реестром настроек--
            using RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\HM\Settings");
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Settings"))
            {
                if (key != null)
                {
                    UserName.Text = key.GetValue("Имя пользователя")?.ToString();
                    UserPass.Password = key.GetValue("Пароль")?.ToString();
                    ShiptorHost.Text = key.GetValue("Shiptor Host")?.ToString();
                    ShiptorDB.Text = key.GetValue("Shiptor DB")?.ToString();
                    PostHost.Text = key.GetValue("Postamat Host")?.ToString();
                    PostDB.Text = key.GetValue("Postamat DB")?.ToString();
                    PVZHost.Text = key.GetValue("PVZ Host")?.ToString();
                    PVZDB.Text = key.GetValue("PVZ DB")?.ToString();
                    SP_HotKey.Text = key.GetValue("Switch Panel HotKey")?.ToString();
                }
            }
            ///--

        }
        //Обьявление классов / глобальных переменных        
        Animations Animations = new Animations();
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

            ///анимация боковой панели  
            if (SetPanel == true)
            {


                while (TB.Margin.Left - TB.Margin.Left / 8 > marginLeftMin)
                {
                    CurrentVarginLeft = TB.Margin.Left;
                    await Task.Delay(1);
                    TB.Margin = new Thickness(CurrentVarginLeft - CurrentVarginLeft / 8, 0, 0, 0);
                    if (TB.Margin.Left < 1)
                    {
                        TB.Margin = new Thickness(0, 0, 0, 0);
                        break;
                    }
                }
                SetPanel = !SetPanel;

            }
            else
            {

                while (CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 8 < marginLeftMax)
                {
                    CurrentVarginLeft = TB.Margin.Left;
                    await Task.Delay(1);
                    TB.Margin = new Thickness(CurrentVarginLeft + (marginLeftMax - CurrentVarginLeft) / 8, 0, 0, 0);
                    if (TB.Margin.Left > 149)
                    {
                        TB.Margin = new Thickness(150, 0, 0, 0);
                        break;
                    }
                }
                SetPanel = !SetPanel;


            }




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
        /// <summary>
        /// Кнопка 
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

        /// <summary>
        /// Постоматы
        /// </summary>
        private void PostomatsCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            MessageBox.Show("Здесь кода-нибудь чтото будет !))");

        }
    }
}