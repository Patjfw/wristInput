using System;
using System.Collections.Generic;
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
using System.IO;

namespace AssignmentTwo
{
    /// <summary>
    /// Interaction logic for Part3.xaml
    /// </summary>
    public partial class Part3 : Window
    {
        StreamReader sr;
        bool nextbuttonclickfirsttime = true;
        Canvas mycanvas = new Canvas();
        Button nextbutton;
        Button prebutton;
        Label label;
        String alldata;
        String[] dataintrial;
        int current = -1;
        const double pixelmm = 3.7795;


        public Part3()
        {
            InitializeComponent();
            this.Content = mycanvas;
            nextbutton = new Button() { Content = "Review Data", Margin = new Thickness(0, 148, 0, 0), Width = 79, Height = 25 };
            nextbutton.Click += button1_Click;
            prebutton = new Button() { Content = "Previous Trial", Margin = new Thickness(0, 65, 0, 0), Width = 79, Height = 25 };
            prebutton.Click += button2_Click;
            prebutton.IsEnabled = false;
            label = new Label() { Content = "Trial #", Height = 50, Width = 85 };
            label.SetValue(Canvas.LeftProperty, 0.0);
            label.SetValue(Canvas.TopProperty, 0.0);
            mycanvas.Children.Add(nextbutton);
            mycanvas.Children.Add(prebutton);
            mycanvas.Children.Add(label);
            sr = new StreamReader("Part3result.txt");
            sr.ReadLine();
            alldata = sr.ReadToEnd();
            dataintrial = alldata.Replace("\r\n\r\n", ":").Split(':');
        }

        //next button
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (nextbuttonclickfirsttime)
            {
                nextbutton.Content = "Next Trial";
            }

            current++;


            //get the data
            string onetrial = dataintrial[current];
            string[] onetrialeachline = onetrial.Replace("\r\n", ":").Split(':');
            if (current == dataintrial.Length-2) {
                nextbutton.IsEnabled = false;
            }
            string trialnum = onetrialeachline[0].Split(' ')[1];
            string startpos = onetrialeachline[0].Split(' ')[4];
            string targetpos = onetrialeachline[0].Split(' ')[5];
            string targetwidth = onetrialeachline[0].Split(' ')[3];
            string[] startpossplit = startpos.Split(',');
            int startx = Int32.Parse(startpossplit[0].Substring(1));
            int starty = Int32.Parse(startpossplit[1].Remove(startpossplit[1].Length - 1, 1));
            string[] targetpossplit = targetpos.Split(',');
            int targetx = Int32.Parse(targetpossplit[0].Substring(1));
            int targety = Int32.Parse(targetpossplit[1].Remove(targetpossplit[1].Length - 1, 1));
            string time = onetrialeachline[onetrialeachline.Length - 1].Split(' ')[6];
            

            //clear the canvas
            mycanvas.Children.RemoveRange(0, mycanvas.Children.Count);

            //controls
            label.Content = "Trial #: " + trialnum + "\nTime: " + time + "ms";
            if (current > 0)
            {
                prebutton.IsEnabled = true;
            }
            else
            {
                prebutton.IsEnabled = false;
            }
            Button start = new Button() { Content = "Start", Height = mmtopixel(10), Width = mmtopixel(10), Background = Brushes.Black, Foreground = Brushes.White };
            Button target = new Button() { Height = Int32.Parse(targetwidth), Width = Int32.Parse(targetwidth), Background = Brushes.Green };
            //location of start and target
            start.SetValue(Canvas.TopProperty, (double)starty);
            start.SetValue(Canvas.LeftProperty, (double)startx);
            target.SetValue(Canvas.TopProperty, (double)targety);
            target.SetValue(Canvas.LeftProperty, (double)targetx);
            //add controls
            mycanvas.Children.Add(nextbutton);
            mycanvas.Children.Add(prebutton);
            mycanvas.Children.Add(label);
            mycanvas.Children.Add(start);
            mycanvas.Children.Add(target);
            //points
            for (int i = 0; i < onetrialeachline.Length; i++)
            {
                string pointcoor = onetrialeachline[i].Split(' ')[8];
                string[] pointsplit = pointcoor.Split(',');
                int pointx = Int32.Parse(pointsplit[0].Substring(1));
                int pointy = Int32.Parse(pointsplit[1].Remove(pointsplit[1].Length - 1, 1));
                Ellipse point = new Ellipse() { Fill = Brushes.Red, Width = 3, Height = 3 };
                point.SetValue(Canvas.LeftProperty, (double)pointx);
                point.SetValue(Canvas.TopProperty, (double)pointy);
                mycanvas.Children.Add(point);
            }
            mycanvas.UpdateLayout();

        }//end button



