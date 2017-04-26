using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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

        const int radius = 20;
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

        Label state = new Label();
        Label remainTest = new Label();
        Label currentTest = new Label();
        Label timeUsed = new Label();
        Label successLabel = new Label();

        Ellipse cursor = new Ellipse();
        Ellipse boundary = new Ellipse();
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

            state.Margin = new Thickness(450, 100, 0, 0);
            state.Width = 100;
            state.Height = 40;
            state.Content = "Status: Idle";

            currentTest.Margin = new Thickness(450, 140, 0, 0);
            currentTest.Width = 100;
            currentTest.Height = 40;
            currentTest.Content = "Current Test: "+serialNum;

            remainTest.Margin = new Thickness(450, 180, 0, 0);
            remainTest.Width = 100;
            remainTest.Height = 40;
            if (serialNum > 0)
            {
                remainTest.Content = "Remain Test: " + (tests.Count);
            }
            else
            {
                remainTest.Content = "Remain Test: " + (tests.Count - testCount - serialNum);
            }

            successLabel.Margin = new Thickness(450, 220, 0, 0);
            successLabel.Width = 100;
            successLabel.Height = 40;
            successLabel.Content = "Success: Null";

            timeUsed.Margin = new Thickness(450, 260, 0, 0);
            timeUsed.Width = 140;
            timeUsed.Height = 40;
            timeUsed.Content = "Time Used: Null";

            

            mycanvas.Children.Add(state);
            mycanvas.Children.Add(currentTest);
            mycanvas.Children.Add(remainTest);
            mycanvas.Children.Add(successLabel);
            mycanvas.Children.Add(timeUsed);

            //testDoughnut = new Doughnut(tests[testCount])
            setUpDoughnut(tests[testCount]);
            setUpBoundary(tests[testCount]);

            Ellipse circle = new Ellipse();
            circle.Stroke = System.Windows.Media.Brushes.Red;
            circle.Margin = new Thickness(150 - radius + testDoughnut.Width / 2, 100 - radius + testDoughnut.Height / 2,0,0);
            circle.Width = radius*2;
            circle.Height = radius*2;
            mycanvas.Children.Add(circle);

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

            //mycanvas.Children.Add(toggleBtn);
            //mycanvas.Children.Add(recalibrationBtn);
			
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

        private void setUpBoundary(string param) {
            String[] ShapeParam = param.Split(' ');
            double height = double.Parse(ShapeParam[3]);
            double centerPos = double.Parse(ShapeParam[5]);
            this.boundary = new Ellipse();
            boundary.Stroke = System.Windows.Media.Brushes.Red;
            boundary.Margin = new Thickness(150 + height +20, 100 + height +20, 0, 0);
            boundary.Width = testDoughnut.Width - height*2-20*2;
            boundary.Height = testDoughnut.Height - height*2-20*2;
            mycanvas.Children.Add(boundary);
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

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (state.Content.Equals("Status: Idle"))
            {
                if (Math.Pow(Math.Abs(flagX - (150 + testDoughnut.Width / 2)+cursor.Width/2),2) + Math.Pow(Math.Abs(flagY - (100 + testDoughnut.Height / 2)+cursor.Height/2),2)<=radius*radius) {
                    startFlag = true;
                    state.Content = "Status: Running";
                    endFlag = false;
                    trailwriter = File.AppendText(this.trailsFileName);
                    stopwatch = Stopwatch.StartNew();
                }
                else
                {
                    MessageBox.Show("Please place cursor in the red circle");
                }
            }
            /*else
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

                this.testCount++;
                this.mycanvas.Children.Remove(this.boundary);
                this.mycanvas.Children.Remove(this.testDoughnut);

                if (testCount < tests.Count)
                {
                    setUpDoughnut(tests[testCount]);
                    setUpBoundary(tests[testCount]);
                }
                else
                {
                    System.Windows.MessageBox.Show("Test End");
                }

                //System.Windows.MessageBox.Show(result + " | " + stopwatch.ElapsedMilliseconds + "ms");

                if (this.count > 0) {
                    currentTest.Content = "Current Test: " + (count + testCount);
                }
                else
                {
                    currentTest.Content = "Current Test: " + testCount;
                }
                
                if (this.count > 0)
                {
                    remainTest.Content = "Remain Test: " + (tests.Count - testCount);
                }
                else
                {
                    remainTest.Content = "Remain Test: " + (tests.Count - testCount - count);
                }
                state.Content = "Status: Idle";

                successLabel.Content = "Success: " + result;
                timeUsed.Content = "Time Used: " + stopwatch.ElapsedMilliseconds + "ms";
            }*/
        }

        private void endOneTest() {
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

            this.testCount++;
            this.mycanvas.Children.Remove(this.boundary);
            this.mycanvas.Children.Remove(this.testDoughnut);

            if (testCount < tests.Count)
            {
                setUpDoughnut(tests[testCount]);
                setUpBoundary(tests[testCount]);
            }
            else
            {
                System.Windows.MessageBox.Show("Test End");
            }

            //System.Windows.MessageBox.Show(result + " | " + stopwatch.ElapsedMilliseconds + "ms");

            if (this.count > 0)
            {
                currentTest.Content = "Current Test: " + (count + testCount);
            }
            else
            {
                currentTest.Content = "Current Test: " + testCount;
            }

            if (this.count > 0)
            {
                remainTest.Content = "Remain Test: " + (tests.Count - testCount);
            }
            else
            {
                remainTest.Content = "Remain Test: " + (tests.Count - testCount - count);
            }
            state.Content = "Status: Idle";

            successLabel.Content = "Success: " + result;
            timeUsed.Content = "Time Used: " + stopwatch.ElapsedMilliseconds + "ms";
        }

        public void updateCursorPos(double top, double left)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                cursor.SetValue(Canvas.LeftProperty, left);
                cursor.SetValue(Canvas.TopProperty, top);

                flagX = left;
				flagY = top;
                    
                if (startFlag)
                {
                    this.trailwriter.WriteLine(tests[testCount] + " " + flagX + " " + flagY);
                    double distance = Math.Sqrt(Math.Pow((flagX - doughnutscenterleft), 2) + Math.Pow((flagY - doughnutscentertop), 2));

                    if (distance >= this.testDoughnut.Height / 2 - this.testDoughnut.inner_width - 20)
                    {
                        endOneTest();
                        //Boolean result = isHitted(this.testDoughnut.start_angle, this.testDoughnut.stop_angle, flagX, flagY);
                    }
                }

                
            }));
        }
    }

    
   
}
