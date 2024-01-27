using ScottPlot;

namespace PlotApp
{
    public partial class PlotApp : Form
    {
        double counter = DateTime.Now.ToNumber(); // program counter ticks at sample rate
        double start = DateTime.Now.ToNumber(); // program counter ticks at sample rate
        readonly int points = 10; // number of previous points to be plotted
        int interval = 1000; // refresh rate
        const int NumberOfSignals = 6; // signals chosen to analyze
        double[][] data = new double[NumberOfSignals][]; // incoming data
        const double secondtoSample = 1D/86400; // seconds to percent

        readonly ScottPlot.AxisPanels.LeftAxis[] axis = new ScottPlot.AxisPanels.LeftAxis[NumberOfSignals];

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
                Interval = interval
            };
            timer1.Start();
            timer1.Tick += timer1_Tick;
        }

        private void updateChart() // update array values and generate legend
        {
            // receive new data
            double[] newValue = new double[NumberOfSignals];

            for (int i = 0; i < NumberOfSignals; i++)
            {
                newValue[i] = generateData();
                data[i] = [.. data[i], newValue[i]];
            }

            // generate the signal
            ScottPlot.Plottables.Signal[] sig = new ScottPlot.Plottables.Signal[NumberOfSignals];

            formsPlot1.Plot.Clear();
            for (int i = 0; i < NumberOfSignals; i++)
            {
                if (!trace[i])
                {
                    sig[i] = formsPlot1.Plot.Add.Signal(data[i], secondtoSample, color: palette[i]);
                    sig[i].Axes.YAxis = axis[i];
                    sig[i].Data.XOffset = start;
                }
            }

            // increment x-axis
            dataX = [.. dataX, counter];
            counter += secondtoSample;
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
            formsPlot1.Plot.XLabel("Time");
            formsPlot1.Plot.Axes.Remove(Edge.Left);
            formsPlot1.Plot.Axes.Remove(Edge.Right);
            formsPlot1.Plot.Axes.Remove(Edge.Top);

            // handle n amount of axes
            for (int i = 0; i < NumberOfSignals;i++)
            {
                if (data[i] != null)
                {
                    axis[i] = formsPlot1.Plot.Axes.AddLeftAxis();
                    axis[i].Color(palette[i]);
                }
            }
            formsPlot1.Plot.Axes.DateTimeTicks(Edge.Bottom);
            formsPlot1.Refresh();
        }

        private void viewMode() // logic for view selection
        {
            if (view) // view last n
            {
                formsPlot1.Plot.Axes.AutoScaleY();
                formsPlot1.Plot.Axes.SetLimitsX(counter-(secondtoSample*points), counter);
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
    }
}
