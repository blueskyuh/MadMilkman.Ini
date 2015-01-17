﻿using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MadMilkman.Ini.Tests
{
    [TestClass]
    public class IniFileReadTests
    {
        [TestMethod]
        public void ReadDefaultTest()
        {
            string iniFileContent = ";Section's trailing comment." + Environment.NewLine +
                                    "[Section's name];Section's leading comment." + Environment.NewLine +
                                    ";Key's trailing comment." + Environment.NewLine +
                                    "Key's name = Key's value;Key's leading comment.";

            IniFile file = LoadIniFileContent(iniFileContent, new IniOptions());

            Assert.AreEqual(1, file.Sections.Count);
            Assert.AreEqual("Section's trailing comment.", file.Sections[0].TrailingComment.Text);
            Assert.AreEqual("Section's name", file.Sections[0].Name);
            Assert.AreEqual("Section's leading comment.", file.Sections[0].LeadingComment.Text);

            Assert.AreEqual(1, file.Sections[0].Keys.Count);
            Assert.AreEqual("Key's trailing comment.", file.Sections[0].Keys[0].TrailingComment.Text);
            Assert.AreEqual("Key's name", file.Sections[0].Keys[0].Name);
            Assert.AreEqual("Key's value", file.Sections[0].Keys[0].Value);
            Assert.AreEqual("Key's leading comment.", file.Sections[0].Keys[0].LeadingComment.Text);
        }

        [TestMethod]
        public void ReadCustomTest()
        {
            string iniFileContent = "#Section's trailing comment." + Environment.NewLine +
                                    "{Section's name}#Section's leading comment." + Environment.NewLine +
                                    "#Key's trailing comment." + Environment.NewLine +
                                    "Key's name : Key's value#Key's leading comment.";

            IniOptions options = new IniOptions()
            {
                CommentStarter = IniCommentStarter.Hash,
                SectionWrapper = IniSectionWrapper.CurlyBrackets,
                KeyDelimiter = IniKeyDelimiter.Colon
            };
            IniFile file = LoadIniFileContent(iniFileContent, options);

            Assert.AreEqual(1, file.Sections.Count);
            Assert.AreEqual("Section's trailing comment.", file.Sections[0].TrailingComment.Text);
            Assert.AreEqual("Section's name", file.Sections[0].Name);
            Assert.AreEqual("Section's leading comment.", file.Sections[0].LeadingComment.Text);

            Assert.AreEqual(1, file.Sections[0].Keys.Count);
            Assert.AreEqual("Key's trailing comment.", file.Sections[0].Keys[0].TrailingComment.Text);
            Assert.AreEqual("Key's name", file.Sections[0].Keys[0].Name);
            Assert.AreEqual("Key's value", file.Sections[0].Keys[0].Value);
            Assert.AreEqual("Key's leading comment.", file.Sections[0].Keys[0].LeadingComment.Text);
        }

        [TestMethod]
        public void ReadGlobalSectionTest()
        {
            string iniFileContent = ";Trailing comment1" + Environment.NewLine +
                                    "Key1 = Value1" + Environment.NewLine +
                                    ";Trailing comment2" + Environment.NewLine +
                                    "Key2 = Value2";

            IniFile file = LoadIniFileContent(iniFileContent, new IniOptions());

            Assert.AreEqual(1, file.Sections.Count);
            Assert.AreEqual(IniSection.GlobalSectionName, file.Sections[0].Name);

            Assert.AreEqual(2, file.Sections[0].Keys.Count);
            Assert.AreEqual("Trailing comment1", file.Sections[0].Keys[0].TrailingComment.Text);
            Assert.AreEqual("Key1", file.Sections[0].Keys[0].Name);
            Assert.AreEqual("Value1", file.Sections[0].Keys[0].Value);
            Assert.AreEqual("Trailing comment2", file.Sections[0].Keys[1].TrailingComment.Text);
            Assert.AreEqual("Key2", file.Sections[0].Keys[1].Name);
            Assert.AreEqual("Value2", file.Sections[0].Keys[1].Value);
        }

        [TestMethod]
        public void ReadUTF8EncodingTest()
        {
            string iniFileContent = "[Καλημέρα κόσμε]" + Environment.NewLine +
                                    "こんにちは 世界 = ¥ £ € $ ¢ ₡ ₢ ₣ ₤ ₥ ₦ ₧ ₨ ₩ ₪ ₫ ₭ ₮ ₯ ₹";

            IniFile file = LoadIniFileContent(iniFileContent, new IniOptions() { Encoding = Encoding.UTF8 });

            Assert.AreEqual("Καλημέρα κόσμε", file.Sections[0].Name);
            Assert.AreEqual("こんにちは 世界", file.Sections[0].Keys[0].Name);
            Assert.AreEqual("¥ £ € $ ¢ ₡ ₢ ₣ ₤ ₥ ₦ ₧ ₨ ₩ ₪ ₫ ₭ ₮ ₯ ₹", file.Sections[0].Keys[0].Value);
        }

        [TestMethod]
        public void ReadEmptyLinesTest()
        {
            string iniFileContent = Environment.NewLine +
                                    "  \t  " + Environment.NewLine +
                                    "[Section]" + Environment.NewLine +
                                    Environment.NewLine +
                                    Environment.NewLine +
                                    "  \t  " + Environment.NewLine +
                                    "Key = Value" + Environment.NewLine +
                                    Environment.NewLine +
                                    "  \t  " + Environment.NewLine +
                                    ";" + Environment.NewLine +
                                    "[Section]" + Environment.NewLine +
                                    Environment.NewLine +
                                    "  \t  " + Environment.NewLine +
                                    Environment.NewLine +
                                    ";" + Environment.NewLine +
                                    "Key = Value";

            IniFile file = LoadIniFileContent(iniFileContent, new IniOptions());

            Assert.AreEqual(2, file.Sections[0].LeadingComment.EmptyLinesBefore);
            Assert.AreEqual(3, file.Sections[0].Keys[0].LeadingComment.EmptyLinesBefore);
            Assert.AreEqual(2, file.Sections[1].TrailingComment.EmptyLinesBefore);
            Assert.AreEqual(3, file.Sections[1].Keys[0].TrailingComment.EmptyLinesBefore);
        }

        [TestMethod]
        public void ReadCommentEdgeCasesTest()
        {
            string iniFileContent = ";" + Environment.NewLine +
                                    ";Section's trailing comment;" + Environment.NewLine +
                                    "[Section]" + Environment.NewLine +
                                    "[Section];" + Environment.NewLine +
                                    "[Section]  ;" + Environment.NewLine +
                                    ";" + Environment.NewLine +
                                    ";Key's trailing comment;" + Environment.NewLine +
                                    "Key = Value  " + Environment.NewLine +
                                    "Key = Value;" + Environment.NewLine +
                                    "Key = Value  ;";

            IniFile file = LoadIniFileContent(iniFileContent, new IniOptions());

            Assert.AreEqual(Environment.NewLine + "Section's trailing comment;", file.Sections[0].TrailingComment.Text);
            Assert.AreEqual("Section", file.Sections[0].Name);
            Assert.AreEqual(null, file.Sections[0].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].LeadingComment.LeftIndentation);

            Assert.AreEqual("Section", file.Sections[1].Name);
            Assert.AreEqual(string.Empty, file.Sections[1].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[1].LeadingComment.LeftIndentation);

            Assert.AreEqual("Section", file.Sections[2].Name);
            Assert.AreEqual(string.Empty, file.Sections[2].LeadingComment.Text);
            Assert.AreEqual(2, file.Sections[2].LeadingComment.LeftIndentation);

            Assert.AreEqual(Environment.NewLine + "Key's trailing comment;", file.Sections[2].Keys[0].TrailingComment.Text);
            Assert.AreEqual("Key", file.Sections[2].Keys[0].Name);
            Assert.AreEqual("Value", file.Sections[2].Keys[0].Value);
            Assert.AreEqual(null, file.Sections[2].Keys[0].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[2].Keys[0].LeadingComment.LeftIndentation);

            Assert.AreEqual("Key", file.Sections[2].Keys[1].Name);
            Assert.AreEqual("Value", file.Sections[2].Keys[1].Value);
            Assert.AreEqual(string.Empty, file.Sections[2].Keys[1].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[2].Keys[1].LeadingComment.LeftIndentation);

            Assert.AreEqual("Key", file.Sections[2].Keys[2].Name);
            Assert.AreEqual("Value", file.Sections[2].Keys[2].Value);
            Assert.AreEqual(string.Empty, file.Sections[2].Keys[2].LeadingComment.Text);
            Assert.AreEqual(2, file.Sections[2].Keys[2].LeadingComment.LeftIndentation);
        }

        [TestMethod]
        public void ReadValueEdgeCasesTest()
        {
            string iniFileContent = "[Section]" + Environment.NewLine +
                                    "Key=" + Environment.NewLine +
                                    "Key=;" + Environment.NewLine +
                                    "Key= " + Environment.NewLine +
                                    "Key= ;" + Environment.NewLine +
                                    "Key =" + Environment.NewLine +
                                    "Key =;" + Environment.NewLine +
                                    "Key = " + Environment.NewLine +
                                    "Key = ;";

            IniFile file = LoadIniFileContent(iniFileContent, new IniOptions());

            Assert.AreEqual(string.Empty, file.Sections[0].Keys[0].Value);
            Assert.AreEqual(null, file.Sections[0].Keys[0].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[0].LeadingComment.LeftIndentation);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[1].Value);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[1].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[1].LeadingComment.LeftIndentation);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[2].Value);
            Assert.AreEqual(null, file.Sections[0].Keys[2].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[2].LeadingComment.LeftIndentation);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[3].Value);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[3].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[3].LeadingComment.LeftIndentation);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[4].Value);
            Assert.AreEqual(null, file.Sections[0].Keys[4].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[4].LeadingComment.LeftIndentation);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[5].Value);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[5].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[5].LeadingComment.LeftIndentation);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[6].Value);
            Assert.AreEqual(null, file.Sections[0].Keys[6].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[6].LeadingComment.LeftIndentation);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[7].Value);
            Assert.AreEqual(string.Empty, file.Sections[0].Keys[7].LeadingComment.Text);
            Assert.AreEqual(0, file.Sections[0].Keys[7].LeadingComment.LeftIndentation);
        }

        private static IniFile LoadIniFileContent(string iniFileContent, IniOptions options)
        {
            IniFile file = new IniFile(options);

            using (var stream = new MemoryStream(options.Encoding.GetBytes(iniFileContent)))
                file.Load(stream);

            return file;
        }
    }
}