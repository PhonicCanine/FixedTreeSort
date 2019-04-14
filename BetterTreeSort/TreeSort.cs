using System;
using System.Collections.Generic;
using System.Text;

namespace TreeSort
{
    class SearchTree
    {
        public SearchTree[] branches;
        public uint valuesHere = 0;
        public uint actualValue = 0;
        public uint maxValue = UInt16.MaxValue + 1;

        public SearchTree above;
        public uint valuesBelow;

        public SearchTree(int treeWidth = 128, SearchTree parent = null, uint max = (UInt16.MaxValue + 1))
        {
            branches = new SearchTree[treeWidth];
            above = parent;
            maxValue = max;
        }

        public void incrementValue(uint value)
        {
            valuesHere++;
            actualValue = value;
        }

        public void addValue(uint value, uint depth = 1)
        {
            uint divisor = (uint)(maxValue / Math.Pow(branches.Length, depth));

            if (divisor < 2)
            {
                uint bucket = (uint)(value % branches.Length);
                if (branches[bucket] == null)
                    branches[bucket] = new SearchTree(1, this, maxValue);
                branches[bucket].incrementValue(value);
                valuesBelow++;
            }
            else
            {
                uint divided = (uint)(value / divisor);

                uint bucket = (uint)(divided % branches.Length);
                if (branches[bucket] == null)
                {
                    branches[bucket] = new SearchTree(branches.Length, this, maxValue);
                }

                branches[bucket].addValue(value, depth + 1);
                valuesBelow++;
            }
        }

        public void initialise(int withWidth, int depth = 1)
        {
            uint divisor = (uint)(maxValue / Math.Pow(branches.Length, depth));

            if (divisor < 2)
            {
                for (int i = 0; i < branches.Length; i++)
                {
                    branches[i] = new SearchTree(1, this, maxValue);
                }
            }
            else
            {
                for (int i = 0; i < branches.Length; i++)
                {
                    branches[i] = new SearchTree(withWidth, this, maxValue);
                    branches[i].initialise(withWidth, depth + 1);
                }
            }
        }

        public SearchTree getNodeAt(uint value, uint depth = 1)
        {
            uint divisor = (uint)(maxValue / Math.Pow(branches.Length, depth));

            if (divisor < 2)
            {
                uint bucket = (uint)(value % branches.Length);
                if (branches[bucket] == null)
                    branches[bucket] = new SearchTree(1, this, maxValue);
                return branches[bucket];
                
            }
            else
            {
                uint divided = (uint)(value / divisor);

                uint bucket = (uint)(divided % branches.Length);
                if (branches[bucket] == null)
                {
                    branches[bucket] = new SearchTree(branches.Length, this, maxValue);
                }

                return branches[bucket].getNodeAt(value, depth + 1);
            }
        }
    }
    class TreeSortingProvider
    {
        static SearchTree staticTree;
        static SearchTree[] endNodes;

        public static void init(uint maxValue = 0, int treeWidth = 128)
        {
            if (maxValue == 0)
            {
                maxValue = (UInt16.MaxValue + 1);
            }
            else
            {
                for (uint i = 0; i < 32; i++)
                {
                    if (Math.Pow(2, i) > maxValue)
                    {
                        maxValue = (UInt32)Math.Pow(2, i);
                        break;
                    }
                }
            }

            staticTree = new SearchTree(treeWidth, max: maxValue);
            staticTree.initialise(treeWidth);

            endNodes = new SearchTree[maxValue];
            for (uint i = 0; i < maxValue; i++)
            {
                endNodes[i] = staticTree.getNodeAt(i);
            }

        }

        public static uint[] sortWithInPlaceTree(uint[] inArray, int treeWidth = 128)
        {

            var tree = staticTree;

            foreach (uint i in inArray)
            {
                //tree.addValue(i);
                endNodes[i].incrementValue(i);
            }

            int index = 0;

            uint searchTree(SearchTree node)
            {
                if (node == null || (index + 1) > inArray.Length)
                    return 0;

                uint op = node.valuesHere;

                while (0 < node.valuesHere)
                {
                    inArray[index] = node.actualValue;
                    index += 1;
                    node.valuesHere--;
                }

                

                foreach (var child in node.branches)
                    if (child != null)
                        op += searchTree(child);
                node.valuesBelow -= op;
                return op;

            }


            searchTree(tree);

            return inArray;
        }

        public static uint[] sort(uint[] inArray, int treeWidth = 128, uint maxValue = 1)
        {
            if (maxValue == 0)
            {
                maxValue = (UInt16.MaxValue + 1);
            }
            else if (maxValue == 1)
            {
                foreach (var itm in inArray)
                {
                    if (itm > maxValue)
                        maxValue = itm;
                }
                for (uint i = 0; i < 32; i++)
                {
                    if (Math.Pow(2, i) > maxValue)
                    {
                        maxValue = (UInt32)Math.Pow(2, i);
                        break;
                    }
                }
            }
            else
            {
                for (uint i = 0; i < 32; i++)
                {
                    if (Math.Pow(2, i) > maxValue)
                    {
                        maxValue = (UInt32)Math.Pow(2, i);
                        break;
                    }
                }
            }

            var tree = new SearchTree(treeWidth, max: maxValue);
            foreach (uint i in inArray)
            {
                tree.addValue(i);
            }

            int index = 0;

            void searchTree(SearchTree node)
            {
                if (node == null || (index + 1) > inArray.Length)
                    return;

                while (0 < node.valuesHere)
                {
                    inArray[index] = node.actualValue;
                    index += 1;
                    node.valuesHere--;
                }

                foreach (var child in node.branches)
                    if (child != null)
                        searchTree(child);

            }
            searchTree(tree);
            return inArray;
        }
    }
}
