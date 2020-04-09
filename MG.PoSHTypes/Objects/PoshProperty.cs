using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MG.Posh.Types
{
    public class PoshProperty : BaseObject, IComparable<PoshProperty>
    {
        public PropertyAttributes Attributes { get; private set; }
        public bool CanRead { get; private set; }
        public bool CanWrite { get; private set; }
        public IEnumerable<CustomAttributeData> CustomAttributes { get; private set; }
        public Type DeclaringType { get; private set; }
        public MethodInfo GetMethod { get; private set; }
        public bool IsSpecialName { get; private set; }
        public MemberTypes MemberType { get; private set; }
        public int MetadataToken { get; private set; }
        public Module Module { get; private set; }
        //public string Name { get; private set; }
        public Type PropertyType { get; private set; }
        public Type ReflectedType { get; private set; }
        public MethodInfo SetMethod { get; private set; }

        private PoshProperty(PropertyInfo pi)
        {
            base.SetProperties(pi);
            base._backingField = pi;
        }

        public int CompareTo(PoshProperty other)
        {
            int retNum = this.Name.CompareTo(other.Name);
            if (retNum == 0)
            {
                string xPropTypeName = this.PropertyType.FullName;
                string yPropTypeName = other.PropertyType.FullName;
                retNum = !string.IsNullOrEmpty(xPropTypeName) && !string.IsNullOrEmpty(yPropTypeName)
                    ? xPropTypeName.CompareTo(yPropTypeName)
                    : string.IsNullOrEmpty(xPropTypeName) && !string.IsNullOrEmpty(yPropTypeName)
                        ? 1
                        : !string.IsNullOrEmpty(xPropTypeName) && string.IsNullOrEmpty(yPropTypeName)
                            ? -1
                            : 0;
            }
            return retNum;
        }

        public PoshMethod[] GetAccessors()
        {
            MethodInfo[] accessors = ((PropertyInfo)base._backingField).GetAccessors();
            var newArr = new PoshMethod[accessors.Length];
            for (int i = 0; i < accessors.Length; i++)
            {
                newArr[i] = accessors[i];
            }
            return newArr;
        }

        public PoshMethod[] GetAccessors(bool nonPublic)
        {
            MethodInfo[] accessors = ((PropertyInfo)base._backingField).GetAccessors(nonPublic);
            var newArr = new PoshMethod[accessors.Length];
            for (int i = 0; i < accessors.Length; i++)
            {
                newArr[i] = accessors[i];
            }
            return newArr;
        }

        public static implicit operator PoshProperty(PropertyInfo pi) => new PoshProperty(pi);
        public static implicit operator PropertyInfo(PoshProperty pp) => (PropertyInfo)pp._backingField;
    }

    //public class PoshPropertySorter : IComparer<PoshProperty>
    //{
    //    public int Compare(PoshProperty this, PoshProperty other)
    //    {
    //        int retNum = this.Name.CompareTo(other.Name);
    //        if (retNum == 0)
    //        {
    //            string xPropTypeName = this.PropertyType.FullName;
    //            string yPropTypeName = other.PropertyType.FullName;
    //            retNum = !string.IsNullOrEmpty(xPropTypeName) && !string.IsNullOrEmpty(yPropTypeName)
    //                ? xPropTypeName.CompareTo(yPropTypeName)
    //                : string.IsNullOrEmpty(xPropTypeName) && !string.IsNullOrEmpty(yPropTypeName)
    //                    ? 1
    //                    : !string.IsNullOrEmpty(xPropTypeName) && string.IsNullOrEmpty(yPropTypeName)
    //                        ? -1
    //                        : 0;
    //        }
    //        return retNum;
    //    }
    //}
}
