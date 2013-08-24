using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Strilanc.Value;

///<summary>A queue that returns the smallest items first.</summary>
public class AscendingPriorityQueue<T> : IEnumerable<T> {
    private readonly List<T> _items;
    private readonly IComparer<T> _comparer;
    public int Count { get { return _items.Count; } }

    private AscendingPriorityQueue(IComparer<T> comparer, IEnumerable<T> heapedItems) {
        this._comparer = comparer;
        this._items = heapedItems.ToList();
    }
    public AscendingPriorityQueue(IComparer<T> comparer = null)
        : this(comparer ?? Comparer<T>.Default, new List<T>()) {
    }

    ///<summary>Adds an item to the priority queue.</summary>
    public void Enqueue(T item) {
        // add to end of heap
        var i = _items.Count;
        _items.Add(item);

        // bubble up the heap
        while (i > 0) {
            var parentIndex = (i - 1) / 2;
            var parentItem = _items[parentIndex];
            if (_comparer.Compare(item, parentItem) >= 0) break;

            _items[i] = parentItem;
            _items[parentIndex] = item;
            i = parentIndex;
        }
    }

    ///<summary>
    ///Returns the smallest item in the priority queue.
    ///Returns no value if there are no items in the queue.
    ///</summary>
    public May<T> MayPeek() {
        if (Count == 0) return May.NoValue;
        return _items[0];
    }

    ///<summary>
    ///Removes and returns the smallest item from the priority queue.
    ///Returns no value and has no effect if there are no items in the queue.
    ///</summary>
    public May<T> MayDequeue() {
        if (Count == 0) return May.NoValue;
        var result = _items[0];

        // iteratively pull up smaller child until we hit the bottom of the heap
        var endangeredIndex = Count - 1;
        var endangeredItem = _items[endangeredIndex];
        var i = 0;
        while (true) {
            var childIndex1 = i * 2 + 1;
            var childIndex12 = i * 2 + 2;
            if (childIndex1 >= endangeredIndex) break;

            var smallerChildIndex = _comparer.Compare(_items[childIndex1], _items[childIndex12]) <= 0 ? childIndex1 : childIndex12;
            var smallerChild = _items[smallerChildIndex];
            var useEndangered = _comparer.Compare(endangeredItem, smallerChild) <= 0;
            if (useEndangered) break;

            _items[i] = smallerChild;
            i = smallerChildIndex;
        }

        // swap the item at the index to be removed into the new empty space at the bottom of heap
        _items[i] = endangeredItem;
        _items.RemoveAt(endangeredIndex);

        return result;
    }


    public IEnumerator<T> GetEnumerator() {
        var clone = new AscendingPriorityQueue<T>(_comparer, _items);
        while (true) {
            var x = clone.MayDequeue();
            if (!x.HasValue) yield break;
            yield return x.ForceGetValue();
        }
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Clear() {
        _items.Clear();
    }
}
