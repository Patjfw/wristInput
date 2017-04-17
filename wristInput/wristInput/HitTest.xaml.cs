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

        public HitTest(int serialNum, string fileName)
        {
            InitializeComponent();
            this.count = serialNum;
            this.fileName = fileName;
            tests = readFromRecordFile(this.count);
            drawShape(testCount);
        }

        private void toggleTest(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Content.Equals("Start"))
            {
                btn.Content = "End";
                drawShape(testCount);
            }
            else
            {
                btn.Content = "Start";
            }
        }

        private void recalibrate(object sender, RoutedEventArgs e)
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

        private void drawShape(int count) {
            String[] ShapeParam = this.tests[count].Split(' ');
            Console.Write(ShapeParam);
        }
    }
}
