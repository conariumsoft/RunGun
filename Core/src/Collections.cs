using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core
{

	public class Disk<T> {
		T[] buffer;
		int size;

		public Disk(int count) {
			size = count;
			buffer = new T[size];
		}

		public T Get(int index) {
			return buffer[index % size];
		}

		public void Set(int index, T obj) {
			buffer[index % size] = obj;
		}
	}

}
