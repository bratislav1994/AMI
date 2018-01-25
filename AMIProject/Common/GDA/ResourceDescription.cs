using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace FTN.Common
{
    /// <summary>
    /// A class that describes generic model resource
    /// </summary>
    [Serializable]
    [DataContract]
    public class ResourceDescription
    {
        private int idDB;
        private long id;
        private List<Property> properties = new List<Property>();

        public ResourceDescription()
        {
        }

        public ResourceDescription(long id)
        {
            this.id = id;
        }

        public ResourceDescription(long id, List<Property> properties)
        {
            this.id = id;
            foreach (Property property in properties)
            {
                this.AddProperty(property);
            }
        }

        //public ResourceDescription(long id, List<ModelCode> propIds)
        //{
        //	this.id = id;
        //	foreach (ModelCode propId in propIds)
        //	{
        //		this.properties.Add(new Property(propId));
        //	}
        //}

        public ResourceDescription(ResourceDescription toCopy)
        {
            this.id = toCopy.id;
            foreach (Property property in toCopy.properties)
            {
                Property toAdd = new Property(property);
                this.AddProperty(toAdd);
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
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        public List<Property> Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        /// <summary>
        /// Gets difference between second and first ResourceDescription.
        /// Difference == second - first
        /// </summary>
        /// <param name="resDesc1">First ResourceDescription.</param>
        /// <param name="resDesc2">Second ResourceDescription.</param>
        /// <param name="addNewProperties">Indicates whether to add new properties to difference.</param>
        /// <param name="ignoredPropertyIds">IDs of ignored properties.</param>
        /// <returns>ResourceDescription with different properties.</returns>
        //public static ResourceDescription GetDifference(ResourceDescription resDesc1, ResourceDescription resDesc2, bool addNewProperties, HashSet<ModelCode> ignoredPropertyIds = null)
        //{
        //	if (resDesc1.Id != resDesc2.Id)
        //	{
        //		throw new Exception(string.Format("Failed to generate difference between resource descriptions. IDs are different. First ID = {0}. Second ID = {1}", resDesc1.Id, resDesc2.Id));
        //	}
        //	else
        //	{
        //		ResourceDescription difference = new ResourceDescription(resDesc1.Id);

        //		foreach (Property secondProperty in resDesc2.Properties)
        //		{
        //			if (ignoredPropertyIds != null && ignoredPropertyIds.Contains(secondProperty.Id))
        //			{
        //				continue;
        //			}
        //			else
        //			{
        //				bool isNew = true;

        //				for (int i = 0; i < resDesc1.Properties.Count; i++)
        //				{
        //					if (resDesc1.Properties[i].Id == secondProperty.Id)
        //					{
        //						if (resDesc1.Properties[i] != secondProperty)
        //						{
        //							difference.AddProperty(secondProperty);
        //						}

        //						isNew = false;
        //					}
        //				}

        //				if (isNew == true && addNewProperties == true)
        //				{
        //					difference.AddProperty(secondProperty);
        //				}
        //			}
        //		}

        //		return difference;
        //	}
        //}

        //public void Update(ResourceDescription updateRD)
        //{
        //	if (this.Id != updateRD.Id)
        //	{
        //		throw new Exception(string.Format("Failed to update resource description. IDs are different. Original ID = {0} . Update ID = {1}", this.Id, updateRD.Id));
        //	}
        //	else
        //	{
        //		foreach (Property updateProp in updateRD.Properties)
        //		{
        //			this.AddProperty(updateProp);
        //		}
        //	}
        //}

        public void AddProperty(Property property)
        {
            bool isNew = true;

            for (int i = 0; i < this.Properties.Count; i++)
            {
                if (this.Properties[i].Id == property.Id)
                {
                    this.Properties[i] = property;
                    isNew = false;
                }
            }

            if (isNew == true)
            {
                this.properties.Add(property);
            }
        }

        public class EqualityComparer : IEqualityComparer<ResourceDescription>
        {

            public bool Equals(ResourceDescription firstRD, ResourceDescription secondRD)
            {
                if ((firstRD == null) || (secondRD == null))
                {
                    throw new NullReferenceException();
                }

                if (ReferenceEquals(firstRD, secondRD) == true)
                {
                    return true;
                }

                if (firstRD.id.Equals(secondRD.id) == false)
                {
                    return false;
                }

                if ((firstRD.Properties.Count == 0) && (secondRD.Properties.Count == 0))
                {
                    return true;
                }

                if (firstRD.Properties.Count.Equals(secondRD.Properties.Count) == false)
                {
                    return false;
                }

                bool result = false;
                bool found = false;

                foreach (Property firstRDProperty in firstRD.Properties)
                {
                    foreach (Property secondRDProperty in secondRD.Properties)
                    {
                        if (firstRDProperty.Id == secondRDProperty.Id)
                        {
                            if (firstRDProperty == secondRDProperty)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found == false)
                    {
                        result = false;
                        break;
                    }
                    else
                    {
                        found = false;
                        result = true;
                    }
                }

                return result;
            }

            public int GetHashCode(ResourceDescription rd)
            {
                int hash = rd.id.GetHashCode();

                foreach (Property p in rd.Properties)
                {
                    if (p.Type.Equals(PropertyType.String) && p.PropertyValue.StringValue == null)
                    {
                        p.PropertyValue.StringValue = String.Empty;
                    }
                    hash = hash + p.Id.GetHashCode() + p.PropertyValue.GetHashCode();
                }
                return hash;
            }
        }
    }
}
