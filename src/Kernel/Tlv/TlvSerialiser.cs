using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Kernel.Utils;

namespace Kernel.Tlv
{
    public static class TlvSerializer
    {

        public static T Deserialize<T>(Tlv value) where T:class
        {
            var obj = Deserialize(value,typeof(T));
            return (T) obj;
        }

        public static object Deserialize(Tlv value, Type type)
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
            if(!tlv.TryGetValue(attribute.Tag,out byte[] tlvValue)) return;
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
            return new Tlv();
        }
        
    }
}