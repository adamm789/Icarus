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

        /// <summary>
        /// .fbx, .dds, .png, .bmp
        /// </summary>
        Raw,

        /// <summary>
        /// .ttmp2
        /// </summary>
        TexToolsModPack,

        /// <summary>
        /// .pmp or file structure
        /// </summary>
        PenumbraModPack,

        /// <summary>
        /// .mdl, .mtrl, .tex
        /// </summary>
        RawGameFile
    }
}
