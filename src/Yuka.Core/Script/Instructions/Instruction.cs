using System.Linq;
using Yuka.Script.Data;

namespace Yuka.Script.Instructions {
	public abstract class Instruction { }

	public class CallInstruction : Instruction {
		public DataElement.Func Function;
		public DataElement[] Arguments;

		public CallInstruction(DataElement.Func function, DataElement[] arguments) {
			Function = function;
			Arguments = arguments;
		}

		public string Name => Function.Name;

		public override string ToString() => $"{Function.Name}({string.Join(", ", Arguments.Select(a => a.ToString()))})";
	}

	public class LabelInstruction : Instruction {
		public DataElement.Ctrl Label;

		public LabelInstruction(DataElement.Ctrl label) {
			Label = label;
		}

		public string Name => Label.Name;

		public override string ToString() => $":{Label.Name}";
	}

	public class TargetInstruction : Instruction {
		public DataElement Target;

		public TargetInstruction(DataElement target) {
			Target = target;
		}

		public override string ToString() => Target.ToString();
	}
}
