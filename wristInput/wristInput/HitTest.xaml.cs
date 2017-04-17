using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace AssignmentTwo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class HitTest : Window
    {
        int count;
        string fileName;

        int testCount = 0;
        List<string> tests;

        Canvas mycanvas = new Canvas();
        Button toggleBtn = new Button();
        Button recalibrationBtn = new Button();

        Ellipse cursor = new Ellipse();
        Doughnut testDoughnut;

        public HitTest(int serialNum, string fileName)
        {
            InitializeComponent();
            this.count = serialNum;
            this.fileName = fileName;
            tests = readFromRecordFile(this.count);

            this.Content = mycanvas;

            toggleBtn.Margin = new Thickness(450, 180, 0, 0);
            toggleBtn.Width = 100;
            toggleBtn.Height = 30;
            toggleBtn.Content = "Start";
            toggleBtn.Click += toggleTest;
            recalibrationBtn.Margin = new Thickness(450, 220, 0, 0);
            recalibrationBtn.Width = 100;
            recalibrationBtn.Height = 30;
            recalibrationBtn.Content = "ReCalibration";
            recalibrationBtn.Click += recalibration;

            //testDoughnut = new Doughnut(tests[testCount])
            setUpDoughnut(tests[testCount]);

            //Draw the cursor
            cursor.Width = 8;
            cursor.Height = 8;
            cursor.Stroke = Brushes.CadetBlue;
            cursor.StrokeThickness = 1;
            double doughnutscenterleft = 150 + testDoughnut.Width / 2 - cursor.Width / 2;
            double doughnutscentertop = 100 + testDoughnut.Height / 2 - cursor.Height / 2;
            double cursorradius = testDoughnut.Width / 2 - testDoughnut.inner_width - cursor.Width / 2;
            cursor.SetValue(Canvas.LeftProperty, doughnutscenterleft);
            cursor.SetValue(Canvas.TopProperty, doughnutscentertop);
            mycanvas.Children.Add(cursor);
            //ComputeAngles();

            mycanvas.Children.Add(toggleBtn);
            mycanvas.Children.Add(recalibrationBtn);
            
        }

        private void toggleTest(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Content.Equals("Start"))
            {
                btn.Content = "End";
            }
            else
            {
                btn.Content = "Start";
                this.testCount++;
                this.mycanvas.Children.Remove(this.testDoughnut);
                setUpDoughnut(tests[testCount]);
            }
        }

        private void recalibration(object sender, RoutedEventArgs e)
        {
            Calibration calibration = new Calibration(this.count, this.fileName);
            calibration.Show();
            this.Close();
        }

        private List<string> readFromRecordFile(int count)
        {
            string line;
            int fileCount = 0;
            System.IO.StreamReader stream = OpenFile();
            List<string> result = new List<string>();
            
            while ((line = stream.ReadLine()) != null)
            {
                //System.Windows.MessageBox.Show(line);
                if (fileCount >= count+1) {
                    int secondSpace = line.IndexOf(' ', line.IndexOf(' ') + 1);
                    result.Add(line.Substring(secondSpace+1));
                }
                fileCount++;
            }
            
            return result;
        }

        private System.IO.StreamReader OpenFile() {
            string path = Directory.GetCurrentDirectory()+"\\";
            System.IO.StreamReader file = new System.IO.StreamReader(path + this.fileName);
            return file;
        }

        private void setUpDoughnut(string param)
        {
            String[] ShapeParam = param.Split(' ');
            double degree = double.Parse(ShapeParam[0]);
            double height = double.Parse(ShapeParam[1]);
            double centerPos = double.Parse(ShapeParam[3]);
            this.testDoughnut = new Doughnut(centerPos - degree / 2, centerPos + degree / 2, height);
            this.testDoughnut.SetValue(Canvas.LeftProperty, (double)150);
            this.testDoughnut.SetValue(Canvas.TopProperty, (double)100);
            this.mycanvas.Children.Add(testDoughnut);
        }
    }
}
