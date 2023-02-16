using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Util.Import
{
    public enum ImportSource
    {
        Vanilla,
        // .fbx, .dds, .png, .bmp
        Raw,
        // .ttmp2
        TexToolsModPack,
        // .pmp or file structure
        PenumbraModPack,
        // .mdl, .mtrl, .tex
        RawGameFile
    }
}
