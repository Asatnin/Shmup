struct SoundData
{
    // параметры звукового файла
    public int channels, bits_per_sample, sample_rate, buffer;

    // данные звукового файла
    public byte[] data;
}