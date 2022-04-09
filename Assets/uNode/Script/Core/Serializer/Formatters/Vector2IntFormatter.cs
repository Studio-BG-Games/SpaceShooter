using UnityEngine;
using MaxyGames.OdinSerializer;


[assembly: RegisterFormatter(typeof(Vector2IntFormatter))]
namespace MaxyGames.OdinSerializer {
	using UnityEngine;

	/// <summary>
	/// Custom formatter for the <see cref="Vector2Int"/> type.
	/// </summary>
	/// <seealso cref="MinimalBaseFormatter{UnityEngine.Vector2Int}" />
	public class Vector2IntFormatter : MinimalBaseFormatter<Vector2Int> {
		private static readonly Serializer<int> DataSerializer = Serializer.Get<int>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Vector2Int value, IDataReader reader) {
			value.x = DataSerializer.ReadValue(reader);
			value.y = DataSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Vector2Int value, IDataWriter writer) {
			DataSerializer.WriteValue(value.x, writer);
			DataSerializer.WriteValue(value.y, writer);
		}
	}
}