using GuptaMigration.Constants;
using GuptaMigration.Services;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TestGuptaMigration
{
    public class DependencyParserTests
    {
        [Fact]
        public void CutFileOnCurrentFunctionAndRestTest1()
        {
            //Arrange
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string contents = File.ReadAllText(dir+"\\TestData\\Functions1.apl").ToLower();
            DependencyParser parser = new DependencyParser();
            FileReader f = new FileReader();
            var tableNames = f.ReadTableNames();
            parser.TableNames = tableNames;
            string functionText = "";

            //Act
            string operation = ConstantsClass.ParsingConstants.InsertString;
            parser.CutFileOnCurrentFunctionAndRest(ref contents,  operation,  ref functionText);

            Console.WriteLine(functionText);
        }

        [Fact]
        public void GetTableNameTest1()
        {
            //Assert
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string contents = File.ReadAllText(dir + "\\TestData\\Functions1.apl").ToLower();
            DependencyParser parser = new DependencyParser();
            FileReader f = new FileReader();
            var tables = f.ReadTableNames();
            parser.TableNames = tables;
            string functionText = "";

            //Act
            string operation = ConstantsClass.ParsingConstants.InsertString;
            parser.CutFileOnCurrentFunctionAndRest(ref contents, operation, ref functionText);
            List<string> tableNames = parser.GetTableNames(functionText, operation);
            string functinName = parser.GetFunctionName(functionText);

            //Asset
            Assert.Equal("ARTIKELTAXIERUNG", tableNames[0].ToUpper());
            Assert.Equal("updateliefertax", functinName);
        }

        [Fact]
        public void GetTableFunctionsDictTest1()
        {
            //Assert
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string contents = File.ReadAllText(dir + "\\TestData\\Functions1.apl").ToLower();
            DependencyParser parser = new DependencyParser();
            FileReader f = new FileReader();
            var tableNames = f.ReadTableNames();
            parser.TableNames = tableNames;

            //Act
            string operation = ConstantsClass.ParsingConstants.InsertString;
            var records = parser.FindTableOperations(operation, contents,"Functions1");


            //Asset
            Assert.Single(records);
        }

        [Fact]
        public void GetTableFunctionsDictTest2()
        {
            //Assert
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string contents = File.ReadAllText(dir + "\\TestData\\artikel.apl").ToLower();
            DependencyParser parser = new DependencyParser();
            FileReader f = new FileReader();           
            var tableNames = f.ReadTableNames();
            parser.TableNames = tableNames;

            //Act
            string operation = ConstantsClass.ParsingConstants.InsertString;
            var records = parser.FindTableOperations(operation, contents, "artikel");


            //Asset
            Assert.NotNull(records);
        }


        [Fact]
        public void GetAllOperationCallsInFunctionTest1()
        {
            //Assert
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string contents = File.ReadAllText(dir + "\\TestData\\Functions1.apl").ToLower();
            DependencyParser parser = new DependencyParser();
            FileReader f = new FileReader();
            var tableNames = f.ReadTableNames();
            parser.TableNames = tableNames;

            //Act
            string operation = ConstantsClass.ParsingConstants.InsertString;
            List<string> tablesAndFunctions = parser.GetTableNames(contents, operation);


            //Asset
            Assert.Equal("ARTIKELTAXIERUNG",tablesAndFunctions[0].ToUpper() );
        }


        [Fact]
        public void ParseDocumentTest1()
        {
            //Assert
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string contents = File.ReadAllText(dir + "\\TestData\\artikel.apl").ToLower();
            DependencyParser parser = new DependencyParser();
            FileReader f = new FileReader();
            var tableNames = f.ReadTableNames();
            parser.TableNames = tableNames;

            //Act
            var tablesAndFunctions = parser.ParseDocument(contents, "artikel");

            //Asset
            Assert.NotEmpty(tablesAndFunctions);
        }
    }
}
