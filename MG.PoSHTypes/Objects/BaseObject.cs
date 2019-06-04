using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MG.PowerShell.Types
{
    public abstract class BaseObject
    {
        private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;
        protected private object _backingField;

        public string Name { get; protected private set; }

        protected private virtual void SetProperties<T>(T obj)
        {
            Type tt = typeof(T);
            Type thisType = this.GetType();
            IEnumerable<PropertyInfo> origProps = tt.GetProperties(FLAGS).Where(x => x.CanRead);

            PropertyInfo[] thisProps = thisType.GetProperties(FLAGS);

            for (int i = 0; i < thisProps.Length; i++)
            {
                var prop = thisProps[i];
                foreach (var thatProp in origProps)
                {
                    if (thatProp.Name.Equals(prop.Name))
                    {
                        object oVal = thatProp.GetValue(obj);
                        prop.SetValue(this, oVal);
                        break;
                    }
                }
            }
        }
    }
}
