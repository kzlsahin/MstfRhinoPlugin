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

            Surface surfaceInp;

            Result rc = Rhino.Input.RhinoGet.GetOneObject("Select object", true, Rhino.DocObjects.ObjectType.Surface, out obref);

            if (rc != Result.Success)
                return rc;

            if (obref == null)
            {
                RhinoApp.WriteLine("Yüzey seçilmedi {0}", rc.ToString());
                return Result.Cancel;
            }
            surfaceInp = obref.Surface();

            RhinoApp.WriteLine("The type of selected object is {0} ", surfaceInp.GetType());


            Double thickness;
            if (!Double.TryParse(surfaceInp.GetUserString("thickness"), out thickness))
            {
                RhinoApp.WriteLine("The thickness is not defined");
            }

            doc.Views.Redraw();

            (Double mass, Point3d Centr) = CalculateMass(surfaceInp, thickness);

            RhinoApp.WriteLine("The thickness of the object is {0} and the mass is {1}", thickness, mass);

            // ---
            return Result.Success;
        }


        public static (Double, Point3d) CalculateMass(Surface obj, Double t)
        {
            Double mass = 0;

            AreaMassProperties AMProp = AreaMassProperties.Compute(obj);

            mass = AMProp.Area * t;

            return (mass, AMProp.Centroid);
        }

        public static Double GetThickness(Surface surf)
        {

            Double thickness;
            if (!Double.TryParse(surf.GetUserString("thickness"), out thickness))
            {
                RhinoApp.WriteLine("The thickness is not defined for a surface");
            }
            return thickness;
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

            Result rc = Rhino.Input.RhinoGet.GetMultipleObjects("Select object", true, Rhino.DocObjects.ObjectType.Surface, out obref);

            if (rc != Result.Success)
                return rc;

            if (obref == null)
            {
                RhinoApp.WriteLine("Yüzey seçilmedi {0}", rc.ToString());
                return Result.Cancel;
            }
            rc = Rhino.Input.RhinoGet.GetNumber("Enter Thickness value in mm", false, ref thickness);

            if (rc != Result.Success)
                return rc;

            foreach (Rhino.DocObjects.ObjRef obj in obref)
            {
                surfaceInp = obj.Surface();

                RhinoApp.WriteLine("The type of selected object is {0} ", surfaceInp.GetType());

                surfaceInp.SetUserString("thickness", thickness.ToString());

                if (!Double.TryParse(surfaceInp.GetUserString("thickness"), out thickness))
                {
                    RhinoApp.WriteLine("The thickness is not defined for an object");
                }
                RhinoApp.WriteLine("The thickness of an object is set to {0}", thickness);
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

            RhinoApp.WriteLine("Theis command will calculate center of mass of the surfaces with defined thicknesses", EnglishName);

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

                (Double M, Point3d C) = MstfRhinoPlugin1Command.CalculateMass(surface, MstfRhinoPlugin1Command.GetThickness(surface));
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

                obj.Attributes.Name = prefixName + counter.ToString(indexFormatter);

                obj.CommitChanges();

                RhinoApp.WriteLine($"{obj.Name} tanımlandı");

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
                var obj = objref.Object();

                if (obj.Name != "" && obj.Name != null)
                {
                    BoundingBox box = obj.Geometry.GetBoundingBox(false);

                    var label = new TextDot(obj.Name, box.Center);

                    activeDoc.Objects.Add(label);
                }

                RhinoApp.WriteLine($"{obj.Name} etiketlendi");
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


            //if (rc != Result.Success)
            //    return rc;


            /*var selectedObjects = from obj in activeDoc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.AnyObject)
                                  where obj.Geometry.GetUserString("thickness") == thicknessStr
                                  select obj;
            */

            RhinoApp.WriteLine(activeDoc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.AnyObject).Count<Rhino.DocObjects.RhinoObject>().ToString());

            foreach (Rhino.DocObjects.BrepObject brepObj in activeDoc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.Brep))
            {
                if(!brepObj.BrepGeometry.IsSurface)
                {
                    continue;
                }

                foreach (Surface surface in brepObj.BrepGeometry.Faces)
                {
                    string str = surface.GetUserString("thickness");

                    RhinoApp.WriteLine($"The thickness: {str} ");

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
}
