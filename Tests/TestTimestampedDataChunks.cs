using System;
using System.Linq;
using NUnit.Framework;
using PLUME.Core.Recorder;
using Unity.Collections;

namespace PLUME.Tests
{
    [TestFixture]
    public class TestTimestampedDataChunks
    {
        private const ulong AnyTimestamp = 5L;
        private const ulong BeforeAllTimestamp = 0L;
        private const ulong InBetweenTimestamp = 2L;
        private const ulong AfterAllTimestamp = 4L;

        private const ulong FirstTimestamp = 1L;
        private const ulong SecondTimestamp = 3L;

        private static readonly byte[] EmptyChunk = Array.Empty<byte>();
        private static readonly byte[] NonEmptyChunk = { 42, 56, 21 };

        private static readonly byte[] FirstDataChunk = { 1, 2, 3, 4 };
        private static readonly byte[] SecondDataChunk = { 5, 6, 7, 8 };

        private DataChunksTimestamped _nonEmptyDataChunksTimestamped;

        [SetUp]
        public void Setup()
        {
            var dataChunks = (ReadOnlySpan<byte>)FirstDataChunk.Concat(SecondDataChunk).ToArray();
            var chunksLength = (ReadOnlySpan<int>)new[] { FirstDataChunk.Length, SecondDataChunk.Length };
            var timestamps = (ReadOnlySpan<ulong>)new[] { FirstTimestamp, SecondTimestamp };
            _nonEmptyDataChunksTimestamped =
                new DataChunksTimestamped(dataChunks, chunksLength, timestamps, Allocator.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            _nonEmptyDataChunksTimestamped.Dispose();
        }

        [Test]
        public void Empty_AddTimestampedDataChunk()
        {
            var data = new DataChunksTimestamped(Allocator.Temp);

            data.Add(NonEmptyChunk, AnyTimestamp);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(NonEmptyChunk);
            expected.Timestamps.Add(AnyTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void Empty_AddTimestampedDataChunk_EmptyChunk()
        {
            var data = new DataChunksTimestamped(Allocator.Temp);
            data.Add(EmptyChunk, AnyTimestamp);

            // Empty chunks should not be added.
            var expected = new DataChunksTimestamped(Allocator.Temp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void Empty_TryRemoveAllBeforeTimestamp_AnyTimestamp_Inclusive()
        {
            var data = new DataChunksTimestamped(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(AnyTimestamp, removedChunks, true);

            Assert.AreEqual(false, removed);
            Assert.IsTrue(removedChunks.IsEmpty());
        }

        [Test]
        public void Empty_TryRemoveAllBeforeTimestamp_AnyTimestamp_Exclusive()
        {
            var data = new DataChunksTimestamped(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(AnyTimestamp, removedChunks, false);

            Assert.AreEqual(false, removed);
            Assert.IsTrue(removedChunks.IsEmpty());
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_AfterAllTimestamp()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            data.Add(NonEmptyChunk, AfterAllTimestamp);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.DataChunks.Add(SecondDataChunk);
            expected.DataChunks.Add(NonEmptyChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);
            expected.Timestamps.Add(AfterAllTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_BeforeAllTimestamp()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            data.Add(NonEmptyChunk, BeforeAllTimestamp);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(NonEmptyChunk);
            expected.DataChunks.Add(FirstDataChunk);
            expected.DataChunks.Add(SecondDataChunk);
            expected.Timestamps.Add(BeforeAllTimestamp);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_InBetweenTimestamp()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            data.Add(NonEmptyChunk, InBetweenTimestamp);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.DataChunks.Add(NonEmptyChunk);
            expected.DataChunks.Add(SecondDataChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(InBetweenTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_ExistingTimestamp_FirstTimestamp()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            data.Add(NonEmptyChunk, FirstTimestamp);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk.Concat(NonEmptyChunk).ToArray());
            expected.DataChunks.Add(SecondDataChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_AddTimestampedDataChunk_ExistingTimestamp_SecondTimestamp()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            data.Add(NonEmptyChunk, SecondTimestamp);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.DataChunks.Add(SecondDataChunk.Concat(NonEmptyChunk).ToArray());
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(expected, data);
        }

        [Test]
        public void NonEmpty_TryRemoveAllBeforeTimestamp_BeforeAllTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(BeforeAllTimestamp, removedChunks, false);
            
            Assert.AreEqual(false, removed);
            Assert.IsTrue(removedChunks.IsEmpty());
        }

        [Test]
        public void NonEmpty_TryRemoveAllBeforeTimestamp_InBetweenTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(InBetweenTimestamp, removedChunks, false);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.Timestamps.Add(FirstTimestamp);

            Assert.AreEqual(true, removed);
            Assert.AreEqual(expected, removedChunks);
        }

        [Test]
        public void NonEmpty_TryRemoveAllBeforeTimestamp_AfterAllTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(AfterAllTimestamp, removedChunks, false);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.DataChunks.Add(SecondDataChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(true, removed);
            Assert.AreEqual(expected, removedChunks);
        }

        [Test]
        public void NonEmpty_TryRemoveAllBeforeTimestamp_FirstTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(FirstTimestamp, removedChunks, false);
            
            Assert.AreEqual(false, removed);
            Assert.IsTrue(removedChunks.IsEmpty());
        }

        [Test]
        public void NonEmpty_TryRemoveAllBeforeTimestamp_FirstTimestamp_Inclusive()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(FirstTimestamp, removedChunks, true);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.Timestamps.Add(FirstTimestamp);

            Assert.AreEqual(true, removed);
            Assert.AreEqual(expected, removedChunks);
        }

        [Test]
        public void NonEmpty_TryRemoveAllBeforeTimestamp_SecondTimestamp_Exclusive()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(SecondTimestamp, removedChunks, false);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.Timestamps.Add(FirstTimestamp);

            Assert.AreEqual(true, removed);
            Assert.AreEqual(expected, removedChunks);
        }

        [Test]
        public void NonEmpty_TryRemoveAllBeforeTimestamp_SecondTimestamp_Inclusive()
        {
            var data = _nonEmptyDataChunksTimestamped.Copy(Allocator.Temp);

            var removedChunks = new DataChunksTimestamped(Allocator.Temp);
            var removed = data.TryRemoveAllBeforeTimestamp(SecondTimestamp, removedChunks, true);

            var expected = new DataChunksTimestamped(Allocator.Temp);
            expected.DataChunks.Add(FirstDataChunk);
            expected.DataChunks.Add(SecondDataChunk);
            expected.Timestamps.Add(FirstTimestamp);
            expected.Timestamps.Add(SecondTimestamp);

            Assert.AreEqual(true, removed);
            Assert.AreEqual(expected, removedChunks);
        }
    }
}