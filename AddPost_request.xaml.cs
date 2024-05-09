using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HM
{
    /// <summary>
    /// Логика взаимодействия для AddPost_request.xaml
    /// </summary>
    public partial class AddPost_request : Window
    {
        private MainWindow _mainWindow;
        public AddPost_request(MainWindow mainWindow)
        {

            InitializeComponent();

            _mainWindow = mainWindow;

            ToolTip myToolTip = new ToolTip();

            L.MouseDown += (a, e) =>
            {
                try { Clipboard.SetText("{{R}}"); } catch { }
                L.ToolTip = "Скопировано!";
            };


            #region автоматический ToolTip для копирования

            // Устанавливаем ToolTip для TextBlock
            L.ToolTip = "Клик, чтобы скопировать";

            #endregion
        }

        #region Buttons

        /// <summary>
        ///Кнопка сохранения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\HM\Posts_requests"))
            {
                if (name_post.Text != "" && url_post.Text != "" && body_post.Text != "")
                {
                    key?.SetValue("Name_" + name_post.Text, name_post.Text);
                    key?.SetValue("Url_" + name_post.Text, url_post.Text);
                    key?.SetValue("Body_" + name_post.Text, body_post.Text);

                }
                else MessageBox.Show("Необходимо заполнить все поля!");


            }

            // MessageBox.Show("Готово!");
            ClearTextBox();
            _mainWindow.LoadPosts();
            Button_Click_1(sender, e);
        }

        /// <summary>
        ///Очистка полей
        /// </summary>
        public void ClearTextBox()
        {
            name_post.Text = null;
            url_post.Text = null;
            body_post.Text = null;

        }

        /// <summary>
        /// Отмена + закрытие окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClearTextBox();
            this.Close();
        }

        #endregion


    }
}
