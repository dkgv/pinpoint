using System.Collections.Generic;

namespace Pinpoint.Core
{
    public static class SyntaxTypeHelper
    {
        public static string ToString(SyntaxType syntax)
        {
            return syntax switch
            {
                SyntaxType.PlainText => "Text",
                SyntaxType.CPlusPlus => "C++",
                SyntaxType.CSharp => "C#",
                SyntaxType.HTML => "HTML",
                SyntaxType.Java => "Java",
                SyntaxType.JavaScript => "JavaScript",
                SyntaxType.PHP => "PHP",
                SyntaxType.Tex => "Tex",
                SyntaxType.XML => "XML",
                _ => "Text"
            };
        }

        public static IEnumerable<SyntaxType> Values()
        {
            yield return SyntaxType.PlainText;
            yield return SyntaxType.CSharp;
            yield return SyntaxType.Java;
            yield return SyntaxType.CPlusPlus;
            yield return SyntaxType.HTML;
            yield return SyntaxType.JavaScript;
            yield return SyntaxType.PHP;
            yield return SyntaxType.Tex;
            yield return SyntaxType.XML;
        }
    }

    public enum SyntaxType
    {
        PlainText,
        CSharp,
        Java,
        CPlusPlus,
        HTML,
        JavaScript,
        PHP,
        Tex,
        XML
    }
}
