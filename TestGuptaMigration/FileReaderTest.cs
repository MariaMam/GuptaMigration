using GuptaMigration.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestGuptaMigration
{
    public class FileReaderTest
    {

        [Fact]
        public void ReadValidTablesTest1()
        {
            //Assert

            FileReader f = new FileReader();

            //Act
            var res = f.ReadTableNames();

            //Asset
            Assert.NotEmpty(res);
        }
    }
}
