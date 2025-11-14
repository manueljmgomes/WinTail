using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Xml;

namespace LogTailPlus.Services
{
    /// <summary>
    /// Service to detect and provide syntax highlighting definitions based on file extensions
    /// </summary>
    public static class SyntaxHighlightingService
    {
        /// <summary>
        /// Gets the appropriate syntax highlighting definition based on file path
        /// </summary>
        public static IHighlightingDefinition? GetHighlightingForFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var highlightingManager = HighlightingManager.Instance;

            return extension switch
            {
                // C/C++
                ".c" => highlightingManager.GetDefinition("C++"),
                ".cpp" or ".cc" or ".cxx" or ".h" or ".hpp" or ".hxx" => highlightingManager.GetDefinition("C++"),
                
                // C#
                ".cs" => highlightingManager.GetDefinition("C#"),
                
                // Java
                ".java" => highlightingManager.GetDefinition("Java"),
                
                // JavaScript/TypeScript
                ".js" => highlightingManager.GetDefinition("JavaScript"),
                ".ts" => highlightingManager.GetDefinition("TypeScript"),
                ".jsx" => highlightingManager.GetDefinition("JavaScript"),
                ".tsx" => highlightingManager.GetDefinition("TypeScript"),
                
                // Python
                ".py" or ".pyw" => highlightingManager.GetDefinition("Python"),
                
                // Markup Languages
                ".xml" => highlightingManager.GetDefinition("XML"),
                ".xaml" or ".axaml" => highlightingManager.GetDefinition("XML"),
                ".html" or ".htm" => highlightingManager.GetDefinition("HTML"),
                ".css" => highlightingManager.GetDefinition("CSS"),
                
                // Data Formats
                ".json" => highlightingManager.GetDefinition("JavaScript"), // JSON highlighting
                ".sql" => highlightingManager.GetDefinition("SQL"),
                
                // PHP
                ".php" or ".php3" or ".php4" or ".php5" => highlightingManager.GetDefinition("PHP"),
                
                // Shell/Batch
                ".sh" or ".bash" => highlightingManager.GetDefinition("Bash"),
                ".bat" or ".cmd" => highlightingManager.GetDefinition("Boo"), // Similar syntax
                
                // Delphi/Pascal
                ".pas" or ".dpr" or ".dfm" or ".dpk" or ".dproj" => GetDelphiHighlighting(),
                
                // Log files - use custom log highlighting
                ".log" or ".txt" => GetLogHighlighting(),
                
                // Ruby
                ".rb" => highlightingManager.GetDefinition("Ruby"),
                
                // PowerShell
                ".ps1" or ".psm1" or ".psd1" => highlightingManager.GetDefinition("PowerShell"),
                
                // Default for unknown extensions
                _ => GetLogHighlighting()
            };
        }

