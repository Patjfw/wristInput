using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO.Ports;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using System.Diagnostics;

namespace AssignmentTwo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class HitTest : Window
    {
		SerialPort serialport;
		Thread arduino_thread, data_processing_thread;
		bool run_arduino_thread;
		Queue<string> arduino_data__buffer;

		const int numofDonuts = 12;
		int count;
        string fileName;
		double cursorradius;
		double[] precoord = new double[2];
		//choose the middle point when the jump is too large
		double threshold = 20.0;
		//put the cursor in the middle
		double middlethreshold = 50.0;
		//two edge sensors are close then we pick middle three
		double pickthreethreshold = 50.0;

		// the cursor 
		double doughnutscenterleft;
		double doughnutscentertop;

        double flagX;
        double flagY;
		Boolean endFlag;
        Boolean startFlag = false;

        double[] sensorangles = new double[numofDonuts];

		int testCount = 0;
        List<string> tests;

        Canvas mycanvas = new Canvas();
        Button toggleBtn = new Button();
        Button recalibrationBtn = new Button();

        Ellipse cursor = new Ellipse();
        Doughnut testDoughnut;

        string trailsFileName = "trails_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt";
        string resultFileName = "testresult_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt";
		StreamWriter filewriter;
        StreamWriter trailwriter;

        Stopwatch stopwatch;

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
            doughnutscenterleft = 150 + testDoughnut.Width / 2 - cursor.Width / 2;
            doughnutscentertop = 100 + testDoughnut.Height / 2 - cursor.Height / 2;
            cursorradius = testDoughnut.Width / 2 - testDoughnut.inner_width - cursor.Width / 2;
            cursor.SetValue(Canvas.LeftProperty, doughnutscenterleft);
            cursor.SetValue(Canvas.TopProperty, doughnutscentertop);
            mycanvas.Children.Add(cursor);
            ComputeAngles();

            mycanvas.Children.Add(toggleBtn);
            mycanvas.Children.Add(recalibrationBtn);
			// Get the data
			//arduino_thread = new Thread(ReadArduino);
			//arduino_thread.Start();
			//data_processing_thread = new Thread(ProcessArduinoData);
			//data_processing_thread.Start();
			//if (!File.Exists(fileName))
			this.filewriter = File.CreateText(resultFileName);
            filewriter.WriteLine("SubjectID TrialNum degree height blockNum centerPos Success Time");
            filewriter.Close();
            this.trailwriter = File.CreateText(trailsFileName);
            trailwriter.WriteLine("SubjectID TrialNum degree height blockNum centerPos X Y");
            trailwriter.Close();

		}

		private Boolean isHitted(double startAngle, double stopAngle, double stopPosX, double stopPosY)
        {
            Boolean flag = true;

            double angle = calAngle(doughnutscenterleft, doughnutscentertop, stopPosX, stopPosY);

            if (stopAngle >= 360)
            {
                startAngle -= 360;
            }

            /*if (stopAngle < startAngle)
            {
                if(angle<)
            }
            else*/ if (angle < startAngle || angle >= stopAngle)
            {
                flag = false;
              
            }
            

            double distance = Math.Sqrt(Math.Pow((stopPosX - doughnutscenterleft), 2)+ Math.Pow((stopPosY - doughnutscentertop), 2));

            if (distance < this.testDoughnut.Height / 2 - this.testDoughnut.inner_width - 20)
            {
                flag = false;
            }
			this.endFlag = false;
            return flag;
        }

        private double calAngle(double centerX, double centerY, double cursorX, double cursorY)
        {
            double diffX = cursorX - centerX;
            double diffY = cursorY - centerY;
            double oriAngle = Math.Atan2(diffY, diffX);
            if (oriAngle < 0)
            {
                //oriAngle += 180;
                oriAngle = (2 * Math.PI + oriAngle) * 180 / Math.PI;
            }
            else
            {
                oriAngle = oriAngle * 180 / Math.PI;
            }
            oriAngle += 90;
            if (oriAngle >= 360)
            {
                oriAngle -= 360;
            }
            return oriAngle;
        }

		private void toggleTest(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Content.Equals("Start"))
            {
                startFlag = true;
                btn.Content = "End";
				endFlag = false;
                trailwriter = File.AppendText(this.trailsFileName);
                stopwatch = Stopwatch.StartNew();
            }
            else
            {
                stopwatch.Stop();
                trailwriter.Close();
                startFlag = false;
                endFlag = true;
                //TODO: cursor position
                Boolean result = isHitted(this.testDoughnut.start_angle, this.testDoughnut.stop_angle, flagX, flagY);
				//write result into file

				//this.recordFileName = fileName;
				filewriter = File.AppendText(this.resultFileName);
				this.filewriter.WriteLine(tests[testCount] + " " + result + " " + stopwatch.ElapsedMilliseconds);
				filewriter.Close();
				
				
				btn.Content = "Start";

                this.testCount++;
                this.mycanvas.Children.Remove(this.testDoughnut);

                if (testCount < tests.Count)
                {
                    setUpDoughnut(tests[testCount]);
                }
                else
                {
                    System.Windows.MessageBox.Show("Test End");
                }

                System.Windows.MessageBox.Show(result + " | " + stopwatch.ElapsedMilliseconds + "ms");
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
					//int secondSpace = line.IndexOf(' ', line.IndexOf(' ') + 1);
					//result.Add(line.Substring(secondSpace+1));
					result.Add(line);
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
            double degree = double.Parse(ShapeParam[2]);
            double height = double.Parse(ShapeParam[3]);
            double centerPos = double.Parse(ShapeParam[5]);
            this.testDoughnut = new Doughnut(centerPos - degree / 2, centerPos + degree / 2, height, false);
            this.testDoughnut.SetValue(Canvas.LeftProperty, (double)150);
            this.testDoughnut.SetValue(Canvas.TopProperty, (double)100);
            this.mycanvas.Children.Add(testDoughnut);
        }

		void ComputeAngles()
		{
			int half = numofDonuts / 2;
			sensorangles[0] = Math.PI;
			sensorangles[half] = 0.0;
			for (int i = 1; i <= (numofDonuts - 2) / 2; i++)
			{
				sensorangles[half - i] = (Math.PI / half) * i;
				sensorangles[half + i] = Math.PI * 2 - sensorangles[half - i];
			}
		}

        public void updateCursorPos(double top, double left, Boolean isSkipdistance)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (isSkipdistance)
                {
                    double cursorTop = (precoord[0] + (doughnutscentertop + top)) / 2;
                    double cursorLeft = (precoord[1] + (doughnutscenterleft + left)) / 2;
                    cursor.SetValue(Canvas.LeftProperty, left+ doughnutscenterleft);
                    cursor.SetValue(Canvas.TopProperty, top+ doughnutscentertop);
                    precoord[0] = cursorTop;
                    precoord[1] = cursorLeft;
					//if (endFlag) {
						flagX = left + doughnutscenterleft;
						flagY = top + doughnutscentertop;
                    //}
                   
                    if (startFlag)
                    {
                        this.trailwriter.WriteLine(tests[testCount] + " " + flagX + " " + flagY);
                    }

                }
                /*else
                {
                    cursor.SetValue(Canvas.LeftProperty, doughnutscenterleft + left);
                    cursor.SetValue(Canvas.TopProperty, doughnutscentertop + top);
                    precoord[0] = doughnutscentertop + top;
                    precoord[1] = doughnutscenterleft + left;
                }*/
            }));
        }
    }

    
   
}
