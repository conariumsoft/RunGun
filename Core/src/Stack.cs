using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core
{
	public class Stack<T> : IEnumerable<T>
	{
		LinkedList<T> list = new LinkedList<T>();

		public void Push(T value) {
			list.AddFirst(value);
		}

		public T Pop() {
			if (list.Count == 0) {
				throw new InvalidOperationException("The stack is empty");
			}
			T value = list.First.Value;
			list.RemoveFirst();
			return value;
		}

		public T Peek() {
			if (list.Count == 0) {
				throw new InvalidOperationException("The stack is empty");
			}
			return list.First.Value;
		}

		public int Count() {
			return list.Count;
		}

		public IEnumerator<T> GetEnumerator() {
			return list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return list.GetEnumerator();
		}
	}
}
