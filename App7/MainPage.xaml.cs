using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App7
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //TODO: 
        // jak realeased poza kolem to zmiana mousepressed
        // zapamietanie kolorow na pasku i potem jak sie znowu otworzy, to odczytuje z pliku
        // dziedziczymy do wasnej klasy rectangle i tam dopisujemy flage
        // zobaczyc czy rectagle byl zmieniany
        // kasowanie kolorow jak sie nacisnie guzik

        private double CircleRadius;

        public int hue; 
        public double saturation;
        public double value;

        private bool MousePressed = false;

        public byte red = 0;
        public byte green = 0;
        public byte blue = 0;

        public MainPage()
        {
            this.InitializeComponent();
            GetRadiusOfCircle();
        }

        private void GetRadiusOfCircle()
        {
            this.CircleRadius = HsvCircleImg.Width / 2;
        }

        // when user taps on the image
        private void ImageTaped(object sender, TappedRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.Write(" Tapped ");
            var MousePositionTuple = GetMousePosition(e, "tapped");
            int xPosition = MousePositionTuple.Item1;
            int yPosition = MousePositionTuple.Item2;

            double distanceFromCenter = GetDistanceFromCenter(xPosition, yPosition);
            double saturation = GetSaturation(distanceFromCenter);

            if (saturation <= 1) // When clicked on circle 
            {
                this.saturation = saturation;
                this.hue = GetAngle(xPosition, yPosition); 
                ChangeHSTextBlocksValue(hue, saturation);
                GetRGBColorsChangeTextBlocksChnageColorOfPreviewColorEllipse();
            }
        }

        private void GetRGBColorsChangeTextBlocksChnageColorOfPreviewColorEllipse()
        {
            HsvToRgb(hue, saturation, value / 100, out red, out green, out blue); // przez referencje te z out
            ChangeRGBTextBlocksValue();
            ChangeColorOfPreviewColorEllipse(red, green, blue);
        }

        private Tuple<int, int> GetMousePosition(RoutedEventArgs e, string eventName)
        {
            var MousePos = new Point();

            if (eventName == "tapped")
            {
                var x = (TappedRoutedEventArgs)e;
                MousePos = x.GetPosition(HsvCircleImg);
            }
            else if (eventName == "moved")
            {
                var x = (PointerRoutedEventArgs)e;                
                MousePos = x.GetCurrentPoint(HsvCircleImg).Position;
                //System.Diagnostics.Debug.Write("movedIn");
            }

            int xPosition = Convert.ToInt16(MousePos.X);
            int yPosition = Convert.ToInt16(MousePos.Y);
            
            return Tuple.Create(xPosition, yPosition);
        }

        private void ChangeHSTextBlocksValue( int hue, double saturation)
        {
            textBlockHueOutput.Text = hue.ToString();
            textBlockSaturationOutput.Text = Convert.ToInt16(saturation * 100).ToString();
        }

        private void ChangeColorOfPreviewColorEllipse (byte red, byte green, byte blue)
        {
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(255, red, green, blue); 
            PreviewColorEllipse.Fill = mySolidColorBrush;
        }

        private void ChangeRGBTextBlocksValue()
        {
            textBlockRedOutput.Text = red.ToString();
            textBlockGreenOutput.Text = green.ToString();
            textBlockBlueOutput.Text = blue.ToString();
        }

        private void ChangeValueTextBlockValue()
        {
            textBlockValueOutput.Text = value.ToString();
        }

        private void ChangeBlackOverlayEllipseOpacity()
        {
            BlackOverlayEllipse.Opacity = 1 - value / 100.0;
        }

        private void ChangeSliderValue(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.value = slider.Value;
            ChangeValueTextBlockValue();
            ChangeBlackOverlayEllipseOpacity();
            GetRGBColorsChangeTextBlocksChnageColorOfPreviewColorEllipse();
        }

        private double GetDistanceFromCenter(int mouseX, int mouseY)
        {
            double middleX = CircleRadius;
            double middleY = CircleRadius;
            double triangleBase = Math.Abs(middleX - mouseX);
            double triangleHeight = Math.Abs(middleY - mouseY);
            double triangleDiagonal = Math.Sqrt(triangleBase * triangleBase + triangleHeight * triangleHeight);

            return triangleDiagonal;
        }

        private double GetSaturation(double distance)
        {
            double saturation = distance / CircleRadius;
            return saturation;
        }

        private int GetAngle(int mouseX,int mouseY)
        {
            double angle = Math.Abs (Math.Atan2( Convert.ToDouble(mouseY - CircleRadius), Convert.ToDouble(CircleRadius - mouseX) ) * 180 / 3.14 - 180);
            angle = (angle + 90) % 360;

            return Convert.ToInt16(angle);
        }
     
        /// Convert HSV to RGB
        /// h is from 0-360
        /// s,v values are 0-1
        /// r,g,b values are 0-255
        void HsvToRgb(int H, double S, double V, out byte r, out byte g, out byte b)
        { 
            double R, G, B;

            if (V <= 0)
            {
                R = G = B = 0;
            }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int) Math.Floor( hf );
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));

                switch (i)
                {

                    // Red is the dominant color
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color
                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color
                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color
                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.
                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = (byte)(R * 255.0);
            g = (byte)(G * 255.0);
            b = (byte)(B * 255.0);
        }

        private void PointerPressedImage(object sender, PointerRoutedEventArgs e)
        {
            //Events.Text = "Pressed";
            MousePressed = true;
            //System.Diagnostics.Debug.Write( " pressed " );
        }

        private void PointerReleasedImage(object sender, PointerRoutedEventArgs e)
        {
            //Events.Text = "Released";
            MousePressed = false;
            //System.Diagnostics.Debug.Write("PRI ");
        }

        private void PointerMovedImage(object sender, PointerRoutedEventArgs e)
        {
            if (MousePressed)
            {
                var MousePositionTuple = GetMousePosition(e, "moved");
                int xPosition = MousePositionTuple.Item1;
                int yPosition = MousePositionTuple.Item2;

                double distanceFromCenter = GetDistanceFromCenter(xPosition, yPosition);
                double saturation = GetSaturation(distanceFromCenter);

                if (saturation <= 1) // When clicked on circle 
                {
                    this.saturation = saturation;
                    this.hue = GetAngle(xPosition, yPosition); 
                    ChangeHSTextBlocksValue(hue, saturation);
                    GetRGBColorsChangeTextBlocksChnageColorOfPreviewColorEllipse();
                }
            }
        }

        private void RectangleTapped(object sender, PointerRoutedEventArgs e)
        {
            Rectangle rectangle = (Rectangle)sender;
           
            if (rectangle.Tag != "changed")
            {
                // when color of rectangle was not changed earlier
                rectangle.Tag = "changed";
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                mySolidColorBrush.Color = Color.FromArgb(255, red, green, blue);
                rectangle.Fill = mySolidColorBrush;       
            }
            else
            {
                Color colorOfRectangle = (rectangle.Fill as SolidColorBrush).Color;
                byte r = colorOfRectangle.R;
                byte g = colorOfRectangle.G;
                byte b = colorOfRectangle.B;
                red = r;
                green = g;
                blue = b;
                ChangeColorOfPreviewColorEllipse(r, g, b);
                
            }

        }
    }
}
