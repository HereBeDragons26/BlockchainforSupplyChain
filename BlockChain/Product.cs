using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain {
    public class Product {
        public List<Feature> Features { get; set; }

        public Product() {
            Features = new List<Feature>();
        }
    }
}
