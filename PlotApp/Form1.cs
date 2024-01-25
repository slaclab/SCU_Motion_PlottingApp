using ScottPlot;

namespace PlotApp
{
    public partial class PlotApp : Form
    {
        int counter = 0; // program counter ticks at sample rate
        int points = 10; // number of previous points to be plotted
        int interval = 100; // refresh rate
        const int NumberOfSignals = 6; // signals chosen to analyze
        double[][] data = new double[NumberOfSignals][]; // incoming data

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

            ScottPlot.Color[] palette =
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

            formsPlot1.Plot.Clear();
            for (int i = 0; i < NumberOfSignals; i++)
            {
                if (trace[i] == false)
                {
                    sig[i] = formsPlot1.Plot.Add.Signal(data[i]);
                    sig[i].Color = palette[i];
                }
            }

            // increment x-axis
            //dataX = dataX.Append(counter).ToArray();
            dataX = [.. dataX, counter];
            counter++;
        }

        private void timer1_Tick(object sender, EventArgs e) // callback performed at tick rate
        {
            // update chart and view mode
            updateChart();

            // stop refreshing if pause is toggled (update ScottPlot when released and use renderLock)
            if (pause == false)
            {
                formsPlot1.Refresh();
                viewMode();
            }
        }

        private void CreateChart() // set plot labels
        {
            formsPlot1.Plot.XLabel("Time");
            formsPlot1.Plot.YLabel("Position");
            formsPlot1.Plot.Title("6 Trace Plot");
            formsPlot1.Refresh();
        }

        private void viewMode() // logic for view selection
        {
            if (view == true) // view last n
            {
                formsPlot1.Plot.Axes.AutoScaleY();
                formsPlot1.Plot.Axes.SetLimitsX(counter - points, counter); // FIX:: based on time not samples
            }
            else // auto scale
            {
                formsPlot1.Plot.Axes.AutoScale();
            }
        }

        private double generateData() // return random value
        {
            Random rand = new();
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
