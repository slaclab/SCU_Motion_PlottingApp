using ScottPlot;
using System.Text;

namespace PlotApp
{
    public partial class PlotApp : Form
    {
        static readonly DateTime currentTime = DateTime.Now;
        double counter = currentTime.ToNumber(); // program counter
        readonly double start = currentTime.ToNumber(); // program starting time
        readonly int seconds = 10; // number of previous seconds to be plotted in n second view mode
        const int MaxPoints = 600; // number of previous points to keep in memory
        const int refresh_rate = 50; // refresh rate (1000/number)
        const int NumberOfSignals = 6; // signals chosen to analyze
        List<double>[] data = Enumerable.Range(0, NumberOfSignals).Select(_ => new List<double>()).ToArray();
        const double conversion = 1000 / refresh_rate;
        const double secondtoSample = (1D / 86400) / conversion; // second to percent and convert based off of refresh rate
        int currentSampleCount = 0;
        Random rand = new();

        // make axis and signal arrays
        readonly ScottPlot.AxisPanels.LeftAxis[] axisL = new ScottPlot.AxisPanels.LeftAxis[NumberOfSignals];
        readonly ScottPlot.AxisPanels.RightAxis[] axisR = new ScottPlot.AxisPanels.RightAxis[NumberOfSignals];
        readonly ScottPlot.Plottables.Signal[] sig = new ScottPlot.Plottables.Signal[NumberOfSignals];

        // define order of colors for traces and axes
        readonly ScottPlot.Color[] palette =
        [
            Colors.SeaGreen,
            Colors.DarkOrange,
            Colors.Lime,
            Colors.DarkGray,
            Colors.DodgerBlue,
            Colors.DeepPink,
            Colors.Yellow,
            Colors.Bisque,
            Colors.MintCream,
            Colors.DarkKhaki,
        ];

        // button toggles
        bool view = false;
        bool pause = false;
        readonly bool[] trace = new bool[NumberOfSignals]; // active low

        double[] dataX = []; // counter data

        public PlotApp()
        {
            InitializeComponent();
            // Create and enable timer in ms
            System.Timers.Timer timer1 = new System.Timers.Timer
            {
                Enabled = true,
                Interval = refresh_rate
            };
            timer1.Start();
            timer1.Elapsed += timer1_Tick;
            FollowMouse();
            CreateChart();
        }

        private void FollowMouse()
        {
            formsPlot1.MouseMove += (s, e) =>
            {
                Pixel mousePixel = new(e.X, e.Y);
                Coordinates mouseCoordinates = formsPlot1.Plot.GetCoordinates(mousePixel);
                Text = $"X={((mouseCoordinates.X) - start) * 10000:N3}, Y={mouseCoordinates.Y:N3}";
            };

            formsPlot1.MouseDown += (s, e) =>
            {
                Pixel mousePixel = new(e.X, e.Y);
                Coordinates mouseCoordinates = formsPlot1.Plot.GetCoordinates(mousePixel);
                Text = $"X={((mouseCoordinates.X) - start) * 10000:N3}, Y={mouseCoordinates.Y:N3} ";
            };
        }

        // generate sinusoid value
        int c = 1;
        public double GetNextSineValue()
        {
            const int SamplesPerPeriod = 600;
            currentSampleCount++;
            if (currentSampleCount > SamplesPerPeriod)
            {
                currentSampleCount = 1;
                c++;
            }
            double phase = 2 * Math.PI * currentSampleCount / SamplesPerPeriod;
            double sineValue = Math.Sin(phase);
            return c + sineValue;
        }

        private void updateData()
        {
            // receive new data and remove old data if necessary
            double[] newValue = new double[NumberOfSignals];

            for (int i = 0; i < NumberOfSignals; i++)
            {
                newValue[i] = GetNextSineValue() + (rand.NextDouble()*0.2);
                if (i == 0)
                {
                    newValue[i] -= c;
                }
                newValue[i] *= (i + 1);
                data[i] = [.. data[i], newValue[i]];

                if (data[i].Count > MaxPoints && !pause)
                {
                    data[i].RemoveRange(0, data[i].Count - MaxPoints);
                }
            }
            WriteDataToCsv();
        }

