using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace simulation.BL
{
    class QueueSimulation
    {
        private int num_delays_required;
        private int Queue_size;
        private int next_event_type =0; //1 or 2
        private int num_custs_delayed;
        private int num_in_q;
        private int server_status;
        private double area_num_in_q;
        private double area_server_status;
        private double total_of_delays;
        private double mean_interarrival = 1;
        private double mean_service = 0.5;
        private double sim_time;
        private double time_last_event;

        private double[] time_arrival;
        private double[] time_next_event = new double[3];
        private Random random = new Random(10000);
        mainForm form;
        private bool stopRequested = false;
        string carrant_event;

        public QueueSimulation(int num_delays_required, int Queue_size, mainForm form)
        {
            this.form = form;
            this.num_delays_required = num_delays_required;
            this.Queue_size = Queue_size;
            this.time_arrival = new double[Queue_size];
            this.sim_time = 0;
            this.server_status = 0;
            this.num_in_q = 0;
            this.time_last_event = 0;
            this.num_custs_delayed = 0;
            this.total_of_delays = 0;
            this.area_num_in_q = 0;
            this.area_server_status = 0;
            this.time_next_event[1] = sim_time + expon(mean_interarrival);
            this.time_next_event[2] = 1.0e+30; //10^30
        }

        public void Start()
        {
            Thread loopThread = new Thread(() =>
            {
                while (!stopRequested && num_custs_delayed < num_delays_required)
                {
                    time_last_event = time_next_event.Skip(1).Take(2).Min();
                    if (next_event_type != 0)
                        carrant_event = next_event_type == 1 ? "arrive time = "+ time_last_event.ToString("0.00") : "depart time = " + time_last_event.ToString("0.00");
                    timing();

                    update_time_avg_stats();
                    switch (next_event_type)
                    {
                        case 1:
                            arrive();
                            break;
                        case 2:
                            depart();
                            break;
                    }


                    UpdateMainFormThread();
                    Thread.Sleep(1000);

                }
                if (!stopRequested)
                    report();
                
            });


            loopThread.Start();

            
        }

        public void Stop()
        {
            stopRequested = true;
            report();
        }

        private void UpdateMainFormThread()
        {
            if (form != null)
            {
                form.Invoke((MethodInvoker)delegate {


                    form.carrantEvent.Text = carrant_event;

                    // System state
                    form.serverStatus.Text = server_status.ToString();
                    form.numberInQueue.Text = num_in_q.ToString();
                    form.timeOfArrivalPanel.Controls.Clear();
                    for (int i = time_arrival.Length - 1; i != 0; i--)
                    {
                        if (time_arrival[i] != 0)
                        {
                            Label newLabel = new Label();
                            newLabel.BackColor = System.Drawing.SystemColors.Window;
                            newLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                            newLabel.Dock = System.Windows.Forms.DockStyle.Top;
                            newLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
                            newLabel.Size = new System.Drawing.Size(111, 24);
                            newLabel.TabIndex = 51;
                            newLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                            newLabel.Name = "timeOfArrivalLabel" + i;
                            newLabel.Text = time_arrival[i].ToString("0.00");
                            form.timeOfArrivalPanel.Controls.Add(newLabel);
                        }
                    }
                    form.timeOfLastEvent.Text = time_last_event.ToString("0.00");


                    //clock
                    form.clock.Text = sim_time.ToString("0.00");
                    form.arriveEvent.Text = time_next_event[1].ToString("0.00");
                    form.departEvent.Text = time_next_event[2] == 1.0e+30 ? "@" : time_next_event[2].ToString("0.00");


                    // Statistical counter
                    form.numberDelayed.Text = num_custs_delayed.ToString();
                    form.totalDelay.Text = total_of_delays.ToString("0.00");
                    form.areaUnderQ.Text = area_num_in_q.ToString("0.00");
                    form.areaUnderB.Text = area_server_status.ToString("0.00");
                });
            }
        }

        void timing()
        {
            if (time_next_event[1] < time_next_event[2])
                next_event_type = 1;//arrival
            else
                next_event_type = 2; //departure
            sim_time = time_next_event[next_event_type];
        }

        void update_time_avg_stats()
        {
            double lag;
            //sim_time: clock, time of current event
            lag = time_last_event - time_arrival[num_in_q];

            //area under Q(t)
            area_num_in_q += num_in_q * lag;

            //area under B(t)
            area_server_status += server_status * lag;

        }
        void arrive()
        {

            double delay;
            time_next_event[1] = sim_time + expon(mean_interarrival);

            if (server_status == 1)
            {
                num_in_q++;
                time_arrival[num_in_q] = sim_time;
            }
            else
            {
                delay = 0;
                total_of_delays += delay;
                num_custs_delayed++;
                server_status = 1;
                time_next_event[2] = sim_time + expon(mean_service);
            }
        }

        void depart()
        {
            double delay;
            if (num_in_q == 0)
            {
                server_status = 0;
                time_next_event[2] = 1.0e+30;
            }
            else
            {
                num_in_q--;
                delay = sim_time - time_arrival[num_in_q];
                total_of_delays += delay;

                num_custs_delayed++;
                time_next_event[2] = sim_time + expon(mean_service);

                for (int i = 0; i <= num_in_q; i++)
                {

                    time_arrival[i] = time_arrival[i + 1];
                    time_arrival[i + 1] = 0;
                }



            }
        }

        void report()
        {

            StringBuilder report = new StringBuilder();
            report.AppendLine("Total customers served: " + num_custs_delayed);
            report.AppendLine("Average delay in queue (minutes): " + (total_of_delays / num_custs_delayed).ToString("0.00"));
            report.AppendLine("Average number in queue: " + (area_num_in_q / sim_time).ToString("0.00"));
            report.AppendLine("Server utilization: " + (area_server_status / sim_time).ToString("0.00"));

            MessageBox.Show(report.ToString(), "Simulation Report");




        }
        double expon(double mean)
        {
            return -mean * Math.Log(random.NextDouble());
        }
    }
}
