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
using System.Windows.Shapes;

namespace AssignmentTwo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ReadFile : Window
    {

        Canvas mycanvas = new Canvas();
        TextBox serialNum = new TextBox();
        ListBox listbox = new ListBox();
        Button btn = new Button();

        public ReadFile()
        {

            this.Content = mycanvas;

            serialNum.Margin = new Thickness(450, 260, 0, 0);
            btn.Margin = new Thickness(450, 300, 0, 0);

            btn.Content = "Start Test";
            btn.Click += sendToTest;

            mycanvas.Children.Add(listbox);
            mycanvas.Children.Add(serialNum);
            mycanvas.Children.Add(btn);

            string filepath = System.IO.Directory.GetCurrentDirectory();
            DirectoryInfo d = new DirectoryInfo(filepath);

            foreach (var file in d.GetFiles("testrecord*.txt"))
            {
                //Directory.Move(file.FullName, filepath + "\\TextFiles\\" + file.Name);
                listbox.Items.Add(file.Name);
            }
        }

        void sendToTest(object sender, RoutedEventArgs e) {
            Calibration calibration = new Calibration(Convert.ToInt32(serialNum.Text), listbox.Items[listbox.SelectedIndex].ToString());
            calibration.Show();
        }
    }
}
