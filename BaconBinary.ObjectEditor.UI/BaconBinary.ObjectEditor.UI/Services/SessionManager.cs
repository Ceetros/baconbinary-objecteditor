using System;
using System.IO;
using System.Text;

namespace BaconBinary.ObjectEditor.UI.Services
{
    public class SessionState
    {
        public string DatPath { get; set; }
        public string SprPath { get; set; }
        public string Version { get; set; }
    }

    public static class SessionManager
    {
        private const string Header = "BSUIT";
        private const string FileName = "bsuite.stt";
        private const byte FileVersion = 1;

        private static string GetFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
        }

        public static void SaveSession(string datPath, string sprPath, string version)
        {
            try
            {
                using var stream = new FileStream(GetFilePath(), FileMode.Create, FileAccess.Write);
                using var writer = new BinaryWriter(stream);
                
                writer.Write(Encoding.ASCII.GetBytes(Header));
                writer.Write(FileVersion);
                
                var datBytes = Encoding.UTF8.GetBytes(datPath);
                writer.Write((ushort)datBytes.Length);
                writer.Write(datBytes);
                
                var sprBytes = Encoding.UTF8.GetBytes(sprPath);
                writer.Write((ushort)sprBytes.Length);
                writer.Write(sprBytes);
                
                var verBytes = Encoding.ASCII.GetBytes(version);
                writer.Write((byte)verBytes.Length);
                writer.Write(verBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save session: {ex.Message}");
            }
        }

        public static SessionState LoadSession()
        {
            try
            {
                string path = GetFilePath();
                if (!File.Exists(path)) return null;

                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(stream);
                
                var headerBytes = reader.ReadBytes(Header.Length);
                string header = Encoding.ASCII.GetString(headerBytes);
                
                if (header != Header) return null;
                
                byte version = reader.ReadByte();
                
                ushort datLen = reader.ReadUInt16();
                byte[] datBytes = reader.ReadBytes(datLen);
                string datPath = Encoding.UTF8.GetString(datBytes);
                
                ushort sprLen = reader.ReadUInt16();
                byte[] sprBytes = reader.ReadBytes(sprLen);
                string sprPath = Encoding.UTF8.GetString(sprBytes);
                
                byte verLen = reader.ReadByte();
                byte[] verBytes = reader.ReadBytes(verLen);
                string clientVersion = Encoding.ASCII.GetString(verBytes);

                return new SessionState
                {
                    DatPath = datPath,
                    SprPath = sprPath,
                    Version = clientVersion
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load session: {ex.Message}");
                return null;
            }
        }
    }
}