        //pre button
        private void button2_Click(object sender, RoutedEventArgs e)
        {

            current--;
            if (current == 0)
            {
                prebutton.IsEnabled = false;
            }
            else {
                nextbutton.IsEnabled = true;
            }
            //get the data
            string onetrial = dataintrial[current];
            string[] onetrialeachline = onetrial.Replace("\r\n", ":").Split(':');
            if (current == onetrialeachline.Length - 1)
            {
                nextbutton.IsEnabled = false;
            }
            string trialnum = onetrialeachline[0].Split(' ')[1];
            string startpos = onetrialeachline[0].Split(' ')[4];
            string targetpos = onetrialeachline[0].Split(' ')[5];
            string targetwidth = onetrialeachline[0].Split(' ')[3];
            string[] startpossplit = startpos.Split(',');
            int startx = Int32.Parse(startpossplit[0].Substring(1));
            int starty = Int32.Parse(startpossplit[1].Remove(startpossplit[1].Length - 1, 1));
            string[] targetpossplit = targetpos.Split(',');
            int targetx = Int32.Parse(targetpossplit[0].Substring(1));
            int targety = Int32.Parse(targetpossplit[1].Remove(targetpossplit[1].Length - 1, 1));
            string time = onetrialeachline[onetrialeachline.Length - 1].Split(' ')[6];


            //clear the canvas
            
            mycanvas.Children.RemoveRange(0, mycanvas.Children.Count);
            //mycanvas.Children.Clear();
            //Rectangle rect = new Rectangle() { Fill = Brushes.Black, Height = 532, Width = 700 };
            //rect.SetValue(Canvas.LeftProperty, 0.0);
            //rect.SetValue(Canvas.TopProperty, 0.0);
            //mycanvas.Children.Add(rect);

            //controls
            label.Content = "Trial #: " + trialnum + "\nTime: " + time + "ms";
            if (current > 0)
            {
                prebutton.IsEnabled = true;
            }
            Button start = new Button() { Content = "Start", Height = mmtopixel(10), Width = mmtopixel(10), Background = Brushes.Black, Foreground = Brushes.White };
            Button target = new Button() { Height = Int32.Parse(targetwidth), Width = Int32.Parse(targetwidth), Background = Brushes.Green };
            //location of start and target
            start.SetValue(Canvas.TopProperty, (double)starty);
            start.SetValue(Canvas.LeftProperty, (double)startx);
            target.SetValue(Canvas.TopProperty, (double)targety);
            target.SetValue(Canvas.LeftProperty, (double)targetx);
            //add controls
            mycanvas.Children.Add(nextbutton);
            mycanvas.Children.Add(prebutton);
            mycanvas.Children.Add(label);
            mycanvas.Children.Add(start);
            mycanvas.Children.Add(target);
            //points
            for (int i = 0; i < onetrialeachline.Length; i++)
            {
                string pointcoor = onetrialeachline[i].Split(' ')[8];
                string[] pointsplit = pointcoor.Split(',');
                int pointx = Int32.Parse(pointsplit[0].Substring(1));
                int pointy = Int32.Parse(pointsplit[1].Remove(pointsplit[1].Length - 1, 1));
                Ellipse point = new Ellipse() { Fill = Brushes.Red, Width = 3, Height = 3 };
                point.SetValue(Canvas.LeftProperty, (double)pointx);
                point.SetValue(Canvas.TopProperty, (double)pointy);
                mycanvas.Children.Add(point);
            }
        }

        public double mmtopixel(double mm)
        {
            return mm * pixelmm;
        }

        public double pixeltomm(double pixel)
        {
            return pixel / pixelmm;
        }
    }
}
