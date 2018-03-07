using System.Linq;
using Yuka.Script.Data;

namespace Yuka.Script.Instructions {
	public abstract class Instruction {
		public readonly InstructionList InstructionList;

		protected Instruction(InstructionList instructionList) {
			InstructionList = instructionList;
		}
	}

	public class CallInstruction : Instruction {
		public DataElement.Func Function;
		public DataElement[] Arguments;

		public CallInstruction(DataElement.Func function, DataElement[] arguments, InstructionList instructionList) : base(instructionList) {
			Function = function;
			Arguments = arguments;
		}

		public string Name => Function.Name.StringValue;

		public override string ToString() => $"{Function.Name}({string.Join(", ", Arguments.Select(a => a.ToString()))})";
	}

	public class LabelInstruction : Instruction {
		public DataElement.Ctrl Label;

		public LabelInstruction(DataElement.Ctrl label, InstructionList instructionList) : base(instructionList) {
			Label = label;
		}

		public string Name => Label.Name.StringValue;

		public override string ToString() => $":{Label.Id}:{Label.Name}" + (Label.LinkedElement != null ? $" [{Label.LinkedElement.Id}]" : "");
	}

	public class TargetInstruction : Instruction {
		public DataElement Target;

		public TargetInstruction(DataElement target, InstructionList instructionList) : base(instructionList) {
			Target = target;
		}

		public override string ToString() => Target.ToString();
	}
}
