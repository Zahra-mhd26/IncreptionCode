using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Encryption
{
    public class EncryptionInfo : GH_AssemblyInfo
    {
        public override string Name => "Encryption";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("B19AC3D0-94B1-4994-9BB0-EF676AAEC532");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}