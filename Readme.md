# FixedTreeSort (A journey through non-comparrative sorting)

## A questionable sorting algorithm for repetetive integer data-sets / multiple sorts of datasets with similar ranges

### Should I use this?

Probably not. Unless you have:
* A data-set that can be simplified to only unsigned integers (and a good amount of RAM)
* A very repetetive, long dataset
* The need to sort multiple lists (which fit in the same bounds) / sort the same list multiple times

TL;DR:
ArraySort is better than FixedTreeSort and IntroSort. fastSort (at the bottom of this) will automate the decision of algorithm for optimal time.

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

Takes only `549` ms to sort a non-repetetive randomised array containing all numbers from 1 - 2,000,000 (and any other array with the same bounds), while OrderBy takes `630` ms.

It's worth noting finally, that RAM usage scales based on the width of the tree, and for a `100,000,000` element pre-baked tree, that's well over `20GB` of RAM.

## Where to from here

Well, as it turns out, just making an array the length of the maximum value then placing counts in each number that is inside the array to be sorted is faster than FixedTreeSort, and faster than introsort.

```CS
static void arraySort(uint[] array)
{
    uint max = 0;
    uint min = uint.MaxValue;
    for (int i = 0; i < array.Length; i++)
    {
        if (array[i] > max)
            max = array[i];
        if (array[i] < min)
            min = array[i];
    }
    

    uint[] sortArray = new uint[(max + 1) - min];

    for (int i = 0; i < array.Length; i++)
        sortArray[array[i] - min]++;

    int idx = 0;
    for (uint i = 0; i < sortArray.Length; i++)
        for (uint x = 0; x < sortArray[i]; x++)
            array[idx++] = min + i;
}
```

```
                                +-----------------------------------------+
                                |  ArraySort | FixedTreeSort* |   .OrderBy|
+-------------------------------|-----------------------------------------|
| 2m rand. items, no repeats    |       16ms |      590ms     |     638ms |
+-------------------------------|-----------------------------------------|
| 1m rand. items, no repeats    |        7ms |      290ms     |     277ms |
+-------------------------------|-----------------------------------------|
| 500k rand. items, 1k repeats  |     1978ms |   101256ms     |  411860ms |
+-------------------------------+-----------------------------------------+

* - with tree sort, the tree was pre-initialised (this would matter most for the first two tests)
```

So, what's the lesson in this?

Well, if you have a range-limited dataset (Such that range < n * log2(n)) (or lots of RAM), ArraySort is almost always going to be the best option.

```CS
static void fastSort(uint[] array)
{
  uint max = 0;
  uint min = uint.MaxValue;
  uint maxBuckets = UInt16.MaxValue * 16;
  for (int i = 0; i < array.Length; i++)
  {
      if (array[i] > max)
          max = array[i];
      if (array[i] < min)
          min = array[i];
  }

  if ((max - min) < Math.Min(array.Length * (Math.Log10(array.Length) / Math.Log10(2)), maxBuckets))
    {
        uint[] sortArray = new uint[(max + 1) - min];

        for (int i = 0; i < array.Length; i++)
            sortArray[array[i] - min]++;

        int idx = 0;
        for (uint i = 0; i < sortArray.Length; i++)
            for (uint x = 0; x < sortArray[i]; x++)
                array[idx++] = min + i;
    }
    else
    {
        array = array.OrderBy((x) => x).ToArray();
    }
}
```

will do that calculation for you. All in all, `fastSort` will result in best case performance of `O(n)` and worst case performance of `O(n log n)`.