        private void updateChart() // update array values and generate legend
        {
            // generate the signal assign axes and calculate time offset
            formsPlot1.Plot.Clear();
            for (int i = 0; i < NumberOfSignals; i++)
            {
                if (!trace[i])
                {
                    sig[i] = formsPlot1.Plot.Add.Signal(data[i], secondtoSample, color: palette[i]);
                    if (i % 2 == 0)
                    {
                        sig[i].Axes.YAxis = axisL[i];
                    }
                    else
                    {
                        sig[i].Axes.YAxis = axisR[i];
                    }
                }
                if (data[i].Count >= MaxPoints)
                {
                    sig[i].Data.XOffset = counter - (secondtoSample * MaxPoints);
                }
                else
                {
                    sig[i].Data.XOffset = start;
                }
            }

            // increment x-axis
            dataX = [.. dataX, counter];
            counter += secondtoSample;
        }

    private void timer1_Tick(object sender, EventArgs e) // callback performed at tick rate
        {
            updateData();
            // update chart and view mode
            try
            {
                formsPlot1.Invoke((MethodInvoker)delegate { // force run on UI thread
                    updateChart();
                    if (!pause)
                    {
                        formsPlot1.Refresh();
                        viewMode();
                    }
                });
            }
            catch (Exception) { }
        }

        private void CreateChart() // set plot labels
        {
            // remove excess borders
            formsPlot1.Plot.Axes.Remove(Edge.Left);
            formsPlot1.Plot.Axes.Remove(Edge.Right);
            formsPlot1.Plot.Axes.Remove(Edge.Top);

            // handle n amount of axes
            for (int i = 0; i < NumberOfSignals; i++)
            {
                if (data[i] != null)
                {
                    if (i % 2 == 0)
                    {
                        axisL[i] = formsPlot1.Plot.Axes.AddLeftAxis();
                        axisL[i].Color(palette[i]);
                    }
                    else
                    {
                        axisR[i] = formsPlot1.Plot.Axes.AddRightAxis();
                        axisR[i].Color(palette[i]);
                    }
                }
            }
            // make bottom axes date and time and refresh chart
            formsPlot1.Plot.Axes.DateTimeTicksBottom();
            formsPlot1.Refresh();
        }

        private void viewMode() // logic for view selection
        {
            if (view) // view last n
            {
                formsPlot1.Plot.Axes.AutoScale();
                formsPlot1.Plot.Axes.SetLimitsX(counter - (secondtoSample * seconds * conversion), counter);
            }
            else // auto scale
            {
                formsPlot1.Plot.Axes.AutoScale();
            }
        }

        private void WriteDataToCsv()
        {
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Time," + string.Join(",", Enumerable.Range(1, NumberOfSignals).Select(i => $"Signal{i}")));

            for (int j = 0; j < dataX.Length; j++)
            {
                DateTime timestamp = currentTime.AddSeconds(j/conversion);
                string formattedTimestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string dataValues = string.Join(",", Enumerable.Range(0, NumberOfSignals).Select(i => data[i].Count > j ? data[i][j].ToString() : ""));
                csvContent.AppendLine($"{formattedTimestamp},{dataValues}");
            }

            try
            {
                using StreamWriter writer = new("data.csv");
                writer.Write(csvContent.ToString());
            }
            catch (Exception){}
        }

        private void button1_Click(object sender, EventArgs e) // view button
        {
            view = !view;
        }

        private void button2_Click(object sender, EventArgs e) // pause button
        {
            pause = !pause;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox != null)
            {
                int num = (int)checkBox.Tag;
                try
                {
                    trace[num] = !trace[num];
                }
                catch (Exception) { }
            }
        }
    }
}
