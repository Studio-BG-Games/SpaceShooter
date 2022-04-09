using UnityEngine;
using MaxyGames.OdinSerializer;


[assembly: RegisterFormatter(typeof(Vector3IntFormatter))]
namespace MaxyGames.OdinSerializer {
	using UnityEngine;

	/// <summary>
	/// Custom formatter for the <see cref="Vector3Int"/> type.
	/// </summary>
	/// <seealso cref="MinimalBaseFormatter{UnityEngine.Vector3Int}" />
	public class Vector3IntFormatter : MinimalBaseFormatter<Vector3Int> {
		private static readonly Serializer<int> DataSerializer = Serializer.Get<int>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Vector3Int value, IDataReader reader) {
			value.x = DataSerializer.ReadValue(reader);
			value.y = DataSerializer.ReadValue(reader);
			value.z = DataSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Vector3Int value, IDataWriter writer) {
			DataSerializer.WriteValue(value.x, writer);
			DataSerializer.WriteValue(value.y, writer);
			DataSerializer.WriteValue(value.z, writer);
		}
	}
}