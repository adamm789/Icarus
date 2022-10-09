using Icarus.Mods.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface ITestService<T> : IServiceProvider where T : IGameFile
    {
        T Foo();
    }

    public class TestImplementation : ServiceBase<TestImplementation>, ITestService<IMaterialGameFile>
    {
        public IMaterialGameFile Foo()
        {
            throw new NotImplementedException();
        }
    }
}
