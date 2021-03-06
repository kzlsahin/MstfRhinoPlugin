/*
* The MIT License (MIT)
*
* Mustafa Şentürk - 2022
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
* IN THE SOFTWARE.
* ----------------------------------------------------------------------------
*/

using Rhino;
using Rhino.Geometry;
using System;
using System.Linq;

namespace MstfRhinoPlugin1
{
    public class Mstf_Tools
    {
        public static bool SetAttributeToObjectName(Rhino.DocObjects.RhinoObject rhinoObject, string attributeKey, string attributeValue)
        {
            bool hasAttribute = IsAnyAttributeSet(rhinoObject);
            attributeKey = attributeKey.ToLower();
            attributeValue = attributeValue.ToLower();
            
            try
            {
                if (hasAttribute)
                {
                    rhinoObject.Attributes.Name += $";{attributeKey}:{attributeValue}";

                    rhinoObject.CommitChanges();
                }
                else
                {
                    rhinoObject.Attributes.Name += $"?{attributeKey}:{attributeValue}";

                    rhinoObject.CommitChanges();
                }
            }
            catch (Exception e)
            {
                RhinoApp.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public static void SetObjectName(Rhino.DocObjects.RhinoObject rhinoObject, string name)
        {
            if (rhinoObject.Name == null || rhinoObject.Name == "")
            {
                rhinoObject.Attributes.Name = name;
                rhinoObject.CommitChanges();
                return;
            }
                string attributeString = rhinoObject.Name.Trim().Split('?')[1];
                rhinoObject.Attributes.Name = name + "?" + attributeString;
                rhinoObject.CommitChanges();
        }

        public static string GetObjectName(Rhino.DocObjects.RhinoObject rhinoObject)
        {
            if (rhinoObject.Name == null || rhinoObject.Name == "")
            {
                return string.Empty;
            }
            return rhinoObject.Name.Trim().Split('?')[0];
        }

        public static string[,] GetAttributes(Rhino.DocObjects.RhinoObject rhinoObject)
        {
            bool hasAttributes = IsAnyAttributeSet(rhinoObject);

            if (!hasAttributes) return new string[,] { { "", "" } };

            string[,] attributes;
            string[] nameWithAttribute = rhinoObject.Name.Split('?');

            if (nameWithAttribute.Length > 2)
            {
                RhinoApp.WriteLine("Attribute bilgisi bozulmuş olabilir");
                return new string[,] { { "", "" } };
            }

            string[] attributeEntries = nameWithAttribute[1].Split(';');
            attributes = new string[attributeEntries.Length, 2];

            for (int i = 0; i < attributeEntries.Length; i++)
            {
                string[] keyvalue = attributeEntries[i].Trim().Split(':');
                attributes[i, 0] = keyvalue[0];
                attributes[i, 1] = keyvalue[1];
            }
            return attributes;
        }


        public static string GetAttributeValue(Rhino.DocObjects.RhinoObject rhinoObject, string attributeKey)
        {

            bool hasAttributes = IsAnyAttributeSet(rhinoObject);

            if (!hasAttributes) return "";

            string[,] attributes = GetAttributes(rhinoObject);

            for(int i = 0; i < attributes.Length; i++)
            {
                
                string key = attributes[i, 0];

                if (key == attributeKey)
                {
                    return attributes[i, 1];
                }
            }
            return string.Empty;
        }

        public static void RemoveAttributeToObjectName(Rhino.DocObjects.RhinoObject rhinoObject, string attributeKey)
        {

            bool hasAttributes = IsAnyAttributeSet(rhinoObject);

            if (!hasAttributes) return;

            string[,] attributes = GetAttributes(rhinoObject);

            string attributeToRemove;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i, 0] == attributeKey)
                {
                    attributeToRemove = attributes[i, 0] + ":" + attributes[i, 1];
                    rhinoObject.Attributes.Name.Replace(attributeToRemove, "");
                    rhinoObject.CommitChanges();
                    break;
                }
            }
            return;
        }

        public static bool IsAnyAttributeSet(Rhino.DocObjects.RhinoObject rhinoObject)
        {
            bool hasAttributes = false;
            if (rhinoObject.Name == null || rhinoObject.Name == "")
            {
                RhinoApp.WriteLine("isimsiz öğe var");
                return false;
            }
                try
            {
                hasAttributes = rhinoObject.Name.Contains("?");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("İsimsiz öğe var");
            }
            

            if (!hasAttributes)
            {
                return false;
            }

            string[] nameWithAttribute = rhinoObject.Name.Split('?');

            if (nameWithAttribute.Length > 2)
            {
                return false;
            }
            return true;
        }

        public static bool HasAttribute(Rhino.DocObjects.RhinoObject rhinoObject, string attributeKey)
        {
            string[,] attributes = GetAttributes(rhinoObject);

            if (attributes == null || attributes[0, 0] == string.Empty) return false;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i, 0] == attributeKey) return true;
            }
            return false;
        }

      
        public static (double, Point3d) CalculateSurfaceMass(Rhino.DocObjects.ObjRef obj)
        {
            Double mass = 0;
            double t = GetThickness(obj.Object());
            Surface surface = obj.Surface();
            AreaMassProperties AMProp = AreaMassProperties.Compute(surface);

            mass = AMProp.Area * t;

            return (mass, AMProp.Centroid);
        }

        public static double GetThickness(Rhino.DocObjects.RhinoObject rhinoObject)
        {

            Double thickness;
            if (!double.TryParse(GetAttributeValue(rhinoObject, "thickness"), out thickness))
            {
                RhinoApp.WriteLine("The thickness is not defined for a surface");
            }
            return thickness;
        }
    }
}
