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
            numtrials = Int32.Parse(this.TrialsUD.Value.ToString()) * widlistBox.Items.Count * amplistBox.Items.Count;
            this.totaltrials.Content = "Total Trials: " + numtrials;
            //update the difficulty
            amplitudelist = amplistBox.Items.OfType<string>().ToList();
            widlist = widlistBox.Items.OfType<string>().ToList();
            if(amplitudelist.Count > 0 && widlist.Count > 0) {
                for (int i = 0; i < amplitudelist.Count; i++)
                {
                    for (int j = 0; j < widlist.Count; j++)
                    {
                        double diff = Math.Log(2.0 * Int32.Parse(amplitudelist[i]) / Int32.Parse(widlist[j]), 2);
                        difficulty.Add(diff.ToString("0.00"));
                    }
                }
                difficulty = difficulty.Distinct().ToList();
                difficulty.Sort();
                this.difflistBox.ItemsSource = difficulty;
                //update the difficulty label
                this.difficultylabel.Content = "Indices of Difficulty\n(" + difficulty.Count + " unique):";
            }
        }

        //ok button
        private void okbutton_Click(object sender, RoutedEventArgs e)
        {
            UserStudy userstudy = new UserStudy(this.amplitudelist,this.widlist,this.numtrials, Int32.Parse(this.TrialsUD.Value.ToString()), Int32.Parse(this.SubjectUD.Value.ToString()));
            userstudy.Show();
            this.Close();
        }

        //cancel button
        private void cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
