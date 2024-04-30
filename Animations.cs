using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HM
{
    internal class Animations
    {
        /// <summary>
        /// Линейная анимация 
        /// </summary>
        /// <param name="b">Обьект</param>
        /// <param name="marginA"> Начальнапя точка 0</param>
        /// <param name="marginB"> Конечная точка 1</param>
        /// <param name="t"> Время (миллисекунды)</param>
        public void LinearAnimation(System.Windows.Controls.TabControl b, int marginA, int marginB, double t)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = marginA;
            animation.To = marginB;
            animation.Duration = TimeSpan.FromMilliseconds(t);
            b.BeginAnimation(System.Windows.Controls.TabControl.MarginProperty, animation);

        }
        /// <summary>
        ///  Плавная анимация числа 
        /// </summary>
        ///  <param name="s"> Ссылка на переменную! </param>
        /// <param name="min"> Минимальное значание (ОТ) </param>
        /// <param name="max"> Максимальное значение</param>
        public void SwitchAnimation(ref double s, double min, double max)
        {
            /// 0 * 0-10
            if (min < max)
            { //обычное прибавление 
                while (s + (max - s) / 8 < max)
                {
                    TimerZ(1);
                    s = s + (max - s) / 8 + 2;
                    if (s > max - 1)
                    {
                        s = max;
                        break;
                    }
                }
            }
            else
            {
                //отрицательное прибавление
                while (s - s / 8 > min)
                {

                    TimerZ(1);
                    s = s - s / 8;
                    if (s < min + 1)
                    {
                        s = min;
                        break;
                    }
                }

            }

        }
        /// <summary>
        ///  Таймер 
        /// </summary>
        /// <param name="sec"> (Миллисекунды)</param>
        async public void TimerZ(int sec)
        {
            await Task.Delay(sec);
        }


        #region ImageAnimation_Rotate

        /// <summary>
        ////Анимация вращения картинки вокруг своей оси
        /// </summary>
        /// <param name="sec">Кол-во в сек. за сколько сделает полный оборот вокург своей оси</param>
        public void Animation_rotate_ON(double sec, Image ob)
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation();
            rotationAnimation.From = 0;
            rotationAnimation.To = 360;
            rotationAnimation.Duration = new Duration(TimeSpan.FromSeconds(sec));
            rotationAnimation.RepeatBehavior = RepeatBehavior.Forever;

            RotateTransform rotateTransform = new RotateTransform();
            ob.RenderTransform = rotateTransform;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }

        public void Animation_rotate_OFF(Image ob)
        {
            ob.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        #endregion

    }



}
