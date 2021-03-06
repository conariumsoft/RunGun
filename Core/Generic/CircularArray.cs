﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Generic
{
	public class CircularArray<T>
	{
		private T[] buffer;

		private int next = 0;

		public int Size { get; private set; }

		public CircularArray(int bufferSize) {
			Size = bufferSize;
			buffer = new T[Size];
		}

		public T this[int i] {
			get { return Get(i); }
			set { Set(i, value); }
		}

		public void Clear() {
			buffer = new T[Size];
		}

		public T Get(int index) {
			return buffer[index % Size];
		}

		public void Set(int index, T obj) {
			buffer[index % Size] = obj;
		}

		public void Next(T obj) {
			buffer[next % Size] = obj;
			next++;
		}

		public T[] GetBuffer() {
			return buffer;
		}
	}
}
