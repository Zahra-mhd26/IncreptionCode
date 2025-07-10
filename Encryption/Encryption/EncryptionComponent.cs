using System;
using System.IO;
using System.Security.Cryptography;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;

public class FileEncryptorComponent : GH_Component
{
    private static readonly byte[] Key = Convert.FromBase64String("GZuHOfU1C5F6SgI+PRKo+g=="); // 16 bytes for AES-128  
    private static readonly byte[] IV = Convert.FromBase64String("0v2u51jxM1dwalKzbWiZHg=="); // 16 bytes for AES  

    public FileEncryptorComponent()
        : base("File Encryptor", "Encryptor", "Encrypt a file using AES encryption.", "Utilities", "Encryption")
    {
    }

    public override Guid ComponentGuid => new Guid("{9B2791FB-4844-4FF9-8C60-6828D2E9AF87}");

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("Input File Path", "I", "The path of the file to encrypt", GH_ParamAccess.item);
        pManager.AddTextParameter("Output File Path", "O", "The path where the encrypted file will be saved", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBooleanParameter("Success", "S", "True if the file was successfully encrypted.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string inputFilePath = string.Empty;
        string outputFilePath = string.Empty;
        bool success = false;

        if (!DA.GetData(0, ref inputFilePath)) return;
        if (!DA.GetData(1, ref outputFilePath)) return;

        try
        {
            EncryptFile(inputFilePath, outputFilePath);
            success = true;
        }
        catch (Exception ex)
        {
            success = false;
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
        }

        DA.SetData(0, success);
    }

    private void EncryptFile(string inputFile, string outputFile)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key; // Make sure the key is 32 bytes for AES-256  
            aes.IV = IV;   // Ensure the IV is 16 bytes  

            using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (FileStream fsEncrypted = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (CryptoStream csEncrypt = new CryptoStream(fsEncrypted, encryptor, CryptoStreamMode.Write))
            {
                fsInput.CopyTo(csEncrypt);
            }
        }
    }

    private static byte[] GenerateRandomKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] key = new byte[32]; // 256-bit key  
            rng.GetBytes(key);
            return key;
        }
    }

    private static byte[] GenerateRandomIV()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] iv = new byte[16]; // 128-bit IV  
            rng.GetBytes(iv);
            return iv;
        }
    }
}