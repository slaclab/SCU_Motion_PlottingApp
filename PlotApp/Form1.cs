using ScottPlot;
using ScottPlot.WinForms;

namespace PlotApp
{
    public partial class PlotApp : Form
    {

        double counter = DateTime.Now.ToNumber(); // program counter
        double start = DateTime.Now.ToNumber(); // program starting time
        readonly int seconds = 10; // number of previous seconds to be plotted
        const int MaxPoints = 2000; // number of previous points to keep in memory
        const int refresh_rate = 10; // refresh rate
        const int NumberOfSignals = 6; // signals chosen to analyze
        List<double>[] data = new List<double>[NumberOfSignals]; // incoming data
        const double conversion = 1000 / refresh_rate;
        const double secondtoSample = (1D/86400)/conversion; // seconds to percent
        int currentSampleCount = 0;

        readonly ScottPlot.AxisPanels.LeftAxis[] axisL = new ScottPlot.AxisPanels.LeftAxis[NumberOfSignals];
        readonly ScottPlot.AxisPanels.RightAxis[] axisR = new ScottPlot.AxisPanels.RightAxis[NumberOfSignals];
        readonly ScottPlot.Plottables.Signal[] sig = new ScottPlot.Plottables.Signal[NumberOfSignals];

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

        // temporary button toggles
        bool view = false;
        bool pause = false;
        readonly bool[] trace = new bool[NumberOfSignals]; // active low

        double[] dataX = []; // counter data

        public PlotApp()
        {
            InitializeComponent();
            InitializeArrays();
            CreateChart();
        }

        private void InitializeArrays() // initialize data arrays to empty for appending
        {
            for (int i = 0; i < NumberOfSignals; i++)
            {
                data[i] = [];
                trace[i] = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e) // enable timer
        {
            // Create and enable timer in ms
            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer
            {
                Enabled = true,
                Interval = refresh_rate
            };
            timer1.Start();
            timer1.Tick += timer1_Tick;
        }

        int c = 1;
        public double GetNextSineValue()
        {
            const int SamplesPerPeriod = 600;
            // Increment the sample count
            currentSampleCount++;

            // Reset the sample count if it exceeds the specified limit
            if (currentSampleCount > SamplesPerPeriod)
            {
                currentSampleCount = 1;
                c++;
            }

            // Calculate the phase based on the current sample count
            double phase = 2 * Math.PI * currentSampleCount / SamplesPerPeriod;

            // Calculate the sine value
            double sineValue = Math.Sin(phase);

            // Return the same value 6 times in a row
            return c+sineValue;
        }

        private void updateChart() // update array values and generate legend
        {
            // receive new data
            double[] newValue = new double[NumberOfSignals];

            for (int i = 0; i < NumberOfSignals; i++)
            {
                newValue[i] = GetNextSineValue();
                if(i == 0)
                {
                    newValue[i] -= c;
                }
                newValue[i] *= (i+1);
                data[i] = [.. data[i], newValue[i]];
            }

            for (int i = 0; i<NumberOfSignals; i++)
            {
                if (data[i].Count > MaxPoints)
                {
                    data[i].RemoveRange(0, data[i].Count - MaxPoints);
                }
            }

            // generate the signal
            formsPlot1.Plot.Clear();
            for (int i = 0; i < NumberOfSignals; i++)
            {
                if (!trace[i])
                {
                    sig[i] = formsPlot1.Plot.Add.Signal(data[i], secondtoSample, color: palette[i]);
                    if(i%2 == 0)
                    {
                        sig[i].Axes.YAxis = axisL[i];
                    }
                    else
                    {
                        sig[i].Axes.YAxis = axisR[i];
                    }
                    sig[i].Data.XOffset = counter;
                }
            }

            // increment x-axis
            dataX = [.. dataX, counter];
            counter += secondtoSample;

            // Write data to CSV when the limit is reached
            if (dataX.Count() == MaxPoints)
            {
                WriteDataToCsv();
            }

        }

        private void timer1_Tick(object sender, EventArgs e) // callback performed at tick rate
        {
            // update chart and view mode
            updateChart();

            // stop refreshing if pause is toggled
            if (!pause)
            {
                formsPlot1.Refresh();
                viewMode();
            }
        }

        private void CreateChart() // set plot labels
        {
            formsPlot1.Plot.Axes.Remove(Edge.Left);
            formsPlot1.Plot.Axes.Remove(Edge.Right);
            formsPlot1.Plot.Axes.Remove(Edge.Top);

            // handle n amount of axes
            for (int i = 0; i < NumberOfSignals;i++)
            {
                if (data[i] != null)
                {
                    if(i%2 == 0)
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
            formsPlot1.Plot.Axes.DateTimeTicks(Edge.Bottom);
            formsPlot1.Refresh();
        }

        private void viewMode() // logic for view selection
        {
            if (view) // view last n
            {
                formsPlot1.Plot.Axes.AutoScale();
                formsPlot1.Plot.Axes.SetLimitsX(counter-(secondtoSample*seconds*conversion)+0.000235, counter+0.000235);
                //double max = 0;
                //for(int i = 0;i < NumberOfSignals; i++)
                //{
                //    if (data[i].Max() > max)
                //    {
                //        max = data[i].Max();
                //    }
                //}
                //formsPlot1.Plot.Axes.SetLimitsY(0, max);
            }
            else // auto scale
            {
                formsPlot1.Plot.Axes.AutoScale();
            }
        }

        Random rand = new();
        private double generateData() // return random value
        {
            return rand.NextDouble() * 10;
        }

        private void button1_Click(object sender, EventArgs e) // view button
        {
            view = !view;
        }

        private void button2_Click(object sender, EventArgs e) // pause button
        {
            pause = !pause;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            int num = 0;
            traceException(num);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            int num = 1;
            traceException(num);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            int num = 2;
            traceException(num);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            int num = 3;
            traceException(num);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            int num = 4;
            traceException(num);
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            int num = 5;
            traceException(num);
        } 
        private void traceException(int num)
        {
            try
            {
                trace[num] = !trace[num];
            }
            catch (Exception) { }
        }

        private void WriteDataToCsv()
        {
            try
            {
                using StreamWriter writer = new("data.csv");
                // Write header
                writer.WriteLine("Time," + string.Join(",", Enumerable.Range(1, NumberOfSignals).Select(i => $"Signal{i}")));

                // Write data
                for (int j = 0; j < dataX.Count(); j++)
                {
                    writer.WriteLine($"{dataX[j]},{string.Join(",", Enumerable.Range(0, NumberOfSignals).Select(i => data[i].Count > j ? data[i][j].ToString() : ""))}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to CSV: {ex.Message}");
            }
        }

    }
}
