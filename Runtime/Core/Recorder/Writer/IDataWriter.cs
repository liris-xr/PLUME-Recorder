namespace PLUME.Core.Recorder.Writer
{
    public interface IDataWriter<out TDataWriterInfo> where TDataWriterInfo:IDataWriterInfo
    {
        public TDataWriterInfo Info { get; }

        public void WriteTimelessData(DataChunks dataChunks);

        public void WriteTimestampedData(DataChunksTimestamped dataChunks);

        void Flush();

        public void Close();
    }

    public interface IDataWriterInfo
    {}
}
