using System;
using Xunit;
using ZbdUnitySDK.Utils;

namespace ZbdUnitySDKTest
{
    public class UtilsTest
    {
        [Fact]
        public void File_Access_Read_Write_Test()
        {
            FileResourceDataAccess fileResourceDataAccess = new FileResourceDataAccess();
            string testText = "Hello Unity";
            fileResourceDataAccess.Save(testText);

            string text = fileResourceDataAccess.Load();
            Assert.Equal("Hello Unity", text);
        }
    }
}
