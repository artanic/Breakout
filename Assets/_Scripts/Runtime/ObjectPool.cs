using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Concurrent;

//Pool<MemoryStream> msPool = new Pool<MemoryStream>(() => new MemoryStream(2048), pms => {
//    pms.Position = 0;
//    pms.SetLength(0);
//}, 500);

namespace Discode.Breakout
{
	/// <summary>
	/// Represents a pool of objects that we can pull from in order
	/// to prevent constantly reallocating new objects. This collection
	/// is meant to be fast, so we limit the "lock" that we use and do not
	/// track the instances that we hand out.
	/// </summary>
	/// <typeparam name="T">The type of object in the pool.</typeparam>
	public sealed class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> _objects;
        private readonly Func<T> _objectGenerator;

        public ObjectPool(Func<T> objectGenerator)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _objects = new ConcurrentBag<T>();
        }

        public T Get() => _objects.TryTake(out T item) ? item : _objectGenerator();

        public void Return(T item) => _objects.Add(item);
    }
}
