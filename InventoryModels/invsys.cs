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
                   LeadDaysDistribution[i].MaxRange = Decimal.ToInt32(LeadDaysDistribution[i].CummProbability * 100);
                   continue;

               }

               LeadDaysDistribution[i].CummProbability = LeadDaysDistribution[i - 1].CummProbability + LeadDaysDistribution[i].Probability;
               LeadDaysDistribution[i].MinRange = LeadDaysDistribution[i - 1].MaxRange + 1;
               LeadDaysDistribution[i].MaxRange = Decimal.ToInt32(LeadDaysDistribution[i].CummProbability * 100);


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
               demand_random.Add(r.Next(1, 101));
               leadday_random.Add(r.Next(1, 101));
           }
       }

       private void generate_table()
       {
           int cycle=1;
           int dayWithinCycle=1;
           int day = 1;
           int du = 0;
           SimulationTable = new List<SimulationCase>();
            Order order=new Order(StartLeadDays-1, StartOrderQuantity);
           for (int i = 0; i < NumberOfDays; i++)
           {
               SimulationTable.Add(new SimulationCase());
               SimulationTable[i].Day =day;
               SimulationTable[i].Cycle = cycle;
               SimulationTable[i].DayWithinCycle = dayWithinCycle;
                //begin inven
               if(i==0)
               {
                   SimulationTable[i].BeginningInventory = StartInventoryQuantity;
               }
               else
               {
                  SimulationTable[i].BeginningInventory = SimulationTable[i - 1].EndingInventory;
     
               }
               if(order.day<0&&!order.deliverd)
                {
                    SimulationTable[i].BeginningInventory += order.quantity;
                    order.deliverd = true;

                }

               SimulationTable[i].RandomDemand = demand_random[i];
               SimulationTable[i].Demand=find_demand(demand_random[i]);
               //ending inv

               SimulationTable[i].EndingInventory = SimulationTable[i].BeginningInventory - (SimulationTable[i].Demand);
                if (i != 0)
                    SimulationTable[i].EndingInventory -= SimulationTable[i - 1].ShortageQuantity;

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
                   SimulationTable[i].ShortageQuantity = (SimulationTable[i].Demand - SimulationTable[i].BeginningInventory) +(SimulationTable[i-1].ShortageQuantity);
                   SimulationTable[i].EndingInventory = 0;
              
                   }
               }

                
                //order qu
                if (dayWithinCycle == ReviewPeriod)
               {
                   SimulationTable[i].OrderQuantity = OrderUpTo - SimulationTable[i].EndingInventory + SimulationTable[i].ShortageQuantity;
                   SimulationTable[i].RandomLeadDays = r.Next(1,11);
                   SimulationTable[i].LeadDays = find_leadday(SimulationTable[i].RandomLeadDays);
                    order=new Order(SimulationTable[i].LeadDays, SimulationTable[i].OrderQuantity);
                    order.deliverd = false;
                }
                //d.u.o.a

                if (order.day >= 0)
                {
                    SimulationTable[i].orderArrival = order.day;
                    order.day = order.day - 1;
                }
                if (dayWithinCycle==ReviewPeriod)
               {
                   cycle++;
                   dayWithinCycle = 1;
               }
               else
               dayWithinCycle++;


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
