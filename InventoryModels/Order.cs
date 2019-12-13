using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModels
{
    class Order
    {
        public Order(int day,int quantity)
        {
            this.day = day;
            this.quantity = quantity;
        }
        public int day { get; set; }

        public int quantity { get; set; }
        public bool deliverd { get; set; }

    }
}
