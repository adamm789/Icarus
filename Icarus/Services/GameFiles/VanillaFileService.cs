using Icarus.Services.GameFiles.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.GameFiles
{
    public class VanillaFileService : ServiceBase<VanillaFileService>
    {
        public IModelFileService ModelFileService;
        public IMaterialFileService MaterialFileService;
        public ITextureFileService TextureFileService;
        public IMetadataFileService MetadataFileService;

        public VanillaFileService(IModelFileService modelFileService, IMaterialFileService materialFileService, ITextureFileService textureFileService, IMetadataFileService metadtaFileService)
        {
            ModelFileService = modelFileService;
            MaterialFileService = materialFileService;
            TextureFileService = textureFileService;
            MetadataFileService = metadtaFileService;
        }
    }
}
