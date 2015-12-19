﻿using System;
using NUnit.Framework;
using SharpTox.HL;
using SharpTox.HL.Transfers;
using SharpTox.Core;
using System.IO;
using System.Threading;

namespace SharpTox.Tests
{
    [TestFixture]
    public class HLTests
    {
        [Test]
        public void HLTest()
        {
            bool finished = false;
            byte[] data = new byte[1 << 16];
            byte[] receivedData = new byte[1 << 16];
            new Random().NextBytes(data);

            var tox1 = new ToxHL(ToxOptions.Default);
            var tox2 = new ToxHL(ToxOptions.Default);

            tox1.Start();
            tox2.Start();

            tox1.AddFriend(tox2.Id, "test");
            tox2.FriendRequestReceived += (sender, args) =>
            {
                var friend = tox2.AddFriendNoRequest(args.PublicKey);
                friend.FileSendRequestReceived += (s, e) => e.Transfer.Accept(new MemoryStream(receivedData));
            };

            while (!tox1.Friends[0].IsOnline)
            {
                Thread.Sleep(100);
            }

            var transfer = tox1.Friends[0].SendFile(new MemoryStream(data), "test.dat", ToxFileKind.Data);
            transfer.StateChanged += (sender, e) =>
            {
                if (e.State == ToxTransferState.Finished)
                    finished = true;
                else if (e.State == ToxTransferState.Canceled)
                    Assert.Fail();
            };
            transfer.StateChanged += (sender, e) => Assert.Fail();

            while (!finished)
            {
                Thread.Sleep(100);
            }

            tox1.Dispose();
            tox2.Dispose();
        }
    }
}
