using System;
using Yuka.IO.Formats;

namespace Yuka.Script.Syntax {
	public abstract class AssignmentTarget {
		public readonly AssignmentTargetType Type;

		protected AssignmentTarget(AssignmentTargetType type) {
			Type = type;
		}

		public class Variable : AssignmentTarget {
			public readonly string VariableType;
			public readonly int VariableId;

			public Variable(string variableType, int variableId) : base((AssignmentTargetType)Enum.Parse(typeof(AssignmentTargetType), variableType)) {
				VariableType = variableType;
				VariableId = variableId;
			}

			public override string ToString() => $"{VariableType}:{VariableId}";
		}

		public class SpecialString : AssignmentTarget {
			public readonly string Id;

			public SpecialString(string id) : base(AssignmentTargetType.SpecialString) {

				switch(id) {
					case YksFormat.TempGlobalString:
					case YksFormat.主人公:
					case YksFormat.汎用文字変数:
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(id), "Special string type must be one of the following: " +
							$"{YksFormat.TempGlobalString}, {YksFormat.主人公}, {YksFormat.汎用文字変数}");
				}
				Id = id;
			}

			public override string ToString() => Id;
		}

		public class Local : AssignmentTarget {
			public readonly uint Id;

			public Local(uint id) : base(AssignmentTargetType.Local) {
				Id = id;
			}

			public override string ToString() => $"${Id}";
		}

		public class VariablePointer : AssignmentTarget {
			public readonly string VariableType;
			public readonly int PointerId;

			public VariablePointer(string variableType, int pointerId) : base((AssignmentTargetType)Enum.Parse(typeof(AssignmentTargetType), variableType + "Pointer")) {

				switch(variableType) {
					case YksFormat.Flag:
					case YksFormat.String:
					case YksFormat.GlobalFlag:
					case YksFormat.GlobalString:
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(variableType), "Variable must be one of the following: " +
							$"{YksFormat.Flag}, {YksFormat.GlobalFlag}, {YksFormat.String}, {YksFormat.GlobalString}");
				}

				VariableType = variableType;
				PointerId = pointerId;
			}

			public override string ToString() => $"{VariableType}:&{PointerId}";
		}

		public class IntPointer : AssignmentTarget {
			public readonly int PointerId;

			public IntPointer(int pointerId) : base(AssignmentTargetType.IntPointer) {
				PointerId = pointerId;
			}

			public override string ToString() => $"&{PointerId}";
		}
	}

	public enum AssignmentTargetType {
		Flag,
		String,
		GlobalFlag,
		GlobalString,
		SpecialString,

		Local,

		IntPointer,
		FlagPointer,
		StringPointer,
		GlobalFlagPointer,
		GlobalStringPointer,
	}
}
