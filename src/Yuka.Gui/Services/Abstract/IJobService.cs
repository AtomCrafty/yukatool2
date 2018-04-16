using Yuka.Gui.Jobs;

namespace Yuka.Gui.Services.Abstract {
	public interface IJobService : IService {
		void QueueJob(Job job);
	}
}
