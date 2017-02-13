using System;
using System.Threading;
using System.Collections.Generic;
using System.Globalization;

namespace EdgeReference
{
	public class ReferenceManager : MarshalByRefObject
	{
		public Dictionary<int, object> referencesById { get; private set; }

		public Dictionary<object, int> idsByReference { get; private set; }

		private static ReferenceManager instance;

		private static object instanceLock = new object();

		private volatile int nextTemplateId = 0;

		protected ReferenceManager()
		{
			this.referencesById = new Dictionary<int, object> ();
			this.idsByReference = new Dictionary<object, int> ();
		}

		/// <summary>
		/// Gets or sets the single instance of wrapper in this AppDomain.
		/// </summary>
		/// <value>The single instance of wrapper in this AppDomain.</value>
		public static ReferenceManager Instance {
			get {
				if (instance == null) {
					lock (instanceLock) {
						if (instance == null) {
							instance = new ReferenceManager ();
						}
					}
				}

				return instance;
			}

			set {
				instance = value;
			}
		}

		/// <summary>
		/// Initializes the lifetime service.
		/// </summary>
		/// <returns>The lifetime service.</returns>
		public override object InitializeLifetimeService ()
		{
			return null;
		}

		public int EnsureReference(object reference)
		{
			int id;
			if (!this.idsByReference.TryGetValue(reference, out id)) {
				id = Interlocked.Increment (ref this.nextTemplateId);
				this.referencesById.Add(id, reference);
				this.idsByReference.Add (reference, id);
			}

			return id;
		}

		public object PullReference(int id)
		{
			object reference;
			if (this.referencesById.TryGetValue(id, out reference)) {
				return reference;
			}

			string message = string.Format (
				CultureInfo.InvariantCulture,
				"Reference not found for id {0}",
				id);

			throw new InvalidOperationException (message);
		}

	}
}

