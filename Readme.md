# FixedTreeSort

## A questionable sorting algorithm for repetetive integer data-sets / multiple sorts of datasets with similar ranges

### Should I use this?

Probably not. Unless you have:
* A data-set that can be simplified to only unsigned integers (and a good amount of RAM)
* A very repetetive, long dataset
* The need to sort multiple lists (which fit in the same bounds) / sort the same list multiple times

### Details

Have you ever needed to sort arbitrarily long lists of highly repetetive unsigned integers with a known range, and been bottlenecked by traditional `O(n log[2] n)` time efficiency? Do you have lots of RAM available?

If so, this is the non-comparrative sorting algorithm for you.

Trade space efficiency for time efficiency.

How does this work, you may wonder -

Let's take a limited example, with a tree width of 2.

`[3,2,3,0]`

Well, we go through that array once, and form a tree:

```
         tree (max = 4, branches = 2)
    
        /    \
    tree      tree
   /    \    /    \
 tree  null  tree  tree
  |          |     |
  1          1     2
_
v|
a|
l|0     1    2     3
u|
e|  
```

To add to the tree, the efficiency is *always* going to be proportional to:
`log[width] max`, and this has to happen `n` times for an `n` item array.

That said, due to the mechanics of RAM allocation, and implementation, real life efficiency is usually going to be a little worse than that.

Now we have a tree, we just need to piece it back together:

We search through all the non-null nodes in the tree, and place the element back into its proper position in the array.

This is obviously a stupid solution to sorting an array. However, the property of this sorting algorithm means it can greatly outperform the best comparrative sorting algorithms in some circumstances:

Take for instance, an array in C# generated with the following:
```CS
var numbers = new List<uint>();
uint max = 10000;
for (uint i = 1; i < max; i++) //generate a 100,000,000 element array, with 10,000 distinct elements, each with 10,000 duplicates
    for (uint x = 0; x < 10000; x++)
        numbers.Add(i);

Random r = new Random();

uint[] correctOrder = numbers.ToArray();

for (int i = 0; i < numbers.Count; i++) //randomly reorder array
{
    uint temp = numbers[i];
    int rand = (int)(r.NextDouble() * numbers.Count);
    numbers[i] = numbers[rand];
    numbers[rand] = temp;
}
```
Obviously, this is an abnormal example, however the performance difference here, using a tree-width of 128 is pretty apparrent:
```CS
TreeSortingProvider.sort(nums,maxValue: 1);
```
takes only `8877 ms` to sort the whole array.

```CS
var ordered = nums.OrderBy((x) => x).ToArray(); //behind the scenes, this is an introsort (quicksort + heapsort)
```
on the other hand, takes a whopping `67537 ms`.

With this example (due to all the duplicates), there are also large gains in memory use with FixedTreeSort; here, FixedTreeSort uses `1.3GB` of RAM (Including storing two copies of the original array), according to the Visual Studio memory profiler.

By contrast, `.OrderBy((x) => x).ToArray();` uses `2.3` GB of RAM.

Now, you may be wondering how it fares with more numbers and less repetition: to be clear, not as well. Not by a long shot. With no repetition, performance is a little worst than half as good as with `.OrderBy`.

`1013 ms` vs `412 ms` for a 1,000,000 element array.

However, all is not lost! As the number of items in the array go up, this gap does in fact shorten.

`90000 ms` vs `60000 ms` is typical for 100,000,000 elements.

Furthermore, for the 1,000,000 element array, only `61` out of `1013` ms were spent traversing the tree to produce the output array. This means that in the future, this code could be greatly improved by allowing the structure of the tree to be pre-initialised for a specific integer size (most of the time lost is to making empty arrays with many elements).

It would also be easy to parallelise the tree formation (although this felt like cheating against a non-parallel `.OrderBy`).

The light at the end of the tunnel for this algorithm is this: If you initialise the tree first (with `TreeSortingProvider.init(1000000,128);`), subsequent sorts will be much faster than the C# default implementation.

```C#
TreeSortingProvider.sortWithInPlaceTree(nums);
```

Takes only `602` ms to sort a non-repetetive randomised array containing all numbers from 1 - 2,000,000 (and any other array with the same bounds), while OrderBy takes `620` ms.

It's worth noting finally, that RAM usage scales based on the width of the tree, and for a `100,000,000` element pre-baked tree, that's well over `20GB` of RAM.

## Where to from here

Well, as it turns out, just making an array the length of the maximum value then placing counts in each number that is inside the array to be sorted is faster than FixedTreeSort, and faster than introsort.