using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventoryModels;
using InventoryTesting;
using System.IO;
namespace InventorySimulation
{
    public partial class Form1 : Form
    {
        invsys system;
        List<string> tests;
        public Form1()
        {
            InitializeComponent();
            tests = new List<string>() {"TestCase1.txt"
                ,"TestCase2.txt",
                "TestCase3.txt",
                "TestCase4.txt"};
            List<string> t = new List<string>(){"1","2","3","4"};
            comboBox1.DataSource = t;
        }
        DataTable table = new DataTable();
        private void Form1_Load(object sender, EventArgs e)
        {
            table.Columns.Add("Day", typeof(int)); table.Columns.Add("Cycle", typeof(int));
            table.Columns.Add("DayWithinCycle", typeof(int)); table.Columns.Add("BeginningInventory", typeof(int));
            table.Columns.Add("RandomDemand", typeof(int)); table.Columns.Add("Demand", typeof(int));
            table.Columns.Add("EndingInventory", typeof(int)); table.Columns.Add("ShortageQuantity", typeof(int));
            table.Columns.Add("OrderQuantity", typeof(int)); table.Columns.Add("RandomLeadDays", typeof(int));
            table.Columns.Add("LeadDays", typeof(int)); table.Columns.Add("OrderArrival", typeof(int));
            dataGridView1.DataSource = table;  
               
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            system = new invsys();
            string file = comboBox1.SelectedItem.ToString();
            

            int tcase = Int32.Parse(file);

            system.start_simulation(tests[tcase-1]);
            string[] row;
            for(int i=0;i<system.NumberOfDays;i++)
            {
                row = new string[12];
                    row[0] = system.SimulationTable[i].Day.ToString();
                    row[1] = system.SimulationTable[i].Cycle.ToString();
                    row[2] = system.SimulationTable[i].DayWithinCycle.ToString();
                    row[3] = system.SimulationTable[i].BeginningInventory.ToString();
                    row[4] = system.SimulationTable[i].RandomDemand.ToString();
                    row[5] = system.SimulationTable[i].Demand.ToString();
                    row[6] = system.SimulationTable[i].EndingInventory.ToString();
                    row[7] = system.SimulationTable[i].ShortageQuantity.ToString();
                    row[8] = system.SimulationTable[i].OrderQuantity.ToString();
                    row[9] = system.SimulationTable[i].RandomLeadDays.ToString();
                    row[10] = system.SimulationTable[i].LeadDays.ToString();
                row[11] = system.SimulationTable[i].orderArrival.ToString();
                    table.Rows.Add(row);
                   
            }
            string msg = TestingManager.Test(system, tests[int.Parse(file)-1]);
            MessageBox.Show(msg);

        }
    }
}
