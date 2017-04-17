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
		double[] sensorangles = new double[numofDonuts];

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

			serialport = new SerialPort();
			serialport.PortName = "COM5";
			serialport.BaudRate = 9600;
			serialport.Open(); //uncomment this line to receive data from serialport
			run_arduino_thread = true;

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
			arduino_thread = new Thread(ReadArduino);
			arduino_thread.Start();
			//data_processing_thread = new Thread(ProcessArduinoData);
			//data_processing_thread.Start();
		}
		void ReadArduino()
		{
			string line;
			while (run_arduino_thread)
			{

				if (serialport.IsOpen)
				{
					line = serialport.ReadLine().Trim();
					if (line.Length > 0)
						arduino_data__buffer.Enqueue(line);
				}//end if serialport.IsOpen

			}//end while

			serialport.Close();
			Console.WriteLine("---Arduino Thread Stopped---");
		}//end method

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

<<<<<<< HEAD
        private Boolean isHitted()
        {
            return false;
        }
    }
=======
		//update the UI
		void Display(int[] filteredintensity)
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				// each element in this list: 0-255
				List<int> darkintensityintlist = new List<int>();

				//Cursor
				var sorted = darkintensityintlist
					.Select((x, i) => new KeyValuePair<int, int>(x, i))
					.OrderBy(x => x.Key)
					.ToList();

				List<int> sortedarray = sorted.Select(x => x.Key).ToList();
				List<int> idx = sorted.Select(x => x.Value).ToList();
				double[] location = new double[2];

				//
				// Start the revise (location & mindistance is overlapped)
				//

				//If the maximum - minimum < threshold, put cursor in the middle
				if (sortedarray[numofDonuts - 1] - sortedarray[0] < middlethreshold)
				{
					//Compute the location without the weight. 
					//Because most of the vectors will be compensated
					location = ComputeLocationWithoutWeight(sortedarray, idx, numofDonuts);
					//Console.WriteLine("Middle");
				}
				else
				{
					//Pick two maximum and check if these two sensors are neighbours
					if (CheckTwoNeighbours(idx[numofDonuts - 1], idx[numofDonuts - 2]))
					{
						//Input the pair and output the location
						//To make sure the second parameter is the left idx, the third should be right index
						//Use the four sensors directly, do not consider two other possibilities...
						//Console.WriteLine("Two Maximum are neighbours, pick these two {0} {1}", idx[numofDonuts - 1], idx[numofDonuts - 2]);
						if (idx[numofDonuts - 1] == numofDonuts - 1 && idx[numofDonuts - 2] == 0 || idx[numofDonuts - 1] == 0 && idx[numofDonuts - 2] == numofDonuts - 1)
						{
							if (idx[numofDonuts - 1] == numofDonuts - 1)
							{
								//location = ComputeLocationPair(darkintensityintlist, idx[numofDonuts - 1], idx[numofDonuts - 2]);
								int[] foursensorsindex = new int[4] { FindLeftNeighbour(idx[numofDonuts - 1]), idx[numofDonuts - 1], idx[numofDonuts - 2], FindRightNeighbour(idx[numofDonuts - 2]) };
								location = ComputeLocationPairFour(darkintensityintlist, foursensorsindex);
							}
							else
							{
								//location = ComputeLocationPair(darkintensityintlist, idx[numofDonuts - 2], idx[numofDonuts - 1]);
								int[] foursensorsindex = new int[4] { FindLeftNeighbour(idx[numofDonuts - 2]), idx[numofDonuts - 2], idx[numofDonuts - 1], FindRightNeighbour(idx[numofDonuts - 1]) };
								location = ComputeLocationPairFour(darkintensityintlist, foursensorsindex);
							}
						}
						else
						{
							if (idx[numofDonuts - 1] < idx[numofDonuts - 2])
							{
								//location = ComputeLocationPair(darkintensityintlist, idx[numofDonuts - 1], idx[numofDonuts - 2]);
								int[] foursensorsindex = new int[4] { FindLeftNeighbour(idx[numofDonuts - 1]), idx[numofDonuts - 1], idx[numofDonuts - 2], FindRightNeighbour(idx[numofDonuts - 2]) };
								location = ComputeLocationPairFour(darkintensityintlist, foursensorsindex);
							}
							else
							{
								//location = ComputeLocationPair(darkintensityintlist, idx[numofDonuts - 2], idx[numofDonuts - 1]);
								int[] foursensorsindex = new int[4] { FindLeftNeighbour(idx[numofDonuts - 2]), idx[numofDonuts - 2], idx[numofDonuts - 1], FindRightNeighbour(idx[numofDonuts - 1]) };
								location = ComputeLocationPairFour(darkintensityintlist, foursensorsindex);
							}
						}
					}
					else
					{
						//Two maximum are not neighbours
						int[] neighbours = new int[2];
						//Record each pair sum
						List<int> pairsums = new List<int>();
						//Record the left idx in pair
						List<int> leftidxinpair = new List<int>();
						List<int> maxsumleftidx = new List<int>();
						double mindistance = 10000.0;
						//Find the three maximums
						//If want to change the picknum, just change the four below
						for (int i = 1; i < 4; i++)
						{
							//Suppose to have 2*picknum sums
							//Add the idx + leftneighbour 
							pairsums.Add(darkintensityintlist[idx[numofDonuts - i]] + darkintensityintlist[FindLeftNeighbour(idx[numofDonuts - i])]);
							leftidxinpair.Add(FindLeftNeighbour(idx[numofDonuts - i]));
							//Add the idx + rightneighbour
							pairsums.Add(darkintensityintlist[idx[numofDonuts - i]] + darkintensityintlist[FindRightNeighbour(idx[numofDonuts - i])]);
							leftidxinpair.Add(idx[numofDonuts - i]);
						}
						//Find all maximum pair, record the left idx in these pairs
						for (int i = 0; i < pairsums.Count(); i++)
						{
							if (pairsums[i] == pairsums.Max())
							{
								maxsumleftidx.Add(leftidxinpair[i]);
							}
						}
						//Delete the repeated elements
						maxsumleftidx = maxsumleftidx.Distinct().ToList();
						//Console.WriteLine("Two maximum are not neighbours, we pick the maxisum of {0} {1}", maxsumleftidx[0], FindRightNeighbour(maxsumleftidx[0]));
						//Consider all maximum, pick the location with the minimum distance with the precoord
						for (int i = 0; i < maxsumleftidx.Count(); i++)
						{
							double[] thislocation = ComputeLocationPair(darkintensityintlist, maxsumleftidx[i], FindRightNeighbour(maxsumleftidx[i]));
							double thisdistance = ComputeDistance(precoord, thislocation);
							if (thisdistance < mindistance)
							{
								mindistance = thisdistance;
								location = thislocation;
							}
						}
					}
				}

				//if skipdistance > threshold, then cursor moves to the middle of precoord and location this time
				//Notification!!
				//Only calculate distance between location and precoord, the location of two parameters can not be changed
				double skipdistance = ComputeDistance(precoord, location);
				if (skipdistance > threshold)
				{
					double top = (precoord[0] + (doughnutscentertop + location[0])) / 2;
					double left = (precoord[1] + (doughnutscenterleft + location[1])) / 2;
					cursor.SetValue(Canvas.LeftProperty, left);
					cursor.SetValue(Canvas.TopProperty, top);
					precoord[0] = top;
					precoord[1] = left;
					//Console.WriteLine("skip");
				}
				else
				{
					cursor.SetValue(Canvas.LeftProperty, doughnutscenterleft + location[1]);
					cursor.SetValue(Canvas.TopProperty, doughnutscentertop + location[0]);
					precoord[0] = doughnutscentertop + location[0];
					precoord[1] = doughnutscenterleft + location[1];
				}

			}));
			
		}
		//Based a pair of sensors, output the location
		//Second parameter should be left index, third should be right index
		double[] ComputeLocationPair(List<int> unsortedarray, int leftidx, int rightidx)
		{
			double[] location = new double[2];
			double[] thislocation = new double[2];
			double mindistance = 100000.0;
			double thisdistance;
			//Try to find the maximum four sensors data
			int[] leftfoursensors = new int[4] { FindLeftNeighbour(FindLeftNeighbour(leftidx)), FindLeftNeighbour(leftidx), leftidx, rightidx };
			int[] middlefoursensors = new int[4] { FindLeftNeighbour(leftidx), leftidx, rightidx, FindRightNeighbour(rightidx) };
			int[] rightfoursensors = new int[4] { leftidx, rightidx, FindRightNeighbour(rightidx), FindRightNeighbour(FindRightNeighbour(rightidx)) };
			int leftfoursum = 0;
			int middlefoursum = 0;
			int rightfoursum = 0;
			for (int i = 0; i < 4; i++)
			{
				leftfoursum += unsortedarray[leftfoursensors[i]];
				middlefoursum += unsortedarray[middlefoursensors[i]];
				rightfoursum += unsortedarray[rightfoursensors[i]];
			}
			// Consider we have several maximums
			// Pick the one with minimum distance with historical data 
			List<int> threesum = new List<int> { leftfoursum, middlefoursum, rightfoursum };
			List<int[]> foursensorswithmaxsum = new List<int[]>();
			for (int i = 0; i < threesum.Count(); i++)
			{
				if (threesum[i] == threesum.Max() && i == 0)
				{
					foursensorswithmaxsum.Add(leftfoursensors);
				}
				else if (threesum[i] == threesum.Max() && i == 1)
				{
					foursensorswithmaxsum.Add(middlefoursensors);
				}
				else if (threesum[i] == threesum.Max() && i == 2)
				{
					foursensorswithmaxsum.Add(rightfoursensors);
				}
			}
			//Console.WriteLine("we pick these four sensors {0} {1} {2} {3}", foursensorswithmaxsum[0][0], foursensorswithmaxsum[0][1], foursensorswithmaxsum[0][2], foursensorswithmaxsum[0][3]);
			for (int i = 0; i < foursensorswithmaxsum.Count(); i++)
			{
				thislocation = ComputeLocationPairFour(unsortedarray, foursensorswithmaxsum[i]);
				thisdistance = ComputeDistance(precoord, thislocation);
				if (thisdistance < mindistance)
				{
					mindistance = thisdistance;
					location = thislocation;
				}
			}
			return location;
		}
		//Used by ComputeLocationPair
		//Input the four sensors index, output the location
		//The index is sorted from left to right
		double[] ComputeLocationPairFour(List<int> unsortedarray, int[] foursensorsindex)
		{
			double[] location = new double[2];
			//the angle
			double theta;
			//try to determine if it is acceptable to use three sensors
			//the edge sensors data should be close
			//But if it satisfies two conditions, then we still pick four sensors
			if (unsortedarray[foursensorsindex[0]] - unsortedarray[FindRightNeighbour(foursensorsindex[3])] < pickthreethreshold || unsortedarray[foursensorsindex[3]] - unsortedarray[FindLeftNeighbour(foursensorsindex[0])] < pickthreethreshold)
			{
				if (unsortedarray[foursensorsindex[0]] - unsortedarray[FindRightNeighbour(foursensorsindex[3])] < pickthreethreshold && unsortedarray[foursensorsindex[3]] - unsortedarray[FindLeftNeighbour(foursensorsindex[0])] >= pickthreethreshold)
				{
					int[] middlethreesensors = new int[3] { foursensorsindex[1], foursensorsindex[2], foursensorsindex[3] };
					location = MergeVectors(unsortedarray, middlethreesensors);
					//Console.WriteLine("We pick the middle three sensors: {0} {1} {2}", middlethreesensors[0], middlethreesensors[1], middlethreesensors[2]);
				}
				else if (unsortedarray[foursensorsindex[0]] - unsortedarray[FindRightNeighbour(foursensorsindex[3])] >= pickthreethreshold && unsortedarray[foursensorsindex[3]] - unsortedarray[FindLeftNeighbour(foursensorsindex[0])] < pickthreethreshold)
				{
					int[] middlethreesensors = new int[3] { foursensorsindex[0], foursensorsindex[1], foursensorsindex[2] };
					location = MergeVectors(unsortedarray, middlethreesensors);
					//Console.WriteLine("We pick the middle three sensors: {0} {1} {2}", middlethreesensors[0], middlethreesensors[1], middlethreesensors[2]);
				}
				else
				{
					//still use four sensors
					location = MergeVectors(unsortedarray, foursensorsindex);
				}
			}
			else
			{
				//still use four sensors
				location = MergeVectors(unsortedarray, foursensorsindex);
			}
			//Divide into four fields
			if (location[0] > 0 && location[1] > 0)
			{
				theta = Math.Atan(location[1] / location[0]);
			}
			else if (location[0] > 0 && location[1] < 0)
			{
				theta = 2 * Math.PI - Math.Atan(Math.Abs(location[1] / location[0]));
			}
			else if (location[0] < 0 && location[1] > 0)
			{
				theta = Math.PI - Math.Atan(Math.Abs(location[1] / location[0]));
			}
			else
			{
				theta = Math.PI + Math.Atan(Math.Abs(location[1] / location[0]));
			}
			int locatepart = (int)Math.Ceiling(theta / (Math.PI * 2 / numofDonuts));
			double smallangle = Math.PI * 2 / numofDonuts * (locatepart - 1);
			double bigangle = Math.PI * 2 / numofDonuts * locatepart;
			double linearratio = (theta - smallangle) / (bigangle - smallangle);
			int[] leftrightidx = FindLeftRightIdxofPart(locatepart - 1);
			//Console.WriteLine("The cursor should locate between {0} and {1}", leftrightidx[0], leftrightidx[1]);
			int leftsensordata = unsortedarray[leftrightidx[0]];
			int rightsensordata = unsortedarray[leftrightidx[1]];
			double radius = ((rightsensordata - leftsensordata) * linearratio + leftsensordata) / 255 * cursorradius;
			//Console.WriteLine("The linearratio: {0}, Small angle: {1}, Big angle: {2}", linearratio, smallangle / Math.PI * 180, bigangle / Math.PI * 180);
			//Console.WriteLine("The interpolate data is {0}", ((rightsensordata - leftsensordata) * linearratio + leftsensordata));
			location[0] = radius * Math.Cos(theta);
			location[1] = radius * Math.Sin(theta);
			return location;
		}

		//Input the sensors index (can be any number of sensors)
		//Output the mergevector location
		double[] MergeVectors(List<int> unsortedarray, int[] sensorsidx)
		{
			double[] location = new double[2];
			for (int i = 0; i < sensorsidx.Length; i++)
			{
				location[0] += unsortedarray[sensorsidx[i]] / 255.0 * cursorradius * Math.Cos(sensorangles[sensorsidx[i]]);
				location[1] += unsortedarray[sensorsidx[i]] / 255.0 * cursorradius * Math.Sin(sensorangles[sensorsidx[i]]);
			}
			return location;
		}
		//
		// Deal with neighbours
		//

		//neighbours[0]: the left neighbour
		//neighbours[1]: the right neighbour
		int[] FindTwoNeighbours(int idx)
		{
			int[] neighbours = new int[2];
			if (idx == 0)
			{
				neighbours[0] = numofDonuts - 1;
				neighbours[1] = 1;
			}
			else if (idx == numofDonuts - 1)
			{
				neighbours[0] = numofDonuts - 2;
				neighbours[1] = 0;
			}
			else
			{
				neighbours[0] = idx - 1;
				neighbours[1] = idx + 1;
			}
			return neighbours;
		}

		int FindLeftNeighbour(int idx)
		{
			int[] neighbours = new int[2];
			neighbours = FindTwoNeighbours(idx);
			return neighbours[0];
		}

		int FindRightNeighbour(int idx)
		{
			int[] neighbours = new int[2];
			neighbours = FindTwoNeighbours(idx);
			return neighbours[1];
		}

		bool CheckTwoNeighbours(int idx1, int idx2)
		{
			return Math.Abs(idx1 - idx2) == 1 || idx1 == 0 && idx2 == numofDonuts - 1 || idx1 == numofDonuts - 1 && idx2 == 0;
		}

		bool CheckThreeNeighbours(int idx1, int idx2, int idx3)
		{
			//idx1,idx2,idx3 should be sorted increasingly
			return idx2 * 2 == idx1 + idx3 && idx2 - idx1 == 1 || idx1 == 0 && idx2 == 1 && idx3 == numofDonuts - 1 || idx1 == 0 && idx2 == numofDonuts - 2 && idx3 == numofDonuts - 1;
		}
		double ComputeDistance(double[] precoord, double[] location)
		{
			double distance = Math.Pow(precoord[0] - (doughnutscentertop + location[0]), 2) + Math.Pow(precoord[1] - (doughnutscenterleft + location[1]), 2);
			return distance;
		}
		//According to the partnumber - 1
		//return the sensor idx on its left and right side
		//First one is left, Second one is right
		int[] FindLeftRightIdxofPart(int partminus1)
		{
			List<int> leftsensoridxofpart = new List<int>();
			int[] leftrightidx = new int[2];
			for (int i = 0; i < numofDonuts; i++)
			{
				if (i <= numofDonuts / 2)
				{
					leftsensoridxofpart.Add(numofDonuts / 2 - i);
				}
				else
				{
					leftsensoridxofpart.Add(numofDonuts - i + numofDonuts / 2);
				}
			}
			int locatepartleftidx = leftsensoridxofpart[partminus1];
			int locatepartrightidx;
			if (locatepartleftidx == 0)
			{
				locatepartrightidx = 11;
			}
			else
			{
				locatepartrightidx = locatepartleftidx - 1;
			}
			leftrightidx[0] = locatepartleftidx;
			leftrightidx[1] = locatepartrightidx;
			return leftrightidx;
		}

		//Compute the location without the weight
		//Can only be used when picknum == numofdonuts or find the angle only
		double[] ComputeLocationWithoutWeight(List<int> sortedarray, List<int> idx, int picknum)
		{
			double[] location = new double[2];
			for (int i = 1; i <= picknum; i++)
			{
				//top
				location[0] += sortedarray[numofDonuts - i] / 255.0 * cursorradius * Math.Cos(sensorangles[idx[numofDonuts - i]]);
				//left
				location[1] += sortedarray[numofDonuts - i] / 255.0 * cursorradius * Math.Sin(sensorangles[idx[numofDonuts - i]]);
			}
			return location;
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

	}
>>>>>>> refs/remotes/origin/master
}
