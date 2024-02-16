using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PLUME.Core.Recorder.Data;

namespace PLUME.Tests
{
    [TestFixture]
    public class TestTimestampedDataChunks
    {
        private const long AnyTimestamp = 5L;
        private const long BeforeAllTimestamp = 0L;
        private const long InBetweenTimestamp = 2L;
        private const long AfterAllTimestamp = 4L;

        private const long FirstTimestamp = 1L;
        private const long SecondTimestamp = 3L;

        private byte[] _emptyChunk;
        private byte[] _dataChunk;

        private byte[] _firstDataChunk;
        private byte[] _secondDataChunk;

        private TimestampedDataChunks _nonEmptyDataChunks;

        [SetUp]
        public void Setup()
        {
            _emptyChunk = Array.Empty<byte>();
            _dataChunk = new byte[] { 42, 56, 21 };
            _firstDataChunk = new byte[] { 1, 2, 3, 4 };
            _secondDataChunk = new byte[] { 5, 6, 7, 8 };

            _nonEmptyDataChunks = new TimestampedDataChunks();
            _nonEmptyDataChunks.DataChunks.Add(_firstDataChunk);
            _nonEmptyDataChunks.Timestamps.Add(FirstTimestamp);
            _nonEmptyDataChunks.DataChunks.Add(_secondDataChunk);
            _nonEmptyDataChunks.Timestamps.Add(SecondTimestamp);
        }

