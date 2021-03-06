﻿namespace SoundFingerprinting.Audio.Bass.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Tests;

    using Un4seen.Bass;

    [TestClass]
    public class BassPlayAudioFileServiceTest : AbstractTest
    {
        private BassPlayAudioFileService playAudioFileService;

        private Mock<IBassServiceProxy> proxy;

        [TestInitialize]
        public void SetUp()
        {
            proxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);

            playAudioFileService = new BassPlayAudioFileService(proxy.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            proxy.VerifyAll();
        }

        [TestMethod]
        public void TestPlayFile()
        {
            const int StreamId = 100;
            proxy.Setup(p => p.CreateStream("path-to-audio-file", BASSFlag.BASS_DEFAULT)).Returns(StreamId);
            proxy.Setup(p => p.StartPlaying(StreamId)).Returns(true);

            var result = playAudioFileService.PlayFile("path-to-audio-file");

            Assert.AreEqual(StreamId, (int)result);
        }

        [TestMethod]
        public void TestStopPlayingFile()
        {
            const int StreamId = 100;

            proxy.Setup(p => p.FreeStream(StreamId)).Returns(true);

            playAudioFileService.StopPlayingFile(StreamId);
        }

        [TestMethod]
        [ExpectedException(typeof(BassException))]
        public void TestPlayFileFailsWithExceptionNoStreamCreated()
        {
            proxy.Setup(p => p.CreateStream(It.IsAny<string>(), It.IsAny<BASSFlag>())).Returns(0);
            proxy.Setup(p => p.GetLastError()).Returns("error-description");

            playAudioFileService.PlayFile("path-to-audio-file");
        }

        [TestMethod]
        [ExpectedException(typeof(BassException))]
        public void TestCouldNotStartPlayingTheFile()
        {
            proxy.Setup(p => p.CreateStream(It.IsAny<string>(), It.IsAny<BASSFlag>())).Returns(1);
            proxy.Setup(p => p.GetLastError()).Returns("error-description");
            proxy.Setup(p => p.StartPlaying(It.IsAny<int>())).Returns(false);

            playAudioFileService.PlayFile("path-to-audio-file");
        }
    }
}
