using System;
using ABLParser.RCodeReader.Elements;
using System.Collections.Generic;
using System.Text;
using ABLParser.RCodeReader.Elements.v11;

namespace ABLParser.RCodeReader
{
    

    public sealed class ProgressClasses
    {
        private static readonly IParameter[] EMPTY_PARAMETERS = new IParameter[] { };
        private const string PROGRESS_LANG_OBJECT = "Progress.Lang.Object";

        private ProgressClasses()
        {
            // No-op
        }

        public static ICollection<ITypeInfo> GetProgressClasses()
        {
            ICollection<ITypeInfo> coll = new List<ITypeInfo>();
            coll.Add(ProgressLangObject);

            return coll;
        }

        private static ITypeInfo ProgressLangObject
        {
            get
            {                
                ITypeInfo info = new TypeInfoV11(PROGRESS_LANG_OBJECT, null, null, 0);
                info.Methods.Add(new MethodElementV11("Clone", AccessType.PUBLIC, 0, DataType.CLASS.GetNum(), PROGRESS_LANG_OBJECT, 0, EMPTY_PARAMETERS));
                info.Methods.Add(new MethodElementV11("Equals", AccessType.PUBLIC, 0, DataType.LOGICAL.GetNum(), "", 0, new IParameter[] { new MethodParameterV11(0, "otherObj", 2, MethodParameterV11.PARAMETER_INPUT, 0, DataType.CLASS.GetNum(), PROGRESS_LANG_OBJECT, 0) }));
                info.Methods.Add(new MethodElementV11("GetClass", AccessType.PUBLIC, 0, DataType.CLASS.GetNum(), "Progress.Lang.Class", 0, EMPTY_PARAMETERS));
                info.Methods.Add(new MethodElementV11("ToString", AccessType.PUBLIC, 0, DataType.CHARACTER.GetNum(), "", 0, EMPTY_PARAMETERS));

                return info;
            }
        }      
    }
}
