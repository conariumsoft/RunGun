using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using static RunGun.Core.Networking.TypeSerializer;
namespace RunGun.Core.Networking
{
	public struct FieldMetadata
	{
		public Type FieldType { get; set; }
		public string FieldName { get; set; }
		public int FieldBufferIndex { get; set; }
		public int FieldLength { get; set; }
	}


	public class SerializationProfile
	{
		public List<FieldMetadata> Fields;
		public int BufferLength;

		public SerializationProfile() {
			Fields = new List<FieldMetadata>();
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
	class Packet : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
	class StringSize : Attribute
	{
		public int Size { get; }
		public StringSize(int size) {
			Size = size;
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
	class Schema : Attribute
	{
		public int ManualSize { get; }
		public Schema(int size = 32) {
			ManualSize = size;
		}
	}

	public static class ClassSerializer
	{
		private static Dictionary<Type, SerializationProfile> profiles = new Dictionary<Type, SerializationProfile>();

		public static int GetTypeDataLength(Type t) {
			if (t.IsEnum)
				return 1;
			if (t == typeof(byte) || t == typeof(bool))
				return 1;
			if (t == typeof(char) || t == typeof(ushort) || t == typeof(short))
				return 2;
			if (t == typeof(int) || t == typeof(uint) || t == typeof(float))
				return 4;
			if (t == typeof(double))
				return 8;
			if (t == typeof(Guid))
				return 16;

			throw new Exception("fucking retard: "+t.ToString());
		}
		public static void GenerateProfile(Type type) {
			SerializationProfile profile = new SerializationProfile();

			FieldInfo[] fields = type.GetFields();

			int bufferOffset = 0;
			for (int i = 0; i < fields.Length; i++) {
				FieldInfo field = fields[i];

				Schema schema = (Schema)Attribute.GetCustomAttribute(field, typeof(Schema));

				if (schema == null)
					continue;

				Type fieldType = field.FieldType;

				int memberLength = (fieldType == typeof(string)) ? schema.ManualSize : GetTypeDataLength(fieldType);

				profile.Fields.Add(new FieldMetadata {
					FieldType = fieldType,
					FieldName = field.Name,
					FieldLength = memberLength,
					FieldBufferIndex = bufferOffset
				});

				bufferOffset += memberLength;
			}
			profile.BufferLength = bufferOffset;
			profiles.Add(type, profile);
		}
		public static SerializationProfile GetProfile(Type type) {
			if (!profiles.ContainsKey(type)) {
				GenerateProfile(type);
			}
			return profiles[type];
		}
		public static T Deserialize<T>(byte[] data) where T : new() {
			Type objType = typeof(T);
			if (!profiles.ContainsKey(objType)) {
				GenerateProfile(objType);
			}
			SerializationProfile profile = profiles[objType];

			T Toutput = new T();

			foreach (FieldMetadata field in profile.Fields) {
				Type fieldType = field.FieldType;

				byte[] bdata = new byte[field.FieldLength];

				Array.Copy(data, field.FieldBufferIndex, bdata, 0, field.FieldLength);
				object val = RawDataToObject(fieldType, bdata);
				objType.GetField(field.FieldName).SetValue(Toutput, val);
				//Console.WriteLine("{0} {1}", field.FieldName, val);
			}

			return Toutput;
		}

		public static byte[] Serialize<T>(T inst) {
			Type type = inst.GetType();

			if (!profiles.ContainsKey(type)) {
				GenerateProfile(type);
			}
			SerializationProfile profile = profiles[type];
			 
			byte[] data = new byte[profile.BufferLength];

			foreach (FieldMetadata field in profile.Fields) {
				object collect = type.GetField(field.FieldName).GetValue(inst);
				if (collect == null) {
					throw new Exception("Value of field "+field.FieldName + " in class instance "+type.Name + " was null");
				}
			
				byte[] typedata = ObjectToRawData(collect, field.FieldLength);

				Array.Copy(typedata, 0, data, field.FieldBufferIndex, field.FieldLength);
			}

			return data;
		}
	}
}
