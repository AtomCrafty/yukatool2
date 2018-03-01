namespace Yuka.Script {
	public abstract class AssignmentTarget {
		public readonly AssignmentTargetType Type;

		protected AssignmentTarget(AssignmentTargetType type) {
			Type = type;
		}

		public abstract class IdAssignmentTarget : AssignmentTarget {
			public readonly int Id;

			protected IdAssignmentTarget(int id, AssignmentTargetType type) : base(type) {
				Id = id;
			}

			public override string ToString() => $"[{GetType().Name}:{Id}]";
		}

		public class LocalFlag : IdAssignmentTarget {
			public LocalFlag(int id) : base(id, AssignmentTargetType.LocalFlag) { }
		}

		public class GlobalFlag : IdAssignmentTarget {
			public GlobalFlag(int id) : base(id, AssignmentTargetType.GlobalFlag) { }
		}

		public class LocalString : IdAssignmentTarget {
			public LocalString(int id) : base(id, AssignmentTargetType.LocalString) { }
		}

		public class GlobalString : IdAssignmentTarget {
			public GlobalString(int id) : base(id, AssignmentTargetType.GlobalString) { }
		}

		public class SpecialString : AssignmentTarget {
			public readonly string Id;

			public SpecialString(string id) : base(AssignmentTargetType.SpecialString) {
				Id = id;
			}

			public override string ToString() => $"[{GetType().Name}:{Id}]";
		}

		public class Local : AssignmentTarget {
			public readonly uint Id;

			public Local(uint id) : base(AssignmentTargetType.Local) {
				Id = id;
			}

			public override string ToString() => $"[{GetType().Name}:{Id}]";
		}
	}

	public enum AssignmentTargetType {
		GlobalFlag,
		LocalFlag,
		GlobalString,
		LocalString,
		SpecialString,
		Local
	}
}
