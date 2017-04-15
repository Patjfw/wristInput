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
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace AssignmentTwo
{
    /// <summary>
    /// Interaction logic for UserStudy.xaml
    /// </summary>
    public partial class UserStudy : Window
    {
        List<string> amplist = new List<string>();
        List<string> widlist = new List<string>();
        List<int> ampwidcomb = new List<int>();
        int totaltrials;
        int trialspercondition;
        Canvas mycanvas = new Canvas();
        const double pixelmm = 3.7795;
        int ntimestrial = 0;
        Button start;
        Button target;
        const int canvasheight = 500;
        const int canvaswidth = 700;
        List<int> location;
        List<int> shuffled;
        StreamWriter filewriter1;
        StreamWriter filewriter2;
        int subjectid;
        int success;
        bool startclick = false;
        DispatcherTimer timer;
        List<string> cursorco = new List<string>();
        List<string> timelist = new List<string>();
        double starttime;


        public UserStudy(List<string> amplitudelist, List<string> widthlist, int numtrials, int trialcondition, int id)
        {
            InitializeComponent();
            this.Content = mycanvas;
            //timer
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            //data
            amplist = amplitudelist;
            widlist = widthlist;
            totaltrials = numtrials;
            trialspercondition = trialcondition;
            subjectid = id;
            for (int i = 0; i < amplist.Count; i++)
            {
                for (int j = 0; j < widlist.Count; j++)
                {
                    for (int k = 0; k < trialspercondition; k++)
                    {
                        ampwidcomb.Add(Int32.Parse(amplist[i]));
                        ampwidcomb.Add(Int32.Parse(widlist[j]));
                    }
                }
            }

            //create a file
            if (!File.Exists("Part2result.txt"))
            {
                filewriter1 = File.CreateText("Part2result.txt");
                filewriter1.WriteLine("SubjectID TrialNum Amplitude Width StartPos TargetPos Time Success");
            }
            else
            {
                filewriter1 = File.AppendText("Part2result.txt");
                filewriter1.WriteLine("SubjectID TrialNum Amplitude Width StartPos TargetPos Time Success");
                MessageBox.Show("There exists Part2result.txt");
            }

            if (!File.Exists("Part3result.txt"))
            {
                filewriter2 = File.CreateText("Part3result.txt");
                filewriter2.WriteLine("SubjectID TrialNum Amplitude Width StartPos TargetPos Time Success CursorPos");
            }
            else
            {
                filewriter2 = File.AppendText("Part3result.txt");
                filewriter2.WriteLine("SubjectID TrialNum Amplitude Width StartPos TargetPos Time Success CursorPos");
                MessageBox.Show("There exists Part3result.txt");
            }

            //generate random number
            shuffled = ShuffleRange(0, totaltrials).ToList<int>();

            //label
            Label triallabel = new Label() { Height = 25, Width = 85 };
            triallabel.SetValue(Canvas.LeftProperty, 0.0);
            triallabel.SetValue(Canvas.TopProperty, 0.0);
            triallabel.Content = totaltrials.ToString() + " trials left";
            mycanvas.Children.Add(triallabel);
            //button
            start = new Button() { Content = "Start", Height = mmtopixel(10), Width = mmtopixel(10), Background = Brushes.Black, Foreground = Brushes.White };
            target = new Button() { Height = ampwidcomb[shuffled[ntimestrial] * 2 + 1], Width = ampwidcomb[shuffled[ntimestrial] * 2 + 1], Background = Brushes.Green };
            start.Click += start_Click;
            target.Click += target_Click;
            //location of start and target
            location = ComputeLocation(ampwidcomb[shuffled[ntimestrial] * 2], ampwidcomb[shuffled[ntimestrial] * 2 + 1]);
            start.SetValue(Canvas.TopProperty, (double)location[0]);
            start.SetValue(Canvas.LeftProperty, (double)location[1]);
            target.SetValue(Canvas.TopProperty, (double)location[2]);
            target.SetValue(Canvas.LeftProperty, (double)location[3]);
            ntimestrial++;
            mycanvas.Children.Add(start);
            mycanvas.Children.Add(target);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Point po = Mouse.GetPosition(this);
            int intx = (int)po.X;
            int inty = (int)po.Y;
            cursorco.Add("(" + intx + "," + inty + ")");
            double timed = new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds - starttime;
            string time = ((int)timed).ToString();
            timelist.Add(time);
        }


        //Compute location of start and target
        List<int> ComputeLocation(int amp, int width)
        {
            //first two start
            //first one top, second one left
            List<int> location = new List<int>();
            Random rand = new Random();
            int starttop = rand.Next(0, canvasheight - (int)Math.Ceiling(start.Height));
            int startleft = rand.Next(85, canvaswidth - (int)Math.Ceiling(start.Width));
            int theta;
            int targettop;
            int targetleft;
            while (true)
            {
                if (starttop < canvasheight / 2 && startleft < canvaswidth / 2)
                {
                    //topleft
                    theta = rand.Next(0, 90);
                }
                else if (starttop >= canvasheight / 2 && startleft >= canvaswidth / 2)
                {
                    //bottomright
                    theta = rand.Next(180, 270);

                }
                else if (starttop >= canvasheight / 2 && startleft < canvaswidth / 2)
                {
                    //bottomleft
                    theta = rand.Next(270, 360);
                }
                else
                {
                    //topright
                    theta = rand.Next(90, 180);
                }
                targettop = starttop + (int)(amp * Math.Sin(theta / 180.0 * Math.PI));
                targetleft = startleft + (int)(amp * Math.Cos(theta / 180.0 * Math.PI));
                if (targettop > 0 && targettop < canvasheight - width && targetleft > 85 && targetleft < canvaswidth - width)
                {
                    break;
                }
            }//end while
            location.Add(starttop);
            location.Add(startleft);
            location.Add(targettop);
            location.Add(targetleft);
            return location;

        }

        //click on start
        private void start_Click(object sender, RoutedEventArgs e)
        {
            start.Background = Brushes.White;
            start.Foreground = Brushes.Black;
            startclick = true;
            success = 1;
            timer.Start();
            cursorco = new List<string>();
            timelist = new List<string>();
            starttime = new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds;
        }

        //click on target
        private void target_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            startclick = false;
            double timed = new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds - starttime;
            string time = ((int)timed).ToString();
            //Label timelabel = new Label() { Height = 25, Width = 85 };
            //timelabel.SetValue(Canvas.LeftProperty, 40.0);
            //timelabel.SetValue(Canvas.TopProperty, 50.0);
            //timelabel.Content = "sdfdsfsf";
            //mycanvas.Children.Add(timelabel);
            MessageBox.Show(time + "ms");
            //write down the data
            filewriter1.WriteLine(subjectid + " " + ntimestrial + " " + ampwidcomb[shuffled[ntimestrial - 1] * 2] + " " + ampwidcomb[shuffled[ntimestrial - 1] * 2 + 1] + " " + "(" + location[1] +"," + location[0] +") " + "(" + location[3] + "," + location[2] + ") " + time + " " + success.ToString());
            for (int i = 0; i < cursorco.Count; i++) {
                filewriter2.WriteLine(subjectid + " " + ntimestrial + " " + ampwidcomb[shuffled[ntimestrial - 1] * 2] + " " + ampwidcomb[shuffled[ntimestrial - 1] * 2 + 1] + " " + "(" + location[1] + "," + location[0] + ") " + "(" + location[3] + "," + location[2] + ") " + timelist[i] + " " + success.ToString() + " " + cursorco[i]);
            }

            filewriter2.WriteLine();
            
            //clear the canvas
            mycanvas.Children.RemoveRange(0, mycanvas.Children.Count);
            if (ntimestrial < totaltrials)
            {
                //label
                Label triallabel = new Label() { Height = 25, Width = 85 };
                triallabel.SetValue(Canvas.LeftProperty, 0.0);
                triallabel.SetValue(Canvas.TopProperty, 0.0);
                triallabel.Content = (totaltrials - ntimestrial).ToString() + " trials left";
                mycanvas.Children.Add(triallabel);
                //button
                start.Background = Brushes.Black;
                start.Foreground = Brushes.White;
                target.Height = ampwidcomb[shuffled[ntimestrial] * 2 + 1];
                target.Width = target.Height;
                //location of start and target
                location = ComputeLocation(ampwidcomb[shuffled[ntimestrial] * 2], ampwidcomb[shuffled[ntimestrial] * 2 + 1]);
                start.SetValue(Canvas.TopProperty, (double)location[0]);
                start.SetValue(Canvas.LeftProperty, (double)location[1]);
                target.SetValue(Canvas.TopProperty, (double)location[2]);
                target.SetValue(Canvas.LeftProperty, (double)location[3]);
                ntimestrial++;
                mycanvas.Children.Add(start);
                mycanvas.Children.Add(target);
            }
            else
            {
                MessageBox.Show("User Study Finished");
                this.Close();
                filewriter1.Close();
                filewriter2.Close();
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

        static IEnumerable<int> ShuffleRange(int start, int count)
        {
            int[] cards = Enumerable.Range(start, count).ToArray<int>();
            Random rng = new Random();
            for (int i = cards.Count() - 1; i > -1; i--)
            {
                int j = rng.Next(i);
                yield return cards[j];
                cards[j] = cards[i];
            }
            yield break;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (startclick) {
                success = 0;
            }
        }
    }
}
