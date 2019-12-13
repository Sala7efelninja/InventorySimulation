using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace InventoryModels
{
   public class invsys : SimulationSystem
    {
       List<int> demand_random;

       List<int> leadday_random;
  
       Random r;

        public void start_simulation(string filename)
        {
            inputs(filename);
            calc_distrbution();
            generate_random();
            generate_table();
        }
       
        private void inputs(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            OrderUpTo = Int32.Parse(lines[1]);
            ReviewPeriod = Int32.Parse(lines[4]);
            StartInventoryQuantity = Int32.Parse(lines[7]);
            StartLeadDays = Int32.Parse(lines[10]);
            StartOrderQuantity = Int32.Parse(lines[13]);
            NumberOfDays = Int32.Parse(lines[16]);
            string[] temp;
            for (int i = 19; i < 24; i++)
            {

                temp = lines[i].Split(',');
                DemandDistribution.Add(new Distribution());
                DemandDistribution[i - 19].Value = Int32.Parse(temp[0]);
                DemandDistribution[i - 19].Probability = Decimal.Parse(temp[1]);

            }
            for (int i = 26; i < 29; i++)
            {
                temp = lines[i].Split(',');
                LeadDaysDistribution.Add(new Distribution());
                LeadDaysDistribution[i - 26].Value = Int32.Parse(temp[0]);
                LeadDaysDistribution[i - 26].Probability = Decimal.Parse(temp[1]);
            }

        }

        private void calc_distrbution()
        {
            DemandDistribution[0].MinRange = 1;
            LeadDaysDistribution[0].MinRange = 1;

           for(int i=0; i<5;i++)
           {
               if (i == 0)
               {
                   DemandDistribution[i].CummProbability = DemandDistribution[i].Probability;
                   DemandDistribution[i].MaxRange =Decimal.ToInt32( DemandDistribution[i].CummProbability * 100);

                   continue;
               }
               
               DemandDistribution[i].CummProbability = DemandDistribution[i - 1].CummProbability + DemandDistribution[i].Probability;
               DemandDistribution[i].MinRange = DemandDistribution[i-1].MaxRange + 1;
               DemandDistribution[i].MaxRange = Decimal.ToInt32(DemandDistribution[i].CummProbability * 100);


               if (DemandDistribution[i].MinRange == DemandDistribution[i].MaxRange)
                   DemandDistribution[i].MinRange = 0;
           }

           for(int i=0;i<3;i++)
           {
               if (i == 0)
               {
                   LeadDaysDistribution[i].CummProbability = LeadDaysDistribution[i].Probability;
                   LeadDaysDistribution[i].MaxRange = Decimal.ToInt32(LeadDaysDistribution[i].CummProbability * 10);
                   continue;

               }

               LeadDaysDistribution[i].CummProbability = LeadDaysDistribution[i - 1].CummProbability + LeadDaysDistribution[i].Probability;
               LeadDaysDistribution[i].MinRange = LeadDaysDistribution[i - 1].MaxRange + 1;
               LeadDaysDistribution[i].MaxRange = Decimal.ToInt32(LeadDaysDistribution[i].CummProbability * 10);


               if (LeadDaysDistribution[i].MinRange == LeadDaysDistribution[i].MaxRange)
                   LeadDaysDistribution[i].MinRange = 0;
               
           }
        }

       private void generate_random()
       {
           r = new Random();
           demand_random = new List<int>();
           leadday_random = new List<int>();
           for(int i=0;i<NumberOfDays;i++)
           {
               demand_random.Add(r.Next(1, 100));
               leadday_random.Add(r.Next(1, 10));
           }
       }

       private void generate_table()
       {
           int cycle=1;
           int d_w_c=1;
           int day = 1;
           int du = 0;
           SimulationTable = new List<SimulationCase>();
           for (int i = 0; i < NumberOfDays; i++)
           {
               SimulationTable.Add(new SimulationCase());
               SimulationTable[i].Day =day;
               SimulationTable[i].Cycle = cycle;
               SimulationTable[i].DayWithinCycle = d_w_c;
                //begin inven
               if(i==0)
               {
                   SimulationTable[i].BeginningInventory = StartInventoryQuantity;
               }
               else
               {
                   if(day %ReviewPeriod==0)
                   {
                       SimulationTable[i].BeginningInventory = SimulationTable[i-1].OrderQuantity;
                   }
                   else
                  SimulationTable[i].BeginningInventory = SimulationTable[i - 1].EndingInventory; 
               

                   
               }

               SimulationTable[i].RandomDemand = demand_random[i];
               SimulationTable[i].Demand=find_demand(demand_random[i]);
               //ending inv

               SimulationTable[i].EndingInventory = SimulationTable[i].BeginningInventory - (SimulationTable[i].Demand + SimulationTable[i].ShortageQuantity);

               //sho
               if (SimulationTable[i].EndingInventory < 0)
               {
                   if(i==0)
                   {
                   SimulationTable[i].ShortageQuantity = SimulationTable[i].EndingInventory * -1;
                   SimulationTable[i].EndingInventory = 0;  
                   }
                   else
                   {
                   SimulationTable[i].ShortageQuantity = (SimulationTable[i].EndingInventory * -1)+(SimulationTable[i-1].ShortageQuantity);
                   SimulationTable[i].EndingInventory = 0;
              
                   }
               }
               //order qu
               if (d_w_c == ReviewPeriod)
               {
                   SimulationTable[i].OrderQuantity = OrderUpTo - SimulationTable[i].EndingInventory + SimulationTable[i].ShortageQuantity;
                   SimulationTable[i].RandomLeadDays = leadday_random[i];
                   SimulationTable[i].LeadDays = find_leadday(leadday_random[i]);
               }
               //d.u.o.a

               if(d_w_c==ReviewPeriod)
               {
                   cycle++;
                   d_w_c = 1;
               }
               else
               d_w_c++;


               day++;
           }
       }


       private int find_demand(int random)
       {
           int demand = 0;

           for (int i = 0; i < 5; i++)
           {
               if (random >= DemandDistribution[i].MinRange && random <= DemandDistribution[i].MaxRange)
               {
                   demand = DemandDistribution[i].Value;
                   break;
               }
           }

           return demand;
       }
       private int find_leadday(int random)
       {
           int leadday = 0;

           for (int i = 0; i < 3; i++)
           {
               if (random >= LeadDaysDistribution[i].MinRange && random <= LeadDaysDistribution[i].MaxRange)
               {
                   leadday = LeadDaysDistribution[i].Value;
                   break;
               }
           }

           return leadday;
       }
    }
}
