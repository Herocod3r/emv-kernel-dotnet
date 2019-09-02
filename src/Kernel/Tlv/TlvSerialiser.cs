using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Kernel.Utils;

namespace Kernel.Tlv
{
    public static class TlvSerializer
    {

        public static T Deserialize<T>(Tlv value,string tag = null) where T:class
        {
            if(tag != null && value.ContainsKey(Convert.ToInt32(tag, 16))) value = new Tlv(value[Convert.ToInt32(tag, 16)]);
            var obj = Deserialize(value,typeof(T));
            return (T) obj;
        }
        

        private static object Deserialize(Tlv value, Type type)
        {
            if(type is null || value is null) throw new ArgumentNullException("Provide an item and tlv");
            var properties = type.GetProperties().Where(x => x.CustomAttributes.Any(y=>y.AttributeType == typeof(TlvPropertyAttribute)));
            var item = Activator.CreateInstance(type);
            foreach (var property in properties)
            {
                SetValue(property,ref item,value);
            }

            return item;
        }
        

        private static void SetValue(PropertyInfo property,ref object item,Tlv tlv)
        {
            if(!property.CanWrite) return;
            var attribute = property.GetCustomAttribute<TlvPropertyAttribute>();
            if(!tlv.TryGetValue(attribute.Tag,out byte[] tlvValue) && property.PropertyType != typeof(Tlv)) return;
            switch (property.PropertyType)
            {
                case {} _ when property.PropertyType == typeof(Tlv):
                    property.SetValue(item,new Tlv(tlv.Encode()));
                    break;
                case { } _ when typeof(ITlvDecoder).IsAssignableFrom(property.PropertyType):
                    var itlvDecoderObj = Activator.CreateInstance(property.PropertyType) as ITlvDecoder;
                    if(itlvDecoderObj is null) break;
                    itlvDecoderObj.Decode(tlvValue);
                    
                    property.SetValue(item,itlvDecoderObj);
                    break;
                case { } _ when property.PropertyType == typeof(byte[]):
                    property.SetValue(item,tlvValue);
                    break;
                case { } _ when property.PropertyType == typeof(int):
                    property.SetValue(item,(int)BerHelpers.DecodeUint(tlvValue));
                    break;
                case { } _ when property.PropertyType == typeof(long):
                    property.SetValue(item,(long)BerHelpers.DecodeUint(tlvValue));
                    break;
                case { } _ when property.PropertyType == typeof(ulong):
                    property.SetValue(item,BerHelpers.DecodeUint(tlvValue));
                    break;
                case { } _ when property.PropertyType == typeof(uint):
                    property.SetValue(item,(uint)BerHelpers.DecodeUint(tlvValue));
                    break;
                case { } _ when property.PropertyType == typeof(string):
                    if(attribute.DecodeOption == DecodeOption.Hex) property.SetValue(item,Helpers.ByteArrayToString(tlvValue));
                    else property.SetValue(item,Encoding.ASCII.GetString(tlvValue));
                    break;
                case { } _ when property.PropertyType == typeof(bool):
                    var xuint = (uint) BerHelpers.DecodeUint(tlvValue);
                    property.SetValue(item,xuint != 0);
                    break;
                case { } _ when property.PropertyType.IsClass:
                    property.SetValue(item,Deserialize(new Tlv(tlvValue),property.PropertyType));
                    break;
                    
            }
        }



        public static Tlv Serialize(object value)
        {
            var tlv = new Tlv();
            var type = value.GetType();
            var properties = type.GetProperties().Where(x => x.CustomAttributes.Any(y=>y.AttributeType == typeof(TlvPropertyAttribute)));
            foreach (var property in properties)
            {
                var (tag, bytes) = GetValue(property, value);
                if (bytes != null) tlv[tag] = bytes;
            }

            return tlv;
        }

        private static (int, byte[]) GetValue(PropertyInfo property,object value)
        {
            if(!property.CanRead) return (0,null);
            var attribute = property.GetCustomAttribute<TlvPropertyAttribute>();
            switch (property.PropertyType)
            {
                case {} _ when property.PropertyType == typeof(Tlv):
                    return (attribute.Tag, ((Tlv)property.GetValue(value)).Encode());
                case { } _ when typeof(ITlvEncoder).IsAssignableFrom(property.PropertyType):
                    var itlvEncoderObj =property.GetValue(value) as ITlvEncoder;
                    if(itlvEncoderObj is null) break;
                    
                    return (attribute.Tag, itlvEncoderObj.Encode());
                case { } _ when property.PropertyType == typeof(byte[]):
                    return (attribute.Tag, property.GetValue(value) as byte[]);
                case { } _ when property.PropertyType == typeof(ulong):
                    return (attribute.Tag,BerHelpers.EncodeUint((ulong)property.GetValue(value)));
                case { } _ when property.PropertyType == typeof(int):
                    return (attribute.Tag,BerHelpers.EncodeUint((ulong)(int)property.GetValue(value)));
                case { } _ when property.PropertyType == typeof(uint): 
                    return (attribute.Tag,BerHelpers.EncodeUint((ulong)(uint)property.GetValue(value)));
                case { } _ when property.PropertyType == typeof(long):
                    return (attribute.Tag,BerHelpers.EncodeUint((ulong)property.GetValue(value)));
                case { } _ when property.PropertyType == typeof(string):
                    if(attribute.DecodeOption == DecodeOption.Hex) return (attribute.Tag,Helpers.HexStringToByteArray(property.GetValue(value) as string));
                    else
                    {
                        var str = property.GetValue(value) as string;
                        return (attribute.Tag,string.IsNullOrEmpty(str)? null: Encoding.ASCII.GetBytes(str));
                    }
                case { } _ when property.PropertyType == typeof(bool):
                    var boolval = (bool)property.GetValue(value);
                    if(boolval) return (attribute.Tag,BerHelpers.EncodeUint(1));
                    else return (attribute.Tag,BerHelpers.EncodeUint(0));
                case { } _ when property.PropertyType.IsClass:
                    return (attribute.Tag, Serialize(property.GetValue(value)).Encode());
                    
            }
            
            return (0,null);
        }
        
    }
}