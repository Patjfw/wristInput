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
    public partial class Part1 : Window
    {
        const double pixeltomills = 3.78;
        public List<string> amplitudelist = new List<string>();
        public List<string> widlist = new List<string>();
        public List<string> difficulty = new List<string>();
        public int numtrials;
        private string recordFileName;
        Random r = new Random();

        public Part1()
        {
            InitializeComponent();
            this.ampminus.IsEnabled = false;
            this.widminus.IsEnabled = false;
        }
      
        //amplitude
        private void ampplus_Click(object sender, RoutedEventArgs e)
        {
            this.amplistBox.Items.Add(this.AmplitudesUD.Value.ToString());
            updateall();
            
        }


        private void ampminus_Click(object sender, RoutedEventArgs e)
        {
            this.amplistBox.Items.RemoveAt(this.amplistBox.SelectedIndex);
            this.ampminus.IsEnabled = false;
            updateall();
        }


        private void amplistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ampminus.IsEnabled = true;
        }


        //width
        private void widplus_Click(object sender, RoutedEventArgs e)
        {
            this.widlistBox.Items.Add(this.WidthsUD.Value.ToString());
            updateall();
        }

        private void widminus_Click(object sender, RoutedEventArgs e)
        {
            this.widlistBox.Items.RemoveAt(this.widlistBox.SelectedIndex);
            this.widminus.IsEnabled = false;
            updateall();
        }

        private void widlistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.widminus.IsEnabled = true;
        }

        //Trials
        private void TrialsUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            updateall();
        }

        //update all values
        private void updateall() {
            //update the trials
            numtrials = Int32.Parse(this.TrialsUD.Value.ToString()) * widlistBox.Items.Count * amplistBox.Items.Count * 8;
            this.totaltrials.Content = "Total Trials: " + numtrials;
            //update the difficulty
            amplitudelist = amplistBox.Items.OfType<string>().ToList();
            widlist = widlistBox.Items.OfType<string>().ToList();
            //if(amplitudelist.Count > 0 && widlist.Count > 0) {
            //    for (int i = 0; i < amplitudelist.Count; i++)
            //    {
            //        for (int j = 0; j < widlist.Count; j++)
            //        {
            //            double diff = Math.Log(2.0 * Int32.Parse(amplitudelist[i]) / Int32.Parse(widlist[j]), 2);
            //            difficulty.Add(diff.ToString("0.00"));
            //        }
            //    }
            //    difficulty = difficulty.Distinct().ToList();
            //    difficulty.Sort();
            //    this.difflistBox.ItemsSource = difficulty;
            //    //update the difficulty label
            //    this.difficultylabel.Content = "Indices of Difficulty\n(" + difficulty.Count + " unique):";
            //}
        }

        //ok button
        private void okbutton_Click(object sender, RoutedEventArgs e)
        {
			//UserStudy userstudy = new UserStudy(this.amplitudelist,this.widlist,this.numtrials, Int32.Parse(this.TrialsUD.Value.ToString()), Int32.Parse(this.SubjectUD.Value.ToString()));
			//userstudy.Show();
			this.amplitudelist = new List<string>();
			amplitudelist.Add("27.70");
			amplitudelist.Add("41.53");
			amplitudelist.Add("55.38");
			amplitudelist.Add("69.23");
			amplitudelist.Add("83.07");


			writeTestFile(this.amplitudelist, this.widlist, this.numtrials, Int32.Parse(this.TrialsUD.Value.ToString()), Int32.Parse(this.SubjectUD.Value.ToString()));
            Calibration calibration = new Calibration(0, this.recordFileName);
            calibration.Show();
            this.Close();
        }

        //cancel button
        private void cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void readFile_Click(object sender, RoutedEventArgs e)
        {
            ReadFile readfile = new ReadFile();
            readfile.Show();
            this.Close();
        }

        private void writeTestFile(List<string> degrees, List<string> heights, int numtrails, int tID, int sID)
        {
            string fileName = "testrecord_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt";
            this.recordFileName = fileName;
            if (!File.Exists(fileName))
            {
                StreamWriter filewriter = File.CreateText(fileName);
             
                //blockNum: the plate divides into 8 parts, centerPos: the center of the segment locates on which degree
                filewriter.WriteLine("SubjectID TrialNum degree height blockNum centerPos");
                List<string> permutation = createPermutation(degrees, heights, Int32.Parse(this.TrialsUD.Value.ToString()));
                for(int i=0; i<permutation.Count; i++)
                {
                    filewriter.WriteLine(sID + " " + i + " " + permutation[i]);
                }
                filewriter.Close();
            }
            else
            {
                MessageBox.Show("record_already_exists");
            }
        }

        private List<string> createPermutation(List<string> degrees, List<string> heights, int repeat) {
            List<string> result = new List<string>();
            for (int i = 0; i < degrees.Count; i++) {
                for(int j=0; j < heights.Count; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        for (int h=0; h< repeat; h++)
                        {
                            string tmp = degrees[i] + " " + heights[j] + " " + k +" " + generateCenterPos(this.r,k);
                            result.Add(tmp);
                        }
                    }
                }
            }

            //Fisher-Yates shuffle
            Random rng = new Random();
            int n = result.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = result[k];
                result[k] = result[n];
                result[n] = value;
            }
            
            return result;
        }

        private double generateCenterPos(Random r, int blockNum) {
            int range = 45;
            double rDouble = r.NextDouble() * range;
            double angle = -22.5 + 45 * blockNum + rDouble;
            if (angle < 0)
            {
                angle += 360;
            }
            return angle;
        }

    }
}
