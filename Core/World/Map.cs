using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RunGun.Core.World
{
	[Serializable]
	public class MapMetadata
	{
		public string Creator { get; set; } = "MapCreator";
		public string Name { get; set; } = "MapName";
		public MapMetadata() { }
	}

	[Serializable]
	public class Map
	{
		public MapMetadata Metadata { get; set; }
		public List<LevelGeometry> Geometry { get; set; }

		public Map() {
			Metadata = new MapMetadata();
			Geometry = new List<LevelGeometry>();
		}

		public static Map Load(string fileName) {
			using (var stream = System.IO.File.OpenRead(fileName)) {
				var serializer = new XmlSerializer(typeof(Map));
				return serializer.Deserialize(stream) as Map;
			}
		}

		public void Save(string fileName) {
			using (var writer = new System.IO.StreamWriter(fileName)) {
				var serializer = new XmlSerializer(this.GetType());
				serializer.Serialize(writer, this);
				writer.Flush();
			}
		}

		public void Draw(SpriteBatch spriteBatch) {
			for (int i = 0; i < Geometry.Count; i++) {
				Geometry[i].Draw(spriteBatch);
			}
		}
	}
}
