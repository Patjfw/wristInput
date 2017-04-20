using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AssignmentTwo
{
    class Doughnut : Shape
    {
        public double inner_width;
        public double start_angle;
        public double stop_angle;
		public bool isCalib;
        public Doughnut(double start_angle, double stop_angle, double inner_width, bool isCalib)
        {
            // Input two variables
            this.start_angle = start_angle;
            this.stop_angle = stop_angle;
            this.inner_width = inner_width;
			this.isCalib = isCalib;
            // Define some constant parameters
            //inner_width = 40;
            Height = 150;
            Width = 150;
            Stroke = new SolidColorBrush(Colors.White);
            StrokeThickness = 1;
            Fill = new SolidColorBrush(Colors.Black);

        }

        protected override Geometry DefiningGeometry
        {
            get { return GenerateMyWeirdGeometry(); }
        }

        private Geometry GenerateMyWeirdGeometry()
        {

            // Setup the Center Point & Radius
            StreamGeometry geom = new StreamGeometry();
            using (StreamGeometryContext context = geom.Open())
            {
                Point c = new Point(ActualWidth / 2, ActualHeight / 2);
                double rOutterX = ActualWidth / 2;
                double rOutterY = ActualHeight / 2;
				double defautCalibWidth = 20;
                double rInnerX = rOutterX - defautCalibWidth;
                double rInnerY = rOutterY - defautCalibWidth;
                double theta = 0;
                bool hasBegun = false;
                double x;
                double y;
                Point currentPoint;

				if (!isCalib)
				{
					rOutterX = rOutterX - defautCalibWidth;
					rOutterY = rOutterY - defautCalibWidth;
				}
                // Draw the Outside Edge
                for (theta = start_angle; theta <= stop_angle; theta++)
                {
                    x = c.X + rOutterX * Math.Cos(GetRadian(theta));
                    y = c.Y + rOutterY * Math.Sin(GetRadian(theta));
                    currentPoint = new Point(x,y);
                    if (!hasBegun)
                    {
                        context.BeginFigure(currentPoint, true, true);
                        hasBegun = true;
                    }
                    context.LineTo(currentPoint, true, true);
                }

                // Connect the Outside Edge to the Inner Edge
                x = c.X + rInnerX * Math.Cos(GetRadian(stop_angle));
                y = c.Y + rInnerY * Math.Sin(GetRadian(stop_angle));
                currentPoint = new Point(x, y);
                context.LineTo(currentPoint, true, true);

                // Draw the Inner Edge
                for (theta = stop_angle; theta >= start_angle; theta--)
                {
					if (!isCalib)
					{
						x = c.X + rInnerX * Math.Cos(GetRadian(theta)) - inner_width * Math.Cos(GetRadian(theta));
						y = c.Y + rInnerY * Math.Sin(GetRadian(theta)) - inner_width * Math.Sin(GetRadian(theta));	
					}
					else
					{
						x = c.X + rInnerX * Math.Cos(GetRadian(theta));
						y = c.Y + rInnerY * Math.Sin(GetRadian(theta));
					}
					currentPoint = new Point(x, y);
                    context.LineTo(currentPoint, true, true);
                }

                // Connect the Inner Edge to the Outside Edge
                x = c.X + rOutterX * Math.Cos(GetRadian(start_angle));
                y = c.Y + rOutterY * Math.Sin(GetRadian(start_angle));
                currentPoint = new Point(x, y);
                context.LineTo(currentPoint, true, true);

                //context.Close();
            }


            return geom;
        }

        private double GetRadian(double angle)
        {
            return (Math.PI / 180.0) * (angle - 90);
        }

    }
}

