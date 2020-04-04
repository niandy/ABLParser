using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v12
{
    public class EnumDescriptorV12 : AbstractElement, IEnumDescriptor
    {
        public EnumDescriptorV12(string name) : base(name)
        {
        }

        public static IEnumDescriptor FromDebugSegment(string name, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
        {
            return new EnumDescriptorV12(name);
        }

        public override int SizeInRCode => 16;
    }

}
