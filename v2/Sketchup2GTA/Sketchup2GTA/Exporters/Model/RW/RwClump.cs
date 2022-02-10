using System.IO;

namespace Sketchup2GTA.Exporters.Model.RW
{
    public class RwClump : RwSection
    {
        public RwClump() : base(0x10)
        {
        }

        protected override void WriteSection(BinaryWriter bw)
        {
            WriteStruct(bw);
            bw.Write((uint)1); // Object count
            bw.Write((uint)0); // Unknown
            bw.Write((uint)0); // Unknown
        }

        protected override uint GetSectionSize()
        {
            return 12;
        }
    }
}