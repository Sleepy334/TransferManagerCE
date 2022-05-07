// (c) 2015 Yuri Tikhomirov @ VRARLab
// Source: https://gist.github.com/yuri-tikhomirov/6bb2cc1a04391451ba60
using System;
using System.Collections.Generic;

namespace TransferManagerCE.Util
{
	/*
		FastList<T> is a System.Collections.Generic.List<T> modification such, that allows to not produce garbage when using foreach. 
		Useful for old versions of c# runtime (like Mono 2.x), i.e. for Unity.
		It implements own instance-type enumerator and caches only one instance of this enumerator inside.
		Instance-type enumerator allows to avoid boxing that occurs when foreach converts IEnumerator to IDisposable and calls Dispose().
		(Default List<T> enumerator is value-type (struct), so when foreach converts to IDisposable, boxing occurs and 20 bytes of garbage will be collected.)
		Warnings:
		 * Not thread-safe! Will fail when enumerating the same FastList in different threads concurrently.
		 * Nested foreach on the same FastList (foreach (var x in list) { foreach (var y in list) {...} }) will still produce garbage since the only one enumerator instance is cached.
		 * Use at your own risk!
		 * Do not refactor your project with replacing List<T> with FastList<T> when you are close to release your app!
		Future plans:
		 * Rewrite List<T> from scratch and add some more optimizations like Clear() without Array.Clear().
		 * Visit Mars.
		(c) 2015 Yuri Tikhomirov @ VRARLab
	*/

		public class FastList<T> : List<T>
		{
			private CustomEnumerator enumerator = null;
			private bool isEnumeratorRetained = false;

			public FastList()
				: base()
			{
			}

			public FastList(int capacity)
				: base(capacity)
			{
			}

			public FastList(IEnumerable<T> collection)
				: base(collection)
			{
			}

			IEnumerator<T> RetainEnumerator()
			{
				if (enumerator == null)
				{
					enumerator = new CustomEnumerator(this);
					isEnumeratorRetained = true;
					return enumerator;
				}
				else
				{
					if (isEnumeratorRetained)
					{
						// sorry, this.enumerator is retained, 
						// so would produce base enumerator
						return base.GetEnumerator();
					}
					else
					{
						// not retained, so will just return an instance
						isEnumeratorRetained = true;
						// reset it with special public method
						enumerator.MyReset();
						return enumerator;
					}
				}
			}

			void ReleaseEnumerator(CustomEnumerator enumerator)
			{
				if (ReferenceEquals(enumerator, this.enumerator))
				{
					// the reference equals, so it is ok to release
					isEnumeratorRetained = false;
				}
			}

			// unfortunatelly, can't 'new' this since it conflicts with IEnumerable<T>.GetEnumerator()
			/*
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				UnityEngine.Debug.Log("GetEnumerator()");
				return RetainEnumerator();
			}
			*/

			List<T>.Enumerator GetBaseEnumerator()
			{
				return base.GetEnumerator();
			}

			new public System.Collections.Generic.IEnumerator<T> GetEnumerator()
			{
				return RetainEnumerator();
			}

			// Encapsulates struct System.Collections.Generic.List <T>.Enumerator to provide enumeration functional.
			// Defined as instance type to avoid boxing when coverting to IDisposable.
			// So this one won't produce GC in foreach statement.
			public class CustomEnumerator : IEnumerator<T>, System.Collections.IEnumerator
			{
				private FastList<T> list;
				private System.Collections.Generic.List<T>.Enumerator enumerator;

				public CustomEnumerator(FastList<T> list)
				{
					this.list = list;
					this.enumerator = list.GetBaseEnumerator();
				}

				public void MyReset()
				{
					this.enumerator = list.GetBaseEnumerator();
				}

				#region IEnumerator implementation
				bool System.Collections.IEnumerator.MoveNext()
				{
					return enumerator.MoveNext();
				}
				void System.Collections.IEnumerator.Reset()
				{
					// can't access to private Reset() method, so just re-create base enumerator
					this.enumerator = list.GetBaseEnumerator();
				}
				object System.Collections.IEnumerator.Current
				{
					get
					{
						return this.enumerator.Current;
					}
				}
				#endregion
				#region IDisposable implementation
				void IDisposable.Dispose()
				{
					// dispose base enumerator (actually, does nothing)
					this.enumerator.Dispose();
					// tell the list it is released
					this.list.ReleaseEnumerator(this);
				}
				#endregion
				#region IEnumerator implementation
				T System.Collections.Generic.IEnumerator<T>.Current
				{
					get
					{
						return this.enumerator.Current;
					}
				}
				#endregion
			}
		}

}