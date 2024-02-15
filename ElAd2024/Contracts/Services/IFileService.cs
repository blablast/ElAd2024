namespace ElAd2024.Contracts.Services;

public interface IFileService
{
    T? Read<T>(string folderPath, string fileName);

    void Save<T>(string folderPath, string fileName, T content);

    void Delete(string folderPath, string fileName);
}
