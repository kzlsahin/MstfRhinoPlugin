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
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace MstfRhinoPlugin1
{
    public class MstfRhinoPlugin1Command : Command
    {
        public MstfRhinoPlugin1Command()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static MstfRhinoPlugin1Command Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "Mstf_MassCalculate";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            RhinoApp.WriteLine("The {0} command will select and calculate a surface mass", EnglishName);

            Rhino.DocObjects.ObjRef obref;

            Result rc = RhinoGet.GetOneObject("Select object", true, Rhino.DocObjects.ObjectType.Surface, out obref);

            if (rc != Result.Success)
                return rc;

            if (obref == null)
            {
                RhinoApp.WriteLine("Yüzey seçilmedi {0}", rc.ToString());
                return Result.Cancel;
            }

            doc.Views.Redraw();

            (Double mass, Point3d Centr) = Mstf_Tools.CalculateSurfaceMass(obref);

            // ---
            return Result.Success;
        }
    }



    public class MyRhinoCommand2 : Command
    {
        public MyRhinoCommand2()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static MyRhinoCommand2 Instance { get; private set; }

        public override string EnglishName => "Mstf_SetThicknesToSurface";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Theis command will define a thickness property to the selected surface", EnglishName);

            Rhino.DocObjects.ObjRef[] obref;

            Surface surfaceInp;

            Double thickness = 0;

            Result rc = RhinoGet.GetMultipleObjects("Select object", true, Rhino.DocObjects.ObjectType.Surface, out obref);

            if (rc != Result.Success)
                return rc;

            if (obref == null)
            {
                RhinoApp.WriteLine("Yüzey seçilmedi {0}", rc.ToString());
                return Result.Cancel;
            }
            rc = RhinoGet.GetNumber("Enter Thickness value in mm", false, ref thickness);

            if (rc != Result.Success)
                return rc;

            foreach (Rhino.DocObjects.ObjRef obj in obref)
            {
                Mstf_Tools.SetAttributeToObjectName(obj.Object(), "thickness", thickness.ToString()); ;
            }
            // TODO: complete command.
            return Result.Success;
        }
    }



    public class MyRhinoCommand3 : Command
    {
        public MyRhinoCommand3()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static MyRhinoCommand3 Instance { get; private set; }

        public override string EnglishName => "Mstf_GetCenterOfMass";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Rhino.DocObjects.ObjRef[] obref;

            var masses = new List<double>();

            Double sumOfMass = 0;

            Double[] sumOfMoment = new double[] { 0, 0, 0 }; //X, Y, Z

            var centers = new List<Point3d>();

            Point3d centerOfMass = new Point3d(0, 0, 0);

            String fileName = "Result_CenterOfMass.csv";
            String Path = Directory.GetCurrentDirectory() + fileName;

            List<string> lines = new List<string>();

            RhinoApp.WriteLine("This command will calculate center of mass of the surfaces with defined thicknesses", EnglishName);

            Result rc = Rhino.Input.RhinoGet.GetMultipleObjects("Select object", true, Rhino.DocObjects.ObjectType.Surface, out obref);

            if (rc != Result.Success)
                return rc;

            if (obref == null)
            {
                RhinoApp.WriteLine("Yüzey seçilmedi {0}", rc.ToString());
                return Result.Cancel;
            }

            File.WriteAllText(fileName, "ObjectName;MAss(volume);X;Y;Z;\r\n");

            foreach (Rhino.DocObjects.ObjRef obj in obref)
            {
                Surface surface = obj.Surface();

                (Double M, Point3d C) = Mstf_Tools.CalculateSurfaceMass(obj);
                masses.Add(M);
                centers.Add(C);

                sumOfMass += M;

                sumOfMoment[0] += M * C.X;
                sumOfMoment[1] += M * C.Y;
                sumOfMoment[2] += M * C.Z;
                lines.Add($"{obj.Object().Name};{M}; {C.X};  {C.Y};  {C.Z};");
            }
            
            File.AppendAllLines(fileName, lines);

            centerOfMass.X = sumOfMoment[0] / sumOfMass;

            centerOfMass.Y = sumOfMoment[1] / sumOfMass;

            centerOfMass.Z = sumOfMoment[2] / sumOfMass;

            RhinoApp.WriteLine($"The calculated Volume is {sumOfMass};");

            RhinoApp.WriteLine($"Center of mass is {centerOfMass.X};  {centerOfMass.Y};  {centerOfMass.Z};");

            doc.Objects.Add(new Point(centerOfMass));

            return Result.Success;
        }
    }



    public class MyRhinoCommand4 : Command
    {
        public MyRhinoCommand4()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static MyRhinoCommand4 Instance { get; private set; }

        public override string EnglishName => "Mstf_Help";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"This plugin has the following commands:");
            RhinoApp.WriteLine($"Commands:  \n mstf_SetThicknesToSurface : sets thicknes value to selected surfaces \n " +
                $"mstf_MassCalculate : for now just calculates volume and prompts the result to console. Mass calculation option will be available soon. \n" +
                $"mstf_GetCenterOfMAss : calculates the center of the volume and adds a point on the location, also promts the value of the volume.  Mass calculation option will be available soon." +
                $"Caution! The unit of the thickness should be same as the unit system of the model. If the model is milimetric then the thickness also should be milimeter.");
            RhinoApp.WriteLine("Mstf_NameSerialObjects : Seçilen nesneleri seçildikleri sıraya göre bir ön isime eklenen tam sayılarla isimlendirir. Örn. Fr-1 Fr-2 Fr-3...");
            RhinoApp.WriteLine("Mstf_NameSerialObjects : Seçilen ünesnelerin merkez noktasında nesnenin ismini gösteren bir TextDot nesnesini etiket olarak ekler");
            RhinoApp.WriteLine($"active directory: {Directory.GetCurrentDirectory()}");
            return Result.Success;
        }
    }


    public class MyRhinoCommand5 : Command
    {
        public MyRhinoCommand5()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static MyRhinoCommand5 Instance { get; private set; }

        public override string EnglishName => "Mstf_NameSerialObjects";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Rhino.DocObjects.ObjRef[] objrefs;

            String prefixName = "";

            String indexFormatter = "";

            Result rc = RhinoGet.GetMultipleObjects("İsimlendirilmesini istediğiniz nesneleri seçiniz:", true, Rhino.DocObjects.ObjectType.AnyObject, out objrefs);

            if (rc != Rhino.Commands.Result.Success)
                return rc;

            if (objrefs == null || objrefs.Length < 1)
                return Rhino.Commands.Result.Failure;

            int formatLength = Convert.ToInt32(Math.Floor(Math.Log10(objrefs.Length))) + 1;

            indexFormatter = "D" + formatLength;

            int counter = 1;

            RhinoApp.WriteLine($"{objrefs.Length} adet nesne seçildi");

            RhinoApp.WriteLine($"{indexFormatter} is formatter");

            RhinoGet.GetString("Ön isim giriniz", false, ref prefixName);

            foreach (Rhino.DocObjects.ObjRef objref in objrefs)
            {
                var obj = objref.Object();
                string name = prefixName + counter.ToString(indexFormatter);

                Mstf_Tools.SetObjectName(obj, name);

                counter++;
            }
            RhinoApp.WriteLine("İsimler nesnelere tanımlandı");

            return Result.Success;
        }
    }

    public class MyRhinoCommand6 : Command
    {
        public MyRhinoCommand6()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static MyRhinoCommand6 Instance { get; private set; }

        public override string EnglishName => "Mstf_LabelSerialObjects";
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Rhino.DocObjects.ObjRef[] objrefs;
            Double textHeight = 20;

            var activeDoc = RhinoDoc.ActiveDoc;

            Result rc = RhinoGet.GetMultipleObjects("Etiketlenmesini istediğiniz nesneleri seçiniz:", true, Rhino.DocObjects.ObjectType.AnyObject, out objrefs);

            if (rc != Result.Success)
                return rc;

            if (objrefs == null || objrefs.Length < 1)
                return Result.Failure;

            RhinoApp.WriteLine($"{objrefs.Length} adet nesne seçildi");

            RhinoApp.WriteLine($"Name özelliği tanımlanmış olan nesneler etiketlenecek (textDot)");


            RhinoGet.GetNumber("Font karakter yüksekliği girebilirsiniz", false, ref textHeight);


            foreach (Rhino.DocObjects.ObjRef objref in objrefs)
            {
                Rhino.DocObjects.RhinoObject obj = objref.Object();
                string name = Mstf_Tools.GetObjectName( obj );
                if (name != string.Empty)
                {
                    BoundingBox box = obj.Geometry.GetBoundingBox(false);

                    var label = new TextDot(name, box.Center);

                    activeDoc.Objects.Add(label);
                }

                RhinoApp.WriteLine($"{name} etiketlendi");
            }
            RhinoApp.WriteLine("etiketleme tamamlandı");

            return Result.Success;
        }
    }
     public class MyRhinoCommand7 : Command
    {
        public MyRhinoCommand7()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static MyRhinoCommand7 Instance { get; private set; }

        public override string EnglishName => "Mstf_SelectObjectsWithThickness";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Double thickness = 0;

            var activeDoc = RhinoDoc.ActiveDoc;

            Result rc = RhinoGet.GetNumber("Kalınlık değeri giriniz", false, ref thickness);

            RhinoApp.WriteLine($"Seçilen değer {thickness}");


            if (rc != Result.Success)
                return rc;


            /*var selectedObjects = from obj in activeDoc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.AnyObject)
                                  where obj.Geometry.GetUserString("thickness") == thicknessStr
                                  select obj;
            */



            foreach (Rhino.DocObjects.BrepObject brepObj in activeDoc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.Brep))
            {
                if(!brepObj.BrepGeometry.IsSurface)
                {
                    continue;
                }

                string str = Mstf_Tools.GetAttributeValue(brepObj, "thickness");

                RhinoApp.WriteLine($"The thickness: {str} ");

                if (!Double.TryParse(str, out double t))
                {
                    RhinoApp.WriteLine("The thickness is not readable for a surface");
                }
                RhinoApp.WriteLine($"thickness parsed: {t}");

                foreach (Surface surface in brepObj.BrepGeometry.Faces)
                {  
                    if (!(t == thickness))
                    {
                        RhinoApp.WriteLine("not equal");
                        continue;
                    }
                    if (brepObj.Select(true, true) != 0)
                    {
                        RhinoApp.WriteLine("ok!");
                    }
                    else
                    {
                        return Result.Failure;
                    }
                }
            }
            doc.Views.Redraw();
            return Result.Success;
        }

    }
    
    public class MyRhinoCommand8 : Command
    {
        public MyRhinoCommand8()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static MyRhinoCommand8 Instance { get; private set; }

        public override string EnglishName => "Mstf_FilterSelectedByThickness";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Double thickness = 0;

            var activeDoc = RhinoDoc.ActiveDoc;

            Result rc = RhinoGet.GetNumber("v02 Kalınlık değeri giriniz", false, ref thickness);

            RhinoApp.WriteLine($"Seçilen değer {thickness}");


            //if (rc != Result.Success)
            //    return rc;


            /*var selectedObjects = from obj in activeDoc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.AnyObject)
                                  where obj.Geometry.GetUserString("thickness") == thicknessStr
                                  select obj;
            */

            rc = RhinoGet.GetMultipleObjects("İçinden arama yapılacak nesneleri seçiniz", true, Rhino.DocObjects.ObjectType.Surface, out Rhino.DocObjects.ObjRef[] objRefs);

            if (rc != Result.Success)
                return rc;
            if (objRefs == null || objRefs.Length == 0)
            {
                RhinoApp.WriteLine("Yüzey seçilmedi {0}", rc.ToString());
                return Result.Cancel;
            }
            foreach (Rhino.DocObjects.ObjRef objRef in objRefs)
            {
                var obj = objRef.Object();

                string str = Mstf_Tools.GetAttributeValue(obj, "thickness");

                if (!Double.TryParse(str, out double t))
                {
                    RhinoApp.WriteLine("The thickness is not readable for a surface");
                }
                RhinoApp.WriteLine($"thickness parsed: {t}");

                if (!(t == thickness))
                {
                    RhinoApp.WriteLine("not equal");
                    continue;
                }
                if (obj.Select(true, true) != 0)
                {
                    RhinoApp.WriteLine("ok!");
                }
                else
                {
                    return Result.Failure;
                }
            }
            doc.Views.Redraw();
            return Result.Success;
        }
    }
    
}
