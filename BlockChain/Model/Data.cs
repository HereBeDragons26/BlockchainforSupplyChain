using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Model {
    public class Data {

        public List<long> ParentID { set; get; }

        public Product Product { set; get; }

        public string Author { set; get; }

        public Data() {
            ParentID = new List<long>();
            Product = new Product();
        }

        public override string ToString() {
            string ret = "\n\tParents: ";
            ParentID.ForEach(l => ret = ret + "\n\t\t" + l);
            ret = ret + "\n\tDescription: ";
            Product.Features.ForEach(f => ret = ret + "\n\t\t" + f.Description + " - " + f.Date);
            ret = ret + "\n\tAuthor: " + Author;
            return ret;
        }

    }
}