        /// <summary>
        /// Creates a custom Delphi/Pascal syntax highlighting definition
        /// </summary>
        private static IHighlightingDefinition GetDelphiHighlighting()
        {
            var xshdString = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Delphi"" extensions="".pas;.dpr;.dfm"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""Green"" />
    <Color name=""String"" foreground=""Blue"" />
    <Color name=""Keyword"" foreground=""Blue"" fontWeight=""bold"" />
    <Color name=""Number"" foreground=""Red"" />
    <Color name=""Directive"" foreground=""Purple"" />
    
    <RuleSet ignoreCase=""true"">
        <!-- Comments -->
        <Span color=""Comment"" begin=""//"" />
        <Span color=""Comment"" multiline=""true"" begin=""{"" end=""}"" />
        <Span color=""Comment"" multiline=""true"" begin=""(*"" end=""*)"" />
        
        <!-- Strings -->
        <Span color=""String"">
            <Begin>'</Begin>
            <End>'</End>
        </Span>
        
        <!-- Compiler Directives -->
        <Span color=""Directive"" begin=""{\$"" end=""}"" />
        
        <!-- Numbers -->
        <Rule color=""Number"">
            \b0[xX][0-9a-fA-F]+|(\b\d+(\.[0-9]+)?|\.[0-9]+)([eE][+-]?[0-9]+)?
        </Rule>
        
        <!-- Keywords -->
        <Keywords color=""Keyword"">
            <Word>and</Word>
            <Word>array</Word>
            <Word>as</Word>
            <Word>asm</Word>
            <Word>begin</Word>
            <Word>case</Word>
            <Word>class</Word>
            <Word>const</Word>
            <Word>constructor</Word>
            <Word>destructor</Word>
            <Word>dispinterface</Word>
            <Word>div</Word>
            <Word>do</Word>
            <Word>downto</Word>
            <Word>else</Word>
            <Word>end</Word>
            <Word>except</Word>
            <Word>exports</Word>
            <Word>file</Word>
            <Word>finalization</Word>
            <Word>finally</Word>
            <Word>for</Word>
            <Word>function</Word>
            <Word>goto</Word>
            <Word>if</Word>
            <Word>implementation</Word>
            <Word>in</Word>
            <Word>inherited</Word>
            <Word>initialization</Word>
            <Word>inline</Word>
            <Word>interface</Word>
            <Word>is</Word>
            <Word>label</Word>
            <Word>library</Word>
            <Word>mod</Word>
            <Word>nil</Word>
            <Word>not</Word>
            <Word>object</Word>
            <Word>of</Word>
            <Word>or</Word>
            <Word>packed</Word>
            <Word>procedure</Word>
            <Word>program</Word>
            <Word>property</Word>
            <Word>raise</Word>
            <Word>record</Word>
            <Word>repeat</Word>
            <Word>resourcestring</Word>
            <Word>set</Word>
            <Word>shl</Word>
            <Word>shr</Word>
            <Word>string</Word>
            <Word>then</Word>
            <Word>threadvar</Word>
            <Word>to</Word>
            <Word>try</Word>
            <Word>type</Word>
            <Word>unit</Word>
            <Word>until</Word>
            <Word>uses</Word>
            <Word>var</Word>
            <Word>while</Word>
            <Word>with</Word>
            <Word>xor</Word>
            <!-- Types -->
            <Word>Boolean</Word>
            <Word>Byte</Word>
            <Word>Cardinal</Word>
            <Word>Char</Word>
            <Word>Currency</Word>
            <Word>Double</Word>
            <Word>Extended</Word>
            <Word>Int64</Word>
            <Word>Integer</Word>
            <Word>LongInt</Word>
            <Word>LongWord</Word>
            <Word>Real</Word>
            <Word>ShortInt</Word>
            <Word>Single</Word>
            <Word>SmallInt</Word>
            <Word>String</Word>
            <Word>Word</Word>
            <Word>WideString</Word>
            <Word>AnsiString</Word>
            <!-- Visibility -->
            <Word>private</Word>
            <Word>protected</Word>
            <Word>public</Word>
            <Word>published</Word>
        </Keywords>
    </RuleSet>
</SyntaxDefinition>";

            using var reader = new StringReader(xshdString);
            using var xmlReader = XmlReader.Create(reader);
            return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
        }

        /// <summary>
        /// Creates a custom log file syntax highlighting definition
        /// </summary>
        private static IHighlightingDefinition GetLogHighlighting()
        {
            var xshdString = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Log"" extensions="".log;.txt"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Error"" foreground=""Red"" fontWeight=""bold"" />
    <Color name=""Warning"" foreground=""Orange"" fontWeight=""bold"" />
    <Color name=""Info"" foreground=""Blue"" />
    <Color name=""Debug"" foreground=""Gray"" />
    <Color name=""Timestamp"" foreground=""DarkCyan"" />
    <Color name=""IPAddress"" foreground=""DarkMagenta"" />
    <Color name=""URL"" foreground=""DarkBlue"" />
    <Color name=""Number"" foreground=""DarkRed"" />
    
    <RuleSet>
        <!-- Timestamps (various formats) -->
        <Rule color=""Timestamp"">
            \b\d{4}-\d{2}-\d{2}[\sT]\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})?\b
        </Rule>
        <Rule color=""Timestamp"">
            \b\d{2}/\d{2}/\d{4}\s\d{2}:\d{2}:\d{2}\b
        </Rule>
        <Rule color=""Timestamp"">
            \[\d{2}/[A-Za-z]{3}/\d{4}:\d{2}:\d{2}:\d{2}\s[+-]\d{4}\]
        </Rule>
        
        <!-- Log Levels -->
        <Rule color=""Error"">
            \b(ERROR|FATAL|CRITICAL|FAIL|EXCEPTION)\b
        </Rule>
        <Rule color=""Warning"">
            \b(WARNING|WARN)\b
        </Rule>
        <Rule color=""Info"">
            \b(INFO|INFORMATION|NOTICE)\b
        </Rule>
        <Rule color=""Debug"">
            \b(DEBUG|TRACE|VERBOSE)\b
        </Rule>
        
        <!-- IP Addresses -->
        <Rule color=""IPAddress"">
            \b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b
        </Rule>
        
        <!-- URLs -->
        <Rule color=""URL"">
            https?://[^\s]+
        </Rule>
        
        <!-- HTTP Status Codes -->
        <Rule color=""Number"">
            \s[1-5][0-9]{2}\s
        </Rule>
        
        <!-- Numbers -->
        <Rule color=""Number"">
            \b\d+\b
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

            using var reader = new StringReader(xshdString);
            using var xmlReader = XmlReader.Create(reader);
            return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
        }
    }
}
