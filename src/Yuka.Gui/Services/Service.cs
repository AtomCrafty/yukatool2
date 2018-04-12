using System.Collections.Generic;
using System.Linq;
using Yuka.Gui.Services.Abstract;

namespace Yuka.Gui.Services {
	public static class Service {
		private static readonly List<IService> Services = new List<IService>();

		public static void Register(IService service) {
			Services.Add(service);
		}

		public static T Get<T>() where T : class, IService {
			return Services.FirstOrDefault(service => service is T) as T;
		}
	}
}
