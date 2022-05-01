using System;
using System.Collections.Generic;
using System.Threading;

namespace PdfProcessing.Entities
{
    public class PriorityLock
    {
        const int InvalidThreadId = -1;
        Dictionary<int, int> threadLockCountDictionary = new Dictionary<int, int>();

        class Item : IComparable
        {
            private int priority;
            private bool isHigh;

            public int Priority
            {
                get { return priority; }
                set { priority = value; }
            }
            private object tag;
            public Item(int priority, object tag, bool isHigh)
            {
                this.priority = priority;
                this.tag = tag;
                this.isHigh = isHigh;
            }

            public object Tag
            {
                get { return tag; }
            }

            public int CompareTo(object other)
            {
                var itemOthes = ((Item)other);
                int diference = ((Item)other).priority - this.priority;
                if ((itemOthes.isHigh && this.isHigh) || (!itemOthes.isHigh && !this.isHigh))
                {
                    return diference;
                }
                else if (itemOthes.isHigh && !this.isHigh)
                {
                    return 1;
                }
                else if (!itemOthes.isHigh && this.isHigh)
                {
                    return -1;
                }
                return 0;
            }
        }

        IPriorityQueue priorityQueue = new BinaryPriorityQueue();
        object queueLock = new object();

        bool isLockFree = true;
        bool isThreadScheduled = false;
        int threadHoldingLock = InvalidThreadId;

        public PriorityLock()
        {
        }

        public void Lock(int priority, bool isHigh)
        {
            object waitObject = null;

            bool waitForLock = true;
            lock (queueLock)
            {
                if ((isThreadScheduled == false && IsLockFree()) || CurrentThreadHoldsLock())
                {
                    SetCurrentThreadAsLockOwner();
                    waitForLock = false;
                }
                else
                {
                    waitObject = new object();
                    Item item = new Item(priority, waitObject, isHigh);
                    priorityQueue.Push(item);
                }
            }
            if (waitForLock)
            {
                lock (waitObject)
                {
                    Monitor.Wait(waitObject);
                    lock (queueLock)
                    {
                        isThreadScheduled = false;
                        SetCurrentThreadAsLockOwner();
                    }
                }
            }
        }

        public void Unlock()
        {
            Item nextItem = null;
            lock (queueLock)
            {
                if (!IsLockHeldByCallingThread())
                {
                    throw new InvalidOperationException("Cannot call Unlock as current thread has not called Lock.");
                }

                SetLockFree();

                if (IsLockFree() && priorityQueue.Count != 0)
                {
                    nextItem = (Item)priorityQueue.Pop();
                    isThreadScheduled = true;
                }
            }

            if (nextItem != null)
            {
                object waitObject = nextItem.Tag;
                lock (waitObject)
                {
                    Monitor.Pulse(waitObject);
                }
            }
        }

        private bool IsLockHeldByCallingThread()
        {
            return Thread.CurrentThread.ManagedThreadId == threadHoldingLock;
        }

        private bool IsLockFree()
        {
            return isLockFree;
        }

        private bool CurrentThreadHoldsLock()
        {
            return (threadHoldingLock == Thread.CurrentThread.ManagedThreadId);
        }

        private void SetCurrentThreadAsLockOwner()
        {
            threadHoldingLock = Thread.CurrentThread.ManagedThreadId;

            IncrementThreadLockCount();
            isLockFree = false;
        }

        private void SetLockFree()
        {
            int decrementedLockCount = DecrementLockCount();
            if (decrementedLockCount == 0)
            {
                threadHoldingLock = InvalidThreadId;
                isLockFree = true;
            }
        }

        private int IncrementThreadLockCount()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int lockCount = 0;
            threadLockCountDictionary.TryGetValue(currentThreadId, out lockCount);

            ++lockCount;
            threadLockCountDictionary[currentThreadId] = lockCount;
            return lockCount;
        }

        private int DecrementLockCount()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int lockCount = 0;
            threadLockCountDictionary.TryGetValue(currentThreadId, out lockCount);

            lockCount--;

            if (lockCount == 0)
            {
                threadLockCountDictionary.Remove(currentThreadId);
            }
            else if (lockCount > 0)
            {
                threadLockCountDictionary[currentThreadId] = lockCount;
            }
            else
            {
                throw new InvalidOperationException("Cannot call Unlock without corresponding call to Lock");
            }

            return lockCount;
        }
    }
}
