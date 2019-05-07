using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain {
    public class Data {

        public List<long> ProductID { set; get; }

        public Product product { set; get; }

        public Data() {
            ProductID = new List<long>();
            product = new Product();
        }

    }
}
