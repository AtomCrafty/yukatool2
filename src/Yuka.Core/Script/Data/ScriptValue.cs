namespace Yuka.Script.Data {
	public abstract class ScriptValue {
		public class Int : ScriptValue {
			public int IntValue;
			public int PointerId = -1;

			public bool IsPointer => PointerId != -1;

			public Int(int value) {
				IntValue = value;
			}

			public override string ToString() => IsPointer ? '&' + PointerId.ToString() : IntValue.ToString();
		}

		public class Str : ScriptValue {
			public string StringValue;

			public Str(string value) {
				StringValue = value;
			}
			public override string ToString() => StringValue;
		}
	}
}