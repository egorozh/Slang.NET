using Slang.Tests.Helpers;
using static Slang.Tests.Integration.EmbeddedLoader;

namespace Slang.Tests.Integration.Main;

public class CompilationTests
{
    [Test]
    public void Single_output_en()
    {
        string en = LoadResource("Slang.Tests.Integration.Resources._expected_en.output");
        string de = LoadResource("Slang.Tests.Integration.Resources._expected_de.output");
        string header = LoadResource("Slang.Tests.Integration.Resources._expected_header.output");

        CompilationAssert.Compiles(en, de, header);
    }
}
