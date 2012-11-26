namespace AsyncEngine.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Signature : IEquatable<Signature>
    {
        public int hashCode;
        public DynamicProperty[] properties;

        public Signature(IEnumerable<DynamicProperty> properties)
        {
            this.properties = properties.ToArray<DynamicProperty>();
            this.hashCode = 0;
            foreach (DynamicProperty property in properties)
            {
                this.hashCode ^= property.Name.GetHashCode() ^ property.Type.GetHashCode();
            }
        }

        public bool Equals(Signature other)
        {
            if (this.properties.Length != other.properties.Length)
            {
                return false;
            }
            for (int i = 0; i < this.properties.Length; i++)
            {
                if ((this.properties[i].Name != other.properties[i].Name) || (this.properties[i].Type != other.properties[i].Type))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return ((obj is Signature) ? this.Equals((Signature)obj) : false);
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }
    }
}

