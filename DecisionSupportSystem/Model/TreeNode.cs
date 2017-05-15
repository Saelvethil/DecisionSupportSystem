using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionSupportSystem.Model
{
    public class TreeNode
    {
        public TreeNode(bool isLeaf, int attribute, int value)
        {
            IsLeaf = isLeaf;
            Attribute = attribute;
            Value = value;
            Class = String.Empty;
            Children = new List<TreeNode>();
            ClassCountLeaf = new Dictionary<string, int>();
        }

        public bool IsLeaf { get; set; }
        public int Attribute { get; set; }
        public int Value { get; set; }
        public string Class { get; set; }
        public List<TreeNode> Children { get; set; }
        public Dictionary<string, int> ClassCountLeaf { get; set; }

        public override string ToString()
        {
            return "IsLeaf: " + IsLeaf + ", Attribute: " + Attribute + ", Value: " + Value + ", Class: " + Class;
        }
    }
}
