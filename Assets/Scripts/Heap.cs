using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T> {
    T[] items;
    int currentItemCount;

    public Heap(int maxHeaspSize) { items = new T[maxHeaspSize]; }

    public void Add(T item) {
        item.HeapIndex = currentItemCount;
        items[currentItemCount++] = item;
        SortUp(item);
    }

    public T RemoveFirst() {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }
    public void UpdateItem(T item) { 
        SortUp(item);
        
    }
    public int Count { get { return currentItemCount; } }
    public bool Contains(T item) { return Equals(items[item.HeapIndex], item); }
    void SortDown(T item) {
        while (true) {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount) {
                swapIndex = childIndexLeft;
                if (childIndexRight < currentItemCount && items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) swapIndex = childIndexRight;
                if (item.CompareTo(items[swapIndex]) < 0) Swap(item, items[swapIndex]);
                else return;
            } else return;
        }
    }
    void SortUp(T item) {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true) {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0) Swap(item, parentItem);
            else break;
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T A, T B) {
        items[A.HeapIndex] = B;
        items[B.HeapIndex] = A;
        int AIndex = A.HeapIndex;
        A.HeapIndex = B.HeapIndex;
        B.HeapIndex = AIndex;

    }
}

public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex { get; set; }
}