        [Test]
        public void Empty_AddTimestampedDataChunk()
        {
            var data = new TimestampedDataChunks();

            data.Push(_dataChunk, AnyTimestamp);

            var expected = new TimestampedDataChunks();
            expected.DataChunks.Add(_dataChunk);
            expected.Timestamps.Add(AnyTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void Empty_AddTimestampedDataChunk_EmptyChunk()
        {
            var data = new TimestampedDataChunks();
            data.Push(_emptyChunk, AnyTimestamp);

            // Empty chunks should not be added.
            var expected = new TimestampedDataChunks();
            expected.Clear();

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void Empty_TryPopAllBeforeTimestamp_AnyTimestamp_Inclusive()
        {
            var data = new TimestampedDataChunks();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(AnyTimestamp, poppedDataChunks, poppedTimestamps, true);

            var expectedDataChunks = new DataChunks();
            var expectedChunksTimestamps = new List<long>();
            expectedDataChunks.Clear();
            expectedChunksTimestamps.Clear();

            Assert.AreEqual(false, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedChunksTimestamps, poppedTimestamps);
        }

        [Test]
        public void Empty_TryPopAllBeforeTimestamp_AnyTimestamp_Exclusive()
        {
            var data = new TimestampedDataChunks();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(AnyTimestamp, poppedDataChunks, poppedTimestamps, false);

            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();
            expectedDataChunks.Clear();
            expectedTimestamps.Clear();

            Assert.AreEqual(false, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_AfterAllTimestamp()
        {
            var data = _nonEmptyDataChunks.Copy();

            data.Push(_dataChunk, AfterAllTimestamp);

            var expected = new TimestampedDataChunks();
            expected.DataChunks.Add(_firstDataChunk);
            expected.DataChunks.Add(_secondDataChunk);
            expected.DataChunks.Add(_dataChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);
            expected.Timestamps.Add(AfterAllTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_BeforeAllTimestamp()
        {
            var data = _nonEmptyDataChunks.Copy();

            data.Push(_dataChunk, BeforeAllTimestamp);

            var expected = new TimestampedDataChunks();
            expected.DataChunks.Add(_dataChunk);
            expected.DataChunks.Add(_firstDataChunk);
            expected.DataChunks.Add(_secondDataChunk);
            expected.Timestamps.Add(BeforeAllTimestamp);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_InBetweenTimestamp()
        {
            var data = _nonEmptyDataChunks.Copy();

            data.Push(_dataChunk, InBetweenTimestamp);

            var expected = new TimestampedDataChunks();
            expected.DataChunks.Add(_firstDataChunk);
            expected.DataChunks.Add(_dataChunk);
            expected.DataChunks.Add(_secondDataChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(InBetweenTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_ExistingTimestamp_FirstTimestamp()
        {
            var data = _nonEmptyDataChunks.Copy();

            data.Push(_dataChunk, FirstTimestamp);

            var expected = new TimestampedDataChunks();
            expected.DataChunks.Add(_firstDataChunk.Concat(_dataChunk).ToArray());
            expected.DataChunks.Add(_secondDataChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_ExistingTimestamp_SecondTimestamp()
        {
            var data = _nonEmptyDataChunks.Copy();

            data.Push(_dataChunk, SecondTimestamp);

            var expected = new TimestampedDataChunks();
            expected.DataChunks.Add(_firstDataChunk);
            expected.DataChunks.Add(_secondDataChunk.Concat(_dataChunk).ToArray());
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_TryPopAllBeforeTimestamp_BeforeAllTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunks.Copy();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(BeforeAllTimestamp, poppedDataChunks, poppedTimestamps, false);
            
            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();

            Assert.AreEqual(false, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }

        [Test]
        public void NonEmpty_TryPopAllBeforeTimestamp_InBetweenTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunks.Copy();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(InBetweenTimestamp, poppedDataChunks, poppedTimestamps, false);
            
            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();
            expectedDataChunks.Add(_firstDataChunk);
            expectedTimestamps.Add(FirstTimestamp);

            Assert.AreEqual(true, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }

        [Test]
        public void NonEmpty_TryPopAllBeforeTimestamp_AfterAllTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunks.Copy();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(AfterAllTimestamp, poppedDataChunks, poppedTimestamps, false);
            
            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();
            expectedDataChunks.Add(_firstDataChunk);
            expectedDataChunks.Add(_secondDataChunk);
            expectedTimestamps.Add(FirstTimestamp);
            expectedTimestamps.Add(SecondTimestamp);

            Assert.AreEqual(true, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }
        
        [Test]
        public void NonEmpty_TryPopAllBeforeTimestamp_FirstTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunks.Copy();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(FirstTimestamp, poppedDataChunks, poppedTimestamps, false);
            
            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();

            Assert.AreEqual(false, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }
        
        [Test]
        public void NonEmpty_TryPopAllBeforeTimestamp_FirstTimestamp_Inclusive()
        {
            var data = _nonEmptyDataChunks.Copy();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(FirstTimestamp, poppedDataChunks, poppedTimestamps, true);
            
            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();
            expectedDataChunks.Add(_firstDataChunk);
            expectedTimestamps.Add(FirstTimestamp);

            Assert.AreEqual(true, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }
        
        [Test]
        public void NonEmpty_TryPopAllBeforeTimestamp_SecondTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunks.Copy();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(SecondTimestamp, poppedDataChunks, poppedTimestamps, false);
            
            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();
            expectedDataChunks.Add(_firstDataChunk);
            expectedTimestamps.Add(FirstTimestamp);

            Assert.AreEqual(true, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }
        
        [Test]
        public void NonEmpty_TryPopAllBeforeTimestamp_SecondTimestamp_Inclusive()
        {
            var data = _nonEmptyDataChunks.Copy();

            var poppedDataChunks = new DataChunks();
            var poppedTimestamps = new List<long>();
            var popped = data.TryPopAllBeforeTimestamp(SecondTimestamp, poppedDataChunks, poppedTimestamps, true);
            
            var expectedDataChunks = new DataChunks();
            var expectedTimestamps = new List<long>();
            expectedDataChunks.Add(_firstDataChunk);
            expectedDataChunks.Add(_secondDataChunk);
            expectedTimestamps.Add(FirstTimestamp);
            expectedTimestamps.Add(SecondTimestamp);

            Assert.AreEqual(true, popped);
            Assert.AreEqual(expectedDataChunks, poppedDataChunks);
            Assert.AreEqual(expectedTimestamps, poppedTimestamps);
        }
    }
}