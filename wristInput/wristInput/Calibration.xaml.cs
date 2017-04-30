using System;
using System.Windows;
using System.Threading;
using System.IO.Ports;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Linq;

namespace AssignmentTwo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Calibration : Window
    {
        SerialPort serialport;
        Thread arduino_thread, data_processing_thread;
        bool run_arduino_thread;
        Queue<string> arduino_data__buffer;
        List<Doughnut> Doughnutlist;
        List<Label> Labellist;
        Canvas mycanvas = new Canvas();
        const int numofDonuts = 12;
        double mincutoff = 1; //minimum cutoff frequency (fcmin)
        double beta = 0.8;
        double rate = 10.0; //data update rate (Hz)
        //a list of eurofilters
        List<OneEuroFilter> eurofilters = new List<OneEuroFilter>();
        //we would like to have from calibration
        double[] maximumintensity = new double[numofDonuts];
        double[] minimumintensity = new double[numofDonuts];
        //Some flags
        bool calibrationnow = false;
        bool finishoneloop = false;
        bool calibrationbefore = false;
        bool startclicksecond = false;
        // calibrationbefore firsttime: to calculate the average
        bool firsttime = true;
        // In order to reset them by button
        int[] maximumoneloop = new int[numofDonuts];
        List<int[]> maximumlist = new List<int[]>();
        int[] minimumoneloop = new int[numofDonuts];
        List<int[]> minimumlist = new List<int[]>();
        // In order to reset its content
        Button startcalibration = new Button();
        Button stopcalibration = new Button();
        Button recalibration = new Button();
        Button finishcalibration = new Button();
        // The cursor
        Ellipse cursor = new Ellipse();
        double doughnutscenterleft;
        double doughnutscentertop;
        double[] sensorangles = new double[numofDonuts];
        double cursorradius;
        double[] precoord = new double[2];
        //choose the middle point when the jump is too large
        double threshold = 20.0;
        //put the cursor in the middle
        double middlethreshold = 50.0;
        //two edge sensors are close then we pick middle three
        double pickthreethreshold = 50.0;

        int serialNum;
        string recordFileName;

        HitTest hitTest;

        public Calibration(int serialNum, string fileName)
        { 
            InitializeComponent();
            this.serialNum = serialNum;
            this.recordFileName = fileName;

            hitTest = new HitTest(serialNum, this.recordFileName);
            hitTest.Show();

            arduino_data__buffer = new Queue<string>();

            serialport = new SerialPort();
            serialport.PortName = "COM5";
            serialport.BaudRate = 9600;
            serialport.Open(); //uncomment this line to receive data from serialport
            run_arduino_thread = true;

            // Draw the Donuts and Labels
            this.Content = mycanvas;
            Doughnutlist = buildDonuts(numofDonuts);
            Labellist = buildLabels(numofDonuts);

            // Draw the buttons
            startcalibration.Margin = new Thickness(450, 180, 0, 0);
            startcalibration.Width = 100;
            startcalibration.Height = 30;
            startcalibration.Content = "StartCalibration";
            startcalibration.Click += start_Click;
            stopcalibration.Margin = new Thickness(450, 220, 0, 0);
            stopcalibration.Width = 100;
            stopcalibration.Height = 30;
            stopcalibration.Content = "StopCalibration";
            stopcalibration.Click += stop_Click;
            recalibration.Margin = new Thickness(450, 260, 0, 0);
            recalibration.Width = 100;
            recalibration.Height = 30;
            recalibration.Content = "ReCalibration";
            recalibration.Click += re_Click;
            //Redirect to hit test
            finishcalibration.Margin = new Thickness(450, 300, 0, 0);
            finishcalibration.Width = 100;
            finishcalibration.Height = 30;
            finishcalibration.Content = "Send to Test";
            finishcalibration.Click += finish_Click;

            mycanvas.Children.Add(startcalibration);
            mycanvas.Children.Add(stopcalibration);
            mycanvas.Children.Add(recalibration);
            //mycanvas.Children.Add(finishcalibration);

            //Draw the cursor
            cursor.Width = 8;
            cursor.Height = 8;
            cursor.Stroke = Brushes.CadetBlue;
            cursor.StrokeThickness = 1;
            doughnutscenterleft = 150 + Doughnutlist[0].Width / 2 - cursor.Width/2;
            doughnutscentertop = 100 + Doughnutlist[0].Height / 2 - cursor.Height/2;
            cursorradius = Doughnutlist[0].Width / 2 - Doughnutlist[0].inner_width - cursor.Width/2;
            cursor.SetValue(Canvas.LeftProperty, doughnutscenterleft);
            cursor.SetValue(Canvas.TopProperty, doughnutscentertop);
            mycanvas.Children.Add(cursor);
            ComputeAngles();

            // Get the data
            arduino_thread = new Thread(ReadArduino);
            arduino_thread.Start();
            data_processing_thread = new Thread(ProcessArduinoData);
            data_processing_thread.Start();

        }

        void ComputeAngles() {
            int half = numofDonuts / 2;
            sensorangles[0] = Math.PI;
            sensorangles[half] = 0.0;
            for (int i = 1; i <= (numofDonuts - 2) / 2; i++) {
                sensorangles[half - i] = (Math.PI / half) * i;
                sensorangles[half + i] = Math.PI * 2 - sensorangles[half - i];
            }
        }

        List<Doughnut> buildDonuts(int numofDonuts)
        {
            List<Doughnut> Doughnutlist = new List<Doughnut>();
            for (int i = 1; i <= numofDonuts; i++)
            {
                Doughnut myDonut = new Doughnut((i - 1) * 360.0 / numofDonuts, i * 360.0 / numofDonuts, 20, true);
                myDonut.SetValue(Canvas.LeftProperty, (double)150);
                myDonut.SetValue(Canvas.TopProperty, (double)100);
                mycanvas.Children.Add(myDonut);
                Doughnutlist.Add(myDonut);
            }
            return Doughnutlist;
        }

        List<Label> buildLabels(int numofDonuts)
        {
            List<Label> Labellist = new List<Label>();
            for (int i = 1; i <= numofDonuts; i++)
            {
                Label mylabel = new Label();
                mylabel.Content = "Sensor " + (i - 1).ToString();
                mylabel.FontSize = 10;
                mylabel.Width = 100;
                double x, y;
                int WindowWidth = 220;
                int WindowHeight = 220;
                x = WindowWidth / 2 + WindowWidth / 2 * Math.Cos((Math.PI / 180.0) * ((i - 1) * 360.0 / numofDonuts - 90));
                y = WindowHeight / 2 + WindowHeight / 2 * Math.Sin((Math.PI / 180.0) * ((i - 1) * 360.0 / numofDonuts - 90))-5;
                mylabel.Margin = new Thickness(x, y, 0, 0);
                mylabel.SetValue(Canvas.LeftProperty, (double)90);
                mylabel.SetValue(Canvas.TopProperty, (double)60);
                mycanvas.Children.Add(mylabel);
                Labellist.Add(mylabel);
            }
            return Labellist;
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

        void ProcessArduinoData()
        {

            String buffered_data;

            //generate the filters
            for (int i = 0; i < numofDonuts; i++) {
                OneEuroFilter eachfilter = new OneEuroFilter(mincutoff, beta);
                eurofilters.Add(eachfilter);        
            }
            for (int i = 0; i < numofDonuts; i++) {
                // initialize the maximumintensity(what we want from calibration)
                maximumintensity[i] = 1023.0;
                minimumintensity[i] = 0.0;
                // set intial value to the largest
                minimumoneloop[i] = 1023;
            }

            while (run_arduino_thread)
            {
                if (arduino_data__buffer.Count > 0)
                {
                    buffered_data = arduino_data__buffer.Dequeue();
                    String[] nstrs = buffered_data.Split(' ');

                    //Discard initial wrong data
                    try
                    {
                        Int32.Parse(nstrs[0]);
                    }
                    catch
                    {
                        Console.WriteLine("Discard a set of data");
                        continue;
                    }
                    if (nstrs.Length != numofDonuts) continue;

                    int[] filteredintensity = new int[numofDonuts];

                    //filter the data with low-pass filter
                    for (int i = 0; i < numofDonuts; i++)
                    {
                        int originalintensity;
                        originalintensity = Int32.Parse(nstrs[i]);
                        double filteredintensityd = eurofilters[i].Filter((double)originalintensity, rate);
                        filteredintensity[i] = 1023 - (int)filteredintensityd;
                    }

                    if (calibrationnow)
                    {
                        if (finishoneloop) {
                            //Add the maximumoneloop into matrix and set it to zero
                            maximumlist.Add(maximumoneloop);
                            maximumoneloop = new int[numofDonuts];
                            minimumlist.Add(minimumoneloop);
                            minimumoneloop = new int[numofDonuts];
                            for (int i = 0; i < numofDonuts; i++) minimumoneloop[i] = 1023;
                            finishoneloop = false;
                        }
                        //update the maximum in one loop
                        FindMaximum(filteredintensity);
                        FindMinimum(filteredintensity);
                        //make a thread safe call to the UI thread to update UI
                        Display(filteredintensity);
                    }
                    else {
                        if (calibrationbefore)
                        {
                            if (firsttime) {
                                ComputeMaxAverage();
                                ComputeMinAverage();
                                firsttime = false;
                            }
                            //make a thread safe call to the UI thread to update UI
                            Display(filteredintensity);
                        }
                        else {
                            //make a thread safe call to the UI thread to update UI
                            Display(filteredintensity);
                        }
                    }
                }
            }//end while
        }//end method


        //update the UI
        void Display(int[] filteredintensity) {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                // each element in this list: 0-255
                List<int> darkintensityintlist = new List<int>();

                for (int i = 0; i < filteredintensity.Length; i++)
                {

                    //Alpha
                    //Current Alpha
                    double darkintensityd;
                    darkintensityd = CheckReachMax(filteredintensity[i], maximumintensity[i], minimumintensity[i]);
                    //Add to the list
                    darkintensityintlist.Add((int)darkintensityd);
                    //Labels
                    int intdarkintensityd = (int)darkintensityd;
                    Labellist[i].Content = 'S' + i.ToString() + ": " + intdarkintensityd.ToString();

                    byte darkintensityb = Convert.ToByte((int)darkintensityd);
                    //Next Alpha
                    double darkintensitydnext;
                    if (i != filteredintensity.Length - 1) // consider the last element
                    {
                        darkintensitydnext = CheckReachMax(filteredintensity[i + 1], maximumintensity[i + 1], minimumintensity[i + 1]);
                    }
                    else {
                        darkintensitydnext = CheckReachMax(filteredintensity[0], maximumintensity[0], minimumintensity[0]);
                    }
                    byte darkintensitybnext = Convert.ToByte((int)darkintensitydnext);
                    LinearGradientBrush Lgb = new LinearGradientBrush();
                    int interval = filteredintensity.Length / 4;
                    //Four situations
                    if (i < interval)
                    {
                        Lgb.StartPoint = new Point(0, 0);
                        Lgb.EndPoint = new Point(1, 1);
                    }
                    else if (i < 2*interval)
                    {
                        Lgb.StartPoint = new Point(1, 0);
                        Lgb.EndPoint = new Point(0, 1);
                    }
                    else if (i < 3*interval)
                    {
                        Lgb.StartPoint = new Point(1, 1);
                        Lgb.EndPoint = new Point(0, 0);
                    }
                    else {
                        Lgb.StartPoint = new Point(0, 1);
                        Lgb.EndPoint = new Point(1, 0);
                    }
                    GradientStop gs1 = new GradientStop();
                    GradientStop gs2 = new GradientStop();
                    gs1.Color = Color.FromArgb(darkintensityb, 0, 0, 0);
                    gs2.Color = Color.FromArgb(darkintensitybnext, 0, 0, 0);
                    gs1.Offset = 0.0;
                    gs2.Offset = 1.0;
                    Lgb.GradientStops.Add(gs1);
                    Lgb.GradientStops.Add(gs2);
                    Doughnutlist[i].OpacityMask = Lgb;
                }
                
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
                        else {
                            if (idx[numofDonuts - 1] < idx[numofDonuts - 2])
                            {
                                //location = ComputeLocationPair(darkintensityintlist, idx[numofDonuts - 1], idx[numofDonuts - 2]);
                                int[] foursensorsindex = new int[4] { FindLeftNeighbour(idx[numofDonuts - 1]), idx[numofDonuts - 1], idx[numofDonuts - 2], FindRightNeighbour(idx[numofDonuts - 2]) };
                                location = ComputeLocationPairFour(darkintensityintlist, foursensorsindex);
                            }
                            else {
                                //location = ComputeLocationPair(darkintensityintlist, idx[numofDonuts - 2], idx[numofDonuts - 1]);
                                int[] foursensorsindex = new int[4] { FindLeftNeighbour(idx[numofDonuts - 2]), idx[numofDonuts - 2], idx[numofDonuts - 1], FindRightNeighbour(idx[numofDonuts - 1]) };
                                location = ComputeLocationPairFour(darkintensityintlist, foursensorsindex);
                            }
                        }
                    }
                    else {
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
                        for (int i = 1; i < 4; i++) {
                            //Suppose to have 2*picknum sums
                            //Add the idx + leftneighbour 
                            pairsums.Add(darkintensityintlist[idx[numofDonuts - i]] + darkintensityintlist[FindLeftNeighbour(idx[numofDonuts - i])]);
                            leftidxinpair.Add(FindLeftNeighbour(idx[numofDonuts - i]));
                            //Add the idx + rightneighbour
                            pairsums.Add(darkintensityintlist[idx[numofDonuts - i]] + darkintensityintlist[FindRightNeighbour(idx[numofDonuts - i])]);
                            leftidxinpair.Add(idx[numofDonuts - i]);
                        }
                        //Find all maximum pair, record the left idx in these pairs
                        for (int i = 0; i < pairsums.Count(); i++) {
                            if (pairsums[i] == pairsums.Max()) {
                                maxsumleftidx.Add(leftidxinpair[i]);
                            }
                        }
                        //Delete the repeated elements
                        maxsumleftidx = maxsumleftidx.Distinct().ToList();
                        //Console.WriteLine("Two maximum are not neighbours, we pick the maxisum of {0} {1}", maxsumleftidx[0], FindRightNeighbour(maxsumleftidx[0]));
                        //Consider all maximum, pick the location with the minimum distance with the precoord
                        for (int i = 0; i < maxsumleftidx.Count(); i++) {
                            double[] thislocation = ComputeLocationPair(darkintensityintlist, maxsumleftidx[i], FindRightNeighbour(maxsumleftidx[i]));
                            double thisdistance = ComputeDistance(precoord, thislocation);
                            if (thisdistance < mindistance) {
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
                    this.hitTest.updateCursorPos(top, left);
                    precoord[0] = top;
                    precoord[1] = left;
                    //Console.WriteLine("skip");
                }
                else
                {
                    cursor.SetValue(Canvas.TopProperty, doughnutscentertop + location[0]);
                    cursor.SetValue(Canvas.LeftProperty, doughnutscenterleft + location[1]);
                    this.hitTest.updateCursorPos(doughnutscentertop + location[0], doughnutscenterleft + location[1]);
                    precoord[0] = doughnutscentertop + location[0];
                    precoord[1] = doughnutscenterleft + location[1];
                }

            }));
        }

        //
        //
        // Compute the cursor location
        //
        //

        //Based a pair of sensors, output the location
        //Second parameter should be left index, third should be right index
        double[] ComputeLocationPair(List<int> unsortedarray, int leftidx, int rightidx) {
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
            for (int i = 0; i < 4; i++) {
                leftfoursum += unsortedarray[leftfoursensors[i]];
                middlefoursum += unsortedarray[middlefoursensors[i]];
                rightfoursum += unsortedarray[rightfoursensors[i]];
            }
            // Consider we have several maximums
            // Pick the one with minimum distance with historical data 
            List<int> threesum = new List<int> { leftfoursum, middlefoursum, rightfoursum };
            List<int[]> foursensorswithmaxsum = new List<int[]>();
            for (int i = 0; i < threesum.Count(); i++) {
                if (threesum[i] == threesum.Max() && i == 0)
                {
                    foursensorswithmaxsum.Add(leftfoursensors);
                }
                else if (threesum[i] == threesum.Max() && i == 1)
                {
                    foursensorswithmaxsum.Add(middlefoursensors);
                }
                else if (threesum[i] == threesum.Max() && i == 2) {
                    foursensorswithmaxsum.Add(rightfoursensors);
                }
            }
            //Console.WriteLine("we pick these four sensors {0} {1} {2} {3}", foursensorswithmaxsum[0][0], foursensorswithmaxsum[0][1], foursensorswithmaxsum[0][2], foursensorswithmaxsum[0][3]);
            for (int i = 0; i < foursensorswithmaxsum.Count(); i++) {
                thislocation = ComputeLocationPairFour(unsortedarray, foursensorswithmaxsum[i]);
                thisdistance = ComputeDistance(precoord, thislocation);
                if (thisdistance < mindistance) {
                    mindistance = thisdistance;
                    location = thislocation;
                }
            }
            return location;
        }

        //Used by ComputeLocationPair
        //Input the four sensors index, output the location
        //The index is sorted from left to right
        double[] ComputeLocationPairFour(List<int> unsortedarray, int[] foursensorsindex) {
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
            else {
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
            else {
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
        double[] MergeVectors(List<int> unsortedarray, int[] sensorsidx) {
            double[] location = new double[2];
            for (int i = 0; i < sensorsidx.Length; i++)
            {
                location[0] += unsortedarray[sensorsidx[i]] / 255.0 * cursorradius * Math.Cos(sensorangles[sensorsidx[i]]);
                location[1] += unsortedarray[sensorsidx[i]] / 255.0 * cursorradius * Math.Sin(sensorangles[sensorsidx[i]]);
            }
            return location;
        }


        //According to the partnumber - 1
        //return the sensor idx on its left and right side
        //First one is left, Second one is right
        int[] FindLeftRightIdxofPart(int partminus1) {
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

        //Notification!!
        //Only calculate distance between location and precoord, the location of two parameters can not be changed
        double ComputeDistance(double[] precoord, double[] location) {
            double distance = Math.Pow(precoord[0] - (doughnutscentertop + location[0]), 2) + Math.Pow(precoord[1] - (doughnutscenterleft + location[1]), 2);
            return distance;
        }



        //Compute the location with the weight (the original "wrong" algorithm)
        double[] ComputeLocation(List<int> sortedarray, List<int> idx, int picknum)
        {
            double[] location = new double[2];
            // In order to compute weight for each direction
            double sum = 0.0;
            for (int i = 1; i <= picknum; i++) sum += sortedarray[numofDonuts - i];

            for (int i = 1; i <= picknum; i++)
            {
                //top
                location[0] += (sortedarray[numofDonuts - i] / sum) * sortedarray[numofDonuts - i] / 255.0 * cursorradius * Math.Cos(sensorangles[idx[numofDonuts - i]]);
                //left
                location[1] += (sortedarray[numofDonuts - i] / sum) * sortedarray[numofDonuts - i] / 255.0 * cursorradius * Math.Sin(sensorangles[idx[numofDonuts - i]]);
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

        //
        //Deal with data
        //

        double CheckReachMax(int intensity, double maxintensity, double minintensity) {
            double intensityd;
            if (intensity > maxintensity)
            {
                intensityd = 255.0;
            }
            else if (intensity < minintensity)
            {
                intensityd = 0.0;
            }
            else {
                intensityd = ((intensity - minintensity) / (maxintensity - minintensity)) * 255;
            }
            return intensityd;
        }

        void FindMaximum(int[] filteredintensity) {
            // Not complete one cirle
            for (int i = 0; i < filteredintensity.Length; i++)
            {
                if (filteredintensity[i] > maximumoneloop[i]) {
                    maximumoneloop[i] = filteredintensity[i];
                }
            }
        }

        void FindMinimum(int[] filteredintensity)
        {
            // Not complete one cirle
            for (int i = 0; i < filteredintensity.Length; i++)
            {
                if (filteredintensity[i] < minimumoneloop[i])
                {
                    minimumoneloop[i] = filteredintensity[i];
                }
            }
        }

        void ComputeMaxAverage() {
            int[] sum = new int[maximumlist[0].Length];
            for (int i = 0; i < maximumlist.Count; i++) {
                for (int j = 0; j < maximumlist[0].Length; j++) {
                    sum[j] += maximumlist[i][j];
                }
            }
            for (int k = 0; k < maximumlist[0].Length; k++) {
                maximumintensity[k] = sum[k] / (double)maximumlist.Count;
            }

        }

        void ComputeMinAverage()
        {
            int[] sum = new int[minimumlist[0].Length];
            for (int i = 0; i < minimumlist.Count; i++)
            {
                for (int j = 0; j < minimumlist[0].Length; j++)
                {
                    sum[j] += minimumlist[i][j];
                }
            }
            for (int k = 0; k < minimumlist[0].Length; k++)
            {
                minimumintensity[k] = sum[k] / (double)minimumlist.Count;
            }


        }

        //
        // Activation
        //

        void start_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            clickedButton.Content = "FinishedLoop";
            calibrationnow = true;
            if (startclicksecond) {
                finishoneloop = true;
            }
            startclicksecond = true;

        }

        void stop_Click(object sender, RoutedEventArgs e)
        {
            calibrationnow = false;
            calibrationbefore = true;
        }

        void re_Click(object sender, RoutedEventArgs e)
        {
            
            //Some flags
            calibrationnow = false;
            finishoneloop = false;
            calibrationbefore = false;
            startclicksecond = false;
            // calibrationbefore firsttime: to calculate the average
            firsttime = true;
            // Reset those variables
            maximumoneloop = new int[numofDonuts];
            maximumlist = new List<int[]>();
            minimumoneloop = new int[numofDonuts];
            minimumlist = new List<int[]>();
            // initialize the maximumintensity(what we want from calibration)
            for (int i = 0; i < numofDonuts; i++)
            {
                maximumintensity[i] = 1023.0;
                minimumintensity[i] = 0.0;
                minimumoneloop[i] = 1023;
            }
  
            //reset the content
            startcalibration.Content = "StartCalibration";
        }

        void finish_Click(object sender, RoutedEventArgs e)
        {
            //this.hitTest.updateFilter();
            //this.Close();
        }

            void Calibration_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            run_arduino_thread = false;
        }//end method
    }
}
