using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ZbdUnitySDK.UnityUtils;
using ZbdUnitySDK.Utils;

namespace ZbdUnitySDKTest
{
    public class KeyUtilsTest
    {


        [Fact]
        public void NBitcoinKey_Generation_Test()
        {
            Key nbKey = KeyUtils.CreateNBEcKey();
            Assert.NotNull(nbKey);

        }
        [Fact]
        public void Derive_SIN_From_PrivateKey_Test()
        {
            string privKey = "zBV5tyLawo27JXe8HJHaW2yVnd2GjBvtBZjTZ1gM2cZBPXnBvZiTXoytXspGTEXTcsuiqmQ4dnerKwiEZEq5zn5UzTqkjzMLjyH7oMWp2fZhcttGmh7aFzDTFjGQjcUpXBWqtbnMQBimEmx8LQSZQLTj6ujDyDpAKDGERrKdok7BdRjm8XXHpxizp4G9yvaoU5oYBYjfbFn6ZSq3eu342c5CB6N8bNVA8xP72PGW6ffjxkhpPtV4XdQye63MqmQWZ6NBqeHKtuYvfF5ciiwEMr3LaoMNPmqcyVzWrWy4H5tV9VcAQLy9Lg42x3G27kXg5aSERjsUdi8YEkim9T6dBshg32BihrNYeSvEu75gabKLsnKLHmQvVChZch5wMM4mebZiKjnYbwZ3pviF3g1eqi6LHgTnN2htqqBmxD8";

            Key nbKey = KeyUtils.LoadNBEcKey(privKey);
            string sid = KeyUtils.DeriveSIN(nbKey);

            Assert.Equal("Tf16LdPo3iR4H9RYhqBXJrr96xHTyKR5teM", sid);
        }

    }
}
