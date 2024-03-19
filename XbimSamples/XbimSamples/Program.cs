using Xbim.Ifc;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

public class Program
{
    private static void Main(string[] args)
    {
        var Editor = new XbimEditorCredentials
        {
            ApplicationDevelopersName = "xBim Developer",
            ApplicationFullName = "Sample xBim Application",
            ApplicationVersion = "1.0",
            EditorsFamilyName = "Microsoft",
            EditorsGivenName = "Microsoft",
            EditorsOrganisationName = "Microsoft",
        };


        string GetIndent(int Level)
        {
            string Indent = "";
            for (int i = 0; i < Level; ++i)
            {
                Indent += "  ";
            }

            return Indent;
        }

        void PrintHierarchy(IIfcObjectDefinition Object, int Level)
        {
            Console.WriteLine(string.Format("{0}{1} [{2}]", GetIndent(Level), Object.Name, Object.GetType().Name));
            IIfcSpatialElement? spatialElement = Object as IIfcSpatialElement;


            if (spatialElement != null)
            {
                IEnumerable<IIfcProduct> elements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
                foreach (var item in elements)
                {
                    //these four lines throw the error
                    var ifcObject = item as IIfcObject;
                    var ifcSelection = ifcObject as IPersistEntity;
                    var geomstore = ifcSelection.Model.GeometryStore;
                    var geomReader = geomstore.BeginRead();
                    var shapeInstances = geomReader.ShapeInstancesOfEntity(ifcSelection);
                    Console.WriteLine("{0}    ->{1} [{2}]", GetIndent(Level), item.Name, item.GetType().Name);
                }
            }

            foreach (var item in Object.IsDecomposedBy.SelectMany(rel => rel.RelatedObjects))
            {
                PrintHierarchy(item, Level + 1);
            }
        }

        using (var Model = IfcStore.Open("C:\\workspace\\Xbim\\XbimSamples\\Test\\20201126GEN_DOE_GSE_ARC_Exploit.ifc"))
        {
            Xbim3DModelContext Context3d;
            if (Model.GeometryStore.IsEmpty)
            {
                var sufficientPrecision = 1.0e-5;
                if (Model.ModelFactors.Precision > sufficientPrecision)
                {
                    Model.ModelFactors.Precision = sufficientPrecision;
                }
                Context3d = new Xbim3DModelContext(Model);
                Context3d.CreateContext();

                if (Model.GeometryStore.IsEmpty)
                    throw new Exception("Error Creating Context");
            }
            var root = Model.Instances.FirstOrDefault<IIfcProject>();
            PrintHierarchy(root, 0);
        }
    }
}