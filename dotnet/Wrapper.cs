using System;
using System.Threading;
using System.Collections.Generic;

namespace EdgeReference
{
	public class Wrapper
	{
		public Dictionary<int, object> referencesById { get; private set; }

		public Dictionary<object, int> idsByReference { get; private set; }

		private volatile int nextTemplateId = 0;

		public Wrapper ()
		{
			this.referencesById = new Dictionary<int, object> ();
			this.idsByReference = new Dictionary<object, int> ();
		}

		public int EnsureReference(object reference)
		{
			int id;
			if (!this.idsByReference.TryGetValue(reference, out id)) {
				id = Interlocked.Increment (ref this.nextTemplateId);
				this.referencesById.Add(id, reference);
				this.idsByReference.Add (reference, id);
			}

			// TODO: Proper return value.
			return -1;
		}


	}
}

