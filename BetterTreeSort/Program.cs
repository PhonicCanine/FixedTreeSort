using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using TreeSort;

namespace BetterTreeSortTest
{
    
    class Program
    {
        static void Main(string[] args)
        {

            var numbers = new List<uint>();
            uint max = 1000000;
            for (uint i = 1; i < max; i++)
                for (uint x = 0; x < 1; x++)
                    numbers.Add(i);

            Random r = new Random();

            uint[] correctOrder = numbers.ToArray();

            for (int i = 0; i < numbers.Count; i++)
            {
                uint temp = numbers[i];
                int rand = (int)(r.NextDouble() * numbers.Count);
                numbers[i] = numbers[rand];
                numbers[rand] = temp;
            }

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();


            //When using massive arrays (like this example of 100,000,000 elements), the RAM usage in the linq implementation means a heavy performance hit on the second run of the algorithm

            TreeSortingProvider.init(1000000,128);
            Console.WriteLine("Initialised");

            uint[] nums = numbers.ToArray();
            s.Start();

            //maxValue: 1 specifies that the size of the tree should be calculated from the size of the largest item in the array. 0 uses the default (2^16), and any other uint will set the max value.
            //TreeSortingProvider.sort(nums,128,maxValue: 1);
            TreeSortingProvider.sortWithInPlaceTree(nums);
            s.Stop();

            Console.WriteLine("Took " + s.ElapsedMilliseconds + " ms to sort with tree");

            bool valid = true;
            for (int z = 0; z < nums.Length; z++)
            {
                if (nums[z] != correctOrder[z])
                {
                    valid = false;
                    Console.WriteLine("Expected " + correctOrder[z] + ". Got: " + nums[z]);
                }
            }

            if (valid)
            {
                Console.WriteLine("Sorting was valid.");
            }
            else
            {
                Console.WriteLine("Sorting was invalid.");
            }

            s.Reset();
                
            Thread.Sleep(500);

            nums = numbers.ToArray();

            s.Start();
            var ordered = nums.OrderBy((x) => x);
            nums = ordered.ToArray();
            s.Stop();

            ordered = null;

            Console.WriteLine("Took " + s.ElapsedMilliseconds + " ms to sort, with .orderby");

            valid = true;
            for (int z = 0; z < nums.Length; z++)
            {
                if (nums[z] != correctOrder[z])
                {
                    valid = false;
                    Console.WriteLine("Expected " + correctOrder[z] + ". Got: " + nums[z]);
                }
            }

            if (valid)
            {
                Console.WriteLine("Sorting was valid.");
            }
            else
            {
                Console.WriteLine("Sorting was invalid.");
            }

            Thread.Sleep(500);

            

            Console.ReadLine();
        }
    }
}
