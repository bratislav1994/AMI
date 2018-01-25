using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Common
{
    /// <summary>
    /// An enumeration that defines property types
    /// </summary>	
    public enum PropertyType : short
    {
        Empty = 0,

        Bool = 1,
        Byte = 2,
        Int32 = 3,
        Int64 = 4,
        Float = 5,

        String = 7,
        DateTime = 8,
        Reference = 9,
        Enum = 10,
        Struct = 11,
        TimeSpan = 12,

        BoolVector = 17,
        ByteVector = 18,
        Int32Vector = 19,
        Int64Vector = 20,
        FloatVector = 21,

        StringVector = 23,
        DateTimeVector = 24,
        ReferenceVector = 25,
        EnumVector = 26,
        StructVector = 27,
        TimeSpanVector = 28,
    }

    /// <summary>
    /// A class that describes property of generic model resource
    /// </summary>
    [Serializable]
    [DataContract]
    public class Property //: IComparable
    {
        private int idDB;
        /// <summary>
        /// Code for property of model type 
        /// </summary>
        private ModelCode id;

        /// <summary>
        /// Current value of the property
        /// </summary>
        private PropertyValue value;

        /// <summary>
        /// Initializes a new instance of the Property class
        /// </summary>
        public Property()
        {
        }

        public Property(Property toCopy)
        {
            this.id = toCopy.id;
            this.value = new PropertyValue(toCopy.PropertyValue);
        }

        /// <summary>
        /// Initializes a new instance of the Property class
        /// </summary>
        /// <param name="id">property ID</param>
        public Property(ModelCode id)
        {
            this.id = id;
            this.value = new PropertyValue();
        }

        public Property(ModelCode id, PropertyValue value)
        {
            this.id = id;
            this.value = value;
        }

        public Property(ModelCode id, bool boolValue)
        {
            this.id = id;
            this.value = new PropertyValue();
            SetValue(boolValue);
        }

        public Property(ModelCode id, int int32Value)
        {
            this.id = id;
            this.value = new PropertyValue();
            SetValue(int32Value);
        }

        public Property(ModelCode id, long int64Value)
        {
            this.id = id;
            this.value = new PropertyValue();
            SetValue(int64Value);
        }

        public Property(ModelCode id, float floatValue)
        {
            this.id = id;
            this.value = new PropertyValue();
            SetValue(floatValue);
        }

        public Property(ModelCode id, string stringValue)
        {
            this.id = id;
            this.value = new PropertyValue();
            SetValue(stringValue);
        }

        public Property(ModelCode id, List<long> longValues)
        {
            this.id = id;
            this.value = new PropertyValue();
            if (longValues != null)
            {
                SetValue(longValues);
            }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDB
        {
            get
            {
                return idDB;
            }

            set
            {
                idDB = value;
            }
        }

        [DataMember]
        public ModelCode Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        public PropertyValue PropertyValue
        {
            get { return value; }
            set { this.value = value; }
        }

        public PropertyType Type
        {
            get
            {
                return (PropertyType)((long)id & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE);
            }
        }

        public static bool operator ==(Property first, Property second)
        {
            if (Object.ReferenceEquals(first, null) && Object.ReferenceEquals(second, null))
            {
                return true;
            }
            else if ((Object.ReferenceEquals(first, null) && !Object.ReferenceEquals(second, null)) || (!Object.ReferenceEquals(first, null) && Object.ReferenceEquals(second, null)))
            {
                return false;
            }
            else
            {
                if (!first.IsCompatibleWith(second.Type))
                {
                    throw new Exception("Incompatible property types.");
                }

                bool result = false;

                switch (first.Type)
                {
                    case PropertyType.Bool:
                    case PropertyType.Byte:
                    case PropertyType.Int32:
                    case PropertyType.Int64:
                    case PropertyType.Enum:
                    case PropertyType.TimeSpan:
                    case PropertyType.DateTime:
                    case PropertyType.Reference:
                        if (first.value.LongValue == second.value.LongValue)
                        {
                            result = true;
                        }

                        break;
                    case PropertyType.Float:
                        if (first.value.FloatValue == second.value.FloatValue)
                        {
                            result = true;
                        }

                        break;

                    case PropertyType.String:
                        if (first.value.StringValue == second.value.StringValue)
                        {
                            result = true;
                        }
                        break;

                    case PropertyType.ReferenceVector:
                        result = CompareHelper.CompareLists(first.value.LongValues, second.value.LongValues, true);
                        break;
                    case PropertyType.BoolVector:
                    case PropertyType.ByteVector:
                    case PropertyType.EnumVector:
                    case PropertyType.Int32Vector:
                    case PropertyType.Int64Vector:
                    case PropertyType.DateTimeVector:
                        result = CompareHelper.CompareLists(first.value.LongValues, second.value.LongValues, false);
                        break;
                    case PropertyType.FloatVector:
                        result = CompareHelper.CompareLists(first.value.FloatValues, second.value.FloatValues);
                        break;
                    case PropertyType.StringVector:
                        result = CompareHelper.CompareLists(first.value.StringValues, second.value.StringValues);
                        break;
                }

                return result;
            }
        }

        public static PropertyType GetPropertyType(ModelCode propId)
        {
            return (PropertyType)((long)propId & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE);
        }

        public static bool operator !=(Property first, Property second)
        {
            return !(first == second);
        }

        // Override the Object.Equals(object o) method:
        public override bool Equals(object o)
        {
            return this == (Property)o;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            switch (this.Type)
            {
                case PropertyType.Bool:
                    return this.AsBool().ToString().ToLower();
                case PropertyType.Byte:
                case PropertyType.Enum:
                    return this.AsEnum().ToString();
                case PropertyType.Int32:
                    return this.AsInt().ToString();
                case PropertyType.Int64:
                case PropertyType.TimeSpan:
                    return this.AsLong().ToString();
                case PropertyType.Reference:
                    //return this.AsLong().ToString("x16");
                    return this.AsLong().ToString();
                case PropertyType.Float:
                    return this.AsFloat().ToString(new System.Globalization.CultureInfo("en-US", false).NumberFormat);
                case PropertyType.String:
                    return this.AsString();
                case PropertyType.DateTime:
                    return this.AsDateTime().ToShortTimeString() + " | " + this.AsDateTime().ToShortDateString();
                default:
                    return null;
            }
        }

        #region Set-Get

        public void SetValue(bool boolValue)
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Bool)
            {
                value.LongValue = (long)(boolValue ? 1 : 0);
            }
            else
            {
                string errorMessage = String.Format("Failed to set value for property ({0}) because specified value ({1}) is not compatible with property type ({2}).", this.id, boolValue, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public void SetValue(short int16Value)
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Enum || type == PropertyType.Int32 || type == PropertyType.Int64 || type == PropertyType.TimeSpan || type == PropertyType.DateTime
                || type == PropertyType.Byte && int16Value <= Byte.MaxValue)
            {
                value.LongValue = (long)int16Value;
            }
            else
            {
                string errorMessage = String.Format("Failed to set value for property ({0}) because specified value ({1}) is not compatible with property type ({2}).", this.id, int16Value, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public void SetValue(int int32Value)
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Int32 || type == PropertyType.Int64 || type == PropertyType.TimeSpan || type == PropertyType.DateTime
                || type == PropertyType.Byte && int32Value <= Byte.MaxValue
                || type == PropertyType.Enum && int32Value <= Int16.MaxValue)
            {
                value.LongValue = (long)int32Value;
            }
            else
            {
                string errorMessage = String.Format("Failed to set value for property ({0}) because specified value ({1}) is not compatible with property type ({2}).", this.id, int32Value, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public void SetValue(long int64Value)
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Int64 || type == PropertyType.Reference || type == PropertyType.TimeSpan || type == PropertyType.DateTime
                || type == PropertyType.Byte && int64Value <= Byte.MaxValue
                || type == PropertyType.Enum && int64Value <= Int16.MaxValue
                || type == PropertyType.Int32 && int64Value <= UInt32.MaxValue)
            {
                value.LongValue = int64Value;
            }
            else
            {
                string errorMessage = String.Format("Failed to set value for property ({0}) because specified value ({1}) is not compatible with property type ({2}).", this.id, int64Value, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public void SetValue(float floatValue)
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Float)
            {
                value.FloatValue = floatValue;
            }
            else
            {
                string errorMessage = String.Format("Failed to set value for property ({0}) because specified value ({1}) is not compatible with property type ({2}).", this.id, floatValue, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public void SetValue(string stringValue)
        {
            PropertyType type = this.Type;
            if (Type == PropertyType.String)
            {
                value.StringValue = stringValue;
            }
            else
            {
                string errorMessage = String.Format("Failed to set value for property ({0}) because specified value ({1}) is not compatible with property type ({2}).", this.id, stringValue, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public void SetValue(List<long> longValues)
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Int64Vector || type == PropertyType.ReferenceVector || type == PropertyType.TimeSpanVector || type == PropertyType.DateTimeVector)
            {
                value.LongValues = longValues.GetRange(0, longValues.Count);
            }
            else
            {
                string errorMessage = String.Format("Failed to set value for property ({0}) because specified vector value is not compatible with property type ({1}).", this.id, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public bool AsBool()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Bool)
            {
                return (value.LongValue != 0) ? true : false;
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.Bool, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public short AsEnum()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Byte || type == PropertyType.Enum)
            {
                return (short)value.LongValue;
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.Enum, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public int AsInt()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Byte || type == PropertyType.Enum || type == PropertyType.Int32 || type == PropertyType.Bool)
            {
                return (int)value.LongValue;
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.Int32, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public long AsLong()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Bool || type == PropertyType.Byte || type == PropertyType.Enum || type == PropertyType.Int32 || type == PropertyType.Int64
                || type == PropertyType.Reference || type == PropertyType.TimeSpan || type == PropertyType.DateTime)
            {
                return value.LongValue;
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.Int64, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public float AsFloat()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Float)
            {
                return value.FloatValue;
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.Float, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public string AsString()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.String)
            {
                if (value.StringValue != null)
                {
                    return value.StringValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.String, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public long AsReference()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.Reference)
            {
                return value.LongValue;
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.Reference, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public DateTime AsDateTime()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.DateTime)
            {
                return new DateTime(value.LongValue, DateTimeKind.Utc);
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.DateTime, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public List<long> AsReferences()
        {
            PropertyType type = this.Type;
            if (type == PropertyType.ReferenceVector)
            {
                return value.LongValues;
            }
            else
            {
                string errorMessage = String.Format("Failed to return value from property for specified type ({0}) because it is incompatible with actual property type ({1})", PropertyType.ReferenceVector, type);
                CommonTrace.WriteTrace(CommonTrace.TraceError, errorMessage);
                throw new Exception(errorMessage);
            }
        }

        #endregion Set-Get

        public bool IsCompatibleWith(PropertyType type)
        {
            return (this.Type == type

                // single value
                || (this.Type == PropertyType.Bool && (type == PropertyType.Byte || type == PropertyType.Int32 || type == PropertyType.Int64 || type == PropertyType.Enum))
                || (this.Type == PropertyType.Byte && (type == PropertyType.Bool || type == PropertyType.Int32 || type == PropertyType.Int64 || type == PropertyType.Enum))
                || (this.Type == PropertyType.Int32 && (type == PropertyType.Bool || type == PropertyType.Byte || type == PropertyType.Int64 || type == PropertyType.Enum))
                || (this.Type == PropertyType.Int64 && (type == PropertyType.Bool || type == PropertyType.Byte || type == PropertyType.Int32 || type == PropertyType.Enum))
                || (this.Type == PropertyType.Enum && (type == PropertyType.Enum || type == PropertyType.Byte || type == PropertyType.Int32 || type == PropertyType.Int64))

                // vector value
                || (this.Type == PropertyType.BoolVector && (type == PropertyType.ByteVector || type == PropertyType.Int32Vector || type == PropertyType.Int64Vector || type == PropertyType.EnumVector))
                || (this.Type == PropertyType.ByteVector && (type == PropertyType.BoolVector || type == PropertyType.Int32Vector || type == PropertyType.Int64Vector || type == PropertyType.EnumVector))
                || (this.Type == PropertyType.Int32Vector && (type == PropertyType.BoolVector || type == PropertyType.ByteVector || type == PropertyType.Int64Vector || type == PropertyType.EnumVector))
                || (this.Type == PropertyType.Int64Vector && (type == PropertyType.BoolVector || type == PropertyType.ByteVector || type == PropertyType.Int32Vector || type == PropertyType.EnumVector))
                || (this.Type == PropertyType.EnumVector && (type == PropertyType.EnumVector || type == PropertyType.ByteVector || type == PropertyType.Int32Vector || type == PropertyType.Int64Vector)));
        }
    }
}
