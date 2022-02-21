using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InAl.Etrade.Models
{
    public class ShopingCardVM
    {
        public  IEnumerable<ShopingCard> ListCard{ get; set; }
        public OrderHeader OrderHeader { get; set; }

    }
}
