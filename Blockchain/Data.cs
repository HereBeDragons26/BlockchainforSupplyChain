using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain {
    public class Data {

        public List<long> ParentID { set; get; }

        public Product Product { set; get; }

        public Data() {
            ParentID = new List<long>();
            Product = new Product();
        }

    }
}
