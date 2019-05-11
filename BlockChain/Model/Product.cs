using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Model {
    public class Product {
        public List<Feature> Features { get; set; }

        public Product() {
            Features = new List<Feature>();
        }
    }
}
